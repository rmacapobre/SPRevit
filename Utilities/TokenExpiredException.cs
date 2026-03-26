using System;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// Exception thrown when the authentication token has expired
    /// </summary>
    public class TokenExpiredException : Exception
    {
        public TokenExpiredException()
            : base("The Specpoint authentication token has expired. Please login again.")
        {
        }

        public TokenExpiredException(string message)
            : base(message)
        {
        }

        public TokenExpiredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
