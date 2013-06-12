using System;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// Exception thrown during annotation.
    /// </summary>
    public class TaaSException : Exception
    {
        /// <inheritdoc/>
        public TaaSException()
            : base()
        {
        }

        /// <inheritdoc/>
        public TaaSException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class with a reference to the 
        /// inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TaaSException(Exception innerException)
            : base("An error related to TaaS occured.", innerException)
        {
        }

        /// <inheritdoc/>
        public TaaSException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}