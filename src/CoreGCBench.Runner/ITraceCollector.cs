// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CoreGCBench.Runner
{
    /// <summary>
    /// An ITraceCollector is responsible for starting, stoping, and saving traces
    /// to disk.
    /// </summary>
    public interface ITraceCollector
    {
        /// <summary>
        /// Begins a trace, saving the result to the given filename.
        /// </summary>
        /// <param name="filename">The destination file to save the trace</param>
        /// <param name="level">The level of events to collect</param>
        void StartTrace(string filename, CollectionLevel level);

        /// <summary>
        /// Stops the current trace;
        /// </summary>
        void StopTrace();
    }
}
