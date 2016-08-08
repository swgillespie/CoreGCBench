// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;

namespace CoreGCBench.Runner.Tests
{
    public static class StringWriterExtensions
    {
        public static string Text(this StringWriter writer)
        {
            return writer.GetStringBuilder().ToString();
        }
    }
}
