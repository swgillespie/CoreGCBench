// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGCBench.Common;
using System;
using System.Collections.Generic;

namespace CoreGCBench.Analysis
{
    public sealed class StandaloneAnalysisResult
    {
        public IList<StandaloneBenchmarkAnalysisResult> Benchmarks { get; set; } = new List<StandaloneBenchmarkAnalysisResult>();
    }

    public sealed class StandaloneBenchmarkAnalysisResult
    {
        public Benchmark Benchmark { get; set; }
        public IList<MetricValue> Metrics { get; set; } = new List<MetricValue>();
    }

    public struct MetricValue
    {
        public string Name;
        public Unit Unit;
        public double Value;
    }

    public sealed class ComparisonAnalysisResult
    {
        public IList<MetricComparison> Benchmarks { get; set; }
    }

    public sealed class ComparisonBenchmarkAnalysisResult
    {
        public Benchmark Benchmark { get; set; }
        public IList<MetricComparison> Diffs { get; set; } = new List<MetricComparison>();
    }

    public class MetricComparison
    {
        public string Name { get; set; }
        public Unit Unit { get; set; }
        public Direction Direction { get; set; }
        public Tuple<CoreClrVersion, double> BaselineValue { get; set; }
        public IDictionary<CoreClrVersion, double> CandidateValues { get; set; }
        public ComparisonDecision Result { get; set; }
    }

    public enum ComparisonDecision
    {
        Regression,
        Indeterminate,
        Improvement
    }
}
