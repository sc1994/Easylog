using System;

namespace EasyLog.WriteLog
{
    internal class EasyLogHttpException : Exception
    {
        internal EasyLogHttpException(string message, bool hasNext, Exception innerException) : base(message, innerException)
        {
            HasNext = hasNext;
        }

        internal bool HasNext { get; set; }
    }
}