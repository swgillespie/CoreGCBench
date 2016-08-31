// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoreGCBench.Common
{
    /// <summary>
    /// A RunResult is the output of a benchmark run. It consists of a number of
    /// CoreclrVersionRunResults, one for each version of CoreCLR that was tested.
    /// </summary>
    public sealed class RunResult
    {
        public RunSettings Settings { get; set; }
        public IList<Tuple<CoreClrVersion, CoreclrVersionRunResult>> PerVersionResults { get; } = new List<Tuple<CoreClrVersion, CoreclrVersionRunResult>>();
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
        public Benchmark Benchmark { get; set; }
        public IList<IterationResult> Iterations { get; set; } = new List<IterationResult>();
    }

    /// <summary>
    /// The result of a single iteration of a benchmark.
    /// </summary>
    public sealed class IterationResult
    {
        public string TracePathLocation { get; set; }
        public long DurationMsec { get; set; }
        public int ExitCode { get; set; }
        public int Pid { get; set; }
    }

    /// <summary>
    /// A version of CoreCLR that we will be benchmarking.
    /// </summary>
    public sealed class CoreClrVersion
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Path { get; set; }

        public override int GetHashCode()
        {
            int hash1 = Name.GetHashCode();
            int hash2 = Path.GetHashCode();
            return (((hash1 << 5) + hash1) ^ hash2);
        }

        public override bool Equals(object obj)
        {
            CoreClrVersion other = obj as CoreClrVersion;
            if (other == null)
            {
                return false;
            }

            return other.Name.Equals(Name)
                && other.Path.Equals(Path);
        }

        /// <summary>
        /// Used by JSON.NET when (de)serializing this object.
        /// </summary>
        /// <returns>The name of this version.</returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Settings global to a benchmark run.
    /// </summary>
    public sealed class RunSettings : IEquatable<RunSettings>
    {
        /// <summary>
        /// Whether or not this run should use Server GC.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool ServerGC { get; set; }

        /// <summary>
        /// Whether or not this run should use Concurrent GC.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public bool ConcurrentGC { get; set; }

        public bool Equals(RunSettings other)
        {
            if (other == null)
            {
                return false;
            }

            return ServerGC == other.ServerGC
                && ConcurrentGC == other.ConcurrentGC;
        }
    }
}
