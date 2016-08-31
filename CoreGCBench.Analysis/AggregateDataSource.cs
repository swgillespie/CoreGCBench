// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CoreGCBench.Common;
using System;
using System.Collections.Generic;

namespace CoreGCBench.Analysis
{
    /// <summary>
    /// An AggregateDataSource is an aggregate of one or more <see cref="AnalysisDataSource"/>.
    /// </summary>
    public sealed class AggregateDataSource : IDisposable
    {
        private List<AnalysisDataSource> m_sources = new List<AnalysisDataSource>();

        /// <summary>
        /// The <see cref="RunSettings"/> used in the run that created these data sources.
        /// Note that there's nothing that requires a user to use the same settings across
        /// multiple runs - we'll emit a warning in that case and pick one of the settings
        /// to store here.
        /// </summary>
        public RunSettings Settings { get; private set; } 

        /// <summary>
        /// Adds a data source to this AggregateDataSource.
        /// </summary>
        /// <param name="source">The data source to add</param>
        public void AddSource(AnalysisDataSource source)
        {
            if (Settings != null && !source.Settings.Equals(Settings))
            {
                Logger.LogWarning("One of the given data sources used a different set of run settings than the others! The analysis results may be inaccurate.");
            }

            if (Settings == null)
            {
                Settings = source.Settings;
            }

            m_sources.Add(source);
        }

        /// <summary>
        /// Enumerates all of the CoreCLR versions contained within this
        /// aggregate data source.
        /// </summary>
        /// <returns>An enumeration of all </returns>
        public IEnumerable<VersionAnalysisDataSource> Versions()
        {
            foreach (var source in m_sources)
            {
                foreach (var version in source.Versions)
                {
                    yield return version;
                }
            }
        }

        public void Dispose()
        {
            foreach (var source in m_sources)
            {
                source.Dispose();
            }

            m_sources = null;
        }
    }
}
