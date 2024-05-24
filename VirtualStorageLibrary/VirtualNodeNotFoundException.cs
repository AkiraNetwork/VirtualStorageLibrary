namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualNodeNotFoundException : Exception
    {
        public VirtualNodeNotFoundException()
        {
        }
        
        public VirtualNodeNotFoundException(string message) : base(message)
        {
        }

        public VirtualNodeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
