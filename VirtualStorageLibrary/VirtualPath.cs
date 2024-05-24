using System.Diagnostics;
using System.Text;

namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualPath : IEquatable<VirtualPath>, IComparable<VirtualPath>, IComparable
    {
        private readonly string _path;

        private VirtualPath? _directoryPath;

        private VirtualNodeName? _nodeName;

        private static readonly string _root;
        
        private static readonly string _dot;

        private static readonly string _dotDot;

        public string Path => _path;

        private List<VirtualNodeName>? _partsList;

        [DebuggerStepThrough]
        public static implicit operator VirtualPath(string path)
        {
            return new VirtualPath(path);
        }

        [DebuggerStepThrough]
        public static implicit operator string(VirtualPath? virtualPath)
        {
            if (virtualPath == null)
            {
                return string.Empty;
            }

            return virtualPath._path;
        }

        public VirtualPath DirectoryPath
        {
            [DebuggerStepThrough]
            get
            {
                if (_directoryPath == null)
                {
                    _directoryPath = new VirtualPath(GetDirectoryPath());
                }
                return _directoryPath;
            }
        }

        public VirtualNodeName NodeName
        {
            [DebuggerStepThrough]
            get
            {
                if (_nodeName == null)
                {
                    _nodeName = GetNodeName();
                }
                return _nodeName;
            }
        }

        public List<VirtualNodeName> PartsList
        {
            [DebuggerStepThrough]
            get
            {
                if (_partsList == null)
                {
                    _partsList = GetPartsList();
                }
                return _partsList;
            }
        }

        public int Depth
        {
            [DebuggerStepThrough]
            get
            {
                // パスの深さを返す
                return this.PartsList.Count;
            }
        }

        public static char Separator
        {
            [DebuggerStepThrough]
            get => VirtualStorageSettings.Settings.PathSeparator;
        }
        
        public static string Root
        {
            [DebuggerStepThrough]
            get => _root;
        }

        public static string Dot
        {
            [DebuggerStepThrough]
            get => _dot;
        }

        public static string DotDot
        {
            [DebuggerStepThrough]
            get => _dotDot;
        }

        [DebuggerStepThrough]
        public override string ToString() => _path;

        public bool IsEmpty
        {
            [DebuggerStepThrough]
            get => _path == string.Empty;
        }

        public bool IsRoot
        {
            [DebuggerStepThrough]
            get => _path == Separator.ToString();
        }

        public bool IsAbsolute
        {
            [DebuggerStepThrough]
            get => _path.StartsWith(Separator);
        }

        public bool IsEndsWithSlash
        {
            [DebuggerStepThrough]
            get => _path.EndsWith(Separator);
        }

        public bool IsDot
        {
            [DebuggerStepThrough]
            get => _path == Dot;
        }

        public bool IsDotDot
        {
            [DebuggerStepThrough]
            get => _path == DotDot;
        }

        [DebuggerStepThrough]
        public override int GetHashCode() => _path.GetHashCode();

        [DebuggerStepThrough]
        public VirtualPath(string path)
        {
            _path = path;
        }

        [DebuggerStepThrough]
        public VirtualPath(IEnumerable<VirtualNodeName> parts)
        {
            _path = Separator + string.Join(Separator, parts.Select(node => node.Name));
        }

        static VirtualPath()
        {
            _root = VirtualStorageSettings.Settings.PathRoot;
            _dot = VirtualStorageSettings.Settings.PathDot;
            _dotDot = VirtualStorageSettings.Settings.PathDotDot;
        }

        [DebuggerStepThrough]
        public override bool Equals(object? obj)
        {
            if (obj is VirtualPath other)
            {
                return _path == other._path;
            }
            return false;
        }

        [DebuggerStepThrough]
        public bool Equals(VirtualPath? other)
        {
            return _path == other?._path;
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public static bool operator !=(VirtualPath? left, VirtualPath? right)
        {
            return !(left == right);
        }

        [DebuggerStepThrough]
        public static VirtualPath operator +(VirtualPath path1, VirtualPath path2)
        {
            return path1.Combine(path2);
        }

        [DebuggerStepThrough]
        public static VirtualPath operator +(VirtualPath path, VirtualNodeName nodeName)
        {
            VirtualPath trimmedPath = path.TrimEndSlash();
            string fullPath = trimmedPath._path + Separator + nodeName.Name;
            return new VirtualPath(fullPath);
        }

        [DebuggerStepThrough]
        public static VirtualPath operator +(VirtualPath path, string str)
        {
            // 末尾がパスセパレータでない場合は追加
            if (!path.Path.EndsWith(Separator))
            {
                str = Separator + str;
            }

            // 新しいパスを生成して返す
            return new VirtualPath(path.Path + str);
        }

        [DebuggerStepThrough]
        public static VirtualPath operator +(string str, VirtualPath path)
        {
            // 末尾がパスセパレータでない場合は追加
            if (!str.EndsWith(Separator))
            {
                str += Separator;
            }

            // 新しいパスを生成して返す
            return new VirtualPath(str + path.Path);
        }

        [DebuggerStepThrough]
        public static VirtualPath operator +(VirtualPath path, char chr)
        {
            string str = chr.ToString();

            // 末尾がパスセパレータでない場合は追加
            if (!path.Path.EndsWith(Separator))
            {
                str = Separator + str;
            }

            // 新しいパスを生成して返す
            return new VirtualPath(path.Path + str);
        }

        [DebuggerStepThrough]
        public static VirtualPath operator +(char chr, VirtualPath path)
        {
            string str = chr.ToString();

            // 末尾がパスセパレータでない場合は追加
            if (!str.EndsWith(Separator))
            {
                str += Separator;
            }

            // 新しいパスを生成して返す
            return new VirtualPath(str + path.Path);
        }

        [DebuggerStepThrough]
        public VirtualPath TrimEndSlash()
        {
            if (_path.EndsWith(Separator))
            {
                return new VirtualPath(_path.Substring(0, _path.Length - 1));
            }
            return this;
        }

        [DebuggerStepThrough]
        public VirtualPath AddEndSlash()
        {
            if (!_path.EndsWith(Separator))
            {
                return new VirtualPath(_path + Separator);
            }
            return this;
        }

        [DebuggerStepThrough]
        public VirtualPath AddStartSlash()
        {
            if (!_path.StartsWith(Separator))
            {
                return new VirtualPath(Separator + _path);
            }
            return this;
        }

        [DebuggerStepThrough]
        public bool StartsWith(VirtualPath path)
        {
            return _path.StartsWith(path.Path);
        }

        [DebuggerStepThrough]
        public VirtualPath NormalizePath()
        {
            // string型でパスを正規化する静的メソッドを呼び出す。
            string normalizedPathString = NormalizePath(_path);

            // 正規化されたパス文字列を使用して新しいVirtualPathインスタンスを作成し、返す。
            return new VirtualPath(normalizedPathString);
        }

        [DebuggerStepThrough]
        public static string NormalizePath(string path)
        {
            // パスが空文字列、または PathSeparator の場合はそのまま返す
            if (path == string.Empty || path == Separator.ToString())
            {
                return path;
            }

            LinkedList<string> parts = new();
            bool isAbsolutePath = path.StartsWith(Separator);
            IList<string> partList = path.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

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

            var normalizedPath = string.Join(Separator, parts);
            if (isAbsolutePath)
            {
                normalizedPath = Separator + normalizedPath;
            }

            // 末尾の PathSeparator を取り除く
            if (normalizedPath.Length > 1 && normalizedPath.EndsWith(Separator))
            {
                normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - 1);
            }

            return normalizedPath;
        }

        [DebuggerStepThrough]
        private string GetDirectoryPath()
        {
            // パスが PathSeparator で始まっていない場合、それは相対パスなのでそのまま返す
            if (!_path.StartsWith(Separator))
            {
                return _path;
            }

            int lastSlashIndex = _path.LastIndexOf(Separator);

            // PathSeparator が見つからない場合は、ルートディレクトリを示す PathSeparator を返す
            if (lastSlashIndex <= 0)
            {
                return Separator.ToString();
            }
            else
            {
                // フルパスから最後の PathSeparator までの部分を抜き出して返す
                return _path.Substring(0, lastSlashIndex);
            }
        }

        [DebuggerStepThrough]
        private VirtualNodeName GetNodeName()
        {
            if (_path == VirtualPath.Separator.ToString())
            {
                // ルートディレクトリの場合は、そのままの文字列を返す
                return new VirtualNodeName(VirtualPath.Separator.ToString());
            }

            StringBuilder path = new(_path);

            if (path.Length > 0 && path[^1] == VirtualPath.Separator)
            {
                // 末尾の PathSeparator を取り除く
                path.Remove(path.Length - 1, 1);
            }

            int lastSlashIndex = path.ToString().LastIndexOf(VirtualPath.Separator);
            if (lastSlashIndex < 0)
            {
                // PathSeparator が見つからない場合は、そのままの文字列を返す
                return new VirtualNodeName(_path);
            }
            else
            {
                // 最後の PathSeparator 以降の部分を抜き出して返す
                return new VirtualNodeName(path.ToString().Substring(lastSlashIndex + 1));
            }
        }

        // TODO: VirtualNodeNameを使ってCombineするように変更するかも
        [DebuggerStepThrough]
        public VirtualPath Combine(params VirtualPath[] paths)
        {
            string[] currentPathArray = [_path];
            string[] pathStrings = paths.Select(p => p.Path).ToArray();
            string[] allPaths = currentPathArray.Concat(pathStrings).ToArray();
            string combinedPathString = Combine(allPaths);

            return new VirtualPath(combinedPathString);
        }

        // TODO: VirtualNodeNameを使ってCombineするように変更するかも
        [DebuggerStepThrough]
        public string Combine(params string[] paths)
        {
            // 現在のパスを基点として新しいパスを構築するStringBuilderインスタンスを作成
            StringBuilder newPathBuilder = new();

            foreach (var path in paths)
            {
                // 2番目以降のパスの場合だけ、PathSeparator を追加
                if (newPathBuilder.Length > 0)
                {
                    newPathBuilder.Append(VirtualPath.Separator);
                }

                // 新しいパスコンポーネントを追加
                newPathBuilder.Append(path);
            }

            // StringBuilderの内容を文字列に変換
            var combinedPath = newPathBuilder.ToString();

            // PathSeparator がダブっている箇所を解消
            var normalizedPath = combinedPath.Replace(
                VirtualPath.Separator.ToString() + VirtualPath.Separator.ToString(),
                VirtualPath.Separator.ToString());

            // 結果が PathSeparator だったら空文字列に変換
            normalizedPath = (normalizedPath == VirtualPath.Separator.ToString()) ? string.Empty : normalizedPath;

            // 末尾の PathSeparator を取り除く
            if (normalizedPath.EndsWith(VirtualPath.Separator))
            {
                normalizedPath = normalizedPath.Substring(0, normalizedPath.Length - 1);
            }

            // 結合された文字列を返却
            return normalizedPath;
        }

        [DebuggerStepThrough]
        // TODO: このメソッドいる? DirectoryPathとNodeNameを使えばいいのでは?
        public VirtualPath GetParentPath()
        {
            // パスの最後の PathSeparator を取り除きます
            string trimmedPath = _path.TrimEnd(VirtualPath.Separator);
            // パスを PathSeparator で分割します
            string[] pathParts = trimmedPath.Split(VirtualPath.Separator);
            // 最後の部分を除去します
            string[] parentPathParts = pathParts.Take(pathParts.Length - 1).ToArray();
            // パスを再構築します
            string parentPath = string.Join(VirtualPath.Separator, parentPathParts);

            // パスが空になった場合は、ルートを返します
            if (string.IsNullOrEmpty(parentPath))
            {
                return VirtualPath.Root;
            }

            return new VirtualPath(parentPath);
        }

        [DebuggerStepThrough]
        public LinkedList<VirtualNodeName> GetPartsLinkedList()
        {
            LinkedList<VirtualNodeName> parts = new();
            foreach (var part in _path.Split(VirtualPath.Separator, StringSplitOptions.RemoveEmptyEntries))
            {
                parts.AddLast(new VirtualNodeName(part));
            }

            return parts;
        }

        [DebuggerStepThrough]
        public List<VirtualNodeName> GetPartsList()
        {
            return GetPartsLinkedList().ToList();
        }

        [DebuggerStepThrough]
        public VirtualPath CombineFromIndex(VirtualPath path, int index)
        {
            // 指定されたインデックスからのパスのパーツを取得
            var partsFromIndex = path.PartsList.Skip(index).ToList();

            // 現在のパス（this）と指定されたインデックスからのパーツを結合
            VirtualPath combinedPath = this;
            foreach (var part in partsFromIndex)
            {
                combinedPath = combinedPath + part;
            }

            return combinedPath;
        }

        public int CompareTo(VirtualPath? other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(_path, other._path, StringComparison.Ordinal);
        }

        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is VirtualPath))
            {
                throw new ArgumentException("Object is not a VirtualPath");
            }
            
            return CompareTo((VirtualPath)obj);
        }

        public VirtualPath GetRelativePath(VirtualPath basePath)
        {
            // このパスが絶対パスでない場合は例外をスロー
            if (!IsAbsolute)
            {
                throw new InvalidOperationException("このパスは絶対パスではありません: " + _path);
            }

            // ベースパスが絶対パスでない場合も例外をスロー
            if (!basePath.IsAbsolute)
            {
                throw new InvalidOperationException("ベースパスは絶対パスでなければなりません: " + basePath.Path);
            }

            // 両方のパスが等しい場合は"."を返却
            if (_path == basePath.Path)
            {
                return VirtualPath.Dot;
            }

            // ベースパスがルートの場合、先頭のスラッシュを除去して返す
            if (basePath.Path == VirtualPath.Root)
            {
                return new VirtualPath(_path.TrimStart('/'));
            }

            var baseParts = basePath.PartsList;
            var targetParts = PartsList;
            int minLength = Math.Min(baseParts.Count, targetParts.Count);

            int commonLength = 0;
            for (int i = 0; i < minLength; i++)
            {
                if (baseParts[i].Name != targetParts[i].Name)
                    break;

                commonLength++;
            }

            // ベースパスとターゲットパスに共通の部分がない場合
            if (commonLength == 0)
            {
                return new VirtualPath(_path);  // 絶対パスをそのまま返却
            }

            // ベースパスの残りの部分に対して ".." を追加
            IEnumerable<string> relativePath = Enumerable.Repeat("..", baseParts.Count - commonLength);

            // ターゲットパスの非共通部分を追加
            relativePath = relativePath.Concat(targetParts.Skip(commonLength).Select(p => p.Name));

            return new VirtualPath(string.Join(VirtualPath.Separator, relativePath));
        }
    }
}
