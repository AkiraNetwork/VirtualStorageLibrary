using System.Collections;

namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualDirectory : VirtualNode, IEnumerable<VirtualNode>
    {
        private Dictionary<VirtualNodeName, VirtualNode> _nodes = new();

        public override VirtualNodeType NodeType => VirtualNodeType.Directory;

        public int Count => _nodes.Count;

        public int DirectoryCount => _nodes.Values.Count(n => n is VirtualDirectory);

        public int ItemCount => _nodes.Values.Count(n => n is VirtualItem);

        public int SymbolicLinkCount => _nodes.Values.Count(n => n is VirtualSymbolicLink);

        public IEnumerable<VirtualNodeName> NodeNames => _nodes.Keys;

        public IEnumerable<VirtualNode> Nodes => GetNodeList();

        public bool NodeExists(VirtualNodeName name) => _nodes.ContainsKey(name);

        public bool ItemExists(VirtualNodeName name)
        {
            // NodeExistsを使用してノードの存在を確認
            if (!NodeExists(name))
            {
                return false; // ノードが存在しない場合はfalseを返す
            }

            var node = _nodes[name];
            var nodeType = node.GetType();
            // ノードの型がジェネリックであり、かつそのジェネリック型定義がVirtualItem<>であるかチェック
            return nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeof(VirtualItem<>);
        }

        public bool DirectoryExists(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                return false;
            }

            return _nodes[name] is VirtualDirectory;
        }

        public bool SymbolicLinkExists(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                return false;
            }

            var node = _nodes[name];
            return node is VirtualSymbolicLink;
        }

        public VirtualDirectory(VirtualNodeName name) : base(name)
        {
            _nodes = new();
        }

        public VirtualDirectory(VirtualNodeName name, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            _nodes = new();
        }

        public override string ToString() => (Name == VirtualPath.Root) ? VirtualPath.Root : $"{Name}{VirtualPath.Separator}";

        public override VirtualNode DeepClone()
        {
            VirtualDirectory newDirectory = new(Name, CreatedDate, UpdatedDate);
            return newDirectory;
        }

        public IEnumerable<VirtualNode> GetNodeList()
        {
            VirtualNodeTypeFilter filter = VirtualStorageState.State.NodeListConditions.Filter;
            VirtualGroupCondition<VirtualNode, object>? groupCondition = VirtualStorageState.State.NodeListConditions.GroupCondition;
            List<VirtualSortCondition<VirtualNode>>? sortConditions = VirtualStorageState.State.NodeListConditions.SortConditions;

            IEnumerable<VirtualNode> nodes = _nodes.Values;

            switch (filter)
            {
                case VirtualNodeTypeFilter.None:
                    return Enumerable.Empty<VirtualNode>();
                case VirtualNodeTypeFilter.All:
                    break;
                default:
                    nodes = _nodes.Values.Where(node =>
                        (filter.HasFlag(VirtualNodeTypeFilter.Directory) && node is VirtualDirectory) ||
                        (filter.HasFlag(VirtualNodeTypeFilter.Item) && node is VirtualItem) ||
                        (filter.HasFlag(VirtualNodeTypeFilter.SymbolicLink) && node is VirtualSymbolicLink));
                    break;
            }

            return nodes.GroupAndSort(groupCondition, sortConditions);
        }

        public void Add(VirtualNode node, bool allowOverwrite = false)
        {
            if (!VirtualNodeName.IsValidNodeName(node.Name.Name))
            {
                throw new ArgumentException($"ノード名 '{node.Name}' は無効です。", nameof(node.Name));
            }

            VirtualNodeName key = node.Name;

            if (_nodes.ContainsKey(key) && !allowOverwrite)
            {
                throw new InvalidOperationException($"ノード '{key}' は既に存在します。上書きは許可されていません。");
            }

            _nodes[key] = node;
        }

        public void AddItem<T>(VirtualNodeName name, T item, bool allowOverwrite = false)
        {
            Add(new VirtualItem<T>(name, item), allowOverwrite);
        }

        public void AddSymbolicLink(VirtualNodeName name, VirtualPath targetPath, bool allowOverwrite = false)
        {
            Add(new VirtualSymbolicLink(name, targetPath), allowOverwrite);
        }

        public void AddDirectory(VirtualNodeName name, bool allowOverwrite = false)
        {
            Add(new VirtualDirectory(name), allowOverwrite);
        }

        public VirtualNode this[VirtualNodeName name]
        {
            get
            {
                if (!NodeExists(name))
                {
                    throw new VirtualNodeNotFoundException($"指定されたノード '{name}' は存在しません。");
                }
                return _nodes[name];
            }
            set
            {
                _nodes[name] = value;
            }
        }

        public void Remove(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{name}' は存在しません。");
            }

            _nodes.Remove(name);
        }

        public VirtualNode? Get(VirtualNodeName name, bool exceptionEnabled = true)
        {
            if (!NodeExists(name))
            {
                if (exceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException($"指定されたノード '{name}' は存在しません。");
                }
                else
                {
                    return null;
                }
            }
            return _nodes[name];
        }

        public void Rename(VirtualNodeName oldName, VirtualNodeName newName)
        {
            if (!NodeExists(oldName))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{oldName}' は存在しません。");
            }

            if (NodeExists(newName))
            {
                throw new InvalidOperationException($"指定されたノード '{newName}' は存在しません。");
            }

            VirtualNode? node = Get(oldName);
            node!.Name = newName;
            Add(node);
            Remove(oldName);
        }

        public IEnumerator<VirtualNode> GetEnumerator() => Nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public VirtualItem<T> GetItem<T>(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{name}' は存在しません。");
            }

            var node = _nodes[name];
            if (node is VirtualItem<T> item)
            {
                return item;
            }

            throw new InvalidOperationException($"指定されたノード '{name}' はVirtualItem<{typeof(T).Name}>型ではありません。");
        }

        public VirtualDirectory GetDirectory(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{name}' は存在しません。");
            }

            if (_nodes[name] is VirtualDirectory directory)
            {
                return directory;
            }

            throw new InvalidOperationException($"指定されたノード '{name}' はディレクトリ型ではありません。");
        }

        public VirtualSymbolicLink GetSymbolicLink(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{name}' は存在しません。");
            }

            var node = _nodes[name];
            if (node is VirtualSymbolicLink link)
            {
                return link;
            }

            throw new InvalidOperationException($"指定されたノード '{name}' はシンボリックリンク型ではありません。");
        }
    }
}
