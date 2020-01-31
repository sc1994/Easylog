using System;
using Serilog.Core;
using Easylog.Extension;
using Microsoft.AspNetCore.Http;
using Serilog.Events;
using Microsoft.AspNetCore.Hosting;

namespace Easylog
{
    public class Easylogger : IEasylogger, IDisposable
    {
        private readonly Logger _logger;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IHostingEnvironment _environment;

        public Easylogger(Logger logger) : this(logger, null, null) { }
        public Easylogger(Logger logger, IHttpContextAccessor httpContext, IHostingEnvironment environment)
        {
            _logger = logger;
            _httpContext = httpContext;
            _environment = environment;
        }

        public void Debug(
            object log,
            string f1 = null,
            string f2 = null,
            string c1 = null,
            string c2 = null,
            string c3 = null)
        => ToLog(LogEventLevel.Debug, log, f1, f2, c1, c2, c3, null);

        public void Information(
           object log,
           string f1 = null,
           string f2 = null,
           string c1 = null,
           string c2 = null,
           string c3 = null)
        => ToLog(LogEventLevel.Information, log, f1, f2, c1, c2, c3, null);

        public void Warning(
            object log,
            string f1 = null,
            string f2 = null,
            string c1 = null,
            string c2 = null,
            string c3 = null,
            Exception ex = null)
        => ToLog(LogEventLevel.Warning, log, f1, f2, c1, c2, c3, ex);

        public void Error(
           object log,
           Exception ex,
           string f1 = null,
           string f2 = null,
           string c1 = null,
           string c2 = null,
           string c3 = null)
        => ToLog(LogEventLevel.Error, log, f1, f2, c1, c2, c3, ex);

        private void ToLog(
           LogEventLevel level,
           object log,
           string f1,
           string f2,
           string c1,
           string c2,
           string c3,
           Exception ex)
        {
            var logContent = LogContent.GetContent(log, Environment.StackTrace, _httpContext, _environment, f1, f2, c1, c2, c3, ex);
            switch (level)
            {
                case LogEventLevel.Debug: _logger.Debug(logContent.template, logContent.@params); break;
                case LogEventLevel.Warning: _logger.Warning(logContent.template, logContent.@params); break;
                case LogEventLevel.Error: _logger.Error(logContent.template, logContent.@params); break;
                default: _logger.Information(logContent.template, logContent.@params); break;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
