// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGCBench.Common;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using Microsoft.Diagnostics.Tracing.Analysis;
using Microsoft.Diagnostics.Tracing;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;

namespace CoreGCBench.Analysis
{
    /// <summary>
    /// An AnalysisDataSource is a collection of data that is consumed
    /// by our various analyses in order to draw conclusions. The input
    /// to create an AnalysisDataSource is a zip file created by the benchmark runner.
    /// It is the job of this class to re-hydrate the information contained within
    /// that zip file, parsing any traces that it encounters along the way.
    /// </summary>
    public sealed class AnalysisDataSource : IDisposable
    {
        private const string TempFolderName = "CoreGCBench";

        /// <summary>
        /// The root of the temporary directory that we unzip the zip file to.
        /// </summary>
        private string m_rootDir;

        /// <summary>
        /// The versions contained within this data source.
        /// </summary>
        public IList<VersionAnalysisDataSource> Versions { get; set; } = new List<VersionAnalysisDataSource>();

        public AnalysisDataSource(string zipFile)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), TempFolderName, Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            m_rootDir = tempPath;

            ZipFile.ExtractToDirectory(zipFile, tempPath);

            // see the comment in CoreGCBench.Runner.Driver::PackageResults(RunResult, Options)
            // for a precise description of what the zip file looks like.
            foreach (var versionFolder in Directory.EnumerateDirectories(tempPath))
            {
                Versions.Add(new VersionAnalysisDataSource(versionFolder));
            }
        }

        #region IDisposable Implementation
        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (m_rootDir != null)
                {
                    Directory.Delete(m_rootDir);
                }

                if (disposing)
                {
                    if (Versions != null)
                    {
                        foreach (var version in Versions)
                        {
                            version.Dispose();
                        }
                    }

                    Versions = null;
                }

                disposedValue = true;
            }
        }

        ~AnalysisDataSource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// A data source for analysis, pertaining to a single version of
    /// CoreCLR tested as part of a benchmark run.
    /// </summary>
    public sealed class VersionAnalysisDataSource : IDisposable
    {
        /// <summary>
        /// The version of CoreCLR that this data came from.
        /// </summary>
        public CoreClrVersion Version { get; set; }

        /// <summary>
        /// The data gathered from analyzing the benchmarks that were run
        /// on this version of CoreCLR.
        /// </summary>
        public IList<BenchmarkDataSource> BenchmarkResults { get; set; }

        public VersionAnalysisDataSource(string versionFolder)
        {
            foreach (var benchmarkFolder in Directory.EnumerateDirectories(versionFolder))
            {
                BenchmarkResults.Add(new BenchmarkDataSource(benchmarkFolder));
            }
        }

        public void Dispose()
        {
            if (BenchmarkResults != null)
            {
                foreach (var bench in BenchmarkResults)
                {
                    bench.Dispose();
                }
            }

            BenchmarkResults = null;
        }
    }

    /// <summary>
    /// A data source for individual benchmarks, containing
    /// one or more iterations (<see cref="IterationDataSource"/>) 
    /// </summary>
    public sealed class BenchmarkDataSource : IDisposable
    {
        /// <summary>
        /// The benchmark that was executed.
        /// </summary>
        public Benchmark Benchmark { get; set; }

        /// <summary>
        /// The iterations that occured within this benchmark.
        /// </summary>
        public IList<IterationDataSource> Iterations { get; set; }


        public BenchmarkDataSource(string benchmarkFolder)
        {
            // there should be numerically indexed folders in this directory.
            foreach (var iterFolder in Directory.EnumerateDirectories(benchmarkFolder))
            {
                Iterations.Add(new IterationDataSource(iterFolder));
            }

            Benchmark = JsonConvert.DeserializeObject<Benchmark>(Path.Combine(benchmarkFolder, Constants.BenchmarkJsonName));
        }

        public void Dispose()
        {
            foreach (var iteration in Iterations)
            {
                iteration.Dispose();
            }

            Iterations = null;
        }
    }

    /// <summary>
    /// A data source for individual iterations.
    /// </summary>
    public sealed class IterationDataSource : IDisposable
    {
        /// <summary>
        /// The TraceEventSource we constructed to parse the trace.
        /// The lifetime of the below TraceLoadedDotNetRuntime is tied
        /// to the lifetime of this class.
        /// </summary>
        private TraceEventSource m_source;

        /// <summary>
        /// The TraceEvent TraceGC object obtained by analyzing this benchmark's
        /// trace, or null if a trace was not gathered.
        /// </summary>
        public TraceLoadedDotNetRuntime Trace { get; set; }

        /// <summary>
        /// The path to the trace that was used to construct the TraceGC
        /// object, or null if no trace was taken.
        /// </summary>
        public string TraceLocation { get; set; }

        /// <summary>
        /// The duration of the benchmark, in milliseconds.
        /// </summary>
        public long DurationMsec { get; set; }

        /// <summary>
        /// The exit code of the benchmark.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// The PID of the benchmark process.
        /// </summary>
        public int Pid { get; set; }

        public IterationDataSource(string iterFolder)
        {
            string resultFile = Path.Combine(iterFolder, Constants.ResultJsonName);
            Debug.Assert(File.Exists(resultFile));
            IterationResult iter = JsonConvert.DeserializeObject<IterationResult>(File.ReadAllText(resultFile));

            Debug.Assert(File.Exists(iter.TracePathLocation));

            // TODO(segilles, xplat) LTTNG.
            // We deliberately don't dispose this event source here since we
            // are moving ownership to the object being constructed.
            var source = new ETWTraceEventSource(iter.TracePathLocation);
            m_source = source;
            source.NeedLoadedDotNetRuntimes();
            source.Process();

            Trace = source.Processes()
                // TODO(segilles) this isn't quite right, it's possible (but HIGHLY unlikely)
                // that we could collide pids.
                .First(t => t.ProcessID == iter.Pid)
                .LoadedDotNetRuntime();
            TraceLocation = iter.TracePathLocation;
            DurationMsec = iter.DurationMsec;
            ExitCode = iter.ExitCode;
        }

        public void Dispose()
        {
            m_source?.Dispose();
        }
    }
}
