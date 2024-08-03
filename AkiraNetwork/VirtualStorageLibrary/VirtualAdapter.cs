using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Adapter class to simplify operations on virtual items.
    /// Reduces the need for casting and simplifies code, allowing
    /// users to easily manage specific node types.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the virtual storage.</typeparam>
    public class VirtualItemAdapter<T>
    {
        private readonly VirtualStorage<T> _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItemAdapter{T}"/> class.
        /// </summary>
        /// <param name="storage">The virtual storage instance.</param>
        public VirtualItemAdapter(VirtualStorage<T> storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Gets or sets the virtual item corresponding to the specified path.
        /// </summary>
        /// <param name="path">The path of the item.</param>
        /// <param name="followLinks">A flag indicating whether to follow links.</param>
        /// <value>The <see cref="VirtualItem{T}"/> corresponding to the specified path.</value>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the node is not a <see cref="VirtualItem{T}"/>.
        /// </exception>
        public VirtualItem<T> this[VirtualPath path, bool followLinks = true]
        {
            get
            {
                var node = _storage[path, followLinks];
                if (node is VirtualItem<T> item)
                {
                    return item;
                }
                throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualItem, node.Name, typeof(T).Name));
            }
            set
            {
                _storage[path, followLinks] = value;
            }
        }
    }

    /// <summary>
    /// Adapter class to simplify operations on virtual directories.
    /// Reduces the need for casting and simplifies code, allowing
    /// users to easily manage specific node types.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the virtual storage.</typeparam>
    public class VirtualDirectoryAdapter<T>
    {
        private readonly VirtualStorage<T> _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualDirectoryAdapter{T}"/> class.
        /// </summary>
        /// <param name="storage">The virtual storage instance.</param>
        public VirtualDirectoryAdapter(VirtualStorage<T> storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Gets or sets the virtual directory corresponding to the specified path.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <param name="followLinks">A flag indicating whether to follow links.</param>
        /// <value>The <see cref="VirtualDirectory"/> corresponding to the specified path.</value>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the node is not a <see cref="VirtualDirectory"/>.
        /// </exception>
        public VirtualDirectory this[VirtualPath path, bool followLinks = true]
        {
            get
            {
                var node = _storage[path, followLinks];
                if (node is VirtualDirectory directory)
                {
                    return directory;
                }
                throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualDirectory, node));
            }
            set
            {
                _storage[path, followLinks] = value;
            }
        }
    }

    /// <summary>
    /// Adapter class to simplify operations on virtual symbolic links.
    /// Reduces the need for casting and simplifies code, allowing
    /// users to easily manage specific node types.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the virtual storage.</typeparam>
    public class VirtualSymbolicLinkAdapter<T>
    {
        private readonly VirtualStorage<T> _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualSymbolicLinkAdapter{T}"/> class.
        /// </summary>
        /// <param name="storage">The virtual storage instance.</param>
        public VirtualSymbolicLinkAdapter(VirtualStorage<T> storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Gets or sets the virtual symbolic link corresponding to the specified path.
        /// </summary>
        /// <param name="path">The path of the symbolic link.</param>
        /// <param name="followLinks">A flag indicating whether to follow links.</param>
        /// <value>The <see cref="VirtualSymbolicLink"/> corresponding to the specified path.</value>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the node is not a <see cref="VirtualSymbolicLink"/>.
        /// </exception>
        /// <remarks>
        /// To operate on the symbolic link itself, specify `followLinks` as false.
        /// Note that the default value of this parameter may change in the future.
        /// </remarks>
        public VirtualSymbolicLink this[VirtualPath path, bool followLinks = true]
        {
            get
            {
                var node = _storage[path, followLinks];
                if (node is VirtualSymbolicLink link)
                {
                    return link;
                }
                throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualSymbolicLink, node));
            }
            set
            {
                _storage[path, followLinks] = value;
            }
        }
    }
}
