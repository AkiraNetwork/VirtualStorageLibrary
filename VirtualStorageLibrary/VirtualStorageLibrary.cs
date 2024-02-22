using System.Text;

namespace VirtualStorageLibrary
{
    public delegate void NodeAction(VirtualPath path, VirtualNode node, bool isEnd);

    public enum VirtualNodeType
    {
        Non = 0x00,
        Item = 0x01,
        Directory = 0x02,
        SymbolicLink = 0x04,
        All = Item | Directory | SymbolicLink
    }

    public class NodeResolutionResult
    {
        public VirtualNode Node { get; }
        public VirtualPath ResolvedPath { get; }

        public NodeResolutionResult(VirtualNode node, VirtualPath resolvedPath)
        {
            Node = node;
            ResolvedPath = resolvedPath;
        }
    }

    public class VirtualNodeNotFoundException : Exception
    {
        public VirtualNodeNotFoundException()
        {
        }
        
        public VirtualNodeNotFoundException(string message) : base(message)
        {
        }

        public VirtualNodeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public interface IDeepCloneable<T>
    {
        T DeepClone();
    }

    public class VirtualPath : IEquatable<VirtualPath>
    {
        private readonly string _path;

        private VirtualPath? _directoryPath = null;

        private VirtualPath? _nodeName = null;

        public string Path => _path;

        private List<VirtualPath>? _partsList = null;

        public VirtualPath DirectoryPath
        {
            get
            {
                if (_directoryPath == null)
                {
                    _directoryPath = new VirtualPath(GetDirectoryPath());
                }
                return _directoryPath;
            }
        }

        public VirtualPath NodeName
        {
            get
            {
                if (_nodeName == null)
                {
                    _nodeName = new VirtualPath(GetNodeName());
                }
                return _nodeName;
            }
        }

        public List<VirtualPath> PartsList
        {
            get
            {
                if (_partsList == null)
                {
                    _partsList = GetPartsList();
                }
                return _partsList;
            }
        }

        public static VirtualPath Root => new("/");

        public static VirtualPath Empty => new(string.Empty);

        public static VirtualPath Dot => new(".");

        public static VirtualPath DotDot => new("..");

        public override string ToString() => _path;

        public bool IsEmpty => _path == string.Empty;

        public bool IsRoot => _path == "/";

        public bool IsAbsolute => _path.StartsWith('/');

        public bool IsEndsWithSlash => _path.EndsWith('/');

        public bool IsDot => _path == ".";

        public bool IsDotDot => _path == "..";

        public override int GetHashCode() => _path.GetHashCode();

        public VirtualPath(string path)
        {
            _path = path;
        }

        public VirtualPath(IEnumerable<VirtualPath> parts)
        {
            _path = string.Join('/', parts.Select(p => p.Path));
        }

        public override bool Equals(object? obj)
        {
            if (obj is VirtualPath other)
            {
                return _path == other._path;
            }
            return false;
        }

        public bool Equals(VirtualPath? other)
        {
            return _path == other?._path;
        }

        public static bool operator ==(VirtualPath? left, VirtualPath? right)
        {
            // 両方が null の場合は true
            if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
            {
                return true;
            }

            // 一方が null の場合は false
            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
            {
                return false;
            }

            // 実際のパスの比較
            return left._path == right._path;
        }

        public static bool operator !=(VirtualPath? left, VirtualPath? right)
        {
            return !(left == right);
        }

        public static VirtualPath operator +(VirtualPath path1, VirtualPath path2)
        {
            return path1.Combine(path2);
        }

        public VirtualPath TrimEndSlash()
        {
            if (_path.EndsWith('/'))
            {
                return new VirtualPath(_path.Substring(0, _path.Length - 1));
            }
            return this;
        }

        public VirtualPath AddEndSlash()
        {
            if (!_path.EndsWith('/'))
            {
                return new VirtualPath(_path + '/');
            }
            return this;
        }

        public VirtualPath AddStartSlash()
        {
            if (!_path.StartsWith('/'))
            {
                return new VirtualPath('/' + _path);
            }
            return this;
        }

        public bool StartsWith(VirtualPath path)
        {
            return _path.StartsWith(path.Path);
        }

        public VirtualPath NormalizePath()
        {
            if (_path == "")
            {
                throw new ArgumentException("パスが空です。");
            }

            var parts = new LinkedList<string>();
            var isAbsolutePath = _path.StartsWith('/');
            IList<string> partList = _path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in partList)
            {
                if (part == "..")
                {
                    if (parts.Count > 0 && parts.Last!.Value != "..")
                    {
                        parts.RemoveLast();
                    }
                    else if (!isAbsolutePath)
                    {
                        parts.AddLast("..");
                    }
                    else
                    {
                        throw new InvalidOperationException("パスがルートディレクトリより上への移動を試みています。無効なパス: " + _path);
                    }
                }
                else if (part != ".")
                {
                    parts.AddLast(part);
                }
            }

            var normalizedPath = String.Join('/', parts);
            VirtualPath result;
            if (isAbsolutePath)
            {
                result = new VirtualPath('/' + normalizedPath);
            }
            else
            {
                result = new VirtualPath(normalizedPath);
            }

            return result;
        }

        private string GetDirectoryPath()
        {
            // パスが '/' で始まっていない場合、それは相対パスなのでそのまま返す
            if (!_path.StartsWith('/'))
            {
                return _path;
            }

            int lastSlashIndex = _path.LastIndexOf('/');
            // '/' が見つからない場合は、ルートディレクトリを示す "/" を返す
            if (lastSlashIndex <= 0)
            {
                return "/";
            }
            else
            {
                // フルパスから最後の '/' までの部分を抜き出して返す
                return _path.Substring(0, lastSlashIndex);
            }
        }
        
        private string GetNodeName()
        {
            if (_path == "/")
            {
                //　ルートの場合は、空文字列を返す
                return string.Empty;
            }

            StringBuilder path = new StringBuilder(_path);

            if (path.Length > 0 && path[^1] == '/')
            {
                // 末尾の '/' を取り除く
                path.Remove(path.Length - 1, 1);
            }

            int lastSlashIndex = path.ToString().LastIndexOf('/');
            if (lastSlashIndex < 0)
            {
                // '/' が見つからない場合は、そのままの文字列を返す
                return _path;
            }
            else
            {
                // 最後の '/' 以降の部分を抜き出して返す
                return path.ToString().Substring(lastSlashIndex + 1);
            }
        }

        public VirtualPath Combine(params VirtualPath[] paths)
        {
            // 現在のパスを基点として新しいパスを構築するStringBuilderインスタンスを作成
            var newPathBuilder = new StringBuilder(_path);

            foreach (var path in paths)
            {
                // 区切り文字"/"を無条件で追加
                newPathBuilder.Append('/');
                // 新しいパスコンポーネントを追加
                newPathBuilder.Append(path.Path);
            }

            // StringBuilderの内容を文字列に変換
            var combinedPath = newPathBuilder.ToString();

            // "/"がダブっている箇所を解消
            var normalizedPath = combinedPath.Replace("//", "/");

            // 結果が"/"だったら空文字列に変換
            normalizedPath = (normalizedPath == "/")? string.Empty : normalizedPath;

            // 末尾の "/" を取り除く
            if (normalizedPath.EndsWith('/'))
            {
                normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - 1);
            }

            // 結合されたパスで新しいVirtualPathインスタンスを生成
            return new VirtualPath(normalizedPath);
        }

        public VirtualPath GetParentPath()
        {
            // パスの最後の '/' を取り除きます
            string trimmedPath = _path.TrimEnd('/');
            // パスを '/' で分割します
            string[] pathParts = trimmedPath.Split('/');
            // 最後の部分を除去します
            string[] parentPathParts = pathParts.Take(pathParts.Length - 1).ToArray();
            // パスを再構築します
            string parentPath = string.Join('/', parentPathParts);

            // パスが空になった場合は、ルートを返します
            if (string.IsNullOrEmpty(parentPath))
            {
                return VirtualPath.Root;
            }

            return new VirtualPath(parentPath);
        }

        public LinkedList<VirtualPath> GetPartsLinkedList()
        {
            LinkedList<VirtualPath> parts = new();
            foreach (var part in _path.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                parts.AddLast(new VirtualPath(part));
            }

            return parts;
        }

        public List<VirtualPath> GetPartsList()
        {
            return GetPartsLinkedList().ToList();
        }
    }

    public abstract class VirtualNode : IDeepCloneable<VirtualNode>
    {
        public VirtualPath Name { get; set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }

        public abstract VirtualNode DeepClone();

        protected VirtualNode(VirtualPath name)
        {
            Name = name;
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }

        protected VirtualNode(VirtualPath name, DateTime createdDate, DateTime updatedDate)
        {
            Name = name;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }
    }

    public static class VirtualNodeExtensions
    {
        public static bool IsItem(this VirtualNode node)
        {
            // nodeの型がVirtualItem<>に基づいているかどうかをチェック
            var nodeType = node.GetType();
            while (nodeType != null)
            {
                if (nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeof(VirtualItem<>))
                {
                    return true;
                }
                nodeType = nodeType.BaseType;
            }
            return false;
        }

        public static bool IsDirectory(this VirtualNode node) => node is VirtualDirectory;
        
        public static bool IsSymbolicLink(this VirtualNode node) => node is VirtualSymbolicLink;
    }

    public class VirtualSymbolicLink : VirtualNode
    {
        public VirtualPath TargetPath { get; set; }

        public VirtualSymbolicLink(VirtualPath name, VirtualPath targetPath) : base(name)
        {
            TargetPath = targetPath;
        }

        public VirtualSymbolicLink(VirtualPath name, VirtualPath targetPath, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            TargetPath = targetPath;
        }

        public override string ToString()
        {
            // シンボリックリンクの名前と、リンク先のパスを返します。
            return $"Symbolic Link: {Name} -> {TargetPath}";
        }

        public override VirtualNode DeepClone()
        {
            return new VirtualSymbolicLink(Name, TargetPath, CreatedDate, UpdatedDate);
        }
    }

    public class VirtualItem<T> : VirtualNode, IDeepCloneable<VirtualItem<T>>
    {
        public T Item { get; set; }

        public VirtualItem(VirtualPath name, T item) : base(name)
        {
            Item = item;
        }

        public VirtualItem(VirtualPath name, T item, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            Item = item;
        }

        public override string ToString()
        {
            string itemInformation = $"Item: {Name}";

            // C# 8.0のnull非許容参照型に対応
            // Itemがnullではないことを確認し、ItemのToString()の結果を使用
            if (Item != null && Item.GetType().ToString() != Item.ToString())
            {
                itemInformation += $", {Item.ToString()}";
            }

            return itemInformation;
        }

        public override VirtualNode DeepClone()
        {
            T clonedItem = Item;
            if (Item is IDeepCloneable<T> cloneableItem)
            {
                clonedItem = cloneableItem.DeepClone();
            }

            return new VirtualItem<T>(Name, clonedItem);
        }

        VirtualItem<T> IDeepCloneable<VirtualItem<T>>.DeepClone()
        {
            return (VirtualItem<T>)DeepClone();
        }
    }

    public class VirtualDirectory : VirtualNode, IDeepCloneable<VirtualDirectory>
    {
        private Dictionary<VirtualPath, VirtualNode> _nodes;

        public int Count => _nodes.Count;

        public int DirectoryCount => _nodes.Values.OfType<VirtualDirectory>().Count();

        public int ItemCount => _nodes.Values.Count(n => !(n is VirtualDirectory));

        public IEnumerable<VirtualPath> NodeNames => _nodes.Keys;

        public IEnumerable<VirtualNode> Nodes => _nodes.Values;

        public bool NodeExists(VirtualPath name) => _nodes.ContainsKey(name);

        public bool ItemExists(VirtualPath name)
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

        public bool DirectoryExists(VirtualPath name)
        {
            if (!NodeExists(name))
            {
                return false;
            }

            return _nodes[name] is VirtualDirectory;
        }

        public bool SymbolicLinkExists(VirtualPath name)
        {
            if (!NodeExists(name))
            {
                return false;
            }

            var node = _nodes[name];
            return node is VirtualSymbolicLink;
        }

        public VirtualDirectory(VirtualPath name) : base(name)
        {
            _nodes = new Dictionary<VirtualPath, VirtualNode>();
        }

        public VirtualDirectory(VirtualPath name, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            _nodes = new Dictionary<VirtualPath, VirtualNode>();
        }

        public override string ToString()
        {
            return $"Directory: {Name}, Count: {Count} ({DirectoryCount} directories, {ItemCount} items)";
        }

        public override VirtualNode DeepClone()
        {
            var clonedDirectory = new VirtualDirectory(Name, CreatedDate, UpdatedDate);

            foreach (var pair in this._nodes)
            {
                VirtualNode clonedNode = pair.Value;
                if (pair.Value is IDeepCloneable<VirtualNode> cloneableNode)
                {
                    clonedNode = cloneableNode.DeepClone();
                }
                clonedDirectory._nodes.Add(pair.Key, clonedNode);
            }

            return clonedDirectory;
        }

        VirtualDirectory IDeepCloneable<VirtualDirectory>.DeepClone()
        {
            return (VirtualDirectory)DeepClone();
        }

        public void Add(VirtualNode node, bool allowOverwrite = false)
        {
            VirtualPath key = node.Name;

            if (_nodes.ContainsKey(key) && !allowOverwrite)
            {
                throw new InvalidOperationException($"ノード '{key}' は既に存在します。上書きは許可されていません。");
            }

            _nodes[key] = node;
        }

        public void AddItem<T>(VirtualPath name, T item, bool allowOverwrite = false)
        {
            Add(new VirtualItem<T>(name, item), allowOverwrite);
        }

        public void AddSymbolicLink(VirtualPath name, VirtualPath targetPath, bool allowOverwrite = false)
        {
            Add(new VirtualSymbolicLink(name, targetPath), allowOverwrite);
        }

        public void AddDirectory(VirtualPath name, bool allowOverwrite = false)
        {
            Add(new VirtualDirectory(name), allowOverwrite);
        }

        public VirtualNode this[VirtualPath name]
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

        public void Remove(VirtualPath name, bool forceRemove = false)
        {
            if (!NodeExists(name))
            {
                if (!forceRemove)
                {
                    throw new VirtualNodeNotFoundException($"指定されたノード '{name}' は存在しません。");
                }
                else
                {
                    // forceRemoveがtrueの場合、ノードが存在しなくても正常終了
                    return;
                }
            }

            _nodes.Remove(name);
        }

        public VirtualNode Get(VirtualPath name)
        {
            if (!NodeExists(name))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{name}' は存在しません。");
            }
            return _nodes[name];
        }

        public void Rename(VirtualPath oldName, VirtualPath newName)
        {
            if (!NodeExists(oldName))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{oldName}' は存在しません。");
            }

            if (NodeExists(newName))
            {
                throw new InvalidOperationException($"指定されたノード '{newName}' は存在しません。");
            }

            var node = Get(oldName);
            node.Name = newName;
            Add(node);
            Remove(oldName);
        }
    }

    public class VirtualStorage
    {
        private VirtualDirectory _root;

        public VirtualPath CurrentPath { get; private set; }

        public VirtualStorage()
        {
            _root = new VirtualDirectory(VirtualPath.Root);
            CurrentPath = VirtualPath.Root;
        }

        public void ChangeDirectory(VirtualPath path)
        {
            VirtualPath resolvedPath = ResolveLinkTarget(path);

            // ディレクトリが存在しない場合は例外をスロー
            if (!DirectoryExists(resolvedPath))
            {
                throw new VirtualNodeNotFoundException($"ディレクトリ '{path}' は存在しません。");
            }

            // カレントパスを変更
            CurrentPath = path.NormalizePath();
        }

        public VirtualPath ConvertToAbsolutePath(VirtualPath virtualRelativePath, VirtualPath? basePath = null)
        {
            basePath ??= CurrentPath;

            // relativePathが空文字列の場合、ArgumentExceptionをスロー
            if (virtualRelativePath.IsEmpty)
            {
                throw new ArgumentException("relativePathが空です。", nameof(virtualRelativePath));
            }

            // relativePathが既に絶対パスである場合は、そのまま使用
            if (virtualRelativePath.IsAbsolute)
            {
                return virtualRelativePath;
            }

            // basePathが空文字列の場合、ArgumentExceptionをスロー
            if (basePath.IsEmpty)
            {
                throw new ArgumentException("basePathが空です。", nameof(basePath));
            }

            // relativePathをeffectiveBasePathに基づいて絶対パスに変換
            var absolutePath = basePath + virtualRelativePath;

            return absolutePath;
        }

        public void AddSymbolicLink(VirtualPath linkPath, VirtualPath targetPath, bool overwrite = false)
        {
            // linkPathを絶対パスに変換
            VirtualPath absoluteLinkPath = ConvertToAbsolutePath(linkPath);

            // directoryPath（ディレクトリパス）とlinkName（リンク名）を分離
            VirtualPath directoryPath = absoluteLinkPath.DirectoryPath;
            VirtualPath linkName = absoluteLinkPath.NodeName;

            // 対象ディレクトリを安全に取得
            VirtualDirectory? directory = TryGetDirectory(directoryPath, followLinks: true);
            if (directory == null)
            {
                throw new VirtualNodeNotFoundException($"ディレクトリ '{directoryPath}' が存在しません。");
            }

            // 既存のノードの存在チェック
            if (directory.NodeExists(linkName))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException($"ノード '{linkName}' は既に存在します。上書きは許可されていません。");
                }
                else
                {
                    // 既存のノードがシンボリックリンクであるかどうかチェック
                    if (!directory.SymbolicLinkExists(linkName))
                    {
                        throw new InvalidOperationException($"既存のノード '{linkName}' はシンボリックリンクではありません。シンボリックリンクのみ上書き可能です。");
                    }
                    // 既存のシンボリックリンクを削除（上書きフラグがtrueの場合）
                    directory.Remove(linkName, true);
                }
            }

            // 新しいシンボリックリンクを追加
            directory.Add(new VirtualSymbolicLink(linkName, targetPath), true);
        }

        public void AddItem<T>(VirtualPath path, T item, bool overwrite = false)
        {
            // 絶対パスに変換
            VirtualPath absolutePath = ConvertToAbsolutePath(path);

            // ディレクトリパスとアイテム名を分離
            VirtualPath directoryPath = absolutePath.DirectoryPath;
            VirtualPath itemName = absolutePath.NodeName;

            // 対象ディレクトリを取得
            VirtualDirectory directory = GetDirectory(directoryPath, true);

            // 既存のアイテムの存在チェック
            if (directory.NodeExists(itemName))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException($"アイテム '{itemName}' は既に存在します。上書きは許可されていません。");
                }
                else
                {
                    // 上書き対象がアイテムであることを確認
                    if (!ItemExists(directoryPath + itemName))
                    {
                        throw new InvalidOperationException($"'{itemName}' はアイテム以外のノードです。アイテムの上書きはできません。");
                    }
                    // 既存アイテムの削除
                    directory.Remove(itemName, true); // 強制削除
                }
            }

            // 新しいアイテムを追加
            directory.Add(new VirtualItem<T>(itemName, item), overwrite);
        }

        public void AddDirectory(VirtualPath path, bool createSubdirectories = false)
        {
            VirtualPath absolutePath = ConvertToAbsolutePath(path);

            if (absolutePath.IsRoot)
            {
                throw new InvalidOperationException("ルートディレクトリは既に存在します。");
            }

            List<VirtualPath> nodeNameList = absolutePath.GetPartsList();

            LinkedList<VirtualNode> nodeLinkedList = new LinkedList<VirtualNode>();
            nodeLinkedList.AddLast(_root);
            int index = 0;

            VirtualPath basePath = VirtualPath.Empty; // 現在のベースパスを追跡

            while (index < nodeNameList.Count)
            {
                VirtualPath nodeName = nodeNameList[index];

                if (nodeName.IsDot)
                {
                    // 現在のディレクトリを示す場合、何もせず次のノードへ
                }
                else if (nodeName.IsDotDot)
                {
                    // 親ディレクトリを示す場合、現在のディレクトリを一つ上のディレクトリに変更
                    basePath = basePath.GetParentPath();
                    if (nodeLinkedList.Count > 1)
                    {
                        nodeLinkedList.RemoveLast();
                    }
                }
                else
                {
                    if (nodeLinkedList.Last!.Value is VirtualDirectory directory)
                    {
                        if (!directory.NodeExists(nodeName))
                        {
                            if (createSubdirectories || index == nodeNameList.Count - 1)
                            {
                                directory.AddDirectory(nodeName);
                            }
                            else
                            {
                                throw new VirtualNodeNotFoundException($"ディレクトリ '{nodeName}' が見つかりません。");
                            }
                        }
                        else
                        {
                            if (index == nodeNameList.Count - 1)
                            {
                                VirtualNode node = directory.Get(nodeName);
                                if (node is VirtualDirectory)
                                {
                                    if (!createSubdirectories)
                                    {
                                        throw new InvalidOperationException($"同じ名前のディレクトリ '{nodeName}' か既に存在します。");
                                    }
                                }
                                else
                                {
                                    throw new InvalidOperationException($"同じ名前のノード '{nodeName}' か既に存在します。");
                                }
                            }
                        }
                        nodeLinkedList.AddLast(directory[nodeName]);
                    }
                    else
                    {
                        throw new VirtualNodeNotFoundException($"ノード '{nodeName}' はディレクトリではありません。");
                    }

                    if (nodeLinkedList.Last.Value is VirtualSymbolicLink symlink)
                    {
                        VirtualPath symlinkTargetPath = ConvertToAbsolutePath(symlink.TargetPath, basePath);
                        List<VirtualPath> targetPathList = symlinkTargetPath.GetPartsList();
                        nodeNameList = targetPathList.Concat(nodeNameList.Skip(index + 1)).ToList();
                        index = -1; // indexをリセットし、次のループで0から開始
                        nodeLinkedList.Clear(); // ノードリストをリセット
                        nodeLinkedList.AddLast(_root); // ルートノードを追加
                        basePath = VirtualPath.Empty; // 現在のパスもリセット
                    }
                    else
                    {
                        basePath = basePath + nodeName;
                    }
                }

                index++;
            }

            return;
        }

        public void WalkPathWithAction(VirtualPath targetPath, NodeAction action, bool followLinks)
        {
            List<VirtualPath> pathList = targetPath.GetPartsList();
            WalkPathWithActionInternal(pathList, 0, VirtualPath.Root, _root, action);
        }

        public void WalkPathWithActionInternal(List<VirtualPath> pathList, int traversalIndex, VirtualPath traversalPath, VirtualDirectory traversalDirectory, NodeAction action)
        {
            VirtualPath traversalNodeName = pathList[traversalIndex];

            // 探索ノードが存在しない場合は終了
            if (!traversalDirectory.NodeExists(traversalNodeName))
            {
                return;
            }

            // 探索ノードを取得
            VirtualNode node = traversalDirectory[traversalNodeName];

            // 探索パスを更新
            traversalPath = traversalPath + traversalNodeName;

            if (node.IsDirectory())
            {
                // 次のノードへ
                traversalIndex++;
                
                // 最後のノードに到達したかチェック
                if (pathList.Count <= traversalIndex)
                {
                    // 末端のノードを通知
                    action(traversalPath, node, true);
                    return;
                }

                // 途中のノードを通知
                action(traversalPath, node, false);

                // 次の探索ノード名を取得
                traversalNodeName = pathList[traversalIndex];

                // 探索ディレクトリを取得
                traversalDirectory = (VirtualDirectory)node;

                // 再帰的に探索
                WalkPathWithActionInternal(pathList, traversalIndex, traversalPath, traversalDirectory, action);
            }
            else if (node.IsItem())
            {
                // 末端のノードを通知
                action(traversalPath, node, true);
            }
            else if (node.IsSymbolicLink())
            {
                VirtualSymbolicLink link = (VirtualSymbolicLink)node;
                VirtualPath linkTargetPath = link.TargetPath;

                // TODO: シンボリックリンクの再帰的な処理を実装
            }
            return;
        }

        public NodeResolutionResult GetNodeInternal(VirtualPath path, bool followLinks)
        {
            VirtualPath absolutePath = ConvertToAbsolutePath(path);

            if (absolutePath.IsRoot)
            {
                return new NodeResolutionResult(_root, VirtualPath.Root);
            }

            List<VirtualPath> nodeNameList = absolutePath.GetPartsList();

            int index;
            VirtualPath basePath;
            LinkedList<VirtualNode> nodeLinkedList = new();

            void reset()
            {
                index = 0;
                nodeLinkedList.Clear();
                nodeLinkedList.AddLast(_root);
                basePath = VirtualPath.Empty;
            }

            reset();

            VirtualPath resolvedPath = VirtualPath.Root;

            while (index < nodeNameList.Count)
            {
                VirtualPath nodeName = nodeNameList[index];

                if (nodeName.IsDot)
                {
                }
                else if (nodeName.IsDotDot)
                {
                    basePath = basePath.GetParentPath();
                    resolvedPath = basePath;
                    if (nodeLinkedList.Count > 1)
                    {
                        nodeLinkedList.RemoveLast();
                    }
                }
                else
                {
                    if (nodeLinkedList.Last!.Value is VirtualDirectory directory)
                    {
                        if (!directory.NodeExists(nodeName))
                        {
                            throw new VirtualNodeNotFoundException($"Node '{nodeName}' does not exist.");
                        }
                        nodeLinkedList.AddLast(directory[nodeName]);
                    }
                    else
                    {
                        break;
                    }

                    if (followLinks && nodeLinkedList.Last.Value is VirtualSymbolicLink link)
                    {
                        VirtualPath linkTargetPath = ConvertToAbsolutePath(link.TargetPath, basePath);
                        List<VirtualPath> targetPathList = linkTargetPath.GetPartsList();
                        nodeNameList = targetPathList.Concat(nodeNameList.Skip(index + 1)).ToList();
                        resolvedPath = linkTargetPath;

                        reset();
                        continue;
                    }
                    else
                    {
                        basePath = basePath + nodeName;
                        resolvedPath = basePath;
                    }
                }

                index++;
            }

            return new NodeResolutionResult(nodeLinkedList.Last!.Value, resolvedPath);
        }

        public VirtualNode GetNode(VirtualPath path, bool followLinks = false)
        {
            NodeResolutionResult nodeResolutionResult = GetNodeInternal(path, followLinks);
            return nodeResolutionResult.Node;
        }

        public VirtualPath ResolveLinkTarget(VirtualPath path)
        {
            NodeResolutionResult nodeResolutionResult = GetNodeInternal(path, true);
            return nodeResolutionResult.ResolvedPath;
        }

        public VirtualDirectory GetDirectory(VirtualPath path, bool followLinks = false)
        {
            VirtualPath absolutePath = ConvertToAbsolutePath(path);
            VirtualNode node = GetNode(absolutePath, followLinks);

            if (node is VirtualDirectory directory)
            {
                return directory;
            }
            else
            {
                throw new VirtualNodeNotFoundException($"ディレクトリ {absolutePath} は存在しません。");
            }
        }

        public VirtualDirectory? TryGetDirectory(VirtualPath path, bool followLinks = false)
        {
            try
            {
                return GetDirectory(path, followLinks);
            }
            catch (VirtualNodeNotFoundException)
            {
                return null;
            }
        }

        private IEnumerable<T> GetNodesInternal<T>(VirtualPath basePath, VirtualNodeType nodeType, bool recursive, Func<VirtualNode, VirtualPath, T> selector, bool followLinks)
        {
            // ベースパスが空の場合は例外をスロー
            if (basePath.IsEmpty)
            {
                throw new ArgumentException("パスが空です。");
            }

            // ベースパスが絶対パスでない場合は例外をスロー
            if (!basePath.IsAbsolute)
            {
                throw new ArgumentException($"絶対パスを指定してください。{basePath}");
            }

            var directory = (VirtualDirectory)GetNode(basePath, followLinks);

            foreach (var node in directory.Nodes)
            {
                if (node.IsDirectory())
                {
                    VirtualDirectory subdirectory = (VirtualDirectory)node;
                    if ((nodeType & VirtualNodeType.Directory) == VirtualNodeType.Directory)
                    {
                        yield return selector(subdirectory, basePath + subdirectory.Name);
                    }

                    if (recursive)
                    {
                        var subdirectoryPath = basePath + subdirectory.Name;
                        foreach (var subNode in GetNodesInternal(subdirectoryPath, nodeType, recursive, selector, followLinks))
                        {
                            yield return subNode;
                        }
                    }
                }
                else if(node.IsItem())
                {
                    if ((nodeType & VirtualNodeType.Item) == VirtualNodeType.Item)
                    {
                        yield return selector(node, basePath + node.Name);
                    }
                }
                else if (node.IsSymbolicLink())
                {
                    if ((nodeType & VirtualNodeType.SymbolicLink) == VirtualNodeType.SymbolicLink)
                    {
                        yield return selector(node, basePath + node.Name);
                    }
                }
            }
        }

        public IEnumerable<VirtualNode> GetNodes(VirtualPath basePath, VirtualNodeType nodeType = VirtualNodeType.All, bool recursive = false, bool followLinks = false)
        {
            return GetNodesInternal(basePath, nodeType, recursive, (node, path) => node, followLinks);
        }

        public IEnumerable<VirtualNode> GetNodes(VirtualNodeType nodeType = VirtualNodeType.All, bool recursive = false, bool followLinks = false)
        {
            return GetNodesInternal(CurrentPath, nodeType, recursive, (node, path) => node, followLinks);
        }

        public IEnumerable<VirtualPath> GetNodesWithPaths(VirtualPath basePath, VirtualNodeType nodeType = VirtualNodeType.All, bool recursive = false, bool followLinks = false)
        {
            return GetNodesInternal(basePath, nodeType, recursive, (node, path) => path, followLinks);
        }

        public IEnumerable<VirtualPath> GetNodesWithPaths(VirtualNodeType nodeType = VirtualNodeType.All, bool recursive = false, bool followLinks = false)
        {
            return GetNodesInternal(CurrentPath, nodeType, recursive, (node, path) => path, followLinks);
        }

        private void CheckCopyPreconditions(VirtualPath sourcePath, VirtualPath destinationPath, bool overwrite, bool recursive)
        {
            VirtualPath absoluteSourcePath = ConvertToAbsolutePath(sourcePath);
            VirtualPath absoluteDestinationPath = ConvertToAbsolutePath(destinationPath);

            // ルートディレクトリのコピーを禁止
            if (absoluteSourcePath.IsRoot)
            {
                throw new InvalidOperationException("ルートディレクトリのコピーは禁止されています。");
            }

            // コピー元の存在確認
            if (!NodeExists(absoluteSourcePath))
            {
                throw new VirtualNodeNotFoundException($"コピー元ノード '{absoluteSourcePath}' は存在しません。");
            }

            // コピー元とコピー先が同じ場合は例外をスロー
            if (absoluteSourcePath == absoluteDestinationPath)
            {
                throw new InvalidOperationException("コピー元とコピー先が同じです。");
            }

            // 循環参照チェック
            if (absoluteDestinationPath.StartsWith(absoluteSourcePath.AddEndSlash()) || absoluteSourcePath.StartsWith(absoluteDestinationPath.AddEndSlash()))
            {
                throw new InvalidOperationException("コピー元またはコピー先が互いのサブディレクトリになっています。");
            }

            bool destinationIsDirectory = DirectoryExists(absoluteDestinationPath) || absoluteDestinationPath.IsEndsWithSlash;
            VirtualPath targetDirectoryPath, newNodeName;
            if (destinationIsDirectory)
            {
                targetDirectoryPath = absoluteDestinationPath;
                newNodeName = absoluteSourcePath.NodeName;
            }
            else
            {
                targetDirectoryPath = absoluteDestinationPath.GetParentPath();
                newNodeName = absoluteDestinationPath.NodeName;
            }

            // コピー先ディレクトリが存在しない場合、新しいディレクトリとして扱う
            if (!DirectoryExists(targetDirectoryPath))
            {
                return; // コピー先ディレクトリが存在しないので、それ以上のチェックは不要
            }

            VirtualDirectory targetDirectory = GetDirectory(targetDirectoryPath);

            // コピー先に同名のノードが存在し、上書きが許可されていない場合、例外を投げる
            if (targetDirectory.NodeExists(newNodeName) && !overwrite)
            {
                throw new InvalidOperationException($"コピー先ディレクトリ '{targetDirectoryPath}' に同名のノード '{newNodeName}' が存在します。上書きは許可されていません。");
            }

            // 再帰的なチェック（ディレクトリの場合のみ）
            VirtualNode sourceNode = GetNode(absoluteSourcePath);
            if (recursive && sourceNode is VirtualDirectory sourceDirectory)
            {
                foreach (var subNode in sourceDirectory.Nodes)
                {
                    VirtualPath newSubSourcePath = absoluteSourcePath + subNode.Name;
                    VirtualPath newSubDestinationPath = absoluteDestinationPath + subNode.Name;
                    CheckCopyPreconditions(newSubSourcePath, newSubDestinationPath, overwrite, true);
                }
            }
        }

        public void CopyNode(VirtualPath sourcePath, VirtualPath destinationPath, bool recursive = false, bool overwrite = false)
        {
            // コピー前の事前条件チェック
            CheckCopyPreconditions(sourcePath, destinationPath, overwrite, recursive);

            VirtualPath absoluteSourcePath = ConvertToAbsolutePath(sourcePath);
            VirtualPath absoluteDestinationPath = ConvertToAbsolutePath(destinationPath);

            VirtualNode sourceNode = GetNode(absoluteSourcePath);

            bool destinationIsDirectory = DirectoryExists(absoluteDestinationPath) || absoluteDestinationPath.IsEndsWithSlash;

            VirtualPath targetDirectoryPath, newNodeName;
            if (destinationIsDirectory)
            {
                targetDirectoryPath = absoluteDestinationPath;
                newNodeName = absoluteSourcePath.NodeName;
            }
            else
            {
                targetDirectoryPath = absoluteDestinationPath.GetParentPath();
                newNodeName = absoluteDestinationPath.NodeName;
            }

            // コピー先ディレクトリが存在しない場合は例外をスロー
            if (!DirectoryExists(targetDirectoryPath))
            {
                throw new VirtualNodeNotFoundException($"コピー先ディレクトリ '{targetDirectoryPath}' は存在しません。");
            }

            VirtualDirectory targetDirectory = GetDirectory(targetDirectoryPath);

            if (sourceNode is VirtualDirectory sourceDirectory)
            {
                // 再帰フラグが false でもディレクトリが空の場合はコピーを許可
                if (!recursive && sourceDirectory.Nodes.Any())
                {
                    throw new InvalidOperationException("非空のディレクトリをコピーするには再帰フラグが必要です。");
                }

                // 再帰的なディレクトリコピーまたは空のディレクトリコピー
                VirtualDirectory newDirectory = new VirtualDirectory(newNodeName);
                targetDirectory.Add(newDirectory, overwrite);
                if (recursive)
                {
                    foreach (var node in sourceDirectory.Nodes)
                    {
                        VirtualPath intermediatePath = targetDirectoryPath + newNodeName;
                        VirtualPath newDestinationPath = intermediatePath + node.Name;
                        CopyNode(absoluteSourcePath + node.Name, newDestinationPath, true, overwrite);
                    }
                }
            }
            else
            {
                // 単一ノードのコピー
                VirtualNode clonedNode = sourceNode.DeepClone();
                clonedNode.Name = newNodeName;
                targetDirectory.Add(clonedNode, overwrite);
            }
        }

        public void RemoveNode(VirtualPath path, bool recursive = false)
        {
            // TODO: 絶対パスに変換後、ノーマライズする(1行で書くか書き方を検討する)
            VirtualPath absolutePath = ResolveLinkTarget(path);
            absolutePath = absolutePath.NormalizePath();

            if (absolutePath.IsRoot)
            {
                throw new InvalidOperationException("ルートディレクトリを削除することはできません。");
            }

            VirtualNode node = GetNode(absolutePath);

            // ディレクトリを親ディレクトリから削除するための共通の親パスと親ディレクトリを取得
            VirtualPath parentPath = absolutePath.GetParentPath();
            VirtualDirectory parentDirectory = GetDirectory(parentPath);

            if (node is VirtualDirectory directory)
            {
                if (!recursive && directory.Count > 0)
                {
                    throw new InvalidOperationException("ディレクトリが空ではなく、再帰フラグが設定されていません。");
                }

                // directory.Nodes コレクションのスナップショットを作成
                var nodesSnapshot = directory.Nodes.ToList();

                // スナップショットを反復処理して、各ノードを削除
                foreach (var subNode in nodesSnapshot)
                {
                    VirtualPath subPath = absolutePath + subNode.Name;
                    RemoveNode(subPath, recursive);
                }

                // ここで親ディレクトリからディレクトリを削除
                parentDirectory.Remove(directory.Name);
            }
            else
            {
                // ここで親ディレクトリからアイテム（ノード）を削除
                parentDirectory.Remove(node.Name);
            }
        }

        public VirtualNode? TryGetNode(VirtualPath path, bool followLinks = false)
        {
            VirtualPath absolutePath = ConvertToAbsolutePath(path);
            try
            {
                // GetNodeメソッドは、ノードが見つからない場合にnullを返すか、例外をスローするように実装されていると仮定
                return GetNode(absolutePath, followLinks);
            }
            catch (VirtualNodeNotFoundException)
            {
                return null; // ノードが存在しない場合はnullを返す
            }
        }

        public bool NodeExists(VirtualPath path, bool followLinks = false)
        {
            VirtualPath absolutePath = ConvertToAbsolutePath(path);
            var node = TryGetNode(absolutePath, followLinks);
            return node != null; // ノードがnullでなければ、存在すると判断
        }

        public bool DirectoryExists(VirtualPath path, bool followLinks = false)
        {
            if (path.IsRoot)
            {
                return true; // ルートディレクトリは常に存在する
            }

            VirtualPath absolutePath = ConvertToAbsolutePath(path);
            VirtualDirectory? directory = TryGetDirectory(absolutePath, followLinks);
            return directory != null;
        }

        public bool ItemExists(VirtualPath path, bool followLinks = false)
        {
            VirtualPath absolutePath = ConvertToAbsolutePath(path);
            var node = TryGetNode(absolutePath, followLinks);
            if (node == null) return false;
            var nodeType = node.GetType();
            return nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeof(VirtualItem<>);
        }

        public bool SymbolicLinkExists(VirtualPath path)
        {
            var absolutePath = ConvertToAbsolutePath(path);
            var parentDirectoryPath = absolutePath.GetParentPath();
            var directory = TryGetDirectory(parentDirectoryPath, true);

            if (directory != null)
            {
                var nodeName = absolutePath.NodeName;
                return directory.SymbolicLinkExists(nodeName);
            }
            return false;
        }

        public void MoveNode(VirtualPath sourcePath, VirtualPath destinationPath, bool overwrite = false)
        {
            VirtualPath absoluteSourcePath = ConvertToAbsolutePath(sourcePath);
            VirtualPath absoluteDestinationPath = ConvertToAbsolutePath(destinationPath);

            // 循環参照チェック
            if (absoluteDestinationPath.StartsWith(new VirtualPath(absoluteSourcePath.Path + '/')))
            {
                throw new InvalidOperationException("移動先が移動元のサブディレクトリになっています。");
            }

            // 移動先と移動元が同じかどうかのチェック
            if (absoluteSourcePath == absoluteDestinationPath)
            {
                throw new InvalidOperationException("移動元と移動先が同じです。");
            }
            
            // 移動元の存在チェック
            if (!NodeExists(absoluteSourcePath))
            {
                // 存在しない場合は例外をスロー
                throw new VirtualNodeNotFoundException($"指定されたノード '{absoluteSourcePath}' は存在しません。");
            }

            // 移動元のルートディレクトリチェック
            if (absoluteSourcePath.IsRoot)
            {
                // ルートディレクトリの場合は例外をスロー
                throw new InvalidOperationException("ルートディレクトリを移動することはできません。");
            }

            // 移動元のディレクトリ存在チェック
            if (DirectoryExists(absoluteSourcePath))
            {
                // ディレクトリの場合

                // 移動先の種類チェック
                if (DirectoryExists(absoluteDestinationPath))
                {
                    // ディレクトリの場合
                    VirtualDirectory destinationDirectory = GetDirectory(absoluteDestinationPath);
                    VirtualDirectory sourceDirectory = GetDirectory(absoluteSourcePath);
                    
                    // 移動先ディレクトリの同名チェック
                    if (destinationDirectory.NodeExists(sourceDirectory.Name))
                    {
                        // 同名のディレクトリが存在する場合
                        throw new InvalidOperationException($"移動先ディレクトリ '{absoluteDestinationPath}' に同名のノード '{sourceDirectory.Name}' が存在します。");
                    }
                    destinationDirectory.Add(sourceDirectory);
                    VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                    sourceParentDirectory.Remove(sourceDirectory.Name);
                }
                else
                {
                    // ディレクトリでない、または存在しない場合

                    // 移動先の存在チェック
                    if (!NodeExists(absoluteDestinationPath))
                    {
                        VirtualPath destinationParentPath = absoluteDestinationPath.GetParentPath();
                        if (!DirectoryExists(destinationParentPath))
                        {
                            // 移動先の親ディレクトリが存在しない場合
                            throw new VirtualNodeNotFoundException($"指定されたノード '{destinationParentPath}' は存在しません。");
                        }
                        VirtualDirectory destinationParentDirectory = GetDirectory(destinationParentPath);
                        VirtualPath destinationNodeName = absoluteDestinationPath.NodeName;
                        VirtualDirectory sourceDirectory = GetDirectory(absoluteSourcePath);

                        VirtualPath oldNodeName = sourceDirectory.Name;
                        sourceDirectory.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceDirectory);
                        VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                    else
                    {
                        // 存在する場合（移動先ノードがアイテム）
                        throw new InvalidOperationException($"移動先ノード '{absoluteDestinationPath}' はアイテムです。");
                    }
                }
            }
            else
            {
                // アイテムの場合

                // 移動先のディレクトリ存在チェック
                if (DirectoryExists(absoluteDestinationPath))
                {
                    // 存在する

                    VirtualDirectory destinationDirectory = GetDirectory(absoluteDestinationPath);
                    VirtualNode sourceNode = GetNode(absoluteSourcePath);

                    // 移動先ディレクトリの同名チェック
                    if (destinationDirectory.NodeExists(sourceNode.Name))
                    {
                        // 同名のノードが存在する場合

                        // 上書きチェック
                        if (overwrite)
                        {
                            // trueの場合、同名のノードを削除
                            destinationDirectory.Remove(sourceNode.Name);
                        }
                        else
                        {
                            // falseの場合、例外をスロー
                            throw new InvalidOperationException($"移動先ディレクトリ '{absoluteDestinationPath}' に同名のノード '{sourceNode.Name}' が存在します。");
                        }
                    }
                    destinationDirectory.Add(sourceNode);
                    VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                    sourceParentDirectory.Remove(sourceNode.Name);
                }
                else
                {
                    // 存在しない

                    // 移動先の存在チェック
                    if (!NodeExists(absoluteDestinationPath))
                    {
                        VirtualPath destinationParentPath = absoluteDestinationPath.GetParentPath();
                        if (!DirectoryExists(destinationParentPath))
                        {
                            // 移動先の親ディレクトリが存在しない場合
                            throw new VirtualNodeNotFoundException($"指定されたノード '{destinationParentPath}' は存在しません。");
                        }
                        VirtualDirectory destinationParentDirectory = GetDirectory(destinationParentPath);
                        VirtualPath destinationNodeName = absoluteDestinationPath.NodeName;
                        VirtualNode sourceNode = GetNode(absoluteSourcePath);

                        VirtualPath oldNodeName = sourceNode.Name;
                        sourceNode.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceNode);
                        VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                    else
                    {
                        // 存在する場合

                        VirtualDirectory destinationParentDirectory = GetDirectory(absoluteDestinationPath.GetParentPath());
                        VirtualPath destinationNodeName = absoluteDestinationPath.NodeName;
                        VirtualNode sourceNode = GetNode(absoluteSourcePath);

                        // 移動先ディレクトリの同名チェック
                        if (destinationParentDirectory.NodeExists(destinationNodeName))
                        {
                            // 同名のノードが存在する場合

                            // 上書きチェック
                            if (overwrite)
                            {
                                // trueの場合、同名のノードを削除
                                destinationParentDirectory.Remove(destinationNodeName);
                            }
                            else
                            {
                                // falseの場合、例外をスロー
                                throw new InvalidOperationException($"移動先ディレクトリ '{absoluteDestinationPath}' に同名のノード '{destinationNodeName}' が存在します。");
                            }
                        }

                        VirtualPath oldNodeName = sourceNode.Name;
                        sourceNode.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceNode);
                        VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                }
            }
        }
    }
}
