namespace VirtualStorageLibrary
{
    public interface IDeepCloneable<T>
    {
        T DeepClone();
    }

    public abstract class VirtualNode : IDeepCloneable<VirtualNode>
    {
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public abstract VirtualNode DeepClone();
        protected VirtualNode(string name)
        {
            Name = name;
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
        }
    }

    public class VirtualItem<T> : VirtualNode, IDeepCloneable<VirtualItem<T>> where T : IDeepCloneable<T>
    {
        public T Item { get; set; }

        public VirtualItem(string name, T item) : base(name)
        {
            Item = item;
        }

        public override VirtualNode DeepClone()
        {
            return new VirtualItem<T>(Name, Item.DeepClone());
        }

        VirtualItem<T> IDeepCloneable<VirtualItem<T>>.DeepClone()
        {
            return (VirtualItem<T>)DeepClone();
        }
    }

    public class VirtualDirectory : VirtualNode, IDeepCloneable<VirtualDirectory>
    {
        public Dictionary<string, VirtualNode> Nodes { get; set; }

        public VirtualDirectory(string name) : base(name)
        {
            Nodes = new Dictionary<string, VirtualNode>();
        }

        public override VirtualNode DeepClone()
        {
            var clonedDirectory = new VirtualDirectory(this.Name)
            {
                CreateDate = this.CreateDate,
                UpdateDate = this.UpdateDate,
                Nodes = new Dictionary<string, VirtualNode>()
            };

            foreach (var pair in this.Nodes)
            {
                clonedDirectory.Nodes.Add(pair.Key, pair.Value.DeepClone());
            }

            return clonedDirectory;
        }

        VirtualDirectory IDeepCloneable<VirtualDirectory>.DeepClone()
        {
            return (VirtualDirectory)DeepClone();
        }

        public void Add(string key, VirtualNode node, bool allowOverwrite = false)
        {
            if (Nodes.ContainsKey(key) && !allowOverwrite)
            {
                throw new InvalidOperationException($"指定されたキー '{key}' に対応するノードは既に存在します。上書きは許可されていません。");
            }

            Nodes[key] = node;
        }

        public VirtualNode this[string key]
        {
            get
            {
                if (!Nodes.ContainsKey(key))
                {
                    throw new KeyNotFoundException($"指定されたキー '{key}' は存在しません。");
                }
                return Nodes[key];
            }
            set
            {
                Nodes[key] = value;
            }
        }
    }

    public class VirtualStorage
    {
        public VirtualStorage()
        {
            Root = new VirtualDirectory("/");
        }

        public VirtualDirectory Root { get; set; }
    }
}
