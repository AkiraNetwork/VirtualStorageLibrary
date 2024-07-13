using System.Runtime.CompilerServices;

namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualDirectory : VirtualNode, IEnumerable<VirtualNode>
    {
        private readonly Dictionary<VirtualNodeName, VirtualNode> _nodes = [];

        public override VirtualNodeType NodeType => VirtualNodeType.Directory;

        public IEnumerable<VirtualNodeName> NodeNames => _nodes.Keys;

        public IEnumerable<VirtualNode> Nodes => _nodes.Values;

        public int Count => _nodes.Count;

        public int DirectoryCount => Nodes.Count(n => n is VirtualDirectory);

        public int ItemCount => Nodes.Count(n => n is VirtualItem);

        public int SymbolicLinkCount => Nodes.Count(n => n is VirtualSymbolicLink);

        public IEnumerable<VirtualNode> NodesView => GetNodeList();

        public int NodesViewCount => NodesView.Count();

        public int DirectoryViewCount => NodesView.Count(n => n is VirtualDirectory);

        public int ItemViewCount => NodesView.Count(n => n is VirtualItem);

        public int SymbolicLinkViewCount => NodesView.Count(n => n is VirtualSymbolicLink);

        public bool NodeExists(VirtualNodeName name) => _nodes.ContainsKey(name);

        public bool ItemExists(VirtualNodeName name)
        {
            // NodeExistsを使用してノードの存在を確認
            if (!NodeExists(name))
            {
                return false; // ノードが存在しない場合は false を返す
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

        public VirtualDirectory()
            : base(VirtualNodeName.GenerateNodeName(VirtualStorageState.State.PrefixDirectory))
        {
            _nodes = [];
        }

        public VirtualDirectory(VirtualNodeName name) : base(name)
        {
            _nodes = [];
        }

        public VirtualDirectory(VirtualNodeName name, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            _nodes = [];
        }

        // 文字列からVirtualDirectoryへの暗黙的な変換
        public static implicit operator VirtualDirectory(VirtualNodeName nodeName)
        {
            return new VirtualDirectory(nodeName);
        }

        public override string ToString() => (Name == VirtualPath.Root) ? VirtualPath.Root : $"{Name}{VirtualPath.Separator}";

        public override VirtualNode DeepClone(bool recursive = false)
        {
            VirtualDirectory newDirectory = new(Name);
            if (recursive)
            {
                foreach (VirtualNode node in Nodes)
                {
                    newDirectory.Add(node.DeepClone(true));
                }
            }
            return newDirectory;
        }

        public IEnumerable<VirtualNode> GetNodeList()
        {
            VirtualNodeTypeFilter filter = VirtualStorageState.State.NodeListConditions.Filter;
            VirtualGroupCondition<VirtualNode, object>? groupCondition = VirtualStorageState.State.NodeListConditions.GroupCondition;
            List<VirtualSortCondition<VirtualNode>>? sortConditions = VirtualStorageState.State.NodeListConditions.SortConditions;

            IEnumerable<VirtualNode> nodes = Nodes;

            switch (filter)
            {
                case VirtualNodeTypeFilter.None:
                    return [];
                case VirtualNodeTypeFilter.All:
                    break;
                default:
                    nodes = nodes.Where(node => IsNodeMatchingFilter(node, filter));
                    break;
            }

            return nodes.GroupAndSort(groupCondition, sortConditions);
        }

        private static bool IsNodeMatchingFilter(VirtualNode node, VirtualNodeTypeFilter filter)
        {
            if (filter.HasFlag(VirtualNodeTypeFilter.Directory) && node is VirtualDirectory)
            {
                return true;
            }

            if (filter.HasFlag(VirtualNodeTypeFilter.Item) && node is VirtualItem)
            {
                return true;
            }

            if (filter.HasFlag(VirtualNodeTypeFilter.SymbolicLink) && node is VirtualSymbolicLink link)
            {
                return filter.HasFlag(link.TargetNodeType.ToFilter());
            }

            return false;
        }

        public VirtualNode Add(VirtualNode node, bool allowOverwrite = false)
        {
            if (!VirtualNodeName.IsValidNodeName(node.Name))
            {
                throw new ArgumentException($"ノード名 '{node}' は無効です。", nameof(node));
            }

            VirtualNodeName key = node.Name;

            if (_nodes.ContainsKey(key) && !allowOverwrite)
            {
                throw new InvalidOperationException($"ノード '{key}' は既に存在します。上書きは許可されていません。");
            }

            // ノードがストレージに追加済みなら、クローンを作成する
            if (node.IsReferencedInStorage)
            {
                if (node is VirtualDirectory)
                {
                    node = node.DeepClone(true);
                }
                else
                {
                    node = node.DeepClone(false);
                }
            }

            _nodes[key] = node;

            // 更新日付を更新
            UpdatedDate = DateTime.Now;

            // 自分自身のIsReferencedInStorageを下位ノードに伝搬させる
            SetIsReferencedInStorageRecursively(node, IsReferencedInStorage);

            return node;
        }

        public VirtualItem<T> AddItem<T>(VirtualNodeName name, T? itemData = default, bool allowOverwrite = false)
        {
            VirtualItem<T> item = (VirtualItem<T>)Add(new VirtualItem<T>(name, itemData), allowOverwrite);
            return item;
        }

        public VirtualSymbolicLink AddSymbolicLink(VirtualNodeName name, VirtualPath targetPath, bool allowOverwrite = false)
        {
            VirtualSymbolicLink link = (VirtualSymbolicLink)Add(new VirtualSymbolicLink(name, targetPath), allowOverwrite);
            return link;
        }

        public VirtualDirectory AddDirectory(VirtualNodeName name, bool allowOverwrite = false)
        {
            VirtualDirectory directory = (VirtualDirectory)Add(new VirtualDirectory(name), allowOverwrite);
            return directory;
        }

        [IndexerName("Indexer")]
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

        internal void SetNodeName(VirtualNodeName oldName, VirtualNodeName newName)
        {
            if (!NodeExists(oldName))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{oldName}' は存在しません。");
            }

            if (NodeExists(newName))
            {
                throw new InvalidOperationException($"指定されたノード '{newName}' は既に存在します。");
            }

            VirtualNode node = _nodes[oldName];
            _nodes.Remove(oldName);
            _nodes[newName] = node;

            // 更新日付を更新
            UpdatedDate = DateTime.Now;
        }

        public void Remove(VirtualNode node)
        {
            if (!NodeExists(node.Name))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{node}' は存在しません。");
            }

            _nodes.Remove(node.Name);

            // 更新日付を更新
            UpdatedDate = DateTime.Now;

            // IsReferencedInStorage = false を下位ノードに伝搬させる
            SetIsReferencedInStorageRecursively(node, false);
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

        public IEnumerator<VirtualNode> GetEnumerator() => NodesView.GetEnumerator();

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

        public static VirtualDirectory operator +(VirtualDirectory directory, VirtualNode node)
        {
            node = directory.Add(node);
            if (directory.IsReferencedInStorage)
            {
                SetIsReferencedInStorageRecursively(node, true);
            }
            return directory;
        }

        public static VirtualDirectory operator -(VirtualDirectory directory, VirtualNode node)
        {
            directory.Remove(node);
            SetIsReferencedInStorageRecursively(node, false);
            return directory;
        }

        private static void SetIsReferencedInStorageRecursively(VirtualNode node, bool value)
        {
            node.IsReferencedInStorage = value;
            if (node is VirtualDirectory subDirectory)
            {
                foreach (var subNode in subDirectory.Nodes)
                {
                    SetIsReferencedInStorageRecursively(subNode, value);
                }
            }
        }

        public override void Update(VirtualNode node)
        {
            if (node is not VirtualDirectory newDirectory)
            {
                throw new ArgumentException($"このノード {node.Name} はディレクトリではありません。");
            }

            if (newDirectory.IsReferencedInStorage)
            {
                newDirectory = (VirtualDirectory)newDirectory.DeepClone(true);
            }

            CreatedDate = newDirectory.CreatedDate;
            UpdatedDate = DateTime.Now;

            foreach (VirtualNode subNode in newDirectory.Nodes)
            {
                if (_nodes.TryGetValue(subNode.Name, out VirtualNode? existingNode))
                {
                    existingNode.Update(subNode.DeepClone(true));
                }
                else
                {
                    Add(subNode.DeepClone(true));
                }
            }
        }
    }
}
