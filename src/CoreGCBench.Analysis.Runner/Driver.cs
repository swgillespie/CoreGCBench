// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CoreGCBench.Analysis.Runner
{
    public static class Driver
    {
        public static void Execute(Options opts)
        {
            // for the first step, we'll validate that our zip files exist.
            if (!ValidateFilesExist(opts))
            {
                return;
            }

            // next, we'll rehydrate our object models from zip files.
            using (AggregateDataSource data = new AggregateDataSource())
            {
                foreach (var zip in opts.ZipFiles)
                {
                    data.AddSource(new AnalysisDataSource(zip));
                }

                if (!LocateBaseline(opts, data))
                {
                    return;
                }

                Logger.Log($"Beginning analysis using version '{opts.BaselineVersion}' as the baseline");
                DoAnalysis(opts, data);
            }
        }

        /// <summary>
        /// Validates that all of our input files exist, returning false if they don't.
        /// </summary>
        /// <param name="opts">Command-line arguments</param>
        /// <returns>True if all of our input files exist, false otherwise</returns>
        private static bool ValidateFilesExist(Options opts)
        {
            foreach (var zip in opts.ZipFiles)
            {
                if (!File.Exists(zip))
                {
                    Logger.LogError($"file {zip} does not exist!");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Locate the baseline run. If no baseline run is provided, the first
        /// one that is found will be used. If a baseline run is provided and
        /// it can't be found on the AggregateDataSource, this method returns false.
        /// </summary>
        /// <param name="opts">Command-line options</param>
        /// <param name="data">The data in which the baseline run should be found</param>
        /// <returns>True if the baseline was found, false otherwise</returns>
        private static bool LocateBaseline(Options opts, AggregateDataSource data)
        {
            if (opts.BaselineVersion == null)
            {
                // pick the first version.
                opts.BaselineVersion = data.Versions().First().Version.Name;
                return true;
            }

            var baseline = data.Versions()
                .FirstOrDefault(v => v.Version.Name.Equals(opts.BaselineVersion));
            if (baseline == null)
            {
                Logger.LogError($"baseline build '{opts.BaselineVersion}' not found in any of the zip files given!");
                Logger.LogWarning("The contained versions are: ");
                foreach (var version in data.Versions())
                {
                    Logger.LogWarning($"  {version.Version.Name}");
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Actually performs the analysis, by constructing an <see cref="AnalysisSession"/> that
        /// will calculate metrics for every benchmark. If multiple versions of CoreCLR were given
        /// to us, we will compare the metrics we are given against the baseline.
        /// </summary>
        /// <param name="opts">Command-line options</param>
        /// <param name="data">Data to use</param>
        private static void DoAnalysis(Options opts, AggregateDataSource data)
        {
            if (data.Versions().Count() == 1)
            {
                DoSingularAnalysis(opts, data);
            }
            else
            {
                DoComparisonAnalysis(opts, data);
            }
        }

        private static void DoSingularAnalysis(Options opts, AggregateDataSource data)
        {
            Logger.Log("Beginning singular analysis");
            Debug.Assert(data.Versions().Count() == 1);
            var session = new StandaloneAnalysisSession(data, MetricCollection.Default);
            StandaloneAnalysisResult results = session.RunAnalysis();
            string json = JsonConvert.SerializeObject(results, Formatting.Indented);
            Logger.Log($"Analysis complete, writing to file: {opts.OutputFile}");
            File.WriteAllText(opts.OutputFile, json);
        }

        private static void DoComparisonAnalysis(Options opts, AggregateDataSource data)
        {
            Logger.Log("Beginning comparison analysis");
            Debug.Assert(data.Versions().Count() != 1);
            var session = new ComparisonAnalysisSession(data, MetricCollection.Default, opts.BaselineVersion, opts.PValue);
            ComparisonAnalysisResult results = session.RunAnalysis();
            Logger.Log($"Analysis complete, writing to file: {opts.OutputFile}");

            try
            {
                switch (opts.OutputType)
                {
                    case OutputType.Json:
                        string json = JsonConvert.SerializeObject(results, Formatting.Indented);
                        File.WriteAllText(opts.OutputFile, json);
                        break;
                    case OutputType.Csv:
                        using (StreamWriter writer = new StreamWriter(opts.OutputFile))
                        {
                            results.ToCsv(writer);
                        }
                        break;
                    default:
                        throw new NotImplementedException("unimplemented output type: " + opts.OutputType);
                }
            }
            catch (IOException exn)
            {
                Logger.LogError($"Failed to write to disk: {exn.Message}");
            }
        }
    }
}
