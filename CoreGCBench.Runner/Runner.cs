﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGCBench.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CoreGCBench.Runner
{
    /// <summary>
    /// Runner of benchmarks. Given a benchmark suite and run options,
    /// runs all of the benchmarks.
    /// </summary>
    public sealed class Runner
    {
        /// <summary>
        /// The benchmark run to execute.
        /// </summary>
        private BenchmarkRun m_run;

        /// <summary>
        /// The command-line arguments to refer to.
        /// </summary>
        private Options m_options;

        /// <summary>
        /// The trace collector to use to collect traces.
        /// </summary>
        private ITraceCollector m_traceCollector;

        /// <summary>
        /// Keeps track of where we are in the directory tree, relative to the
        /// root directory.
        /// </summary>
        private Stack<string> m_relativePath = new Stack<string>();

        /// <summary>
        /// Map from benchmarks to resolved absolute paths to executables. Probing for
        /// executables is done as part of the "validation" step of the run, and this
        /// class is only concerned about running tests on a configuration that is known
        /// to be valid, so this map is provided to the runner upon construction.
        /// </summary>
        private IDictionary<Benchmark, string> m_executableProbeMap;

        /// <summary>
        /// Constructs a new runner that will run the given suite
        /// with the given options.
        /// </summary>
        /// <param name="suite">The benchmark suite to run. Must be validated already.</param>
        /// <param name="options">The options governing the benchmark run</param>
        public Runner(BenchmarkRun suite, Options options, IDictionary<Benchmark, string> executableProbeMap)
        {
            Debug.Assert(suite != null);
            Debug.Assert(options != null);
            m_run = suite;
            m_options = options;
            m_traceCollector = TraceCollectorFactory.Create();
            m_executableProbeMap = executableProbeMap;
        }

        /// <summary>
        /// Runs the benchmark suite and returns the results.
        /// </summary>
        /// <returns>The results of the benchmark run</returns>
        public RunResult Run()
        {
            RunResult result = new RunResult();
            result.Settings = m_run.Settings;
            Logger.LogAlways($"Running benchmarks with server GC: {m_run.Settings.ServerGC}");
            Logger.LogAlways($"Running benchmarks with concurrent GC: {m_run.Settings.ConcurrentGC}");
            foreach (var version in m_run.CoreClrVersions)
            {
                // these should have been validated already before runnning
                Debug.Assert(!string.IsNullOrEmpty(version.Path));
                Debug.Assert(!string.IsNullOrEmpty(version.Name));
                Debug.Assert(Directory.Exists(version.Path));
                CoreclrVersionRunResult versionResult = RunVersion(version);
                result.PerVersionResults.Add(Tuple.Create(version, versionResult));
            }

            return result; 
        }
        
        /// <summary>
        /// Runs the benchmark suite on a single version of CoreCLR and
        /// returns the results.
        /// </summary>
        /// <param name="coreRootPath">The path to CORE_ROOT for the version
        /// of CoreCLR being tested.</param>
        /// <returns>The results of this run</returns>
        private CoreclrVersionRunResult RunVersion(CoreClrVersion version)
        {
            Logger.LogAlways($"Beginning run of version \"{version.Name}\"");
            CoreclrVersionRunResult result = new CoreclrVersionRunResult();
            Debug.Assert(Directory.GetCurrentDirectory() == m_options.OutputDirectory);
            // TODO(segilles) error handling here. We should avoid propegating exceptions
            // as best we can.
            string folderName = Path.Combine(Directory.GetCurrentDirectory(), version.Name);
            Directory.CreateDirectory(folderName);
            Directory.SetCurrentDirectory(folderName);
            m_relativePath.Push(version.Name);
            try
            {
                foreach (var benchmark in m_run.Suite)
                {
                    Debug.Assert(benchmark != null);
                    BenchmarkResult benchResult = RunBenchmark(version, benchmark);
                    result.BenchmarkResults.Add(benchResult);
                }

                // write out the version description
                File.WriteAllText(
                    Path.Combine(folderName, Constants.VersionJsonName),
                    JsonConvert.SerializeObject(version));

                return result;
            }
            finally
            {
                string upOneDir = Path.Combine(Directory.GetCurrentDirectory(), "..");
                Directory.SetCurrentDirectory(upOneDir);
                m_relativePath.Pop();
            }
        }

        /// <summary>
        /// Runs a single benchmark on a given version of CoreCLR and saves the results.
        /// </summary>
        /// <param name="version">The version of CoreCLR to run on</param>
        /// <param name="bench">The benchmark to run</param>
        /// <returns>The result from running the benchmark</returns>
        private BenchmarkResult RunBenchmark(CoreClrVersion version, Benchmark bench)
        {
            Logger.LogAlways($"Running benchmark {bench.Name}");
            string folderName = Path.Combine(Directory.GetCurrentDirectory(), bench.Name);
            Directory.CreateDirectory(folderName);
            Directory.SetCurrentDirectory(folderName);
            m_relativePath.Push(bench.Name);
            try
            {
                return RunBenchmarkImplWithIterations(version, bench);
            }
            finally
            {
                string upOneDir = Path.Combine(Directory.GetCurrentDirectory(), "..");
                Directory.SetCurrentDirectory(upOneDir);
                m_relativePath.Pop();
            }
        }

        /// <summary>
        /// Runs a single iteration of a benchmark. If no iteration number if specified,
        /// the benchmark is run once.
        /// </summary>
        /// <param name="version">The version of CoreCLR to run the benchmark on</param>
        /// <param name="bench">The benchmark to run</param>
        /// <returns>The result of running the benchmark</returns>
        private BenchmarkResult RunBenchmarkImplWithIterations(CoreClrVersion version, Benchmark bench)
        {
            Logger.LogAlways($"Running iterations for benchmark {bench.Name}");
            BenchmarkResult result = new BenchmarkResult();
            result.Benchmark = bench;
            int iterations = bench.Iterations ?? 1;
            for (int i = 0; i < iterations; i++)
            {
                Logger.LogAlways($"Beginning iteration {i} for benchmark {bench.Name}");
                // we'll create subdirectories for every iteration.
                string folderName = Path.Combine(Directory.GetCurrentDirectory(), i.ToString());
                Directory.CreateDirectory(folderName);
                Directory.SetCurrentDirectory(folderName);
                m_relativePath.Push(i.ToString());
                try
                {
                    IterationResult iterResult;
                    string traceName = bench.Name + ".etl";
                    m_traceCollector.StartTrace(bench.Name + ".etl", m_run.CollectionLevel);
                    try
                    {
                        // we've got everything set up, time to run.
                        iterResult = RunBenchmarkImpl(version, bench);
                    }
                    finally
                    {
                        m_traceCollector.StopTrace();
                    }

                    var currentRelativePath = Path.Combine(m_relativePath.Reverse().ToArray());

                    // TODO(segilles, xplat) adding .zip on the end is done by PerfView, perfcollect
                    // probably doesn't do this.
                    iterResult.TracePathLocation = Path.Combine(currentRelativePath, traceName + ".zip");

                    // write out the result json file that the analysis engine is expecting
                    File.WriteAllText(
                        Path.Combine(Directory.GetCurrentDirectory(), Constants.ResultJsonName),
                        JsonConvert.SerializeObject(iterResult, Formatting.Indented));
                    result.Iterations.Add(iterResult);
                }
                finally
                {
                    string upOneDir = Path.Combine(Directory.GetCurrentDirectory(), "..");
                    Directory.SetCurrentDirectory(upOneDir);
                    m_relativePath.Pop();
                }
            }

            // write out the benchmark json
            File.WriteAllText(
                Path.Combine(Directory.GetCurrentDirectory(), Constants.BenchmarkJsonName), 
                JsonConvert.SerializeObject(bench, Formatting.Indented));

            return result;
        }

        /// <summary>
        /// Runs a single benchmark by spawning a process and monitoring it until
        /// its exit.
        /// </summary>
        /// <param name="version">The coreclr version to test</param>
        /// <param name="bench">The benchmark to run</param>
        /// <returns>The result from running the benchmark</returns>
        private IterationResult RunBenchmarkImpl(CoreClrVersion version, Benchmark bench)
        {
            // TODO(segilles) we'd like to have precise control over when the benchmark
            // terminates. After some number of GCs, after some mechanisms get exercised,
            // after some amount of time elapses, etc. This can all be done here with
            // some work.

            string coreRun = Path.Combine(version.Path, Utils.CoreRunName);
            string arguments = bench.Arguments ?? "";
            Debug.Assert(File.Exists(coreRun));
            Debug.Assert(m_executableProbeMap.ContainsKey(bench));
            string exePath = m_executableProbeMap[bench];

            Process proc = new Process();
            proc.StartInfo.FileName = coreRun;
            proc.StartInfo.Arguments = exePath + " " + arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = false;
            foreach (var pair in bench.EnvironmentVariables)
            {
                proc.StartInfo.Environment[pair.Key] = pair.Value;
            }

            proc.StartInfo.Environment[Constants.ServerGCVariable] = m_run.Settings.ServerGC ? "1" : "0";
            proc.StartInfo.Environment[Constants.ConcurrentGCVariable] = m_run.Settings.ConcurrentGC ? "1" : "0";

            Stopwatch timer = new Stopwatch();
            timer.Start();
            proc.Start();
            proc.WaitForExit();
            timer.Stop();

            IterationResult result = new IterationResult();
            result.DurationMsec = timer.ElapsedMilliseconds;
            result.ExitCode = proc.ExitCode;
            result.Pid = proc.Id;
            return result;
        }
    }
}
