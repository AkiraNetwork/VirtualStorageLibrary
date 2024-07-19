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
                throw new InvalidCastException("ノードはVirtualItemではありません。");
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
                throw new InvalidCastException("ノードはVirtualDirectoryではありません。");
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
                throw new InvalidCastException("ノードはVirtualSymbolicLinkではありません。");
            }
            set
            {
                _storage[path, followLinks] = value;
            }
        }
    }
}
