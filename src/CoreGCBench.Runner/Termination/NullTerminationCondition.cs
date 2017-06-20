// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace CoreGCBench.Runner.Termination
{
    /// <summary>
    /// NullTerminationCondition does nothing and always returns false for <see cref="ShouldTerminate"/>.
    /// This TerminationCondition is used by the runner when no termination condition is specified.
    /// </summary>
    public sealed class NullTerminationCondition : TerminationCondition
    {
        public override bool ShouldTerminate(Process proc)
        {
            return false;
        }
    }
}
