// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.IO;

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
        /// Constructs a new runner that will run the given suite
        /// with the given options.
        /// </summary>
        /// <param name="suite">The benchmark suite to run. Must be validated already.</param>
        /// <param name="options">The options governing the benchmark run</param>
        public Runner(BenchmarkRun suite, Options options)
        {
            Debug.Assert(suite != null);
            Debug.Assert(options != null);
            m_run = suite;
            m_options = options;
            m_traceCollector = TraceCollectorFactory.Create();
        }

        /// <summary>
        /// Runs the benchmark suite and returns the results.
        /// </summary>
        /// <returns>The results of the benchmark run</returns>
        public RunResult Run()
        {
            RunResult result = new RunResult();
            foreach (var version in m_run.CoreCLRVersions)
            {
                Logger.LogAlways($"Beginning run of version \"{version.HumanReadableName}\"");
                // these should have been validated already before runnning
                Debug.Assert(!string.IsNullOrEmpty(version.CoreRootPath));
                Debug.Assert(!string.IsNullOrEmpty(version.HumanReadableName));
                Debug.Assert(Directory.Exists(version.CoreRootPath));
                CoreclrVersionRunResult versionResult = RunVersion(version);
                result.PerVersionResults[version] = versionResult;
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
            CoreclrVersionRunResult result = new CoreclrVersionRunResult();
            Debug.Assert(Directory.GetCurrentDirectory() == m_options.OutputDirectory);
            // TODO(segilles) error handling here. We should avoid propegating exceptions
            // as best we can.
            string folderName = Path.Combine(Directory.GetCurrentDirectory(), version.HumanReadableName);
            Directory.CreateDirectory(folderName);
            Directory.SetCurrentDirectory(folderName);
            try
            {
                foreach (var benchmark in m_run.Suite)
                {
                    Debug.Assert(benchmark != null);
                    BenchmarkResult benchResult = RunBenchmark(version, benchmark);
                    result.BenchmarkResults.Add(benchResult);
                }

                return result;
            }
            finally
            {
                string upOneDir = Path.Combine(Directory.GetCurrentDirectory(), "..");
                Directory.SetCurrentDirectory(upOneDir);
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
            try
            {
                string traceName = bench.Name + ".etl";
                m_traceCollector.StartTrace(bench.Name + ".etl", m_run.CollectionLevel);
                try
                {
                    // we've got everything set up, time to run.
                    BenchmarkResult result = RunBenchmarkImpl(version, bench);
                    result.TracePathLocation = Path.Combine(Directory.GetCurrentDirectory(), traceName);
                    return result;
                }
                finally
                {
                    m_traceCollector.StopTrace();
                }
            }
            finally
            {
                string upOneDir = Path.Combine(Directory.GetCurrentDirectory(), "..");
                Directory.SetCurrentDirectory(upOneDir);
            }
        }

        /// <summary>
        /// Runs a single benchmark by spawning a process and monitoring it until
        /// its exit.
        /// </summary>
        /// <param name="version">The coreclr version to test</param>
        /// <param name="bench">The benchmark to run</param>
        /// <returns></returns>
        private BenchmarkResult RunBenchmarkImpl(CoreClrVersion version, Benchmark bench)
        {
            // TODO(segilles) we'd like to have precise control over when the benchmark
            // terminates. After some number of GCs, after some mechanisms get exercised,
            // after some amount of time elapses, etc. This can all be done here with
            // some work.

            string coreRun = Path.Combine(version.CoreRootPath, Utils.CoreRunName);
            Debug.Assert(File.Exists(coreRun));
            Debug.Assert(File.Exists(bench.ExecutablePath));

            Process proc = new Process();
            proc.StartInfo.FileName = coreRun;
            proc.StartInfo.Arguments = bench.ExecutablePath;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = false;

            Stopwatch timer = new Stopwatch();
            timer.Start();
            proc.Start();
            proc.WaitForExit();
            timer.Stop();

            BenchmarkResult result = new BenchmarkResult();
            result.DurationMsec = timer.ElapsedMilliseconds;
            result.ExitCode = proc.ExitCode;
            return result;
        }
    }
}
