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
