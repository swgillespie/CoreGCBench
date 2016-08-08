// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CoreGCBench.Analysis
{
    /// <summary>
    /// An analysis session is a pairing of a <see cref="MetricCollection"/>
    /// and a <see cref="AggregateDataSource"/>. We will combine these two sets
    /// of data by calculating all metrics and, if multiple versions of CoreCLR
    /// were benchmarked, will pit these metrics against a baseline.
    /// </summary>
    public abstract class AnalysisSession
    {
        /// <summary>
        /// The metrics that will be used during this analysis.
        /// </summary>
        protected MetricCollection m_metrics;

        /// <summary>
        /// The data source that will be used during this analysis.
        /// Does not own the data source.
        /// </summary>
        protected AggregateDataSource m_dataSource;

        public AnalysisSession(AggregateDataSource data, MetricCollection metrics)
        {
            m_metrics = metrics;
            m_dataSource = data;
        }

        protected IEnumerable<MetricValue> CalculateMetrics(BenchmarkDataSource bench)
        {
            foreach (var metric in m_metrics.Metrics)
            {
                List<double> values = new List<double>();
                foreach (var iter in bench.Iterations)
                {
                    values.Add(metric.GetValue(iter));
                }

                yield return new MetricValue
                {
                    Name = metric.Name,
                    Unit = metric.Unit,
                    Value = values.Average(),
                    StandardDeviation = values.StandardDeviation(),
                    SampleSize = values.Count
                };
            }
        }
    }

    /// <summary>
    /// StandaloneAnalysisSession is a <see cref="AnalysisSession"/> that acts
    /// on a single version of CoreCLR. It is only constructed when the benchmark
    /// was run on a single version of CoreCLR, so there's nothing to compare to.
    /// </summary>
    public sealed class StandaloneAnalysisSession : AnalysisSession
    {
        public StandaloneAnalysisSession(AggregateDataSource data, MetricCollection metrics)
            : base(data, metrics) { }

        public StandaloneAnalysisResult RunAnalysis()
        {
            if (m_dataSource.Versions().Count() != 1)
            {
                throw new InvalidOperationException("Can't run a standalone analysis session on a data source with more than one version");
            }

            var version = m_dataSource.Versions().First();
            var result = new StandaloneAnalysisResult();
            foreach (var bench in version.BenchmarkResults)
            {
                var benchResult = new StandaloneBenchmarkAnalysisResult();
                benchResult.Benchmark = bench.Benchmark;
                benchResult.Metrics = CalculateMetrics(bench).ToList();
                result.Benchmarks.Add(benchResult);
            }

            return result;
        }
    }

    /// <summary>
    /// Performs a comparison on one or more candidate builds against a baseline build. A "decision" is made
    /// for every metric on whether or not that metric regressed, improved, or stayed the same, based on
    /// statistical analysis.
    /// </summary>
    public class ComparisonAnalysisSession : AnalysisSession
    {
        private string m_baseline;
        private double m_pvalue;

        public ComparisonAnalysisSession(AggregateDataSource data, MetricCollection metrics, string baseline, double pvalue)
            : base(data, metrics)
        {
            m_baseline = baseline;
            m_pvalue = pvalue;
        }

        public ComparisonAnalysisResult RunAnalysis()
        {
            VersionAnalysisDataSource baseline = null;
            foreach (var version in m_dataSource.Versions())
            {
                if (version.Version.Name.Equals(m_baseline))
                {
                    baseline = version;
                    break;
                }
            }

            Debug.Assert(baseline != null);

            return new ComparisonAnalysisResult
            {
                PValue = m_pvalue,
                Candidates = DoVersionComparisons(baseline).ToList()
            };
        }

        private IEnumerable<VersionComparisonAnalysisResult> DoVersionComparisons(VersionAnalysisDataSource baseline)
        {
            foreach (var version in m_dataSource.Versions())
            {
                if (version.Version.Name.Equals(baseline.Version.Name))
                {
                    continue;
                }

                yield return DoSingleVersionComparison(baseline, version);
            }
        }

        private VersionComparisonAnalysisResult DoSingleVersionComparison(VersionAnalysisDataSource baseline, VersionAnalysisDataSource candidate)
        {
            Debug.Assert(baseline.BenchmarkResults.Count == candidate.BenchmarkResults.Count);
            var result = new VersionComparisonAnalysisResult();

            foreach (var bench in baseline.BenchmarkResults.Zip(candidate.BenchmarkResults, Tuple.Create)) {
                result.Benchmarks.Add(DoSingleBenchmarkComparison(bench.Item1, bench.Item2));
            }

            result.Version = candidate.Version;
            return result;
        }

        private BenchmarkComparisonAnalysisResult DoSingleBenchmarkComparison(BenchmarkDataSource baseline, BenchmarkDataSource candidate)
        {
            var result = new BenchmarkComparisonAnalysisResult();
            var baselineMetrics = CalculateMetrics(baseline);
            var candidateMetrics = CalculateMetrics(candidate);
            foreach (var metric in baselineMetrics.Zip(candidateMetrics, Tuple.Create))
            {
                var baselineMetric = metric.Item1;
                var candidateMetric = metric.Item2;
                var comp = new MetricComparison();
                comp.Metric = baselineMetric.Name;
                comp.Unit = baselineMetric.Unit;
                comp.Baseline = baselineMetric;
                comp.Candidate = candidateMetric;
                comp.Decision = MakeDecision(baselineMetric, candidateMetric);
                result.Metrics.Add(comp);
            }

            result.Benchmark = baseline.Benchmark;
            return result;
        }

        private ComparisonDecision MakeDecision(MetricValue baselineMetric, MetricValue candidateMetric)
        {
            // we're doing a t-test to make a decision on the test metrics, for now.
            return TTest.Run(baselineMetric, candidateMetric, m_pvalue);
        }
    }
}
