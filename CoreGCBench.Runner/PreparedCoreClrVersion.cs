// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGCBench.Common;
using System;
using System.Diagnostics;
using System.IO;

namespace CoreGCBench.Runner
{
    /// <summary>
    /// A PreparedCoreClrVersion is a CoreClrVersion that has been prepared for execution.
    /// A <see cref="CoreClrVersion"/> is not sufficient to execute a
    /// version, because the user does not have to specify a full set of binaries in order
    /// to perform a test run - they can supply a subset of the required binaries and the
    /// infrastructure will draw the remaining binaries from a common source.
    /// </summary>
    public sealed class PreparedCoreClrVersion : IDisposable
    {
        /// <summary>
        /// The CoreClrVersion that this PreparedCoreClrVersion was derived from.
        /// </summary>
        public CoreClrVersion OriginalVersion { get; private set; }

        /// <summary>
        /// The path to the prepared version, suitable to be consumed by the runner.
        /// Everything that coreclr needs to run a binary should be in this folder.
        /// </summary>
        public string Path { get; private set; }

        public string Name => OriginalVersion.Name;

        /// <summary>
        /// If we had to do something to prepare this version, we ended up creating
        /// a temp directory. This means that we have to delete the directory
        /// upon disposal.
        /// </summary>
        private bool m_createdATempDirectory;

        /// <summary>
        /// Creates a PreparedCoreClrVersion with the given version as the
        /// "original" version and a path to the "fixed" version.
        /// </summary>
        /// <param name="version">The original version</param>
        /// <param name="path">Path to the directory created for this version.
        /// Must be a temporary path and will be deleted by this class upon disposal.</param>
        public PreparedCoreClrVersion(CoreClrVersion version, string path)
        {
            Debug.Assert(version.IsPartial);
            OriginalVersion = version;
            Path = path;
            m_createdATempDirectory = true;
        }

        /// <summary>
        /// Creates a PreparedCoreClrVersion with the given version as
        /// the "original" version and using the original version's path as
        /// the path.
        /// </summary>
        /// <param name="version">The original version</param>
        public PreparedCoreClrVersion(CoreClrVersion version)
        {
            Debug.Assert(!version.IsPartial);
            OriginalVersion = version;
            Path = version.Path;
            m_createdATempDirectory = false;
        }

        /// <summary>
        /// Best-effort attempt at deleting the temporary directory that got created
        /// for this PreparedCoreClrVersion, if one was created.
        /// </summary>
        private void DisposeImpl()
        {
            if (m_createdATempDirectory)
            {
                Debug.Assert(OriginalVersion.IsPartial);
                try
                {
                    Directory.Delete(Path);
                }
                catch (Exception exn)
                {
                    // best effort.
                    Logger.LogWarning($"Failed to delete temporary version directory: {exn.Message}");
                }

                m_createdATempDirectory = false;
            }
        }

        ~PreparedCoreClrVersion() {
           DisposeImpl();
        }

        public void Dispose()
        {
            DisposeImpl();
            GC.SuppressFinalize(this);
        }
    }
}
