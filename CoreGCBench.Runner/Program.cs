// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.CommandLine;
using System.Diagnostics;

namespace CoreGCBench.Runner
{
    public class Program
    {
        private const string HelpTextHeader = @"
CoreCLR GC Benchmark test harness, by Microsoft.
This program accepts a json configuration file and uses it
to run a series of benchmarks designed to stress the CoreCLR
garbage collector.
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
        /// The actual main method. Exceptions are free to propegate past this method,
        /// they'll get caught by the above Main method.
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        private static void ActualMain(string[] args)
        {
            Options options = ParseCommandLine(args);
            Driver.Execute(options);
        }

        /// <summary>
        /// Parses the given command line into a <see cref="Options"/> object
        /// that is referenced throughout the run.
        /// </summary>
        /// <param name="args">The arguments received by Main</param>
        /// <returns>An Options object</returns>
        private static Options ParseCommandLine(string[] args)
        {
            Options options = new Options();
            bool verbose = false;
            bool veryVerbose = false;
            bool breakOnStartup = false;
            bool showHelp = false;
            string configFile = null;
            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.HandleHelp = false;
                syntax.DefineOption("h|help", ref showHelp, "Displays this message.");
                syntax.DefineOption("v|verbose", ref verbose, "Outputs additional verbose logging.");
                syntax.DefineOption("d|debug", ref veryVerbose, "Outputs very verbose logging, useful for debugging.");
                syntax.DefineOption("break-on-startup", ref breakOnStartup, "Launches a debugger on startup. Useful for debugging only.");
                syntax.DefineParameter("configuration", ref configFile, "The configuration file to use to configure the benchmark run.");

                if (showHelp)
                {
                    Console.WriteLine(HelpTextHeader);
                    Console.WriteLine(syntax.GetHelpText());
                    Environment.Exit(1);
                }

                if (string.IsNullOrEmpty(configFile))
                {
                    syntax.ReportError("the configuration file argument is required. See the helptext (--help) for usage information.");
                }
            });

            options.ConfigFile = configFile;
            options.Verbosity = Verbosity.None;
            if (verbose)
            {
                options.Verbosity = Verbosity.Verbose;
            }

            if (veryVerbose)
            {
                options.Verbosity = Verbosity.Diagnostic;
            }

            if (breakOnStartup)
            {
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
            }

            return options;
        }
    }
}
