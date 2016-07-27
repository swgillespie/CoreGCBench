// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace CoreGCBench.Runner
{
    /// <summary>
    /// A RunResult is the output of a benchmark run. It consists of a number of
    /// CoreclrVersionRunResults, one for each version of CoreCLR that was tested.
    /// </summary>
    public sealed class RunResult
    {
        public IDictionary<CoreClrVersion, CoreclrVersionRunResult> PerVersionResults { get; } = new Dictionary<CoreClrVersion, CoreclrVersionRunResult>();
    }

    /// <summary>
    /// The result of running benchmarks on a specific version of CoreCLR.
    /// </summary>
    public sealed class CoreclrVersionRunResult
    {
        public IList<BenchmarkResult> BenchmarkResults { get; set; } = new List<BenchmarkResult>();
    }

    /// <summary>
    /// The result of running a single benchmark.
    /// </summary>
    public sealed class BenchmarkResult
    {
        public string TracePathLocation { get; set; }
        public long DurationMsec { get; set; }
        public int ExitCode { get; set; }
    }
}
