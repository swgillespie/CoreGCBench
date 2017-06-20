// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGCBench.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace CoreGCBench.Analysis
{
    public sealed class StandaloneAnalysisResult
    {
        public RunSettings Settings { get; set; }
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
        [JsonConverter(typeof(StringEnumConverter))]
        public Unit Unit;
        [JsonConverter(typeof(StringEnumConverter))]
        public Direction Direction;
        public double Value;
        public double StandardDeviation;
        public int SampleSize;
    }

    public sealed class ComparisonAnalysisResult
    {
        public RunSettings Settings { get; set; }
        public double PValue { get; set; }
        public IList<VersionComparisonAnalysisResult> Candidates { get; set; } = new List<VersionComparisonAnalysisResult>();
    }

    public sealed class VersionComparisonAnalysisResult
    {
        public CoreClrVersion Version { get; set; }
        public IList<BenchmarkComparisonAnalysisResult> Benchmarks { get; set; } = new List<BenchmarkComparisonAnalysisResult>();
    }

    public sealed class BenchmarkComparisonAnalysisResult
    {
        public Benchmark Benchmark { get; set; }
        public IList<MetricComparison> Metrics { get; set; } = new List<MetricComparison>();
    }

    public sealed class MetricComparison
    {
        public string Metric;
        [JsonConverter(typeof(StringEnumConverter))]
        public Unit Unit;
        public MetricValue Baseline { get; set; }
        public MetricValue Candidate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ComparisonDecision Decision { get; set; }
    }

    public enum ComparisonDecision
    {
        Regression,
        Indeterminate,
        Improvement
    }
}
