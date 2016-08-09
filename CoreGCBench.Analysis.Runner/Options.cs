// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace CoreGCBench.Analysis.Runner
{
    /// <summary>
    /// Options gathered by the command-line parser, used by the driver.
    /// </summary>
    public sealed class Options
    {
        /// <summary>
        /// The zip files given to us at the command-line. Each zip file may
        /// contain one or more versions of CoreCLR that we will analyze.
        /// </summary>
        public IList<string> ZipFiles { get; set; } = new List<string>();

		/// <summary>
        /// The baseline version that, when more than one version of CoreCLR is being analyzed,
        /// will serve as the baseline for all metric calculates.
        /// </summary>
		public string BaselineVersion { get; set; }

        /// <summary>
        /// Enable verbose logging. Defaults to false.
        /// </summary>
        public bool Verbose { get; set; } = false;

		/// <summary>
        /// The output file to write our chosen result to.
        /// </summary>
		public string OutputFile { get; set; }

        /// <summary>
        /// The PValue to use when analyzing whether or not a regression
        /// occured in a competitive analysis. Defaults to 0.05.
        /// </summary>
        public double PValue { get; set; } = TTest.StandardPValue;

        /// <summary>
        /// The type of file that the infrastructure will output.
        /// Defaults to JSON.
        /// </summary>
        public OutputType OutputType { get; set; } = OutputType.Json;
    }

    public enum OutputType
    {
        Json,
        Csv
    }
}
