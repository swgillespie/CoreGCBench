// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace CoreGCBench.Analysis
{
    /// <summary>
    /// This class implements an unpaired Student's T-Test. 
    /// </summary>
    public static partial class TTest
    {
        /// <summary>
        /// This p-value is a standard p-value for t-tests. It's
        /// proven to be good enough to catch regressions in my tests, but
        /// it may need tweaking.
        /// </summary>
        public const double StandardPValue = 0.05;

        /// <summary>
        /// These are the p-values whose values are recorded in the t-test table.
        /// </summary>
        public static double[] AllowedPValues = { 0.5, 0.4, 0.3, 0.2, 0.1, 0.05, 0.02, 0.01, 0.005, 0.002, 0.001 };

        /// <summary>
        /// Runs an unpaired t-test on the sample data, making a decision on the data
        /// based on the results of the test. Whether or not the null hypothesis
        /// (that the means are the same betwen the baseline and the candidate) is rejected
        /// is based upon the value of the test statistic compared to the pvalue.
        /// </summary>
        /// <param name="baseline"></param>
        /// <param name="candidate"></param>
        /// <param name="pvalue">The PValue to use for this test. Must be specified up-front
        /// in order to avoid bias.</param>
        /// <returns>A <see cref="ComparisonDecision"/> made using the data.</returns>
        public static ComparisonDecision Run(MetricValue baseline, MetricValue candidate, double pvalue = StandardPValue)
        {
            // it is not required by this statistical test for the baseline and candidate
            // to have the same number of samples, but it /is/ required that their variances
            // be the same.
            //
            // TODO(segilles) - if, throughout the course of using this tool we find that its
            // false positive rate is high due to high variance differences, we should consider
            // switching to Welch's t-test, which is more robust in the case of unequal variances.

            // We use a table for the t-distribution, and we only have values for certain p-values.
            if (pvalue != StandardPValue && Array.IndexOf(AllowedPValues, pvalue) < 0)
            {
                throw new ArgumentException($"invalid pvalue: {pvalue}");
            }

            // First, we calculate the test statistic.
            double testStatistic = CalculateStatistic(baseline, candidate);
            int degreesOfFreedom = baseline.SampleSize + candidate.SampleSize - 2;

            if (!s_tdistTable.ContainsKey(degreesOfFreedom))
            {
                // TODO(segilles) if this happens a lot, we may consider adding more keys to the table.
                int closest = int.MaxValue;
                foreach (var key in s_tdistTable.Keys)
                {
                    if (Math.Abs(key - degreesOfFreedom) <= Math.Abs(closest - degreesOfFreedom))
                    {
                        closest = key;
                    }
                }

                Logger.LogVerbose($"DOF {degreesOfFreedom} not found in table, rounding to closest DOF: {closest}");
                degreesOfFreedom = closest;
            }

            double targetStatistic = s_tdistTable[degreesOfFreedom][pvalue];

            Logger.LogVerbose($"metric {baseline.Name} has test statistic {testStatistic} vs. target {targetStatistic}");
            if (Math.Abs(testStatistic) < targetStatistic)
            {
                // there's not enough data to prove or disprove the null hypothesis.
                return ComparisonDecision.Indeterminate;
            }


            // we've got a statistically significant difference!
            switch (baseline.Direction)
            {
                case Direction.HigherIsBetter:
                    return baseline.Value < candidate.Value ? ComparisonDecision.Improvement : ComparisonDecision.Regression;
                case Direction.LowerIsBetter:
                    return baseline.Value > candidate.Value ? ComparisonDecision.Improvement : ComparisonDecision.Regression;
                default:
                    throw new InvalidOperationException("invalid value for Direction enum");
            }
        }

        private static double CalculateStatistic(MetricValue baseline, MetricValue candidate)
        {
            // $$ t = \frac{X_1 - X_2}{s_{x_1x_2} \cdot \sqrt{\frac{1}{n_1} + \frac{1}{n_2}}} $$
            // where $X_1$ and $X_2$ are sample means, $n_1$ and $n_2$ are sample sizes, and
            // $s_{x_1x_2}$ is an estimator of the common standard deviation (see below).

            double numerator = baseline.Value - candidate.Value;
            double denominator = CommonStandardDeviation(baseline, candidate) * Math.Sqrt(1.0 / baseline.SampleSize + 1.0 / candidate.SampleSize);
            return numerator / denominator;
        }

        private static double CommonStandardDeviation(MetricValue baseline, MetricValue candidate)
        {
            // $$ s_{x_1x_2} = \sqrt{\frac{(n_1 - 1)s^2_{X_1} + (n_2 - 1)s^2_{X_2}}{n_1 + n_2 - 2}} $$
            double numerator = (baseline.SampleSize - 1) * (baseline.StandardDeviation * baseline.StandardDeviation)
                + (candidate.SampleSize - 1) * (candidate.StandardDeviation * candidate.StandardDeviation);
            double denominator = baseline.SampleSize + candidate.SampleSize - 2;
            return numerator / denominator;
        }
    }
}
