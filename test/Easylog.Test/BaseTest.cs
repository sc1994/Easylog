using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Easylog.Test
{
    public class BaseTest
    {
        protected readonly Easylogger Logger;
        protected readonly ITestOutputHelper Output;

        public BaseTest(ITestOutputHelper output)
        {
            Output = output;
            Logger = new Easylogger(ServiceCollectionExtension.CreateLogger());
        }
    }
}
