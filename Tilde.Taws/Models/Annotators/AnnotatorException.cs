using System;

namespace Tilde.Taws.Models
{
    /// <summary>
    /// Exception thrown during annotation.
    /// </summary>
    public class AnnotatorException : Exception
    {
        /// <inheritdoc/>
        public AnnotatorException()
            : base()
        {
        }

        /// <inheritdoc/>
        public AnnotatorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class with a reference to the 
        /// inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public AnnotatorException(Exception innerException)
            : base("An error occured during annotation.", innerException)
        {
        }

        /// <inheritdoc/>
        public AnnotatorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}