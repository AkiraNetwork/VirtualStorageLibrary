using System.Runtime.CompilerServices;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        public VirtualItemAdapter<T> Item { get; }

        public VirtualDirectoryAdapter<T> Dir { get; }

        public VirtualSymbolicLinkAdapter<T> Link { get; }

        // パスを受け取るインデクサ
        [IndexerName("Indexer")]
        public VirtualNode this[VirtualPath path, bool followLinks = true]
        {
            get => GetNode(path, followLinks);
            set => SetNode(path, value);
        }
    }
}
