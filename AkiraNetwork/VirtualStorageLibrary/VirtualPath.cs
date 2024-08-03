using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Represents a path within the virtual storage.
    /// Provides functionality for manipulating, comparing, 
    /// normalizing, and splitting paths.
    /// </summary>
    [DebuggerStepThrough]
    public class VirtualPath : IEquatable<VirtualPath>, IComparable<VirtualPath>, IComparable
    {
        /// <summary>
        /// The internal string representing the path.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// The cached directory path for this path.
        /// </summary>
        private VirtualPath? _directoryPath;

        /// <summary>
        /// The cached node name for this path.
        /// </summary>
        private VirtualNodeName? _nodeName;

        /// <summary>
        /// The root path value.
        /// </summary>
        private static readonly string _root;

        /// <summary>
        /// The dot symbol (".") used in paths.
        /// </summary>
        private static readonly string _dot;

        /// <summary>
        /// The double dot symbol ("..") used in paths.
        /// </summary>
        private static readonly string _dotDot;

        /// <summary>
        /// Gets the path string.
        /// </summary>
        /// <value>The string representation of the path.</value>
        public string Path { get => _path; }

        /// <summary>
        /// The cached list of parts that make up this path.
        /// </summary>
        private List<VirtualNodeName>? _partsList;

        /// <summary>
        /// The cached fixed path after applying wildcard matching.
        /// </summary>
        private VirtualPath? _fixedPath;

        /// <summary>
        /// The cached fixed depth of this path.
        /// </summary>
        private int? _fixedDepth;

        /// <summary>
        /// Implicitly converts a <see cref="string"/> to a <see cref="VirtualPath"/>.
        /// </summary>
        /// <param name="path">The string representation of the path.</param>
        /// <returns>A <see cref="VirtualPath"/> object representing the given string path.</returns>
        public static implicit operator VirtualPath(string path)
        {
            return new VirtualPath(path);
        }

        /// <summary>
        /// Implicitly converts a <see cref="VirtualPath"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="virtualPath">The <see cref="VirtualPath"/> object.</param>
        /// <returns>The string representation of the <see cref="VirtualPath"/>, or an empty string if the path is null.</returns>
        public static implicit operator string(VirtualPath? virtualPath)
        {
            if (virtualPath == null)
            {
                return string.Empty;
            }

            return virtualPath._path;
        }

        /// <summary>
        /// Gets the directory part of the path.
        /// </summary>
        /// <value>A <see cref="VirtualPath"/> representing the directory part of the path.</value>
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

        /// <summary>
        /// Gets the node name of the path.
        /// </summary>
        /// <value>A <see cref="VirtualNodeName"/> representing the node name of the path.</value>
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

        /// <summary>
        /// Gets the list of parts that make up the path.
        /// </summary>
        /// <value>A list of <see cref="VirtualNodeName"/> objects representing the parts of the path.</value>
        public List<VirtualNodeName> PartsList
        {
            get
            {
                _partsList ??= GetPartsList();
                return _partsList;
            }
        }

        /// <summary>
        /// Gets the depth of the path.
        /// </summary>
        /// <value>An integer representing the depth of the path.</value>
        public int Depth => this.PartsList.Count;

        /// <summary>
        /// Gets the fixed path after applying wildcard matching.
        /// </summary>
        /// <remarks>
        /// The fixed path excludes parts containing wildcards.
        /// </remarks>
        /// <value>A <see cref="VirtualPath"/> object representing the fixed path.</value>
        public VirtualPath FixedPath
        {
            get
            {
                if (_fixedPath == null)
                {
                    (_fixedPath, _fixedDepth) = GetFixedPath();
                }
                return _fixedPath;
            }
        }

        /// <summary>
        /// Gets the depth of the fixed path.
        /// </summary>
        /// <remarks>
        /// The fixed depth indicates the number of parts in the path, 
        /// excluding those with wildcards.
        /// </remarks>
        /// <value>An integer representing the depth of the fixed path.</value>
        public int FixedDepth
        {
            get
            {
                if (_fixedDepth == null)
                {
                    (_fixedPath, _fixedDepth) = GetFixedPath();
                }
                return _fixedDepth.Value;
            }
        }

        /// <summary>
        /// Gets the path separator character.
        /// </summary>
        /// <value>A character representing the path separator.</value>
        public static char Separator => VirtualStorageSettings.Settings.PathSeparator;

        /// <summary>
        /// Gets the root path.
        /// </summary>
        /// <value>A string representing the root path.</value>
        public static string Root
        {
            get => _root;
        }

        /// <summary>
        /// Gets the dot (".") symbol.
        /// </summary>
        /// <value>A string representing the dot symbol.</value>
        public static string Dot
        {
            get => _dot;
        }

        /// <summary>
        /// Gets the double dot ("..") symbol.
        /// </summary>
        /// <value>A string representing the double dot symbol.</value>
        public static string DotDot
        {
            get => _dotDot;
        }

        /// <summary>
        /// Returns a string that represents the current path.
        /// </summary>
        /// <returns>A string that represents the current path.</returns>
        public override string ToString() => _path;

        /// <summary>
        /// Gets a value indicating whether the current path is empty.
        /// </summary>
        /// <value><c>true</c> if the current path is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get => _path == string.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether the current path is the root path.
        /// </summary>
        /// <value><c>true</c> if the current path is the root path; otherwise, <c>false</c>.</value>
        public bool IsRoot
        {
            get => _path == Separator.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether the current path is absolute.
        /// </summary>
        /// <value><c>true</c> if the current path is absolute; otherwise, <c>false</c>.</value>
        public bool IsAbsolute
        {
            get => _path.StartsWith(Separator);
        }

        /// <summary>
        /// Gets a value indicating whether the current path ends with a slash.
        /// </summary>
        /// <value><c>true</c> if the current path ends with a slash; otherwise, <c>false</c>.</value>
        public bool IsEndsWithSlash
        {
            get => _path.EndsWith(Separator);
        }

        /// <summary>
        /// Gets a value indicating whether the current path is a dot.
        /// </summary>
        /// <value><c>true</c> if the current path is a dot; otherwise, <c>false</c>.</value>
        public bool IsDot
        {
            get => _path == Dot;
        }

        /// <summary>
        /// Gets a value indicating whether the current path is a double dot.
        /// </summary>
        /// <value><c>true</c> if the current path is a double dot; otherwise, <c>false</c>.</value>
        public bool IsDotDot
        {
            get => _path == DotDot;
        }

        /// <summary>
        /// Returns the hash code for this path.
        /// </summary>
        /// <returns>The hash code for this path.</returns>
        public override int GetHashCode() => _path.GetHashCode();

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualPath"/> class with the specified path string.
        /// </summary>
        /// <param name="path">The initial path string.</param>
        public VirtualPath(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualPath"/> class with the specified collection of path parts.
        /// </summary>
        /// <param name="parts">The collection of path parts.</param>
        public VirtualPath(IEnumerable<VirtualNodeName> parts)
        {
            _path = Separator + string.Join(Separator, parts.Select(node => node.Name));
        }

        /// <summary>
        /// Initializes static members of the <see cref="VirtualPath"/> class.
        /// </summary>
        static VirtualPath()
        {
            _root = VirtualStorageSettings.Settings.PathRoot;
            _dot = VirtualStorageSettings.Settings.PathDot;
            _dotDot = VirtualStorageSettings.Settings.PathDotDot;
        }

        /// <summary>
        /// Indicates whether this path is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with the current path.</param>
        /// <returns>true if the specified object is equal to the current path; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is VirtualPath other)
            {
                return _path == other._path;
            }
            return false;
        }

        /// <summary>
        /// Indicates whether this path is equal to the specified <see cref="VirtualPath"/> object.
        /// </summary>
        /// <param name="other">The <see cref="VirtualPath"/> object to compare with the current path.</param>
        /// <returns>true if the specified <see cref="VirtualPath"/> is equal to the current path; otherwise, false.</returns>
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
                return (_path, PartsList.Count);
            }

            IEnumerable<string> wildcards = wildcardMatcher.Wildcards;
            List<VirtualNodeName> parts = PartsList.TakeWhile(part => !wildcards.Any(wildcard => part.Name.Contains(wildcard))).ToList();

            if (parts.Count == PartsList.Count)
            {
                return (_path, PartsList.Count);
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
