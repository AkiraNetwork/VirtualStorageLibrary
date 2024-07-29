using System.Collections.ObjectModel;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public interface IVirtualDeepCloneable<T>
    {
        T DeepClone(bool recursive = false);
    }

    public interface IVirtualWildcardMatcher
    {
        ReadOnlyDictionary<string, string> WildcardDictionary { get; }

        IEnumerable<string> Wildcards { get; }

        IEnumerable<string> Patterns { get; }

        int Count { get; }

        bool PatternMatcher(string nodeName, string pattern);
    }
}
