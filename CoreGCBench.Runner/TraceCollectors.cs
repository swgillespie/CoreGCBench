// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Runtime.InteropServices;

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
    /// An implementation of ITraceCollector that makes use of ETW.
    /// </summary>
    public class EtwTraceCollector : ITraceCollector
    {
        public void StartTrace(string filename, CollectionLevel level)
        {
            // TODO(segilles) until we get this working, drop something
            // into the given filename so that we get something.
            File.WriteAllText("this is a test", filename);
        }

        public void StopTrace()
        {
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
