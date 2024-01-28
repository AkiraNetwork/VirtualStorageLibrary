namespace VirtualStorageLibrary
{
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

    public static class VirtualPath
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
            path = NormalizePath(path);

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
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public abstract VirtualNode DeepClone();
        protected VirtualNode(string name)
        {
            Name = name;
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
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

        public override VirtualNode DeepClone()
        {
            var clonedDirectory = new VirtualDirectory(this.Name)
            {
                CreatedDate = this.CreatedDate,
                UpdatedDate = this.UpdatedDate,
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

        public string ConvertToAbsolutePath(string relativePath)
        {
            if (relativePath == "")
            {
                throw new ArgumentException("パスが空です。");
            }

            if (relativePath.StartsWith("/"))
            {
                return VirtualPath.NormalizePath(relativePath);
            }

            var combinedPath = VirtualPath.Combine(CurrentPath, relativePath);
            return VirtualPath.NormalizePath(combinedPath);
        }

        public void MakeDirectory(string path, bool createSubdirectories = false)
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
                            directory.Add(new VirtualDirectory(nodeName));
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

        public VirtualNode GetNode(string path)
        {
            string absolutePath = ConvertToAbsolutePath(path);

            if (absolutePath == "/")
            {
                return _root;
            }

            string[] nodeNameList = absolutePath.Split('/');
            VirtualNode node = _root;

            foreach (var nodeName in nodeNameList)
            {
                if (nodeName != "")
                {
                    if (node is VirtualDirectory directory)
                    {
                        node = directory.Get(nodeName);
                    }
                }
            }

            return node;
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

        public void AddItem<T>(VirtualItem<T> item, string path = ".") where T : IDeepCloneable<T>
        {
            var absolutePath = ConvertToAbsolutePath(path);
            var directory = (VirtualDirectory)GetNode(absolutePath);

            directory.Add(item);
        }

        private IEnumerable<T> EnumerateNodesRecursively<T>(string basePath, bool includeItems, Func<VirtualNode, string, T> selector)
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
                    yield return selector(subdirectory, VirtualPath.Combine(basePath, subdirectory.Name));

                    var subdirectoryPath = VirtualPath.Combine(basePath, subdirectory.Name);
                    foreach (var subNode in EnumerateNodesRecursively(subdirectoryPath, includeItems, selector))
                    {
                        yield return subNode;
                    }
                }
                else if (includeItems)
                {
                    yield return selector(node, VirtualPath.Combine(basePath, node.Name));
                }
            }
        }

        public IEnumerable<VirtualNode> EnumerateNodesRecursively(string basePath, bool includeItems = true)
        {
            return EnumerateNodesRecursively(basePath, includeItems, (node, path) => node);
        }

        public IEnumerable<string> EnumerateNodeNamesRecursively(string basePath, bool includeItems = true)
        {
            return EnumerateNodesRecursively(basePath, includeItems, (node, path) => path);
        }

        public void CopyNode(string sourcePath, string destinationPath, bool recursive = false, bool overwrite = false)
        {
            // TODO: コピー失敗時のロールバックをどうするか検討する。
            string absoluteSourcePath = ConvertToAbsolutePath(sourcePath);
            string absoluteDestinationPath = ConvertToAbsolutePath(destinationPath);

            // ルートディレクトリのコピーを禁止
            if (absoluteSourcePath == "/")
            {
                throw new InvalidOperationException("ルートディレクトリのコピーは禁止されています。");
            }
            
            // 循環参照チェック
            if (absoluteDestinationPath.StartsWith(absoluteSourcePath + "/") || absoluteSourcePath.StartsWith(absoluteDestinationPath + "/"))
            {
                throw new InvalidOperationException("コピー元またはコピー先が互いのサブディレクトリになっています。");
            }

            // コピー元とコピー先が同じ場合は例外をスロー
            if (absoluteSourcePath == absoluteDestinationPath)
            {
                throw new InvalidOperationException("コピー元とコピー先が同じです。");
            }

            // コピー元ノードが存在しない場合は例外をスロー
            if (!NodeExists(absoluteSourcePath))
            {
                throw new VirtualNodeNotFoundException($"コピー元ノード '{absoluteSourcePath}' は存在しません。");
            }

            VirtualNode sourceNode = GetNode(absoluteSourcePath);

            bool destinationIsDirectory = DirectoryExists(absoluteDestinationPath) || absoluteDestinationPath.EndsWith("/");

            string targetDirectoryPath, newNodeName;
            if (destinationIsDirectory)
            {
                targetDirectoryPath = absoluteDestinationPath;
                newNodeName = VirtualPath.GetNodeName(absoluteSourcePath);
            }
            else
            {
                targetDirectoryPath = VirtualPath.GetParentPath(absoluteDestinationPath);
                newNodeName = VirtualPath.GetNodeName(absoluteDestinationPath);
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
                        string intermediatePath = VirtualPath.Combine(targetDirectoryPath, newNodeName);
                        string newDestinationPath = VirtualPath.Combine(intermediatePath, node.Name);
                        CopyNode(VirtualPath.Combine(absoluteSourcePath, node.Name), newDestinationPath, true, overwrite);
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
            string parentPath = VirtualPath.GetParentPath(absolutePath);
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
                    string subPath = VirtualPath.Combine(absolutePath, subNode.Name);
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

        public void MoveNode(string sourcePath, string destinationPath, bool overwrite = false)
        {
            string absoluteSourcePath = ConvertToAbsolutePath(sourcePath);
            string absoluteDestinationPath = ConvertToAbsolutePath(destinationPath);

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
                    VirtualDirectory sourceParentDirectory = GetDirectory(VirtualPath.GetParentPath(absoluteSourcePath));
                    sourceParentDirectory.Remove(sourceDirectory.Name);
                }
                else
                {
                    // ディレクトリでない、または存在しない場合

                    // 移動先の存在チェック
                    if(!NodeExists(absoluteDestinationPath))
                    {
                        // 存在しない場合
                        throw new VirtualNodeNotFoundException($"指定されたノード '{absoluteDestinationPath}' は存在しません。");
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
                    VirtualDirectory sourceParentDirectory = GetDirectory(VirtualPath.GetParentPath(absoluteSourcePath));
                    sourceParentDirectory.Remove(sourceNode.Name);
                }
                else
                {
                    // 存在しない

                    // 移動先の存在チェック
                    if (!NodeExists(absoluteDestinationPath))
                    {
                        // 存在しない場合
                        throw new VirtualNodeNotFoundException($"指定されたノード '{absoluteDestinationPath}' は存在しません。");
                    }
                    else
                    {
                        // 存在する場合

                        VirtualDirectory destinationParentDirectory = GetDirectory(VirtualPath.GetParentPath(absoluteDestinationPath));
                        string destinationNodeName = VirtualPath.GetNodeName(absoluteDestinationPath);
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
                        VirtualDirectory sourceParentDirectory = GetDirectory(VirtualPath.GetParentPath(absoluteSourcePath));
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                }
            }
        }
    }
}
