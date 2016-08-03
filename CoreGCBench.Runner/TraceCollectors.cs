// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace CoreGCBench.Runner
{
    /// <summary>
    /// TraceCollectorFactory creates the trace collector that's most appropriate for
    /// the operating system we are running on: ETW on Windows and LTTNG on Linux.
    /// </summary>
    public static class TraceCollectorFactory
    {
        /// <summary>
        /// Constructs a new ITraceCollector appropriate for this operating system.
        /// </summary>
        /// <returns>An ITraceCollector appropriate for this OS</returns>
        public static ITraceCollector Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new EtwTraceCollector();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LttngTraceCollector();
            }

            return new NoopTraceCollector();
        }
    }

    /// <summary>
    /// An implementation of ITraceCollector that makes use of ETW by shelling out
    /// to PerfView.
    /// </summary>
    public class EtwTraceCollector : ITraceCollector
    {
        private const string PerfViewCommandLineGcOnly =
            @"/LogFile:perfview.log.txt /GCCollectOnly /NoV2Rundown /NoNGENRundown /NoRundown /merge:true /SessionName:CoreGCBench /zip:true start {0}";
        private const string PerfViewStop =
            @"/LogFile:Perfview.log.txt /SessionName:CoreGCBench stop";
        private const int PerfViewSpinTolerance = 12;

        // TODO(segilles) the PerfView that's used should be an option
        // and not rely on being on a Microsoft network.
        private const string PerfView = @"\\clrmain\tools\PerfView.exe";

        /// <summary>
        /// The filename that this trace creates. It needs to be at the class-level because
        /// both the start and stop trace functions need to know about it.
        /// </summary>
        private string m_filename;

        public void StartTrace(string filename, CollectionLevel level)
        {
            m_filename = filename;
            // TODO(segilles) - respect the level argument.
            Process proc = Process.Start(PerfView, string.Format(PerfViewCommandLineGcOnly, filename));
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                throw new InvalidOperationException($"PerfView exited with unexpected exit code: {proc.ExitCode}");
            }

            // PerfView exits immediately, without starting the session. We sleep here to ensure
            // that we don't start the benchmark until PerfView is ready. Five seconds should be long enough.
            Thread.Sleep(5000);
        }

        public void StopTrace()
        {
            Process proc = Process.Start(PerfView, PerfViewStop);
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                throw new InvalidOperationException($"PerfView exited with unexpected exit code: {proc.ExitCode}");
            }

            // PerfView also exits immediately when stop is invoked, despite doing things (like zipping the ETL file)
            // well after the completion of the process.
            //
            // Since this is obviously bad for us, we spin on whether or not the output file we're looking for exists.
            Logger.LogVerbose("Preparing to wait on PerfView output zip");
            Thread.Sleep(10000);
            string actualFilename = m_filename + ".zip";
            Logger.LogDiagnostic($"Spinning on file {actualFilename}");
            int spinCount = 0;
            while (true)
            {
                if (spinCount > PerfViewSpinTolerance)
                {
                    throw new InvalidOperationException("PerfView failed to generate the zip we're looking for after two minutes");
                }

                if (File.Exists(actualFilename))
                {
                    Logger.LogDiagnostic("Observed the file exists, waiting another five seconds");
                    Thread.Sleep(5000);
                    break;
                }

                Logger.LogDiagnostic("Observed the file does not exist, waiting ten seconds before trying again");
                Thread.Sleep(10000);
                spinCount++;
            }
        }
    }

    /// <summary>
    /// An implementation of ITraceCollector that makes use of LTTNG.
    /// </summary>
    public class LttngTraceCollector : ITraceCollector
    {
        public void StartTrace(string filename, CollectionLevel level)
        {
            throw new NotImplementedException();
        }

        public void StopTrace()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An implementation of ITraceCollector that does nothing.
    /// </summary>
    public class NoopTraceCollector : ITraceCollector
    {
        public void StartTrace(string filename, CollectionLevel level)
        {
        }

        public void StopTrace()
        {
        }
    }
}
