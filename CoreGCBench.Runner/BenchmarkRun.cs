// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoreGCBench.Runner
{
    /// <summary>
    /// A BenchmarkRun consists of a list of benchmarks, a
    /// set of data to collect, and one (or more) versions of CoreCLR to run
    /// the test against.
    /// </summary>
    public sealed class BenchmarkRun
    {
        /// <summary>
        /// The benchmark suite to run as part of this benchmark run.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<Benchmark> Suite { get; set; }

        /// <summary>
        /// The data to collect during the benchmark run.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public CollectionLevel CollectionLevel { get; set; }

        /// <summary>
        /// The list of CoreCLR versions to run this benchmark for. These
        /// are paths to CORE_ROOTS for each version of CoreCLR that was built.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<CoreClrVersion> CoreCLRVersions { get; set; }
    }

    /// <summary>
    /// A CoreCLR version that we will be running benchmarks on.
    /// </summary>
    public class CoreClrVersion
    {
        [JsonProperty(Required = Required.Always)]
        public string HumanReadableName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string CoreRootPath { get; set; }

        public override int GetHashCode()
        {
            int hash1 = HumanReadableName.GetHashCode();
            int hash2 = CoreRootPath.GetHashCode();
            return (((hash1 << 5) + hash1) ^ hash2);
        }

        public override bool Equals(object obj)
        {
            CoreClrVersion other = obj as CoreClrVersion;
            if (other == null)
            {
                return false;
            }

            return other.HumanReadableName.Equals(HumanReadableName)
                && other.CoreRootPath.Equals(CoreRootPath);
        }
    }

    [Flags]
    public enum CollectionLevel
    {
        GCInformational = 0x1,
        GCVerbose       = 0x2,
        CPUEvents       = 0x4,
        All = GCInformational | GCVerbose | CPUEvents
    }
}
