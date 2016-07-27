using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CoreGCBench.Runner.Tests
{
    public class Program
    {
        [Fact]
        public void PassingTest()
        {
            Assert.Equal(4, 2 + 2);
        }
    }
}
