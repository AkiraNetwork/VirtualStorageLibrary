using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// The virtual item.
    /// </summary>
    public abstract class VirtualItem : VirtualNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected VirtualItem(VirtualNodeName name) : base(name) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="createdDate">The created date.</param>
        protected VirtualItem(VirtualNodeName name, DateTime createdDate) : base(name, createdDate) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="createdDate">The created date.</param>
        /// <param name="updatedDate">The updated date.</param>
        protected VirtualItem(VirtualNodeName name, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate) { }

        /// <summary>
        /// Deeps the clone.
        /// </summary>
        /// <param name="recursive">If true, recursive.</param>
        /// <returns>A VirtualNode</returns>
        public override abstract VirtualNode DeepClone(bool recursive = false);
    }

    /// <summary>
    /// The virtual item.
    /// </summary>
    /// <typeparam name="T"/>
    public class VirtualItem<T> : VirtualItem, IDisposable
    {
        /// <summary>
        /// The item data.
        /// </summary>
        private T? _itemData;

        /// <summary>
        /// Gets or sets the item data.
        /// </summary>
        public T? ItemData
        {
            get => _itemData;
            set => _itemData = value;
        }

        /// <summary>
        /// The disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Gets the node type.
        /// </summary>
        public override VirtualNodeType NodeType => VirtualNodeType.Item;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItem"/> class.
        /// </summary>
        public VirtualItem()
            : base(VirtualNodeName.GenerateNodeName(VirtualStorageState.State.PrefixItem))
        {
            ItemData = default;
            _disposed = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public VirtualItem(VirtualNodeName name) : base(name)
        {
            ItemData = default;
            _disposed = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="item">The item.</param>
        public VirtualItem(VirtualNodeName name, T? item) : base(name)
        {
            ItemData = item;
            _disposed = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="item">The item.</param>
        /// <param name="createdDate">The created date.</param>
        public VirtualItem(VirtualNodeName name, T? item, DateTime createdDate) : base(name, createdDate)
        {
            ItemData = item;
            _disposed = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="item">The item.</param>
        /// <param name="createdDate">The created date.</param>
        /// <param name="updatedDate">The updated date.</param>
        public VirtualItem(VirtualNodeName name, T? item, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            ItemData = item;
            _disposed = false;
        }

        // タプルからVirtualItem<T>への暗黙的な変換
        public static implicit operator VirtualItem<T>((VirtualNodeName nodeName, T? itemData) tuple)
        {
            return new VirtualItem<T>(tuple.nodeName, tuple.itemData);
        }

        // データからVirtualItem<T>への暗黙的な変換
        public static implicit operator VirtualItem<T>(T? itemData)
        {
            string prefix = VirtualStorageState.State.PrefixItem;
            VirtualNodeName nodeName = VirtualNodeName.GenerateNodeName(prefix);
            return new VirtualItem<T>(nodeName, itemData);
        }

        // ノード名からVirtualItem<T>への暗黙的な変換
        public static implicit operator VirtualItem<T>(VirtualNodeName name)
        {
            return new VirtualItem<T>(name);
        }

        /// <summary>
        /// Converts to the string.
        /// </summary>
        /// <returns>A string</returns>
        public override string ToString() => $"{Name}";

        /// <summary>
        /// Deeps the clone.
        /// </summary>
        /// <param name="recursive">If true, recursive.</param>
        /// <returns>A VirtualNode</returns>
        public override VirtualNode DeepClone(bool recursive = false)
        {
            T? newItemData = ItemData;

            // ItemDataがIDeepCloneable<T>を実装している場合はDeepClone()を呼び出す
            if (ItemData is IVirtualDeepCloneable<T> cloneableItem)
            {
                newItemData = cloneableItem.DeepClone();
            }

            return new VirtualItem<T>(Name, newItemData);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TがIDisposableを実装していればDisposeを呼び出す
                    (ItemData as IDisposable)?.Dispose();
                }

                // VirtualItem<T>はアンマネージドリソースを扱ってないので、ここでは何もしない
                _disposed = true;
            }
        }

        public override void Update(VirtualNode node)
        {
            if (node is not VirtualItem<T> newItem)
            {
                throw new ArgumentException(string.Format(Resources.NodeIsNotVirtualItem, node.Name, typeof(T).Name), nameof(node));
            }

            if (newItem.IsReferencedInStorage)
            {
                newItem = (VirtualItem<T>)newItem.DeepClone();
            }

            CreatedDate = newItem.CreatedDate;
            UpdatedDate = DateTime.Now;
            ItemData = newItem.ItemData;
        }

        ~VirtualItem()
        {
            Dispose(false);
        }
    }
}
