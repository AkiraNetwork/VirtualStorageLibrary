namespace AkiraNetwork.VirtualStorageLibrary
{
    public enum VirtualNodeType
    {
        Directory,
        Item,
        SymbolicLink,
        None
    }

    public enum VirtualNodeTypeFilter
    {
        None = 0x00,
        Item = 0x01,
        Directory = 0x02,
        SymbolicLink = 0x04,
        All = Item | Directory | SymbolicLink
    }
}
