// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

        public void AddSource(AnalysisDataSource source)
        {
            m_sources.Add(source);
        }

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
