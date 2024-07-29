using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualPath : IEquatable<VirtualPath>, IComparable<VirtualPath>, IComparable
    {
        private readonly string _path;

        private VirtualPath? _directoryPath;

        private VirtualNodeName? _nodeName;

        private static readonly string _root;

        private static readonly string _dot;

        private static readonly string _dotDot;

        public string Path { get => _path; }

        private List<VirtualNodeName>? _partsList;

        private VirtualPath? _fixedPath;

        private int? _baseDepth;

        public static implicit operator VirtualPath(string path)
        {
            return new VirtualPath(path);
        }

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
            get
            {
                _partsList ??= GetPartsList();
                return _partsList;
            }
        }

        public int Depth
        {
            get
            {
                // パスの深さを返す
                return this.PartsList.Count;
            }
        }

        public VirtualPath FixedPath
        {
            get
            {
                if (_fixedPath == null)
                {
                    (_fixedPath, _baseDepth) = GetFixedPath();
                }
                return _fixedPath;
            }
        }

        public int BaseDepth
        {
            get
            {
                if (_baseDepth == null)
                {
                    (_fixedPath, _baseDepth) = GetFixedPath();
                }
                return _baseDepth.Value;
            }
        }

        public static char Separator
        {
            get => VirtualStorageSettings.Settings.PathSeparator;
        }

        public static string Root
        {
            get => _root;
        }

        public static string Dot
        {
            get => _dot;
        }

        public static string DotDot
        {
            get => _dotDot;
        }

        public override string ToString() => _path;

        public bool IsEmpty
        {
            get => _path == string.Empty;
        }

        public bool IsRoot
        {
            get => _path == Separator.ToString();
        }

        public bool IsAbsolute
        {
            get => _path.StartsWith(Separator);
        }

        public bool IsEndsWithSlash
        {
            get => _path.EndsWith(Separator);
        }

        public bool IsDot
        {
            get => _path == Dot;
        }

        public bool IsDotDot
        {
            get => _path == DotDot;
        }

        public override int GetHashCode() => _path.GetHashCode();

        public VirtualPath(string path)
        {
            _path = path;
        }

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
            if (left is null && right is null)
            {
                return true;
            }

            // 一方が null の場合は false
            if (left is null || right is null)
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
            return path1.Combine(path2).NormalizePath();
        }

        public static VirtualPath operator +(VirtualPath path, VirtualNodeName nodeName)
        {
            string combinedPath = VirtualPath.Combine(path.Path, nodeName.Name);
            return new VirtualPath(combinedPath).NormalizePath();
        }

        public static VirtualPath operator +(VirtualPath path, string str)
        {
            string combinedPath = VirtualPath.Combine(path.Path, str);
            return new VirtualPath(combinedPath).NormalizePath();
        }

        public static VirtualPath operator +(string str, VirtualPath path)
        {
            string combinedPath = VirtualPath.Combine(str, path.Path);
            return new VirtualPath(combinedPath).NormalizePath();
        }

        public static VirtualPath operator +(VirtualPath path, char chr)
        {
            string combinedPath = path.Path + chr;
            return new VirtualPath(combinedPath); // 正規化せずに結合
        }

        public static VirtualPath operator +(char chr, VirtualPath path)
        {
            string combinedPath = chr + path.Path;
            return new VirtualPath(combinedPath); // 正規化せずに結合
        }

        public VirtualPath TrimEndSlash()
        {
            if (_path.EndsWith(Separator))
            {
                return new VirtualPath(_path[..^1]);
            }
            return this;
        }

        public VirtualPath AddEndSlash()
        {
            if (!_path.EndsWith(Separator))
            {
                return new VirtualPath(_path + Separator);
            }
            return this;
        }

        public VirtualPath AddStartSlash()
        {
            if (!_path.StartsWith(Separator))
            {
                return new VirtualPath(Separator + _path);
            }
            return this;
        }

        public bool StartsWith(VirtualPath path)
        {
            return _path.StartsWith(path.Path);
        }

        public VirtualPath NormalizePath()
        {
            // string型でパスを正規化する静的メソッドを呼び出す。
            string normalizedPathString = NormalizePath(_path);

            // 正規化されたパス文字列を使用して新しいVirtualPathインスタンスを作成し、返す。
            return new VirtualPath(normalizedPathString);
        }

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
                        throw new InvalidOperationException(string.Format(Resources.PathNormalizationAboveRoot, path));
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

            return normalizedPath;
        }

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
                return _path[..lastSlashIndex];
            }
        }

        public VirtualNodeName GetNodeName()
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
                return new VirtualNodeName(path.ToString()[(lastSlashIndex + 1)..]);
            }
        }

        public VirtualPath Combine(params VirtualPath[] paths)
        {
            string[] currentPathArray = [_path];
            string[] pathStrings = paths.Select(p => p.Path).ToArray();
            string[] allPaths = [.. currentPathArray, .. pathStrings];
            string combinedPathString = Combine(allPaths);

            return new VirtualPath(combinedPathString);
        }

        public static string Combine(params string[] paths)
        {
            if (paths.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder newPathBuilder = new();
            bool isFirstPath = true;
            bool isAbsolutePath = false;

            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (isFirstPath && path.StartsWith(VirtualPath.Separator.ToString()))
                {
                    isAbsolutePath = true;
                }

                var trimmedPath = path.Trim(VirtualPath.Separator);

                if (newPathBuilder.Length > 0 && newPathBuilder[^1] != VirtualPath.Separator)
                {
                    newPathBuilder.Append(VirtualPath.Separator);
                }

                newPathBuilder.Append(trimmedPath);
                isFirstPath = false;
            }

            var combinedPath = newPathBuilder.ToString();

            // 結果が空文字列の場合、そのまま返す
            if (string.IsNullOrEmpty(combinedPath))
            {
                return string.Empty;
            }

            // 絶対パスの場合、先頭にセパレータを追加
            if (isAbsolutePath)
            {
                combinedPath = VirtualPath.Separator + combinedPath;
            }

            // 末尾のPathSeparatorを取り除く
            if (combinedPath.EndsWith(VirtualPath.Separator))
            {
                combinedPath = combinedPath[..^1];
            }

            return combinedPath;
        }

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
                return Root;
            }

            return new VirtualPath(parentPath);
        }

        public LinkedList<VirtualNodeName> GetPartsLinkedList()
        {
            LinkedList<VirtualNodeName> parts = new();
            foreach (var part in _path.Split(VirtualPath.Separator, StringSplitOptions.RemoveEmptyEntries))
            {
                parts.AddLast(new VirtualNodeName(part));
            }

            return parts;
        }

        public List<VirtualNodeName> GetPartsList()
        {
            return [.. GetPartsLinkedList()];
        }

        public VirtualPath CombineFromIndex(VirtualPath path, int index)
        {
            // 指定されたインデックスからのパスのパーツを取得
            var partsFromIndex = path.PartsList.Skip(index).ToList();

            // 現在のパス（this）と指定されたインデックスからのパーツを結合
            VirtualPath combinedPath = this;
            foreach (var part in partsFromIndex)
            {
                combinedPath += part;
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

            if (obj is not VirtualPath)
            {
                throw new ArgumentException(Resources.ParameterIsNotVirtualPath, nameof(obj));
            }

            return CompareTo((VirtualPath)obj);
        }

        public VirtualPath GetRelativePath(VirtualPath basePath)
        {
            // このパスが絶対パスでない場合は例外をスロー
            if (!IsAbsolute)
            {
                throw new InvalidOperationException(string.Format(Resources.PathIsNotAbsolutePath, _path));
            }

            // ベースパスが絶対パスでない場合も例外をスロー
            if (!basePath.IsAbsolute)
            {
                throw new InvalidOperationException(string.Format(Resources.BasePathIsNotAbsolute, basePath.Path));
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

        private (VirtualPath, int) GetFixedPath()
        {
            IVirtualWildcardMatcher? wildcardMatcher = VirtualStorageState.State.WildcardMatcher;

            if (wildcardMatcher == null)
            {
                return (this, PartsList.Count);
            }

            IEnumerable<string> wildcards = wildcardMatcher.Wildcards;
            List<VirtualNodeName> parts = PartsList.TakeWhile(part => !wildcards.Any(wildcard => part.Name.Contains(wildcard))).ToList();

            if (parts.Count == PartsList.Count)
            {
                return (DirectoryPath, DirectoryPath.PartsList.Count);
            }

            return (new VirtualPath(parts), parts.Count);
        }

        public static bool ArePathsSubdirectories(VirtualPath path1, VirtualPath path2)
        {
            if (path1.IsEmpty)
            {
                throw new ArgumentException(Resources.PathCannotBeEmpty, nameof(path1));
            }
            else if (path2.IsEmpty)
            {
                throw new ArgumentException(Resources.PathCannotBeEmpty, nameof(path2));
            }

            // パスがルートであるかを確認
            if (path1.IsRoot || path2.IsRoot)
            {
                return true;
            }

            var sourceParts = path1.PartsList;
            var destinationParts = path2.PartsList;

            // パスが完全に一致するか確認
            if (sourceParts.SequenceEqual(destinationParts))
            {
                return true;
            }

            // path1 が path2 のサブディレクトリか確認
            if (sourceParts.Count <= destinationParts.Count &&
                sourceParts.SequenceEqual(destinationParts.Take(sourceParts.Count)))
            {
                return true;
            }

            // path2 が path1 のサブディレクトリか確認
            if (destinationParts.Count <= sourceParts.Count &&
                destinationParts.SequenceEqual(sourceParts.Take(destinationParts.Count)))
            {
                return true;
            }

            return false;
        }

        public bool IsSubdirectory(VirtualPath parentPath)
        {
            if (IsEmpty)
            {
                throw new ArgumentException(Resources.PathCannotBeEmpty);
            }
            else if (parentPath.IsEmpty)
            {
                throw new ArgumentException(Resources.PathCannotBeEmpty, nameof(parentPath));
            }

            var sourceParts = parentPath.PartsList;
            var destinationParts = PartsList;

            // パスが完全に一致するか確認
            if (sourceParts.SequenceEqual(destinationParts))
            {
                return false;
            }

            // parentPath がルートディレクトリの場合、destinationPartsがルート以下か確認
            if (parentPath.IsRoot)
            {
                return true;
            }

            // parentPath が現在のパスの親ディレクトリか確認
            if (sourceParts.Count <= destinationParts.Count &&
                sourceParts.SequenceEqual(destinationParts.Take(sourceParts.Count)))
            {
                return true;
            }

            return false;
        }
    }
}
