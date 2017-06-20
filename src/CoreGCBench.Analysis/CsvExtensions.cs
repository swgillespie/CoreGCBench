// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGCBench.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace CoreGCBench.Analysis
{
    public static class CsvExtensions
    {
        /// <summary>
        /// Writes a <see cref="ComparisonAnalysisResult"/> to the given output stream,
        /// in CSV form.
        /// </summary>
        /// <param name="result">The result to write as a CSV</param>
        /// <param name="outStream">The output stream to write the CSV to</param>
        public static void ToCsv(this ComparisonAnalysisResult result, TextWriter outStream)
        {
            // write out the settings used to record this run.
            foreach (var property in typeof(RunSettings).GetProperties())
            {
                Debug.Assert(result.Settings != null);
                var name = property.Name;
                var value = property.GetValue(result.Settings);
                outStream.WriteLine($"{name},{value}");
            }

            foreach (var version in result.Candidates)
            {
                ToCsvVersion(version, outStream);
            }
        }

        private static void ToCsvVersion(VersionComparisonAnalysisResult result, TextWriter outStream)
        {
            outStream.WriteLine($"Version,{CsvEscape(result.Version.Name)}");
            foreach (var benchmark in result.Benchmarks)
            {
                ToCsvBenchmark(benchmark, outStream);
            }
        }

        private static void ToCsvBenchmark(BenchmarkComparisonAnalysisResult benchmark, TextWriter outStream)
        {
            outStream.WriteLine($",Benchmark,{CsvEscape(benchmark.Benchmark.Name)}");
            outStream.WriteLine(",,Metric,Baseline Value,Candidate Value,Baseline StdDev, Candidate StdDev, % Change,Decision");
            foreach (var metric in benchmark.Metrics)
            {
                double percent = ((metric.Candidate.Value - metric.Baseline.Value) * 100) / metric.Baseline.Value;
                outStream.WriteLine($",,{CsvEscape(metric.Metric)},"
                    + $"{CsvEscape(metric.Baseline.Value.ToString())},"
                    + $"{CsvEscape(metric.Candidate.Value.ToString())},"
                    + $"{CsvEscape(metric.Baseline.StandardDeviation.ToString())},"
                    + $"{CsvEscape(metric.Candidate.StandardDeviation.ToString())},"
                    + $"{CsvEscape(percent.ToString())},"
                    + $"{CsvEscape(metric.Decision.ToString())}");
            }
        }

        private static string CsvEscape(string stringToEscape)
        {
            if (stringToEscape.IndexOfAny(new [] { ',', '\n', '"'}) != -1)
            {
                stringToEscape = Regex.Replace(stringToEscape, "\"", "\"\"");
                return $"\"{stringToEscape}\"";
            }

            return stringToEscape;
        }
    }
}
