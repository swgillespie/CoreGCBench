// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Threading;

namespace CoreGCBench.Runner
{
    /// <summary>
    /// Various options that can be supplied to the runner via command-line.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Controls the verbosity of output. Defaults to none.
        /// </summary>
        public Verbosity Verbosity { get; set; } = Verbosity.None;

        /// <summary>
        /// The output stream that the runner will use to output its logging.
        /// Defaults to standard out.
        /// </summary>
        public TextWriter LogStream { get; set; } = Console.Out;

        /// <summary>
        /// Controls the output directory of the benchmark. Defaults to a folder named
        /// "CoreGCBench" in the current directory.
        /// </summary>
        public string OutputDirectory { get; set; } 
            = Path.Combine(Directory.GetCurrentDirectory(), "CoreGCBench");

        /// <summary>
        /// Controls the configuration file the benchmark runner will use to
        /// influence its behavior.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Whether or not this is a "dry run", or a run started for the purpose
        /// of testing the validation code. Defaults to false.
        /// </summary>
        public bool DryRun { get; set; } = false;

        /// <summary>
        /// If present, the driver will load the configuration from this variable
        /// (expected to be json) instead of loading the config file from disk.
        /// </summary>
        public string ConfigJson { get; set; }

        /// <summary>
        /// The CancellationToken signaled if the runner process is
        /// Ctrl+C'd.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }

    public enum Verbosity
    {
        None        = 0,
        Verbose     = 1,
        Diagnostic  = 2
    }
}
