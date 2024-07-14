namespace AkiraNet.VirtualStorageLibrary
{
    public readonly record struct VirtualID
    {
        private readonly Guid _id;

        public Guid ID => _id;

        public VirtualID()
        {
            _id = Guid.NewGuid();
        }
    }
}
