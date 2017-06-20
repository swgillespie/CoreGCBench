// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace CoreGCBench.Runner.Termination
{
    /// <summary>
    /// An ITerminationCondition is a class whose responsibility is to
    /// determine, when asked, whether or not the running benchmark should terminate.
    /// </summary>
    public abstract class TerminationCondition : IDisposable
    {
        /// <summary>
        /// Asks this TerminationCondition whether or not it thinks it should terminate.
        /// </summary>
        /// <param name="proc">The Process that the termination condition should consider for termination.</param>
        /// <returns>True if the process should be terminated.</returns>
        public abstract bool ShouldTerminate(Process proc);

        /// <summary>
        /// Disposes this TerminationCondition. Does nothing unless overridden.
        /// </summary>
        public virtual void Dispose()
        {
            // do nothing.
        }
    }
}
