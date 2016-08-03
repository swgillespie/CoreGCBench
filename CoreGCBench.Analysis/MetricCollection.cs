// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace CoreGCBench.Analysis
{
    /// <summary>
    /// A collection of metrics, to be run on the corpus of data
    /// collected by the analysis engine.
    /// </summary>
    public sealed class MetricCollection
    {
        private List<Metric> m_metrics = new List<Metric>();

        public static MetricCollection Default
        {
            get
            {
                var collection = new MetricCollection();
                collection.AddMetric(new DurationMetric());
                return collection;
            }
        }

        public void AddMetric(Metric metric)
        {
            m_metrics.Add(metric);
        }

        public IEnumerable<Metric> Metrics => m_metrics;
    }
}
