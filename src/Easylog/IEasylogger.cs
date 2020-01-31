using System;

namespace Easylog
{
    public interface IEasylogger
    {
        void Debug(
            object log,
            string f1 = null,
            string f2 = null,
            string c1 = null,
            string c2 = null,
            string c3 = null);

        void Information(
            object log,
            string f1 = null,
            string f2 = null,
            string c1 = null,
            string c2 = null,
            string c3 = null);

        void Warning(
            object log,
            string f1 = null,
            string f2 = null,
            string c1 = null,
            string c2 = null,
            string c3 = null,
            Exception ex = null);

        void Error(
            object log,
            Exception ex,
            string f1 = null,
            string f2 = null,
            string c1 = null,
            string c2 = null,
            string c3 = null);
    }
}
