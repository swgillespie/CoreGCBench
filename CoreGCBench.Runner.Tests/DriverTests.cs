using System.IO;
using Xunit;

namespace CoreGCBench.Runner.Tests
{
    public class DriverTests
    {
        [Fact]
        public void ConfigDoesntExist()
        {
            var log = new StringWriter();
            var options = new Options
            {
                LogStream = log,
                ConfigFile = "something that doesn't exist"
            };

            Driver.Execute(options);
            Assert.Contains("Failed to load configuration file", log.Text());
        }

        [Fact]
        public void ConfigIsInvalidJson()
        {
            var log = new StringWriter();
            var options = new Options
            {
                LogStream = log,
                ConfigJson = @"{"
            };

            Driver.Execute(options);
            Assert.Contains("Failed to load configuration file", log.Text());
        }

        [Fact]
        public void ConfigDoesntMatchSchema()
        {
            var log = new StringWriter();
            var options = new Options
            {
                LogStream = log,
                ConfigJson = @"
                    {
                        ""asdf"": 4
                    }"
            };

            Driver.Execute(options);
            Assert.Contains("Failed to load configuration file", log.Text());
        }

    }
}
