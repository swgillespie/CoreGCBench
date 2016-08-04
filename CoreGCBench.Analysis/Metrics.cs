// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System.Linq;

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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
        public override double VarianceThreshold => 50;
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
}
