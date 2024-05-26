namespace AkiraNet.VirtualStorageLibrary
{
    public delegate void NotifyNodeDelegate(VirtualPath path, VirtualNode? node, bool isEnd);

    public delegate bool ActionNodeDelegate(VirtualDirectory directory, VirtualNodeName nodeName);

    public delegate bool PatternMatch(string nodeName, string pattern);
}
