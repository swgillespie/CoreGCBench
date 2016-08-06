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
    }

    /*
    public sealed class ComparisonAnalysisResult
    {
        public IList<ComparisonBenchmarkAnalysisResult> Benchmarks { get; set; }
    }

    public sealed class ComparisonBenchmarkAnalysisResult
    {
        public Benchmark Benchmark { get; set; }
        public IList<MetricComparison> Diffs { get; set; } = new List<MetricComparison>();
    }

    public class MetricComparison
    {
        public string Name { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Unit Unit { get; set; }
        public MetricSingleValue BaselineValue { get; set; }
        public IDictionary<string, MetricSingleValue> CandidateValues { get; set; } = new Dictionary<string, MetricSingleValue>();
        [JsonConverter(typeof(StringEnumConverter))]
        public ComparisonDecision Result { get; set; }
    }

    public class MetricSingleValue
    {
        public double Value;
        public double StandardDeviation;
    }*/

    public sealed class ComparisonAnalysisResult
    {
        public IList<VersionComparisonAnalysisResult> Candidates { get; set; } = new List<VersionComparisonAnalysisResult>();
    }

    public sealed class VersionComparisonAnalysisResult
    {
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
        public Unit Unit;
        public MetricValue Baseline { get; set; }
        public MetricValue Candidate { get; set; }
        public ComparisonDecision Decision { get; set; }
    }

    public enum ComparisonDecision
    {
        Regression,
        Indeterminate,
        Improvement
    }
}
