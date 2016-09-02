// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGCBench.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            IDictionary<Benchmark, string> probeMap;

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

            if (!ProbeForExecutables(run, out probeMap))
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

            Runner runner = new Runner(run, options, probeMap);
            RunResult result = runner.Run();
            PackageResults(result, options);
        }

        /// <summary>
        /// Given a benchmark run, resolves each benchmark's test executable by probing the path given to us.
        /// </summary>
        /// <param name="run">The BenchmarkRun whose benchmarks need to be resolved</param>
        /// <param name="probeMap">The probeMap calculated for every benchmark in the set</param>
        /// <returns>True if all benchmarks were resolved successfully, false otherwise.</returns>
        private static bool ProbeForExecutables(BenchmarkRun run, out IDictionary<Benchmark, string> probeMap)
        {
            Logger.LogVerbose("Beginning test executable probe");
            Dictionary<Benchmark, string> map = new Dictionary<Benchmark, string>();
            foreach (var bench in run.Suite)
            {
                if (Path.IsPathRooted(bench.ExecutablePath))
                {
                    Logger.LogError($"Benchmark {bench.Name} has an absolute path for its executable - please change this to a path relative to the TestProbeRoot.");
                    probeMap = null;
                    return false;
                }

                string absolutePath;
                if (!ProbeForExecutable(run.Settings.TestProbeRoot, bench.ExecutablePath, out absolutePath))
                {
                    Logger.LogError($"Failed to locate test executable for {bench.Name}, probing from directory {run.Settings.TestProbeRoot}!");
                    probeMap = null;
                    return false;
                }

                Debug.Assert(Path.IsPathRooted(absolutePath));
                map[bench] = absolutePath;
            }

            probeMap = map;
            Logger.LogVerbose("Test executable probe complete");
            return true;
        }

        /// <summary>
        /// Given a probe root and a file path to probe for, constructs an absolute path connecting the
        /// probe root and the executable path, if it exists.
        /// 
        /// Note that the performance of this method will be quite bad if it can't find the executable
        /// that its looking for. That's generally okay since we're about to bail anyway.
        /// </summary>
        /// <param name="testProbeRoot">The root directory to begin the probe</param>
        /// <param name="executablePath">The target file to probe for</param>
        /// <param name="absolutePath">The resolved absolute path for the target executable path</param>
        /// <returns>True if the probe was successful (we found a candidate file), false otherwise</returns>
        private static bool ProbeForExecutable(string testProbeRoot, string executablePath, out string absolutePath)
        {
            Debug.Assert(Path.IsPathRooted(testProbeRoot));
            Debug.Assert(!Path.IsPathRooted(executablePath));
            Logger.LogDiagnostic($"Beginning executable probe for {executablePath}");
            // we're doing a BFS here because the test probe root folder could potentially be huge (i.e. the coreclr
            // test directory) and we don't want to spend all of our time in random directories if we don't have to.
            Queue<string> worklist = new Queue<string>();
            worklist.Enqueue(testProbeRoot);
            while (worklist.Count != 0)
            {
                string path = worklist.Dequeue();

                // the executable path is most likely a path itself.
                string targetPath = Path.Combine(path, executablePath);
                Debug.Assert(Path.IsPathRooted(targetPath));
                if (File.Exists(targetPath))
                {
                    absolutePath = targetPath;
                    Logger.LogDiagnostic($"Executable probe: mapped {executablePath} to {absolutePath}");
                    return true;
                }

                foreach (var dir in Directory.EnumerateDirectories(path))
                {
                    worklist.Enqueue(dir);
                }
            }

            absolutePath = null;
            return false;
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
            }

            if (!Path.IsPathRooted(run.Settings.TestProbeRoot))
            {
                Logger.LogError($"Probe path {run.Settings.TestProbeRoot} is not absolute!");
                return false;
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
                Directory.SetCurrentDirectory("..");
                Directory.Delete(options.OutputDirectory, true);
            }
            catch (Exception exn)
            {
                Logger.LogWarning($"Failed to delete output directory: {exn.Message}");
            }
        }
    }
}
