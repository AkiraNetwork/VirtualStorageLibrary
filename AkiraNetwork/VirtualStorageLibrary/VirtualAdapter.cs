using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualItemAdapter<T>(VirtualStorage<T> storage)
    {
        private readonly VirtualStorage<T> _storage = storage;

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

    public class VirtualDirectoryAdapter<T>(VirtualStorage<T> storage)
    {
        private readonly VirtualStorage<T> _storage = storage;

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

    public class VirtualSymbolicLinkAdapter<T>(VirtualStorage<T> storage)
    {
        private readonly VirtualStorage<T> _storage = storage;

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
