using System;
using Xunit.Abstractions;

namespace ExtendTest
{
    public class BaseTest
    {
        protected readonly ITestOutputHelper Output;

        public BaseTest(ITestOutputHelper output)
        {
            Output = output;
            {
                Environment.SetEnvironmentVariable("DAOKEAPPUK", "jiesan.netcore.surprisegamepollapi");
            }
        }
    }
}
