// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace CoreGCBench.Analysis
{
    /// <summary>
    /// A utility class used to log information as various stages of
    /// benchmark execution.
    /// </summary>
    public static class Logger
    {
        private static bool m_verbose;
        private static object m_lock = new object();

        /// <summary>
        /// Initializes this logger.
        /// </summary>
        /// <param name="opts">The command-line options.</param>
        public static void Initialize(bool shouldLogVerbose)
        {
            m_verbose = shouldLogVerbose;
        }

        /// <summary>
        /// If the requested verbosity is higher than the verbosity
        /// given to this message, logs a message to the text stream
        /// specified in the options.
        /// </summary>
        /// <param name="v">The verbosity of this message</param>
        /// <param name="fmt">A format string to print</param>
        public static void Log(string fmt)
        {
            lock (m_lock)
            {
                var timestamp = DateTime.Now.ToString("[MM-dd-yyyy HH:mm:ss] ");
                Console.WriteLine(timestamp + fmt);
            }
        }

        /// <summary>
        /// Logs a message at the verbose level, so it will only be printed
        /// if the -v option is given.
        /// </summary>
        /// <param name="fmt">A format string to print</param>
        public static void LogVerbose(string fmt)
        {
            if (m_verbose)
            {
                Log(fmt);
            }
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
            Log(fmt);
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
            Log(fmt);
            Console.ForegroundColor = old;
        }
    }
}
