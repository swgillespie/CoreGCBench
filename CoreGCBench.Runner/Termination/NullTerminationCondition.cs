using System;
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
