using System;

namespace Specpoint.Revit2026
{
    public class UserCancelledException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserCancelledException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UserCancelledException(string message)
            : base(message)
        {
        }
    }
}
