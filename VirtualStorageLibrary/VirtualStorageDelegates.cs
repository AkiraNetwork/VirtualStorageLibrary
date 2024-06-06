namespace AkiraNet.VirtualStorageLibrary
{
    public delegate void NotifyNodeDelegate(VirtualPath path, VirtualNode? node, bool isEnd);

    public delegate bool ActionNodeDelegate(VirtualDirectory directory, VirtualNodeName nodeName, VirtualPath nodePath);

    public delegate bool PatternMatch(string nodeName, string pattern);
}
