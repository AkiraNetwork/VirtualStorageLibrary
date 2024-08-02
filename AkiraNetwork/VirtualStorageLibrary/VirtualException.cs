namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// The base class for exceptions that occur within the virtual storage library.
    /// </summary>
    public abstract class VirtualException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualException"/> class.
        /// </summary>
        public VirtualException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public VirtualException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that is the cause of this exception.</param>
        public VirtualException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// The exception that is thrown when a node is not found in the virtual storage.
    /// </summary>
    public class VirtualNodeNotFoundException : VirtualException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNodeNotFoundException"/> class.
        /// </summary>
        public VirtualNodeNotFoundException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNodeNotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public VirtualNodeNotFoundException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNodeNotFoundException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that is the cause of this exception.</param>
        public VirtualNodeNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
