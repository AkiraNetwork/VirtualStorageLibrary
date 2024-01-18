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
        private Dictionary<string, VirtualNode> _nodes;

        public int Count => _nodes.Count;

        public IEnumerable<string> GetNodeNames() => _nodes.Keys;

        public bool IsExists(string name) => _nodes.ContainsKey(name);

        public VirtualDirectory(string name) : base(name)
        {
            _nodes = new Dictionary<string, VirtualNode>();
        }

        public override VirtualNode DeepClone()
        {
            var clonedDirectory = new VirtualDirectory(this.Name)
            {
                CreateDate = this.CreateDate,
                UpdateDate = this.UpdateDate,
                _nodes = new Dictionary<string, VirtualNode>()
            };

            foreach (var pair in this._nodes)
            {
                clonedDirectory._nodes.Add(pair.Key, pair.Value.DeepClone());
            }

            return clonedDirectory;
        }

        VirtualDirectory IDeepCloneable<VirtualDirectory>.DeepClone()
        {
            return (VirtualDirectory)DeepClone();
        }

        public void Add(VirtualNode node, bool allowOverwrite = false)
        {
            string key = node.Name;

            if (_nodes.ContainsKey(key) && !allowOverwrite)
            {
                throw new InvalidOperationException($"ノード '{key}' は既に存在します。上書きは許可されていません。");
            }

            _nodes[key] = node;
        }

        public void AddDirectory(string name, bool allowOverwrite = false)
        {
            Add(new VirtualDirectory(name), allowOverwrite);
        }

        public VirtualNode this[string key]
        {
            get
            {
                if (!_nodes.ContainsKey(key))
                {
                    throw new KeyNotFoundException($"指定されたノード '{key}' は存在しません。");
                }
                return _nodes[key];
            }
            set
            {
                _nodes[key] = value;
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
