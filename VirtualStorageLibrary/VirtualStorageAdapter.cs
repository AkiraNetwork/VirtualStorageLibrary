namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualItemAdapter<T>(VirtualStorage<T> storage)
    {
        private readonly VirtualStorage<T> _storage = storage;

        public VirtualItem<T> this[VirtualPath path]
        {
            get
            {
                var node = _storage[path];
                if (node is VirtualItem<T> item)
                {
                    return item;
                }
                throw new InvalidCastException("ノードはVirtualItemではありません。");
            }
            set
            {
                if (value is VirtualItem<T> item)
                {
                    _storage[path] = item;
                }
                else
                {
                    throw new ArgumentException("値はVirtualItemでなければなりません。");
                }
            }
        }
    }

    public class VirtualDirectoryAdapter<T>(VirtualStorage<T> storage)
    {
        private readonly VirtualStorage<T> _storage = storage;

        public VirtualDirectory this[VirtualPath path]
        {
            get
            {
                var node = _storage[path];
                if (node is VirtualDirectory directory)
                {
                    return directory;
                }
                throw new InvalidCastException("ノードはVirtualDirectoryではありません。");
            }
            set
            {
                if (value is VirtualDirectory directory)
                {
                    _storage[path] = directory;
                }
                else
                {
                    throw new ArgumentException("値はVirtualDirectoryでなければなりません。");
                }
            }
        }
    }

    public class VirtualSymbolicLinkAdapter<T>(VirtualStorage<T> storage)
    {
        private readonly VirtualStorage<T> _storage = storage;

        public VirtualSymbolicLink this[VirtualPath path]
        {
            get
            {
                var node = _storage[path];
                if (node is VirtualSymbolicLink link)
                {
                    return link;
                }
                throw new InvalidCastException("ノードはVirtualSymbolicLinkではありません。");
            }
            set
            {
                if (value is VirtualSymbolicLink link)
                {
                    _storage[path] = link;
                }
                else
                {
                    throw new ArgumentException("値はVirtualSymbolicLinkでなければなりません。");
                }
            }
        }
    }
}
