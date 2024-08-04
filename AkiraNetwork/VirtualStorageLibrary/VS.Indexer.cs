using System.Runtime.CompilerServices;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        // パスを受け取るインデクサ
        [IndexerName("Indexer")]
        public VirtualNode this[VirtualPath path, bool followLinks = true]
        {
            get => GetNode(path, followLinks);
            set => SetNode(path, value);
        }
    }
}
