// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System.Collections.Generic;

namespace CoreGCBench.Common
{
    /// <summary>
    /// A single benchmark that will be run by the benchmark framework.
    /// These objects are populated by reading a json configuration file
    /// provided to the runner.
    /// </summary>
    public sealed class Benchmark
    {
        /// <summary>
        /// The name of the benchmark, to be used in reporting.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// The path to the test executable.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string ExecutablePath { get; set; }

        /// <summary>
        /// Whether or not this test uses Server GC. Defaults to false.
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public bool ServerGC { get; } = false;

        /// <summary>
        /// Whether or not this test uses Concurrent GC. Defaults to true.
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public bool ConcurrentGC { get; } = true;

        /// <summary>
        /// Whether this test should end after some number of Gen0 GC's.
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public int? EndAfterGen0GCCount { get; } = null;

        /// <summary>
        /// Whether this test should end after some number of Gen1 GC's
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public int? EndAfterGen1GCCount { get; } = null;

        /// <summary>
        /// Whether this test should end after some number of Gen2 background GC's
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public int? EndAfterBackgroundGen2GCCount { get; } = null;

        /// <summary>
        /// Whether this test should end after some number of Gen2 blocking GC's
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public int? EndAfterBlockingGen2GCCount { get; } = null;

        /// <summary>
        /// Whether this test should terminate after some period of time.
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public int? EndAfterTimeElapsed { get; } = null;

        /// <summary>
        /// A set of variables to pass to the child process.
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public Dictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>();
    }
}
