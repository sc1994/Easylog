using Xunit;
using Xunit.Abstractions;

namespace Easylog.Test
{
    public class ILogger_Test : BaseTest
    {
        public ILogger_Test(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public void WriteLog_Test()
        {
            Logger.Information("test");
        }

        [Fact]
        public void WriteLog_Category_Test()
        {
            Logger.Information("test", c1: "ILogger_Test", c2: "WriteLog_Category_Test");
        }

        [Fact]
        public void WriteLog_Filter_Test()
        {
            Logger.Information("test", f1: "id", f2: "9988");
        }

        [Fact]
        public void WriteLog_Error_Test()
        {
            Logger.Error("test error", new System.Exception("this is deliberately exception"));
        }
    }
}
