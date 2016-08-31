// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGCBench.Common;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CoreGCBench.Runner
{
    public static class Driver
    {
        /// <summary>
        /// Drives the benchmark running process, using the given arguments.
        /// </summary>
        /// <param name="options">The options parsed from the command-line</param>
        public static void Execute(Options options)
        {
            Logger.Initialize(options);
            Logger.LogAlways($"Beginning benchmark run with config: {options.ConfigFile}");
            Logger.LogAlways($"Output directory: {options.OutputDirectory}");

            BenchmarkRun run;

            if (!LoadConfigFile(options, out run))
            {
                // contract - all of these validation messages
                // log an error message before returning.
                return;
            }

            Debug.Assert(run != null);
            if (!ValidateConfig(run))
            {
                return;
            }

            if (options.DryRun)
            {
                // if this is a dry run, we're done here.
                return;
            }

            // otherwise, we're going to begin execution.
            // create the output directory, if it doesn't exist yet.
            if (!Directory.Exists(options.OutputDirectory))
            {
                Directory.CreateDirectory(options.OutputDirectory);
            }

            Directory.SetCurrentDirectory(options.OutputDirectory);

            Runner runner = new Runner(run, options);
            RunResult result = runner.Run();
            PackageResults(result, options);
        }

        private static bool LoadConfigFile(Options opts, out BenchmarkRun run)
        {
            try
            {
                string fileText;
                if (opts.ConfigJson != null)
                {
                    fileText = opts.ConfigJson;
                }
                else
                {
                    fileText = File.ReadAllText(opts.ConfigFile);
                }
                run = JsonConvert.DeserializeObject<BenchmarkRun>(fileText);
                return true;
            }
            catch (Exception exn)
            {
                // TODO(segilles) we can probably produce a better error message here.
                Logger.LogError($"Failed to load configuration file: {exn.Message}");
                run = null;
                return false;
            }
        }

        /// <summary>
        /// Validates a configuration given to us. Prints an error
        /// message and returns false upon failure, returns true
        /// upon success.
        /// </summary>
        /// <param name="run">The configuration to validate</param>
        /// <returns>True if the validation was successful, false otherwise</returns>
        private static bool ValidateConfig(BenchmarkRun run)
        {
            Logger.LogVerbose("Validating configuration");
            foreach (var version in run.CoreClrVersions)
            {
                if (version.Name.Any(c => Path.GetInvalidPathChars().Contains(c))
                    || string.IsNullOrEmpty(version.Name))
                {
                    Logger.LogError($"Version name \"{version.Name}\" has inapporpriate characters for a file path.");
                    return false;
                }

                if (!Directory.Exists(version.Path))
                {
                    Logger.LogError($"Version path {version.Path} does not exist.");
                    return false;
                }

                string coreRun = Path.Combine(version.Path, Utils.CoreRunName);
                if (!File.Exists(coreRun))
                {
                    Logger.LogError($"Corerun not found on path {version.Path}.");
                    return false;
                }
            }

            if (run.CoreClrVersions.Count == 0)
            {
                Logger.LogError("Must provide at least one version of CoreCLR to test.");
                return false;
            }

            foreach (var benchmark in run.Suite)
            {
                if (benchmark.Name.Any(c => Path.GetInvalidPathChars().Contains(c)) || string.IsNullOrEmpty(benchmark.Name))
                {
                    Logger.LogError($"Benchmark name \"{benchmark.Name}\" has inapporpriate characters for a file path.");
                    return false;
                }

                if (!File.Exists(benchmark.ExecutablePath))
                {
                    Logger.LogError($"Benchmark executable {benchmark.ExecutablePath} does not exist.");
                    return false;
                }
            }

            Logger.LogVerbose("Validation successful");
            return true;
        }

        private static void PackageResults(RunResult result, Options options)
        {
            // if we did this right, we've created a directory structure that looks like this:
            // name
            // |
            // +--version1
            // |  |
            // |  + bench1
            // |  |
            // |  + bench2
            // |
            // +--version2
            // |  |
            // |  + bench1
            // |  |
            // |  + bench2
            //
            // From here we can serialize our RunResult to a json file, drop it in the
            // toplevel directory, zip the whole directory and call it a day.
            Debug.Assert(Directory.GetCurrentDirectory() == options.OutputDirectory);
            string jsonFile = Path.Combine(options.OutputDirectory, Constants.OverallResultsJsonName);
            try
            {
                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                File.WriteAllText(jsonFile, json);
            }
            catch (Exception exn)
            {
                Logger.LogError($"Failed to serialize run result to disk: {exn.ToString()}");
                return;
            }

            // This isn't very nice, but it works.
            string zipFileName = Path.Combine(
                options.OutputDirectory,
                "..", 
                Path.GetFileName(Path.GetDirectoryName(jsonFile)) + ".zip");
            Logger.LogVerbose($"Writing zip file to {zipFileName}");

            try
            {
                if (File.Exists(zipFileName))
                {
                    Logger.LogWarning($"Overwriting existing zip file {zipFileName}");
                    File.Delete(zipFileName);
                }

                ZipFile.CreateFromDirectory(options.OutputDirectory, zipFileName);
            }
            catch (Exception exn)
            {
                Logger.LogError($"Failed to zip results folder: {exn.ToString()}");
                return;
            }

            Logger.LogAlways($"Wrote results to zip file: {zipFileName}");

            try
            {
                Directory.Delete(options.OutputDirectory, true);
            }
            catch (Exception exn)
            {
                Logger.LogWarning($"Failed to delete output directory: {exn.Message}");
            }
        }
    }
}
