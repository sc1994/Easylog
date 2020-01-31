using System;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.Builder
{
    internal class EasyLogHttpException : Exception
    {
        private string v1;
        private bool v2;
        private Exception ex;

        public EasyLogHttpException()
        {
        }

        public EasyLogHttpException(string message) : base(message)
        {
        }

        public EasyLogHttpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EasyLogHttpException(string v1, bool v2, Exception ex)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.ex = ex;
        }

        protected EasyLogHttpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}