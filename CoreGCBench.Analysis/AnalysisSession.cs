// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
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
            }

            return result;
        }

        private IEnumerable<MetricValue> CalculateMetrics(BenchmarkDataSource bench)
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

    public class ComparisonAnalysisSession : AnalysisSession
    {
        public ComparisonAnalysisSession(AggregateDataSource data, MetricCollection metrics)
            : base(data, metrics) { }

        public ComparisonAnalysisResult RunAnalysis()
        {
            throw new NotImplementedException();
        }
    }
}
