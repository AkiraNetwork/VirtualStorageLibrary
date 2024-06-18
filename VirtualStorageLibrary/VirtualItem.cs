namespace AkiraNet.VirtualStorageLibrary
{
    public abstract class VirtualItem : VirtualNode
    {
        protected VirtualItem(VirtualNodeName name) : base(name) { }

        protected VirtualItem(VirtualNodeName name, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate) { }

        public override abstract VirtualNode DeepClone();
    }

    public class VirtualItem<T> : VirtualItem, IDisposable
    {
        public T? ItemData { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }

        private bool disposed;

        public override VirtualNodeType NodeType => VirtualNodeType.Item;

        public VirtualItem(VirtualNodeName name) : base(name)
        {
            ItemData = default;
            disposed = false;
        }

        public VirtualItem(VirtualNodeName name, T? item) : base(name)
        {
            ItemData = item;
            disposed = false;
        }

        public VirtualItem(VirtualNodeName name, T? item, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            ItemData = item;
            disposed = false;
        }

        // タプルからVirtualItem<T>への暗黙的な変換
        public static implicit operator VirtualItem<T>((string nodeName, T? itemData) tuple)
        {
            return new VirtualItem<T>(tuple.nodeName, tuple.itemData);
        }

        // データからVirtualItem<T>への暗黙的な変換
        public static implicit operator VirtualItem<T>(T? itemData)
        {
            string prefix = VirtualStorageState.State.prefixItem;
            VirtualNodeName nodeName = VirtualNodeName.GenerateNodeName(prefix);
            return new VirtualItem<T>(nodeName, itemData);
        }

        public override string ToString() => $"{Name}";

        public override VirtualNode DeepClone()
        {
            T? newItemData = ItemData;

            // ItemDataがIDeepCloneable<T>を実装している場合はDeepClone()を呼び出す
            if (ItemData is IVirtualDeepCloneable<T> cloneableItem)
            {
                newItemData = cloneableItem.DeepClone();
            }

            return new VirtualItem<T>(Name, newItemData);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TがIDisposableを実装していればDisposeを呼び出す
                    (ItemData as IDisposable)?.Dispose();
                }

                // VirtualItem<T>はアンマネージドリソースを扱ってないので、ここでは何もしない
                disposed = true;
            }
        }

        ~VirtualItem()
        {
            Dispose(false);
        }
    }
}
