﻿// Licensed to the .NET Foundation under one or more agreements.
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
        /// The path to the test executable. Always a relative path.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string ExecutablePath { get; set; }

        /// <summary>
        /// The command-line arguments to pass to the benchmark.
        /// </summary>
        [JsonProperty(
            Required = Required.Default,
            NullValueHandling = NullValueHandling.Ignore)]
        public string Arguments { get; set; } = null;

        /// <summary>
        /// Whether this test should end after some number of Gen0 GC's.
        /// </summary>
        [JsonProperty(
            Required = Required.Default, 
            NullValueHandling = NullValueHandling.Ignore)]
        public int? EndAfterGen0GCCount { get; set; } = null;

        /// <summary>
        /// Whether this test should end after some number of Gen1 GC's
        /// </summary>
        [JsonProperty(
            Required = Required.Default, 
            NullValueHandling = NullValueHandling.Ignore)]
        public int? EndAfterGen1GCCount { get; set; } = null;

        /// <summary>
        /// Whether this test should end after some number of Gen2 background GC's
        /// </summary>
        [JsonProperty(
            Required = Required.Default, 
            NullValueHandling = NullValueHandling.Ignore)]
        public int? EndAfterBackgroundGen2GCCount { get; set; } = null;

        /// <summary>
        /// Whether this test should end after some number of Gen2 blocking GC's
        /// </summary>
        [JsonProperty(
            Required = Required.Default, 
            NullValueHandling = NullValueHandling.Ignore)]
        public int? EndAfterBlockingGen2GCCount { get; set; } = null;

        /// <summary>
        /// Whether this test should terminate after some period of time.
        /// </summary>
        [JsonProperty(
            Required = Required.Default, 
            NullValueHandling = NullValueHandling.Ignore)]
        public int? EndAfterTimeElapsed { get; set; } = null;

        /// <summary>
        /// The number of times to run the benchmark, averaging all data
        /// collected across all iterations. Defaults to null, meaning
        /// the benchmark will run once.
        /// </summary>
        [JsonProperty(
            Required = Required.Default, 
            NullValueHandling = NullValueHandling.Ignore)]
        public int? Iterations { get; set; } = null;

        /// <summary>
        /// A set of variables to pass to the child process.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
    }
}
