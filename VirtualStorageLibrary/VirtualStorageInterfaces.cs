namespace AkiraNet.VirtualStorageLibrary
{
    public interface IVirtualDeepCloneable<T>
    {
        T DeepClone();
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
