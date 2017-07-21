// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace CoreGCBench.Runner.Termination
{
    /// <summary>
    /// TimeTerminationCondition implements time-based termination. It will
    /// signal a process for termination once it has run for a given period of time.
    /// </summary>
    public sealed class TimeTerminationCondition : TerminationCondition
    {
        /// <summary>
        /// The time span that the Process should be allowed to run,
        /// after which the process should be terminated.
        /// </summary>
        private TimeSpan m_span;

        /// <summary>
        /// Constructs a new TimeTerminationCondition that will signal termination
        /// at the end of the given number of seconds.
        /// </summary>
        /// <param name="proc">The process to observe</param>
        /// <param name="span">The number of seconds after which this condition will
        /// signal termination.</param>
        public TimeTerminationCondition(TimeSpan span)
        {
            m_span = span;
        }

        /// <summary>
        /// Signals termination once the process has been running m_seconds seconds.
        /// </summary>
        /// <param name="process">The Process to consider for termination.</param>
        /// <returns>Whether or not this process should terminate.</returns>
        public override bool ShouldTerminate(Process process)
        {
            return DateTime.Now - process.StartTime > m_span;
        }
    }
}
