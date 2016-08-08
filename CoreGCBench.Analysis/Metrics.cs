// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System.Linq;
using System;

// TraceEvent marked all of its bleeding-edge stuff as obsolete
// to avoid premature uptake. We're opting-in to the new stuff
// by suppressing this warning.
#pragma warning disable CS0618 // Type or member is obsolete

namespace CoreGCBench.Analysis
{
    /// <summary>
    /// A metric that yields the duration of the benchmark run.
    /// </summary>
    public sealed class DurationMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        // TODO(segilles) - this will need to get tweaked.
        public override string Name => "Duration";
        public override double GetValue(IterationDataSource data)
        {
            return data.DurationMsec;
        }
    }

    #region Pause duration metrics
    public sealed class MaximumPauseMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "Max Pause";
        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.Stats().MaxPauseDurationMSec;
        }
    }

    public sealed class MeanPauseMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "MeanPause";
        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.Stats().MeanPauseDurationMSec;
        }
    }

    public sealed class MaximumPauseGenZeroMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "Max Pause (Generation 0)";
        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.Generations()[0].MaxPauseDurationMSec;
        }
    }

    public sealed class MeanPauseGenZeroMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "Mean Pause (Generation 0)";
        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.Generations()[0].MeanPauseDurationMSec;
        }
    }

    public sealed class MaximumPauseGenOneMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "Max Pause (Generation 1)";
        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.Generations()[1].MaxPauseDurationMSec;
        }
    }

    public sealed class MeanPauseGenOneMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "Mean Pause (Generation 1)";
        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.Generations()[1].MeanPauseDurationMSec;
        }
    }

    public sealed class MaximumPauseBlockingGenTwoMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "Max Pause (Blocking Generation 2)";
        public override double GetValue(IterationDataSource data)
        {
            // TODO(segilles, perf) this is very inefficient
            return data.Trace.GC.GCs
                .Where(gc => gc.Generation == 2 && gc.Type != GCType.BackgroundGC)
                .Max(gc => gc.PauseDurationMSec);
        }
    }

    public sealed class MeanPauseBlockingGenTwoMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "Mean Pause (Blocking Generation 2)";
        public override double GetValue(IterationDataSource data)
        {
            // TODO(segilles, perf) this is very inefficient
            return data.Trace.GC.GCs
                .Where(gc => gc.Generation == 2 && gc.Type != GCType.BackgroundGC)
                .Average(gc => gc.PauseDurationMSec);
        }
    }

    public sealed class MaximumPauseBackgroundGenTwoMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "Max Pause (Background Generation 2)";
        public override double GetValue(IterationDataSource data)
        {
            // TODO(segilles, perf) this is very inefficient
            return data.Trace.GC.GCs
                .Where(gc => gc.Generation == 2 && gc.Type == GCType.BackgroundGC)
                .Max(gc => gc.PauseDurationMSec);
        }
    }

    public sealed class MeanPauseBackgroundGenTwoMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Milliseconds;
        public override string Name => "Mean Pause (Background Generation 2)";
        public override double GetValue(IterationDataSource data)
        {
            // TODO(segilles, perf) this is very inefficient
            return data.Trace.GC.GCs
                .Where(gc => gc.Generation == 2 && gc.Type == GCType.BackgroundGC)
                .Average(gc => gc.PauseDurationMSec);
        }
    }

    #endregion

    #region Miscellaneous
    public sealed class ForegroundGCMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Count;
        public override string Name => "# of Foreground GCs";

        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.GCs.Count(gc => gc.Type == GCType.ForegroundGC);
        }
    }

    public sealed class GCNumberMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Count;
        public override string Name => "# of GCs";

        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.GCs.Count();
        }
    }

    public sealed class MeanGenTwoFragmentationMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Megabytes;
        public override string Name => "Mean Gen Two Fragmentation";

        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.GCs.Average(gc => gc.GenFragmentationMB(Gens.Gen2));
        }
    }

    public sealed class MeanGenTwoSizeMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Megabytes;
        public override string Name => "Mean Gen Two Size";

        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.GCs.Average(gc => gc.GenSizeAfterMB(Gens.Gen2));
        }
    }

    public sealed class MeanEphemeralSizeMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Megabytes;
        public override string Name => "Mean Ephemeral Heap Size";

        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.GCs.Average(gc => gc.GenSizeAfterMB(Gens.Gen0) 
                + gc.GenSizeAfterMB(Gens.Gen1) 
                + gc.GenSizeAfterMB(Gens.GenLargeObj));
        }
    }
    #endregion

    #region Mechanisms
    public sealed class CompactingGCMechanismMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Count;
        public override string Name => "# of GCs with Compacting mechanism";

        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.GCs.Count(gc => (gc.GlobalHeapHistory.GlobalMechanisms & GCGlobalMechanisms.Compaction) != 0);
        }
    }

    public sealed class PromotingGCMechanismMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Count;
        public override string Name => "# of GCs with Promotion mechanism";

        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.GCs.Count(gc => (gc.GlobalHeapHistory.GlobalMechanisms & GCGlobalMechanisms.Promotion) != 0);
        }
    }

    public sealed class DemotionGCMechanismMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Count;
        public override string Name => "# of GCs with Demotion mechanism";

        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.GCs.Count(gc => (gc.GlobalHeapHistory.GlobalMechanisms & GCGlobalMechanisms.Demotion) != 0);
        }
    }

    public sealed class CardBundleGCMechanismMetric : Metric
    {
        public override Direction Direction => Direction.LowerIsBetter;
        public override Unit Unit => Unit.Count;
        public override string Name => "# of GCs with CardBundles mechanism";

        public override double GetValue(IterationDataSource data)
        {
            return data.Trace.GC.GCs.Count(gc => (gc.GlobalHeapHistory.GlobalMechanisms & GCGlobalMechanisms.CardBundles) != 0);
        }
    }

    #endregion
}

#pragma warning restore CS0618 // Type or member is obsolete
