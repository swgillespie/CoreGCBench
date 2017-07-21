// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreGCBench.Analysis
{
    public static class Extensions
    {
        /// <summary>
        /// Calculates the standard deviation of a given sample.
        /// </summary>
        /// <param name="values">The collection of values.</param>
        /// <returns>The sample standard deviation of the given values.</returns>
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double aggregate = 0;
            int count = values.Count();

            // standard deviation doesn't make sense with only one sample.
            if (count <= 1)
            {
                return aggregate;
            }

            double mean = values.Average();
            double meanSquaredDiff = values.Sum(v => (v - mean) * (v - mean));
            return Math.Sqrt(meanSquaredDiff / (count - 1));
        }

        public static double MaxOrDefault<T>(this IEnumerable<T> values, Func<T, double> selector)
        {
            double accumulator = 0.0;
            foreach (var value in values)
            {
                double calc = selector(value);
                if (accumulator < calc)
                {
                    accumulator = calc;
                }
            }

            return accumulator;
        }

        public static double AverageOrDefault<T>(this IEnumerable<T> values, Func<T, double> selector)
        {
            var list = values.ToList();
            if (list.Count == 0)
            {
                return 0.0;
            }

            return list.Average(selector);
        }
    }
}
