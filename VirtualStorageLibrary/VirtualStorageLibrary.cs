﻿namespace VirtualStorageLibrary
{
    public interface IDeepCloneable<T>
    {
        T DeepClone();
    }

    public static class VirtualPath
    {
        public static string GetDirectoryPath(string path)
        {
            // パスが '/' で始まっていない場合、それは相対パスなのでそのまま返す
            if (!path.StartsWith("/"))
            {
                return path;
            }

            int lastSlashIndex = path.LastIndexOf('/');
            // '/' が見つからない場合は、ルートディレクトリを示す '/' を返す
            if (lastSlashIndex <= 0)
            {
                return "/";
            }
            else
            {
                // フルパスから最後の '/' までの部分を抜き出して返す
                return path.Substring(0, lastSlashIndex);
            }
        }

        public static string GetNodeName(string path)
        {
            if (path == "/")
            {
                //　ルートの場合は、ルートディレクトリを示す '/' を返す
                return "/";
            }

            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex >= 0)
            {
                // フルパスから最後の '/' より後の部分を抜き出して返す
                return path.Substring(lastSlashIndex + 1);
            }
            else
            {
                // '/' が見つからない場合は、そのままのパスを返す
                return path;
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
                if (!NodeExists(key))
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

        public void Remove(string name, bool forceRemove = false)
        {
            if (!NodeExists(name))
            {
                if (!forceRemove)
                {
                    throw new KeyNotFoundException($"ノード '{name}' は存在しません。");
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
                throw new KeyNotFoundException($"指定されたノード '{name}' は存在しません。");
            }
            return _nodes[name];
        }

        public void Rename(string oldName, string newName)
        {
            if (!NodeExists(oldName))
            {
                throw new KeyNotFoundException($"ノード '{oldName}' は存在しません。");
            }

            if (NodeExists(newName))
            {
                throw new InvalidOperationException($"ノード '{newName}' は既に存在します。");
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
            if (!DirectoryExists(absolutePath))
            {
                throw new DirectoryNotFoundException($"ディレクトリ '{absolutePath}' は存在しません。");
            }

            // Change the current path
            CurrentPath = absolutePath;
        }

        public string ConvertToAbsolutePath(string relativePath)
        {
            if (relativePath.StartsWith("/"))
            {
                return relativePath;
            }
            else
            {
                string[] currentPathParts = CurrentPath.Split('/');
                string[] relativePathParts = relativePath.Split('/');

                var newPathParts = new LinkedList<string>(currentPathParts);
                foreach (var part in relativePathParts)
                {
                    if (part == ".")
                    {
                        // Do nothing, '.' refers to the current directory
                    }
                    else if (part == "..")
                    {
                        // '..' refers to the parent directory, so remove the last part
                        if (newPathParts.Count > 0)
                        {
                            newPathParts.RemoveLast();
                        }
                    }
                    else
                    {
                        newPathParts.AddLast(part);
                    }
                }

                return string.Join("/", newPathParts);
            }
        }

        public bool DirectoryExists(string path)
        {
            string absolutePath = ConvertToAbsolutePath(path);
            string[] nodeNameList = absolutePath.Split('/');
            VirtualDirectory directory = _root;

            foreach (var nodeName in nodeNameList)
            {
                if (directory.DirectoryExists(nodeName))
                {
                    directory = (VirtualDirectory)directory.Get(nodeName);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public void MakeDirectory(string path, bool createSubdirectories = false)
        {
            string absolutePath = ConvertToAbsolutePath(path);
            string[] nodeNameList = absolutePath.Split('/');
            VirtualDirectory directory = _root;

            foreach (var nodeName in nodeNameList)
            {
                if (!directory.DirectoryExists(nodeName))
                {
                    if (createSubdirectories)
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

        public VirtualDirectory GetDirectory(string path)
        {
            // ルートディレクトリが要求された場合、すぐにそれを返します
            if (path == "/")
            {
                return _root;
            }

            string absolutePath = ConvertToAbsolutePath(path);
            string[] directoryNameList = absolutePath.Split('/');
            VirtualDirectory directory = _root;

            foreach (var directoryName in directoryNameList)
            {
                if (!directory.DirectoryExists(directoryName))
                {
                    throw new Exception($"ディレクトリ {directoryName} は存在しません。");
                }

                directory = (VirtualDirectory)directory.Get(directoryName);
            }

            return directory;
        }

        public void RemoveDirectory(string path, bool forceDelete = false)
        {
            VirtualDirectory directory = GetDirectory(path);

            if (!forceDelete && directory.Count > 0)
            {
                throw new InvalidOperationException("ディレクトリが空ではありません。削除するには強制削除フラグを設定してください。");
            }

            string parentPath = VirtualPath.GetParentPath(path);
            VirtualDirectory parentDirectory = GetDirectory(parentPath);

            parentDirectory.Remove(directory.Name);
        }
    }
}
