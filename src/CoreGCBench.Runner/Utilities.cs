// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CoreGCBench.Runner
{
    /// <summary>
    /// A utility class used to log information as various stages of
    /// benchmark execution.
    /// </summary>
    public static class Logger
    {
        private static Options m_options;

        /// <summary>
        /// Initializes this logger.
        /// </summary>
        /// <param name="opts">The command-line options.</param>
        public static void Initialize(Options options)
        {
            m_options = options;
        }

        /// <summary>
        /// If the requested verbosity is higher than the verbosity
        /// given to this message, logs a message to the text stream
        /// specified in the options.
        /// </summary>
        /// <param name="v">The verbosity of this message</param>
        /// <param name="fmt">A format string to print</param>
        public static void Log(Verbosity v, string fmt)
        {
            if (v <= m_options.Verbosity)
            {
                var timestamp = DateTime.Now.ToString("[MM-dd-yyyy HH:mm:ss] ");
                m_options.LogStream.WriteLine(timestamp + fmt);
            }
        }

        /// <summary>
        /// Logs a message at the highest priority, so it is always displayed
        /// to the user.
        /// </summary>
        /// <param name="fmt">A format string to print</param>
        public static void LogAlways(string fmt)
        {
            Log(Verbosity.None, fmt);
        }

        /// <summary>
        /// Logs a message at the verbose level, so it will only be printed
        /// if the -v option is given.
        /// </summary>
        /// <param name="fmt">A format string to print</param>
        public static void LogVerbose(string fmt)
        {
            Log(Verbosity.Verbose, fmt);
        }

        /// <summary>
        /// Logs a message at the most verbose level, so it is only printed
        /// if the -d option is given on debug builds and is never printed
        /// at all on release builds.
        /// </summary>
        /// <param name="fmt">A format string to print</param>
        [Conditional("DEBUG")]
        public static void LogDiagnostic(string fmt)
        {
            Log(Verbosity.Diagnostic, fmt);
        }

        /// <summary>
        /// Logs a message that will always be echoed to console,
        /// with an additional color indicating that it's a warning.
        /// </summary>
        /// <param name="fmt">A string to print</param>
        public static void LogWarning(string fmt)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            LogAlways(fmt);
            Console.ForegroundColor = old;
        }

        /// <summary>
        /// Logs a message that will always be echoed to console,
        /// with an additional color indicating that it's an error.
        /// </summary>
        /// <param name="fmt">A string to print</param>
        public static void LogError(string fmt)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            LogAlways(fmt);
            Console.ForegroundColor = old;
        }
    }

    /// <summary>
    /// A collection of utilities.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Returns the name of the CoreRun executable on the current platform.
        /// </summary>
        public static string CoreRunName
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return "CoreRun.exe";
                }
                else
                {
                    return "corerun";
                }
            }
        }
    }
}
