using System.Runtime.CompilerServices;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        /// <summary>
        /// Gets or sets the virtual node corresponding to the specified virtual path.
        /// </summary>
        /// <param name="path">The virtual path</param>
        /// <param name="followLinks">Flag indicating whether to follow symbolic links</param>
        /// <value>The virtual node corresponding to the specified virtual path</value>
        [IndexerName("Indexer")]
        public VirtualDirectory this[VirtualPath path, bool followLinks = true]
        {
            get => GetDirectory(path, followLinks);
            set => SetNode(path, value);
        }
    }
}
