// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        /// <param name="args">Arguments for the format string</param>
        public static void Log(Verbosity v, string fmt, params object[] args)
        {
            if (v <= m_options.Verbosity)
            {
                m_options.LogStream.WriteLine(fmt, args);
            }
        }

        /// <summary>
        /// Logs a message at the highest priority, so it is always displayed
        /// to the user.
        /// </summary>
        /// <param name="fmt">A format string to print</param>
        /// <param name="args">Arguments for the format string</param>
        public static void LogAlways(string fmt, params object[] args)
        {
            Log(Verbosity.None, fmt, args);
        }

        /// <summary>
        /// Logs a message at the verbose level, so it will only be printed
        /// if the -v option is given.
        /// </summary>
        /// <param name="fmt">A format string to print</param>
        /// <param name="args">Arguments for the format string</param>
        public static void LogVerbose(string fmt, params object[] args)
        {
            Log(Verbosity.Verbose, fmt, args);
        }

        /// <summary>
        /// Logs a message at the most verbose level, so it is only printed
        /// if the -d option is given.
        /// </summary>
        /// <param name="fmt">A format string to print</param>
        /// <param name="args">Arguments for the format string</param>
        public static void LogDiagnostic(string fmt, params object[] args)
        {
            Log(Verbosity.Diagnostic, fmt, args);
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
