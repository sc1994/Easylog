using System;
using System.Diagnostics;
using EasyLog.WriteLog;

namespace SampleWeb.Services
{
    public class LogTestService
    {
        private readonly EasyLogger _log;

        public LogTestService(EasyLogger log)
        {
            _log = log;
        }

        public void Log()
        {
            _log.Debug("123");
            _log.Information("123");
            _log.Warning("123");
            try
            {
                throw new Exception("模拟异常");
            }
            catch (Exception ex)
            {
                _log.Error("123", ex);
            }

            _log.Debug(new { A = 345 });
            _log.Information(new { A = 345 });
            _log.Warning(new { A = 345 });
            try
            {
                throw new Exception("模拟异常");
            }
            catch (Exception ex)
            {
                _log.Error(new { A = 345 }, ex);
            }

        }
    }
}