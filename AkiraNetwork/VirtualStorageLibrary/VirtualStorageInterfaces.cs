namespace AkiraNetwork.VirtualStorageLibrary
{
    public interface IVirtualDeepCloneable<T>
    {
        T DeepClone(bool recursive = false);
    }

    public interface IVirtualWildcardMatcher
    {
        Dictionary<string, string> WildcardDictionary { get; }

        IEnumerable<string> Wildcards { get; }

        IEnumerable<string> Patterns { get; }

        bool PatternMatcher(string nodeName, string pattern);

        public IVirtualWildcardMatcher DeepClone();
    }
}
