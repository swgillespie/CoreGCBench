// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CoreGCBench.Analysis
{
    /// <summary>
    /// A metric is a value of some sort that the analysis engine will
    /// draw out of a benchmark run and present. Metrics will be compared
    /// across versions to search for regressions.
    /// </summary>
    public abstract class Metric
    {
        /// <summary>
        /// The "direction" of this metric - whether a lower value
        /// is better or a higher value is better.
        /// </summary>
        public abstract Direction Direction { get; }

        /// <summary>
        /// The amount of variance this metric will allow before the
        /// analysis frameworks considers it to have been regressed or improved.
        /// </summary>
        public abstract double VarianceThreshold { get; }

        /// <summary>
        /// The unit of this metric.
        /// </summary>
        public abstract Unit Unit { get; }

        /// <summary>
        /// Gets the value of this metric, using the given data
        /// source.
        /// </summary>
        /// <param name="data">The data to inspect when calculating this metric</param>
        /// <returns>The value of the metric</returns>
        public abstract double GetValue(BenchmarkDataSource data);

        /// <summary>
        /// Gets the name of this metric, in a human-friendly representation.
        /// </summary>
        public abstract string Name { get; }
    }

    public enum Direction
    {
        LowerIsBetter,
        HigherIsBetter
    }

    public enum Unit
    {
        Milliseconds,
        Count
    }
}
