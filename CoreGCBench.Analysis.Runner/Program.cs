// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace CoreGCBench.Analysis.Runner
{
    public static class Program
    {
        private const string Usage = @"
CoreCLR GC Benchmark test analysis engine, by Microsoft.
This program accepts one or more zip files output by the CoreCLR
GC Benchmark runner and performs analyses upon them.

usage: CoreGCBench.Analysis.Runner.exe [example1.zip] [example2.zip] ... [exampleN.zip] 
                                       [-b|--baseline] [-v|--verbose] [-h|--help]

Options:
    -b|--baseline        Selects the baseline build for this analysis session. Must be
                         one of the named CoreCLR versions contained in the given zip files.
    -v|--verbose         Enable verbose logging.
    -o|--output-file     Sets the output file.
    -h|--help            Display this message.
";

        public static int Main(string[] args)
        {
            try
            {
                ActualMain(args);
                return 0;
            }
            catch (Exception exn)
            {
                Console.Error.WriteLine("We encountered a fatal exception while running. This is a bug. We'd greatly appreciate a report!");
                Console.Error.WriteLine("");
                Console.Error.WriteLine("Stack trace:");
                Console.Error.WriteLine(exn.ToString());
                return 1;
            }
        }

        /// <summary>
        /// The actual main method. The analysis driver doesn't need any
        /// kind of sophisticated command-line parsing so it's quite simple.
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        public static void ActualMain(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.WriteLine(Usage);
                Environment.Exit(1);
            }

            Options opts = ParseArguments(args);
        }

        /// <summary>
        /// Parse our command-line arguments. We're doing this ourselves here
        /// to avoid having to bring in dependencies - System.CommandLine that we
        /// use in the runner isn't available on Desktop .NET.
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Options parsed from the command-line</returns>
        private static Options ParseArguments(string[] args)
        {
            Options opts = new Options();
            int idx = 0;
            bool help = false;
            while (idx < args.Length)
            {
                switch (args[idx])
                {
                    case "-b":
                    case "--baseline":
                        idx++;
                        if (idx >= args.Length)
                        {
                            ArgumentParseError("expected an argument after baseline parameter");
                        }
                        opts.BaselineVersion = args[idx++];
                        break;
                    case "-v":
                    case "--verbose":
                        idx++;
                        opts.Verbose = true;
                        break;
                    case "-o":
                    case "--output-file":
                        idx++;
                        if (idx >= args.Length)
                        {
                            ArgumentParseError("expected an argument after output file parameter");
                        }
                        opts.OutputFile = args[idx++];
                        break;
                    case "-h":
                    case "--help":
                        idx++;
                        help = true;
                        break;
                    default:
                        opts.ZipFiles.Add(args[idx++]);
                        break;
                }
            }

            if (help)
            {
                Console.WriteLine(Usage);
                Environment.Exit(0);
            }

            if (opts.ZipFiles.Count == 0)
            {
                ArgumentParseError("must provide at least one zip file to analyze");
            }

            return opts;
        }

        private static void ArgumentParseError(string error)
        {
            Console.WriteLine("error parsing arguments: " + error);
            Environment.Exit(1);
        }
    }
}
