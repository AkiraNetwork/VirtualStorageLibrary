using System.Runtime.CompilerServices;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        /// <summary>
        /// Gets the adapter providing operations for virtual items.
        /// </summary>
        /// <value>An instance of <see cref="VirtualItemAdapter{T}"/></value>
        public VirtualItemAdapter<T> Item { get; }

        /// <summary>
        /// Gets the adapter providing operations for virtual directories.
        /// </summary>
        /// <value>An instance of <see cref="VirtualDirectoryAdapter{T}"/></value>
        public VirtualDirectoryAdapter<T> Dir { get; }

        /// <summary>
        /// Gets the adapter providing operations for virtual symbolic links.
        /// </summary>
        /// <value>An instance of <see cref="VirtualSymbolicLinkAdapter{T}"/></value>
        public VirtualSymbolicLinkAdapter<T> Link { get; }

        /// <summary>
        /// Gets or sets the virtual node corresponding to the specified virtual path.
        /// </summary>
        /// <param name="path">The virtual path</param>
        /// <param name="followLinks">Flag indicating whether to follow symbolic links</param>
        /// <value>The virtual node corresponding to the specified virtual path</value>
        [IndexerName("Indexer")]
        public VirtualNode this[VirtualPath path, bool followLinks = true]
        {
            get => GetNode(path, followLinks);
            set => SetNode(path, value);
        }
    }
}
