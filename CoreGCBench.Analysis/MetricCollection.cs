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
                collection.AddMetric(new MaximumPauseMetric());
                collection.AddMetric(new MeanPauseMetric());
                collection.AddMetric(new MaximumPauseGenZeroMetric());
                collection.AddMetric(new MeanPauseGenZeroMetric());
                collection.AddMetric(new MaximumPauseGenOneMetric());
                collection.AddMetric(new MeanPauseGenOneMetric());
                collection.AddMetric(new MaximumPauseBlockingGenTwoMetric());
                collection.AddMetric(new MeanPauseBlockingGenTwoMetric());
                collection.AddMetric(new MaximumPauseBackgroundGenTwoMetric());
                collection.AddMetric(new MeanPauseBackgroundGenTwoMetric());
                collection.AddMetric(new ForegroundGCMetric());
                collection.AddMetric(new GCNumberMetric());
                collection.AddMetric(new MeanGenTwoFragmentationMetric());
                collection.AddMetric(new MeanEphemeralSizeMetric());
                collection.AddMetric(new CompactingGCMechanismMetric());
                collection.AddMetric(new PromotingGCMechanismMetric());
                collection.AddMetric(new DemotionGCMechanismMetric());
                collection.AddMetric(new CardBundleGCMechanismMetric());
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
