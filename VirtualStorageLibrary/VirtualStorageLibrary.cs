﻿using System.Text;

namespace VirtualStorageLibrary
{
    public enum VirtualNodeType
    {
        All,
        Directory,
        Item
    }

    public class NodeResolutionResult
    {
        public VirtualNode Node { get; set; }
        public string ResolvedPath { get; set; }

        public NodeResolutionResult(VirtualNode node, string resolvedPath)
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

    public class VirtualPath
    {
        private readonly string _path;

        public string Path => _path;

        public override string ToString() => _path;

        public override int GetHashCode() => _path.GetHashCode();

        public VirtualPath(string path)
        {
            _path = path;
        }

        public override bool Equals(object? obj)
        {
            if (obj is VirtualPath other)
            {
                return _path == other._path;
            }
            return false;
        }

        public VirtualPath NormalizePath()
        {
            if (_path == "")
            {
                throw new ArgumentException("パスが空です。");
            }

            var parts = new LinkedList<string>();
            var isAbsolutePath = _path.StartsWith("/");
            IList<string> partList = _path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

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

            var normalizedPath = String.Join("/", parts);
            VirtualPath result;
            if (isAbsolutePath)
            {
                result = new VirtualPath("/" + normalizedPath);
            }
            else
            {
                result = new VirtualPath(normalizedPath);
            }

            return result;
        }

        public VirtualPath GetDirectoryPath()
        {
            // パスが '/' で始まっていない場合、それは相対パスなのでそのまま返す
            if (!_path.StartsWith("/"))
            {
                return this;
            }

            int lastSlashIndex = _path.LastIndexOf('/');
            // '/' が見つからない場合は、ルートディレクトリを示す '/' を返す
            if (lastSlashIndex <= 0)
            {
                return new VirtualPath("/");
            }
            else
            {
                // フルパスから最後の '/' までの部分を抜き出して返す
                return new VirtualPath(_path.Substring(0, lastSlashIndex));
            }
        }
        
        public VirtualPath GetNodeName()
        {
            if (_path == "/")
            {
                //　ルートの場合は、ルートディレクトリを示す '/' を返す
                return new VirtualPath("/");
            }

            int lastSlashIndex = _path.LastIndexOf('/');
            if (lastSlashIndex >= 0)
            {
                // フルパスから最後の '/' より後の部分を抜き出して返す
                return new VirtualPath(_path.Substring(lastSlashIndex + 1));
            }
            else
            {
                // '/' が見つからない場合は、そのままのパスを返す
                return this;
            }
        }

        public VirtualPath Combine(params string[] paths)
        {
            // 現在のパスを基点として新しいパスを構築するStringBuilderインスタンスを作成
            var newPathBuilder = new StringBuilder(_path);

            foreach (var path in paths)
            {
                // 区切り文字"/"を無条件で追加
                newPathBuilder.Append("/");
                // 新しいパスコンポーネントを追加
                newPathBuilder.Append(path);
            }

            // StringBuilderの内容を文字列に変換
            var combinedPath = newPathBuilder.ToString();

            // "/"がダブっている箇所を解消
            var normalizedPath = combinedPath.Replace("//", "/");

            // 結果が"/"だったら空文字列に変換
            normalizedPath = (normalizedPath == "/")? string.Empty : normalizedPath;

            // 末尾の "/" を取り除く
            if (normalizedPath.EndsWith("/"))
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
            string parentPath = string.Join("/", parentPathParts);

            // パスが空になった場合は、ルートを返します
            if (string.IsNullOrEmpty(parentPath))
            {
                return new VirtualPath("/");
            }

            return new VirtualPath(parentPath);
        }

        public LinkedList<string> GetPartsLinkedList()
        {
            return new LinkedList<string>(_path.Split('/', StringSplitOptions.RemoveEmptyEntries));
        }

        public List<string> GetPartsList()
        {
            return _path.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

    }

    public static class VirtualPathOld
    {
        public static string NormalizePath(string path)
        {
            if (path == "")
            {
                throw new ArgumentException("パスが空です。");
            }

            var parts = new LinkedList<string>();
            var isAbsolutePath = path.StartsWith("/");
            IList<string> partList = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

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
                        throw new InvalidOperationException("パスがルートディレクトリより上への移動を試みています。無効なパス: " + path);
                    }
                }
                else if (part != ".")
                {
                    parts.AddLast(part);
                }
            }

            var normalizedPath = String.Join("/", parts);
            return isAbsolutePath ? "/" + normalizedPath : normalizedPath;
        }

        public static string GetDirectoryPath(string absolutePath)
        {
            // パスが '/' で始まっていない場合、それは相対パスなのでそのまま返す
            if (!absolutePath.StartsWith("/"))
            {
                return absolutePath;
            }

            int lastSlashIndex = absolutePath.LastIndexOf('/');
            // '/' が見つからない場合は、ルートディレクトリを示す '/' を返す
            if (lastSlashIndex <= 0)
            {
                return "/";
            }
            else
            {
                // フルパスから最後の '/' までの部分を抜き出して返す
                return absolutePath.Substring(0, lastSlashIndex);
            }
        }

        public static string GetNodeName(string absolutePath)
        {
            if (absolutePath == "/")
            {
                //　ルートの場合は、ルートディレクトリを示す '/' を返す
                return "/";
            }

            int lastSlashIndex = absolutePath.LastIndexOf('/');
            if (lastSlashIndex >= 0)
            {
                // フルパスから最後の '/' より後の部分を抜き出して返す
                return absolutePath.Substring(lastSlashIndex + 1);
            }
            else
            {
                // '/' が見つからない場合は、そのままのパスを返す
                return absolutePath;
            }
        }

        public static string Combine(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1) && string.IsNullOrEmpty(path2))
            {
                return "/";
            }

            if (string.IsNullOrEmpty(path1))
            {
                return path2;
            }

            if (string.IsNullOrEmpty(path2))
            {
                return path1;
            }

            string combinedPath = path1.TrimEnd('/') + "/" + path2.TrimStart('/');
            return combinedPath;
        }

        public static string GetParentPath(string path)
        {
            // パスの最後の '/' を取り除きます
            string trimmedPath = path.TrimEnd('/');
            // パスを '/' で分割します
            string[] pathParts = trimmedPath.Split('/');
            // 最後の部分を除去します
            string[] parentPathParts = pathParts.Take(pathParts.Length - 1).ToArray();
            // パスを再構築します
            string parentPath = string.Join("/", parentPathParts);

            // パスが空になった場合は、ルートを返します
            if (string.IsNullOrEmpty(parentPath))
            {
                return "/";
            }

            return parentPath;
        }
    }

    public abstract class VirtualNode : IDeepCloneable<VirtualNode>
    {
        public string Name { get; set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }

        public abstract VirtualNode DeepClone();

        protected VirtualNode(string name)
        {
            Name = name;
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }

        protected VirtualNode(string name, DateTime createdDate, DateTime updatedDate)
        {
            Name = name;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }
    }

    public class VirtualSymbolicLink : VirtualNode
    {
        public string TargetPath { get; set; }

        public VirtualSymbolicLink(string name, string targetPath) : base(name)
        {
            TargetPath = targetPath;
        }

        public VirtualSymbolicLink(string name, string targetPath, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
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

        public VirtualItem(string name, T item) : base(name)
        {
            Item = item;
        }

        public VirtualItem(string name, T item, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
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
        private Dictionary<string, VirtualNode> _nodes;

        public int Count => _nodes.Count;

        public int DirectoryCount => _nodes.Values.OfType<VirtualDirectory>().Count();

        public int ItemCount => _nodes.Values.Count(n => !(n is VirtualDirectory));

        public IEnumerable<string> NodeNames => _nodes.Keys;

        public IEnumerable<VirtualNode> Nodes => _nodes.Values;

        public bool NodeExists(string name) => _nodes.ContainsKey(name);

        public bool DirectoryExists(string name)
        {
            if (!NodeExists(name))
            {
                return false;
            }

            return _nodes[name] is VirtualDirectory;
        }

        public VirtualDirectory(string name) : base(name)
        {
            _nodes = new Dictionary<string, VirtualNode>();
        }

        public VirtualDirectory(string name, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            _nodes = new Dictionary<string, VirtualNode>();
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
            string key = node.Name;

            if (_nodes.ContainsKey(key) && !allowOverwrite)
            {
                throw new InvalidOperationException($"ノード '{key}' は既に存在します。上書きは許可されていません。");
            }

            _nodes[key] = node;
        }

        public void AddItem<T>(string name, T item, bool allowOverwrite = false)
        {
            Add(new VirtualItem<T>(name, item), allowOverwrite);
        }

        public void AddSymbolicLink(string name, string targetPath, bool allowOverwrite = false)
        {
            Add(new VirtualSymbolicLink(name, targetPath), allowOverwrite);
        }

        public void AddDirectory(string name, bool allowOverwrite = false)
        {
            Add(new VirtualDirectory(name), allowOverwrite);
        }

        public VirtualNode this[string name]
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

        public void Remove(string name, bool forceRemove = false)
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

        public VirtualNode Get(string name)
        {
            if (!NodeExists(name))
            {
                throw new VirtualNodeNotFoundException($"指定されたノード '{name}' は存在しません。");
            }
            return _nodes[name];
        }

        public void Rename(string oldName, string newName)
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

        public string CurrentPath { get; private set; }

        public VirtualStorage()
        {
            _root = new VirtualDirectory("/");
            CurrentPath = "/";
        }

        public void ChangeDirectory(string path)
        {
            string absolutePath = ConvertToAbsolutePath(path);

            // Check if the path exists
            if (!NodeExists(absolutePath))
            {
                throw new VirtualNodeNotFoundException($"ディレクトリ '{absolutePath}' は存在しません。");
            }

            // Change the current path
            CurrentPath = absolutePath;
        }

        public string ConvertToAbsolutePath(string relativePath, string? basePath = null)
        {
            // relativePathが空文字列の場合、ArgumentExceptionをスロー
            if (relativePath == "")
            {
                throw new ArgumentException("relativePathが空です。", nameof(relativePath));
            }

            // relativePathが既に絶対パスである場合は、そのまま使用
            if (relativePath.StartsWith("/"))
            {
                return relativePath;
            }

            // basePathが空文字列の場合、ArgumentExceptionをスロー
            if (basePath == "")
            {
                throw new ArgumentException("basePathが空です。", nameof(basePath));
            }

            // basePathがnullまたは空文字列でない場合はその値を使用し、そうでなければCurrentPathを使用
            string effectiveBasePath = basePath ?? CurrentPath;

            // relativePathをeffectiveBasePathに基づいて絶対パスに変換
            var absolutePath = VirtualPathOld.Combine(effectiveBasePath, relativePath);

            return absolutePath;
        }

        public void AddSymbolicLink(string linkPath, string targetPath, bool overwrite = false)
        {
            var absoluteLinkPath = ConvertToAbsolutePath(linkPath);

            // シンボリックリンクの存在確認
            bool linkExists = SymbolicLinkExists(absoluteLinkPath);
            var node = TryGetNode(absoluteLinkPath);

            // 上書きフラグがfalseで、リンクパスに既にノードが存在する場合はエラー
            if (!overwrite && node != null)
            {
                throw new InvalidOperationException($"パス '{absoluteLinkPath}' は既にノードが存在しており、上書きは許可されていません。");
            }

            // 上書きフラグがtrueでも、存在するノードがシンボリックリンク以外の場合はエラー
            if (overwrite && node != null && !linkExists)
            {
                throw new InvalidOperationException($"パス '{absoluteLinkPath}' には上書きできないノードが存在します（シンボリックリンク以外）。");
            }

            // シンボリックリンクの作成または上書き
            var symbolicLink = new VirtualSymbolicLink(Path.GetFileName(absoluteLinkPath), targetPath);
            var parentPath = VirtualPathOld.GetParentPath(absoluteLinkPath);
            var parentNode = TryGetNode(parentPath) as VirtualDirectory;

            if (parentNode == null)
            {
                throw new VirtualNodeNotFoundException($"親ディレクトリ '{parentPath}' が見つかりません。");
            }

            parentNode.Add(symbolicLink, overwrite);
        }

        public void AddItem<T>(string path, T item, bool overwrite = false)
        {
            // 絶対パスに変換
            string absolutePath = ConvertToAbsolutePath(path);

            // ディレクトリパスとアイテム名を分離
            string directoryPath = VirtualPathOld.GetDirectoryPath(absolutePath);
            string itemName = VirtualPathOld.GetNodeName(absolutePath);

            // 対象ディレクトリの存在チェック
            if (!DirectoryExists(directoryPath))
            {
                throw new VirtualNodeNotFoundException($"ディレクトリ '{directoryPath}' が見つかりません。");
            }

            // 対象ディレクトリを取得
            VirtualDirectory directory = GetDirectory(directoryPath);

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
                    if (!ItemExists(VirtualPathOld.Combine(directoryPath, itemName)))
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

        public void AddDirectory(string path, bool createSubdirectories = false)
        {
            string absolutePath = ConvertToAbsolutePath(path);
            string[] nodeNameList = absolutePath.Split('/');
            VirtualDirectory directory = _root;

            for (int i = 0; i < nodeNameList.Length; i++)
            {
                string nodeName = nodeNameList[i];

                if (nodeName != "")
                {
                    if (!directory.DirectoryExists(nodeName))
                    {
                        if (createSubdirectories || i == nodeNameList.Length - 1)
                        {
                            directory.AddDirectory(nodeName);
                        }
                        else
                        {
                            throw new Exception($"ディレクトリ {nodeName} は存在しません。");
                        }
                    }

                    directory = (VirtualDirectory)directory.Get(nodeName);
                }
            }
        }

        public NodeResolutionResult GetNodeInternal(string path, bool followLinks)
        {
            LinkedList<VirtualNode> nodeLinkedList = new LinkedList<VirtualNode>();
            VirtualPath absolutePath = new VirtualPath(ConvertToAbsolutePath(path));

            if (absolutePath.Path == "/")
            {
                return new NodeResolutionResult(_root, "/");
            }

            List<string> nodeNameList = absolutePath.GetPartsList();
            nodeLinkedList.AddLast(_root);
            int index = 0;
            VirtualPath currentPath = new VirtualPath(""); // 現在のパスを追跡
            VirtualPath resolvedPath = new VirtualPath("/"); // 解決後のフルパスを組み立てるための変数

            while (index < nodeNameList.Count)
            {
                string nodeName = nodeNameList[index];

                if (nodeName == ".")
                {
                    // 現在のディレクトリを示す場合、何もせず次のノードへ
                }
                else if (nodeName == "..")
                {
                    // 親ディレクトリを示す場合、現在のディレクトリを一つ上のディレクトリに変更
                    currentPath = currentPath.GetParentPath();
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
                        break; // ディレクトリでない場合、この時点で処理を終了
                    }

                    if (followLinks && nodeLinkedList.Last.Value is VirtualSymbolicLink symlink)
                    {
                        VirtualPath symlinkTargetPath = new VirtualPath(ConvertToAbsolutePath(symlink.TargetPath, currentPath.Path));
                        List<string> targetPathList = symlinkTargetPath.GetPartsList();
                        nodeNameList = targetPathList.Concat(nodeNameList.Skip(index + 1)).ToList();
                        index = -1; // indexをリセットし、次のループで0から開始
                        nodeLinkedList.Clear(); // ノードリストをリセット
                        nodeLinkedList.AddLast(_root); // ルートノードを追加
                        currentPath = new VirtualPath(""); // 現在のパスもリセット
                        resolvedPath = symlinkTargetPath; // 解決後のパスを更新
                    }
                    else
                    {
                        currentPath = currentPath.Combine(nodeName);
                        if (index == nodeNameList.Count - 1)
                        {
                            // ノードリストの最後の要素でシンボリックリンクでない場合、resolvedPathを更新
                            resolvedPath = currentPath;
                        }
                    }
                }

                index++;
            }

            return new NodeResolutionResult(nodeLinkedList.Last!.Value, resolvedPath.Path);
        }

        public VirtualNode GetNode(string path, bool followLinks = false)
        {
            NodeResolutionResult nodeResolutionResult = GetNodeInternal(path, followLinks);
            return nodeResolutionResult.Node;
        }

        public string GetLinkPath(string path)
        {
            NodeResolutionResult nodeResolutionResult = GetNodeInternal(path, true);
            return nodeResolutionResult.ResolvedPath;
        }

        public VirtualDirectory GetDirectory(string path)
        {
            string absolutePath = ConvertToAbsolutePath(path);
            VirtualNode node = GetNode(absolutePath);

            if (node is VirtualDirectory directory)
            {
                return directory;
            }
            else
            {
                throw new VirtualNodeNotFoundException($"ディレクトリ {absolutePath} は存在しません。");
            }
        }

        private IEnumerable<T> GetNodesInternal<T>(string basePath, VirtualNodeType nodeType, bool recursive, Func<VirtualNode, string, T> selector)
        {
            if (basePath == "")
            {
                throw new ArgumentException("パスが空です。");
            }
            if (!basePath.StartsWith("/"))
            {
                throw new ArgumentException($"絶対パスを指定してください。{basePath}");
            }

            var directory = (VirtualDirectory)GetNode(basePath);

            foreach (var node in directory.Nodes)
            {
                if (node is VirtualDirectory subdirectory)
                {
                    if (nodeType == VirtualNodeType.All || nodeType == VirtualNodeType.Directory)
                    {
                        yield return selector(subdirectory, VirtualPathOld.Combine(basePath, subdirectory.Name));
                    }

                    if (recursive)
                    {
                        var subdirectoryPath = VirtualPathOld.Combine(basePath, subdirectory.Name);
                        foreach (var subNode in GetNodesInternal(subdirectoryPath, nodeType, recursive, selector))
                        {
                            yield return subNode;
                        }
                    }
                }
                else if (nodeType == VirtualNodeType.All || nodeType == VirtualNodeType.Item)
                {
                    yield return selector(node, VirtualPathOld.Combine(basePath, node.Name));
                }
            }
        }

        public IEnumerable<VirtualNode> GetNodes(string basePath, VirtualNodeType nodeType = VirtualNodeType.All, bool recursive = false)
        {
            return GetNodesInternal(basePath, nodeType, recursive, (node, path) => node);
        }

        public IEnumerable<VirtualNode> GetNodes(VirtualNodeType nodeType = VirtualNodeType.All, bool recursive = false)
        {
            return GetNodesInternal(CurrentPath, nodeType, recursive, (node, path) => node);
        }

        public IEnumerable<string> GetNodesWithPaths(string basePath, VirtualNodeType nodeType = VirtualNodeType.All, bool recursive = false)
        {
            return GetNodesInternal(basePath, nodeType, recursive, (node, path) => path);
        }

        public IEnumerable<string> GetNodesWithPaths(VirtualNodeType nodeType = VirtualNodeType.All, bool recursive = false)
        {
            return GetNodesInternal(CurrentPath, nodeType, recursive, (node, path) => path);
        }

        private void CheckCopyPreconditions(string sourcePath, string destinationPath, bool overwrite, bool recursive)
        {
            string absoluteSourcePath = ConvertToAbsolutePath(sourcePath);
            string absoluteDestinationPath = ConvertToAbsolutePath(destinationPath);

            // ルートディレクトリのコピーを禁止
            if (absoluteSourcePath == "/")
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
            if (absoluteDestinationPath.StartsWith(absoluteSourcePath + "/") || absoluteSourcePath.StartsWith(absoluteDestinationPath + "/"))
            {
                throw new InvalidOperationException("コピー元またはコピー先が互いのサブディレクトリになっています。");
            }

            bool destinationIsDirectory = DirectoryExists(absoluteDestinationPath) || absoluteDestinationPath.EndsWith("/");
            string targetDirectoryPath, newNodeName;
            if (destinationIsDirectory)
            {
                targetDirectoryPath = absoluteDestinationPath;
                newNodeName = VirtualPathOld.GetNodeName(absoluteSourcePath);
            }
            else
            {
                targetDirectoryPath = VirtualPathOld.GetParentPath(absoluteDestinationPath);
                newNodeName = VirtualPathOld.GetNodeName(absoluteDestinationPath);
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
                    string newSubSourcePath = VirtualPathOld.Combine(absoluteSourcePath, subNode.Name);
                    string newSubDestinationPath = VirtualPathOld.Combine(absoluteDestinationPath, subNode.Name);
                    CheckCopyPreconditions(newSubSourcePath, newSubDestinationPath, overwrite, true);
                }
            }
        }

        public void CopyNode(string sourcePath, string destinationPath, bool recursive = false, bool overwrite = false)
        {
            // コピー前の事前条件チェック
            CheckCopyPreconditions(sourcePath, destinationPath, overwrite, recursive);

            string absoluteSourcePath = ConvertToAbsolutePath(sourcePath);
            string absoluteDestinationPath = ConvertToAbsolutePath(destinationPath);

            VirtualNode sourceNode = GetNode(absoluteSourcePath);

            bool destinationIsDirectory = DirectoryExists(absoluteDestinationPath) || absoluteDestinationPath.EndsWith("/");

            string targetDirectoryPath, newNodeName;
            if (destinationIsDirectory)
            {
                targetDirectoryPath = absoluteDestinationPath;
                newNodeName = VirtualPathOld.GetNodeName(absoluteSourcePath);
            }
            else
            {
                targetDirectoryPath = VirtualPathOld.GetParentPath(absoluteDestinationPath);
                newNodeName = VirtualPathOld.GetNodeName(absoluteDestinationPath);
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
                        string intermediatePath = VirtualPathOld.Combine(targetDirectoryPath, newNodeName);
                        string newDestinationPath = VirtualPathOld.Combine(intermediatePath, node.Name);
                        CopyNode(VirtualPathOld.Combine(absoluteSourcePath, node.Name), newDestinationPath, true, overwrite);
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

        public void RemoveNode(string path, bool recursive = false)
        {
            string absolutePath = ConvertToAbsolutePath(path);

            if (absolutePath == "/")
            {
                throw new InvalidOperationException("ルートディレクトリを削除することはできません。");
            }

            VirtualNode node = GetNode(absolutePath);

            // ディレクトリを親ディレクトリから削除するための共通の親パスと親ディレクトリを取得
            string parentPath = VirtualPathOld.GetParentPath(absolutePath);
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
                    string subPath = VirtualPathOld.Combine(absolutePath, subNode.Name);
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

        public VirtualNode? TryGetNode(string path)
        {
            string absolutePath = ConvertToAbsolutePath(path);
            try
            {
                // GetNodeメソッドは、ノードが見つからない場合にnullを返すか、例外をスローするように実装されていると仮定
                return GetNode(absolutePath);
            }
            catch (VirtualNodeNotFoundException)
            {
                return null; // ノードが存在しない場合はnullを返す
            }
        }

        public bool NodeExists(string path)
        {
            var node = TryGetNode(path);
            return node != null; // ノードがnullでなければ、存在すると判断
        }

        public bool DirectoryExists(string path)
        {
            var node = TryGetNode(path);
            return node is VirtualDirectory; // ノードがVirtualDirectoryのインスタンスであれば、ディレクトリが存在すると判断
        }

        public bool ItemExists(string path)
        {
            var node = TryGetNode(path);
            if (node == null) return false; // ノードがnullなら、アイテムは存在しない
            var nodeType = node.GetType();
            return nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeof(VirtualItem<>); // ジェネリック型がVirtualItem<T>であるかチェック
        }

        public bool SymbolicLinkExists(string path)
        {
            var node = TryGetNode(path);
            return node is VirtualSymbolicLink;
        }

        public void MoveNode(string sourcePath, string destinationPath, bool overwrite = false)
        {
            string absoluteSourcePath = ConvertToAbsolutePath(sourcePath);
            string absoluteDestinationPath = ConvertToAbsolutePath(destinationPath);

            // 循環参照チェック
            if (absoluteDestinationPath.StartsWith(absoluteSourcePath + "/"))
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
            if (absoluteSourcePath == "/")
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
                    VirtualDirectory sourceParentDirectory = GetDirectory(VirtualPathOld.GetParentPath(absoluteSourcePath));
                    sourceParentDirectory.Remove(sourceDirectory.Name);
                }
                else
                {
                    // ディレクトリでない、または存在しない場合

                    // 移動先の存在チェック
                    if (!NodeExists(absoluteDestinationPath))
                    {
                        string destinationParentPath = VirtualPathOld.GetParentPath(absoluteDestinationPath);
                        if (!DirectoryExists(destinationParentPath))
                        {
                            // 移動先の親ディレクトリが存在しない場合
                            throw new VirtualNodeNotFoundException($"指定されたノード '{destinationParentPath}' は存在しません。");
                        }
                        VirtualDirectory destinationParentDirectory = GetDirectory(destinationParentPath);
                        string destinationNodeName = VirtualPathOld.GetNodeName(absoluteDestinationPath);
                        VirtualDirectory sourceDirectory = GetDirectory(absoluteSourcePath);

                        string oldNodeName = sourceDirectory.Name;
                        sourceDirectory.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceDirectory);
                        VirtualDirectory sourceParentDirectory = GetDirectory(VirtualPathOld.GetParentPath(absoluteSourcePath));
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
                    VirtualDirectory sourceParentDirectory = GetDirectory(VirtualPathOld.GetParentPath(absoluteSourcePath));
                    sourceParentDirectory.Remove(sourceNode.Name);
                }
                else
                {
                    // 存在しない

                    // 移動先の存在チェック
                    if (!NodeExists(absoluteDestinationPath))
                    {
                        string destinationParentPath = VirtualPathOld.GetParentPath(absoluteDestinationPath);
                        if (!DirectoryExists(destinationParentPath))
                        {
                            // 移動先の親ディレクトリが存在しない場合
                            throw new VirtualNodeNotFoundException($"指定されたノード '{destinationParentPath}' は存在しません。");
                        }
                        VirtualDirectory destinationParentDirectory = GetDirectory(destinationParentPath);
                        string destinationNodeName = VirtualPathOld.GetNodeName(absoluteDestinationPath);
                        VirtualNode sourceNode = GetNode(absoluteSourcePath);

                        string oldNodeName = sourceNode.Name;
                        sourceNode.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceNode);
                        VirtualDirectory sourceParentDirectory = GetDirectory(VirtualPathOld.GetParentPath(absoluteSourcePath));
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                    else
                    {
                        // 存在する場合

                        VirtualDirectory destinationParentDirectory = GetDirectory(VirtualPathOld.GetParentPath(absoluteDestinationPath));
                        string destinationNodeName = VirtualPathOld.GetNodeName(absoluteDestinationPath);
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

                        string oldNodeName = sourceNode.Name;
                        sourceNode.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceNode);
                        VirtualDirectory sourceParentDirectory = GetDirectory(VirtualPathOld.GetParentPath(absoluteSourcePath));
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                }
            }
        }
    }
}
