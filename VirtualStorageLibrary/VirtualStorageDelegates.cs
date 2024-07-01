namespace AkiraNet.VirtualStorageLibrary
{
    public delegate void NotifyNodeDelegate(VirtualPath path, VirtualNode? node);

    public delegate bool ActionNodeDelegate(VirtualDirectory parentDirectory, VirtualNodeName nodeName, VirtualPath nodePath);

    public delegate bool PatternMatch(string nodeName, string pattern);
}
