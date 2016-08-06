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
                    StandardDeviation = values.StandardDeviation()
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

    public class ComparisonAnalysisSession : AnalysisSession
    {
        public string m_baseline;

        public ComparisonAnalysisSession(AggregateDataSource data, MetricCollection metrics, string baseline)
            : base(data, metrics)
        {
            m_baseline = baseline;
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
            Debug.Assert(baselineMetric.Name.Equals(candidateMetric.Name));
            Debug.Assert(baselineMetric.Unit == candidateMetric.Unit);
            Debug.Assert(baselineMetric.Direction == candidateMetric.Direction);

            // What we have is a mean and a standard deviation. From here, assuming
            // a normal distribution (which is guaranteed to occur if the number of iterations
            // is high enough), we can assume that ~68% of the samples taken from this hypothetical
            // distribution are within one standard deviation of th emean.
            //
            // for the initial version of this analysis engine, we're going to raise an error
            // if the mean of the candidate build has drifted more than one standard deviation
            // from the mean.
            double baselineIntervalLo = baselineMetric.Value - baselineMetric.StandardDeviation;
            double baselineIntervalHi = baselineMetric.Value + baselineMetric.StandardDeviation;
            ComparisonDecision decision;
            if (candidateMetric.Value < baselineIntervalLo)
            {
                decision = ComparisonDecision.Improvement;
            }
            else if (candidateMetric.Value > baselineIntervalHi)
            {
                decision = ComparisonDecision.Regression;
            }
            else
            {
                decision = ComparisonDecision.Indeterminate;
            }

            if (baselineMetric.Direction != Direction.LowerIsBetter)
            {
                switch (decision)
                {
                    case ComparisonDecision.Improvement:
                        decision = ComparisonDecision.Regression;
                        break;
                    case ComparisonDecision.Regression:
                        decision = ComparisonDecision.Improvement;
                        break;
                    default:
                        break;
                }
            }

            return decision;
        }

        /*
        /// <summary>
        /// The version to use as a baseline.
        /// </summary>
        private string m_baseline;


        public ComparisonAnalysisResult RunAnalysis()
        {
            Tuple<CoreClrVersion, StandaloneAnalysisResult> baseline = null;
            List<Tuple<CoreClrVersion, StandaloneAnalysisResult>> results = new List<Tuple<CoreClrVersion, StandaloneAnalysisResult>>();
            foreach (var tup in CalculateVersionData())
            {
                CoreClrVersion version = tup.Item1;
                StandaloneAnalysisResult result = tup.Item2;
                if (version.Name.Equals(m_baseline))
                {
                    baseline = tup;
                }
                else
                {
                    results.Add(tup);
                }
            }

            return new ComparisonAnalysisResult
            {
                Benchmarks = DoComparison(baseline, results).ToList()
            };
        }

        private IEnumerable<Tuple<CoreClrVersion, StandaloneAnalysisResult>> CalculateVersionData()
        {
            foreach (var version in m_dataSource.Versions())
            {
                var result = new StandaloneAnalysisResult();
                foreach (var bench in version.BenchmarkResults)
                {
                    var benchResult = new StandaloneBenchmarkAnalysisResult();
                    benchResult.Benchmark = bench.Benchmark;
                    benchResult.Metrics = CalculateMetrics(bench).ToList();
                    result.Benchmarks.Add(benchResult);
                }

                yield return Tuple.Create(version.Version, result);
            }
        }

        private IEnumerable<ComparisonBenchmarkAnalysisResult> DoComparison(
            Tuple<CoreClrVersion, StandaloneAnalysisResult> baseline,
            List<Tuple<CoreClrVersion, StandaloneAnalysisResult>> results)
        {
            for (int i = 0; i < baseline.Item2.Benchmarks.Count; i++)
            {
                var baselineBench = baseline.Item2.Benchmarks[i];
                var candidates = results.Select(t => Tuple.Create(t.Item1, t.Item2.Benchmarks[i]));

                for (int j = 0; i < baselineBench.Metrics.Count; j++)
                {
                    var metric = baselineBench.Metrics[j];
                    var comp = new MetricComparison();
                    comp.Name = metric.Name;
                    comp.Unit = metric.Unit;

                    var candidateValues = candidates.Select(t => Tuple.Create(t.Item1, t.Item2.Metrics[j]));

                    foreach (var value in candidateValues)
                    {
                        comp.CandidateValues[value.Item1.Name] = DiffMetric(metric, value.Item2);
                    }
                }
            }
        }

        /// <summary>
        /// Given a baseline value and a candidate value, performs a diff on the two metrics
        /// and decides whether or not the candidate value has regressed, improved, or left
        /// the metric the same.
        /// </summary>
        /// <param name="baselineValue">The baseline metric value</param>
        /// <param name="candidate">The candidate metric value</param>
        /// <returns>The <see cref="MetricSingleValue"/> for these two metrics</returns>
        private MetricSingleValue DiffMetric(MetricValue baselineValue, MetricValue candidate)
        {
            throw new NotImplementedException();
        }
        */
    }
}
