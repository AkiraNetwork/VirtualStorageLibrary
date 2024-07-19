namespace AkiraNetwork.VirtualStorageLibrary
{
    public readonly record struct VirtualID
    {
        private readonly Guid _id;

        public Guid ID => _id;

        public override string ToString() => _id.ToString();

        public VirtualID()
        {
            _id = Guid.NewGuid();
        }
    }
}
