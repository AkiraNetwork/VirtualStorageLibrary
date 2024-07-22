namespace AkiraNetwork.VirtualStorageLibrary
{
    public abstract class VirtualException : Exception
    {
        public VirtualException() { }

        public VirtualException(string message) : base(message) { }

        public VirtualException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class VirtualNodeNotFoundException : VirtualException
    {
        public VirtualNodeNotFoundException() { }

        public VirtualNodeNotFoundException(string message) : base(message) { }

        public VirtualNodeNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class VirtualPathNotFoundException : VirtualException
    {
        public VirtualPathNotFoundException() { }

        public VirtualPathNotFoundException(string message) : base(message) { }

        public VirtualPathNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class VirtualUnexpectedException : VirtualException
    {
        public VirtualUnexpectedException() { }

        public VirtualUnexpectedException(string message) : base(message) { }

        public VirtualUnexpectedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
