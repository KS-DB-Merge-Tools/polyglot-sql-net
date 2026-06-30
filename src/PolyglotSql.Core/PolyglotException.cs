using System;

namespace PolyglotSql
{
    public class PolyglotException : Exception
    {
        public int Status { get; }

        public PolyglotException(int status, string message)
            : base(message)
        {
            Status = status;
        }
    }
}
