using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace VirtualStorageLibrary
{
    public class VirtualStorageSettings
    {
        private static VirtualStorageSettings _settings = new();

        public static VirtualStorageSettings Settings => _settings;

        private VirtualStorageSettings()
        {
            InvalidNodeNameCharacters = [ PathSeparator ];
            InvalidFullNodeNames = [ PathDot, PathDotDot ];
        }

        public static void Initialize() => _settings = new();

        public char PathSeparator { get; set; } = '/';

        public string PathRoot { get; set; } = "/";
        
        public string PathDot { get; set; } = ".";

        public string PathDotDot { get; set; } = "..";

        public char[] InvalidNodeNameCharacters { get; set; }

        public string[] InvalidFullNodeNames { get; set; }

        public PatternMatcher? PatternMatcher { get; set; } = PowerShellWildcardDictionary.PowerShellRegexMatch;

        public GroupCondition<VirtualNode, object>? NodeGroupCondition { get; set; } = new (node => node.NodeType, true);

        public List<SortCondition<VirtualNode>>? NodeSortConditions { get; set; } = new()
        {
            new (node => node.Name, true)
        };

        public VirtualNodeTypeFilter NodeTypeFilter { get; set; } = VirtualNodeTypeFilter.All;
    }

    public delegate void NotifyNodeDelegate(VirtualPath path, VirtualNode? node, bool isEnd);

    public delegate bool ActionNodeDelegate(VirtualDirectory directory, VirtualNodeName nodeName);

    public delegate bool PatternMatcher(string nodeName, string pattern);

    public static class PowerShellWildcardDictionary
    {
        // TODO: エスケープ(`)については別途、検討。
        // ワイルドカードとそれに対応する正規表現のパターンの配列
        private static Dictionary<string, string> _wildcardDictionary = new()
        {
            { "*", ".*" },  // 0文字以上に一致
            { "?", "." },   // 任意の1文字に一致
            { "[", "[" },   // 文字クラスの開始
            { "]", "]" }    // 文字クラスの終了
        };

        public static Dictionary<string, string> WildcardDictionary => _wildcardDictionary;

        public static IEnumerable<string> Wildcards => _wildcardDictionary.Keys;

        public static IEnumerable<string> Patterns => _wildcardDictionary.Values;

        // ワイルドカードの実装（PowerShell）
        public static bool PowerShellRegexMatch(string nodeName, string pattern)
        {
            // エスケープ処理を考慮して正規表現のパターンを作成
            string regexPattern = "^";
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == '`' && i + 1 < pattern.Length && Wildcards.Contains(pattern[i + 1].ToString()))
                {
                    // エスケープされたワイルドカード文字をリテラルとして扱う
                    regexPattern += Regex.Escape(pattern[i + 1].ToString());
                    i++; // エスケープされた文字をスキップ
                }
                else
                {
                    string currentChar = pattern[i].ToString();
                    if (WildcardDictionary.ContainsKey(currentChar))
                    {
                        regexPattern += WildcardDictionary[currentChar];
                    }
                    else
                    {
                        regexPattern += Regex.Escape(currentChar);
                    }
                }
            }
            regexPattern += "$";

            // 正規表現を用いてマッチングを行う
            return Regex.IsMatch(nodeName, regexPattern);
        }
    }

    public enum VirtualNodeType
    {
        Directory,
        Item,
        SymbolicLink,
        None
    }

    public enum VirtualNodeTypeFilter
    {
        None = 0x00,
        Item = 0x01,
        Directory = 0x02,
        SymbolicLink = 0x04,
        All = Item | Directory | SymbolicLink
    }

    public class VirtualNodeContext
    {
        public VirtualNode? Node { [DebuggerStepThrough] get; }

        public VirtualPath TraversalPath { [DebuggerStepThrough] get; }

        public VirtualDirectory? ParentDirectory { [DebuggerStepThrough] get; }

        public int Depth { [DebuggerStepThrough] get; }

        public int Index { [DebuggerStepThrough] get; }

        public VirtualPath? ResolvedPath { [DebuggerStepThrough] get; }

        public bool Resolved { [DebuggerStepThrough] get; }

        [DebuggerStepThrough]
        public VirtualNodeContext(VirtualNode? node, VirtualPath traversalPath, VirtualDirectory? parentNode = null, int depth = 0, int index = 0, VirtualPath? resolvedPath = null, bool resolved = false)
        {
            Node = node;
            TraversalPath = traversalPath;
            ParentDirectory = parentNode;
            Depth = depth;
            Index = index;
            ResolvedPath = resolvedPath;
            Resolved = resolved;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            List<string> parts = new()
            {
                $"NodeName: {Node?.Name}",
                $"TraversalPath: {TraversalPath}",
                $"ParentDirectory: {ParentDirectory?.Name}",
                $"Depth: {Depth}",
                $"Index: {Index}"
            };

            if (ResolvedPath != null)
            {
                parts.Add($"ResolvedPath: {ResolvedPath}");
            }

            parts.Add($"Resolved: {Resolved}");

            return string.Join(", ", parts);
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

    public class VirtualNodeName : IEquatable<VirtualNodeName>, IComparable<VirtualNodeName>, IComparable
    {
        private readonly string _name;

        public string Name
        {
            [DebuggerStepThrough]
            get
            {
                return _name;
            }
        }

        [DebuggerStepThrough]
        public override string ToString() => _name;

        [DebuggerStepThrough]
        public VirtualNodeName(string name)
        {
            _name = name;
        }

        // ユーザーが使用してはいけないノード名の文字のチェック
        public static bool IsValidNodeName(string name)
        {
            if (name == string.Empty)
            {
                return false;
            }

            foreach (char c in VirtualStorageSettings.Settings.InvalidNodeNameCharacters)
            {
                if (name.Contains(c))
                {
                    return false;
                }
            }

            foreach (string invalidFullNodeName in VirtualStorageSettings.Settings.InvalidFullNodeNames)
            {
                if (name == invalidFullNodeName)
                {
                    return false;
                }
            }

            return true;
        }

        [DebuggerStepThrough]
        public static implicit operator VirtualNodeName(string name)
        {
            return new VirtualNodeName(name);
        }

        [DebuggerStepThrough]
        public static implicit operator string(VirtualNodeName nodeName)
        {
            return nodeName._name;
        }

        [DebuggerStepThrough]
        public bool Equals(VirtualNodeName? other)
        {
            return _name == other?._name;
        }

        [DebuggerStepThrough]
        public override bool Equals(object? obj)
        {
            if (obj is VirtualNodeName other)
            {
                return _name == other._name;
            }
            return false;
        }

        [DebuggerStepThrough]
        public override int GetHashCode() => _name.GetHashCode();

        [DebuggerStepThrough]
        public int CompareTo(VirtualNodeName? other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(_name, other._name, StringComparison.Ordinal);
        }

        [DebuggerStepThrough]
        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is VirtualNodeName))
            {
                throw new ArgumentException("Object is not a VirtualNodeName");
            }

            return CompareTo((VirtualNodeName)obj);
        }

        [DebuggerStepThrough]
        public static bool operator ==(VirtualNodeName? left, VirtualNodeName? right)
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

            // 実際のノード名の比較
            return left._name == right._name;
        }

        [DebuggerStepThrough]
        public static bool operator !=(VirtualNodeName? left, VirtualNodeName? right)
        {
            return !(left == right);
        }
    }

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

    public abstract class VirtualNode : IDeepCloneable<VirtualNode>
    {
        public VirtualNodeName Name { get; set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }

        public abstract VirtualNodeType NodeType { get; }

        public abstract VirtualNode DeepClone();

        protected VirtualNode(VirtualNodeName name)
        {
            Name = name;
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }

        protected VirtualNode(VirtualNodeName name, DateTime createdDate, DateTime updatedDate)
        {
            Name = name;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }

        public override string ToString() => $"{Name}";
    }

    public class VirtualSymbolicLink : VirtualNode
    {
        public VirtualPath? TargetPath { get; set; }

        public VirtualNodeType TargetNodeType { get; set; }

        public override VirtualNodeType NodeType => VirtualNodeType.SymbolicLink;

        public VirtualSymbolicLink(VirtualNodeName name) : base(name)
        {
            TargetPath = null;
        }

        public VirtualSymbolicLink(VirtualNodeName name, VirtualPath? targetPath) : base(name)
        {
            TargetPath = targetPath;
        }

        public VirtualSymbolicLink(VirtualNodeName name, VirtualPath? targetPath, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            TargetPath = targetPath;
        }

        public override string ToString() => $"{Name} -> {TargetPath}";

        public override VirtualNode DeepClone()
        {
            return new VirtualSymbolicLink(Name, TargetPath, CreatedDate, UpdatedDate);
        }
    }

    public abstract class VirtualItem : VirtualNode
    {
        protected VirtualItem(VirtualNodeName name) : base(name) { }

        protected VirtualItem(VirtualNodeName name, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate) { }

        public override abstract VirtualNode DeepClone();
    }

    public class VirtualItem<T> : VirtualItem, IDisposable
    {
        public T? ItemData { get; set; }

        private bool disposed;

        public override VirtualNodeType NodeType => VirtualNodeType.Item;

        public VirtualItem(VirtualNodeName name) : base(name)
        {
            ItemData = default;
            disposed = false;
        }

        public VirtualItem(VirtualNodeName name, T? item) : base(name)
        {
            ItemData = item;
            disposed = false;
        }

        public VirtualItem(VirtualNodeName name, T? item, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
            ItemData = item;
            disposed = false;
        }

        public override string ToString() => $"{Name}";

        public override VirtualNode DeepClone()
        {
            T? newItemData = ItemData;

            // ItemDataがIDeepCloneable<T>を実装している場合はDeepClone()を呼び出す
            if (ItemData is IDeepCloneable<T> cloneableItem)
            {
                newItemData = cloneableItem.DeepClone();
            }

            return new VirtualItem<T>(Name, newItemData);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TがIDisposableを実装していればDisposeを呼び出す
                    (ItemData as IDisposable)?.Dispose();
                }

                // VirtualItem<T>はアンマネージドリソースを扱ってないので、ここでは何もしない
                disposed = true;
            }
        }
    }

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
            VirtualNodeTypeFilter nodeTypeFilter = VirtualStorageSettings.Settings.NodeTypeFilter;
            GroupCondition<VirtualNode, object>? nodeGroupCondition = VirtualStorageSettings.Settings.NodeGroupCondition;
            List<SortCondition<VirtualNode>>? nodeSortConditions = VirtualStorageSettings.Settings.NodeSortConditions;

            IEnumerable<VirtualNode> nodes = _nodes.Values;

            switch (nodeTypeFilter)
            {
                case VirtualNodeTypeFilter.None:
                    return Enumerable.Empty<VirtualNode>();
                case VirtualNodeTypeFilter.All:
                    break;
                default:
                    nodes = _nodes.Values.Where(node =>
                        (nodeTypeFilter.HasFlag(VirtualNodeTypeFilter.Directory) && node is VirtualDirectory) ||
                        (nodeTypeFilter.HasFlag(VirtualNodeTypeFilter.Item) && node is VirtualItem) ||
                        (nodeTypeFilter.HasFlag(VirtualNodeTypeFilter.SymbolicLink) && node is VirtualSymbolicLink));
                    break;
            }

            return nodes.GroupAndSort(nodeGroupCondition, nodeSortConditions);
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

    public class VirtualStorage
    {
        private VirtualDirectory _root;

        public VirtualDirectory Root => _root;

        public VirtualPath CurrentPath { get; private set; }

        private Dictionary<VirtualPath, List<VirtualPath>> _linkDictionary;

        public VirtualStorage()
        {
            _root = new VirtualDirectory(VirtualPath.Root);
            CurrentPath = VirtualPath.Root;
            _linkDictionary = new();
        }

        public void UpdateLinkTargetNodeTypes(VirtualPath targetPath)
        {
            if (_linkDictionary.ContainsKey(targetPath))
            {
                VirtualNodeType targetType = GetNodeType(targetPath, true);
                foreach (VirtualPath linkPath in _linkDictionary[targetPath])
                {
                    VirtualSymbolicLink symbolicLink = (VirtualSymbolicLink)GetNode(linkPath);
                    symbolicLink.TargetNodeType = targetType;
                }
            }
        }

        public void AddLinkToDictionary(VirtualPath targetPath, VirtualPath linkPath)
        {
            if (!targetPath.IsAbsolute)
            {
                throw new ArgumentException("リンク先のパスは絶対パスである必要があります。", nameof(targetPath));
            }

            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            if (!_linkDictionary.ContainsKey(targetPath))
            {
                _linkDictionary[targetPath] = new();
            }

            _linkDictionary[targetPath].Add(linkPath);

            VirtualSymbolicLink symbolicLink = (VirtualSymbolicLink)GetNode(linkPath);
            symbolicLink.TargetNodeType = GetNodeType(targetPath, true);
        }

        public void RemoveLinkToDictionary(VirtualPath targetPath, VirtualPath linkPath)
        {
            if (!targetPath.IsAbsolute)
            {
                throw new ArgumentException("リンク先のパスは絶対パスである必要があります。", nameof(targetPath));
            }

            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            if (_linkDictionary.ContainsKey(targetPath))
            {
                _linkDictionary[targetPath].Remove(linkPath);

                if (_linkDictionary[targetPath].Count == 0)
                {
                    _linkDictionary.Remove(targetPath);
                }

                VirtualSymbolicLink symbolicLink = (VirtualSymbolicLink)GetNode(linkPath);
                symbolicLink.TargetNodeType = VirtualNodeType.None;
            }
        }

        public void ChangeDirectory(VirtualPath path)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNodeContext? nodeContext = WalkPathToTarget(path, null, null, true, true);
            CurrentPath = nodeContext.TraversalPath;

            return;
        }

        public VirtualPath ConvertToAbsolutePath(VirtualPath? relativePath, VirtualPath? basePath = null)
        {
            basePath ??= CurrentPath;

            // relativePathがnullまたは空文字列の場合は、ArgumentExceptionをスロー
            if (relativePath == null || relativePath.IsEmpty)
            {
                throw new ArgumentException("relativePathがnullまたは空です。", nameof(relativePath));
            }

            // relativePathが既に絶対パスである場合は、そのまま使用
            if (relativePath.IsAbsolute)
            {
                return relativePath;
            }

            // basePathが空文字列の場合、ArgumentExceptionをスロー
            if (basePath.IsEmpty)
            {
                throw new ArgumentException("basePathが空です。", nameof(basePath));
            }

            // relativePathをeffectiveBasePathに基づいて絶対パスに変換
            var absolutePath = basePath + relativePath;

            return absolutePath;
        }

        public void AddSymbolicLink(VirtualPath path, VirtualPath targetPath, bool overwrite = false)
        {
            // linkPathを絶対パスに変換し正規化も行う
            path = ConvertToAbsolutePath(path).NormalizePath();

            // directoryPath（ディレクトリパス）と linkName（リンク名）を分離
            VirtualPath directoryPath = path.DirectoryPath;
            VirtualNodeName linkName = path.NodeName;

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
                    // 既存のシンボリックリンクを削除（上書きフラグが true の場合）
                    directory.Remove(linkName);
                }
            }

            // シンボリックリンクを作成
            VirtualSymbolicLink symbolicLink = new(linkName, targetPath);
            
            // 新しいシンボリックリンクを追加
            directory.Add(symbolicLink);

            // targetPathを絶対パスに変換して正規化する必要がある。その際、シンボリックリンクを作成したディレクトリパスを基準とする
            VirtualPath absoluteTargetPath = ConvertToAbsolutePath(targetPath, directoryPath).NormalizePath();

            // リンク辞書にリンク情報を追加
            AddLinkToDictionary(absoluteTargetPath, path);

            // TODO: 作成したノードがリンクターゲットとして登録されている場合、リンクターゲットのノードタイプを更新

            // TODO: シンボリックリンクがシンボリックリンクへリンクしていたらどうするか検討する
        }

        public void AddItem<T>(VirtualPath path, VirtualItem<T> item, bool overwrite = false)
        {
            // 絶対パスに変換
            path = ConvertToAbsolutePath(path).NormalizePath();

            // 対象ディレクトリを取得
            VirtualDirectory directory = GetDirectory(path, true);

            // 既存のアイテムの存在チェック
            if (directory.NodeExists(item.Name))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException($"アイテム '{item.Name}' は既に存在します。上書きは許可されていません。");
                }
                else
                {
                    // 上書き対象がアイテムであることを確認
                    if (!ItemExists(path + item.Name))
                    {
                        throw new InvalidOperationException($"'{item.Name}' はアイテム以外のノードです。アイテムの上書きはできません。");
                    }
                    // 既存アイテムの削除
                    directory.Remove(item.Name);
                }
            }

            // 新しいアイテムを追加
            directory.Add(item, overwrite);

            // 作成したノードがリンクターゲットとして登録されている場合、リンクターゲットのノードタイプを更新
            UpdateLinkTargetNodeTypes(path + item.Name);
        }

        public void AddItem<T>(VirtualPath path, T data, bool overwrite = false)
        {
            // 絶対パスに変換
            path = ConvertToAbsolutePath(path).NormalizePath();

            // ディレクトリパスとアイテム名を分離
            VirtualPath directoryPath = path.DirectoryPath;
            VirtualNodeName itemName = path.NodeName;

            // アイテムを作成
            VirtualItem<T> item = new(itemName, data);

            // AddItemメソッドを呼び出し
            AddItem(directoryPath, item, overwrite);
        }

        public void AddDirectory(VirtualPath path, bool createSubdirectories = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();

            if (path.IsRoot)
            {
                throw new InvalidOperationException("ルートディレクトリは既に存在します。");
            }

            VirtualPath directoryPath = path.DirectoryPath;
            VirtualNodeName newDirectoryName = path.NodeName;
            VirtualNodeContext nodeContext;

            if (createSubdirectories)
            {
                nodeContext = WalkPathToTarget(directoryPath, null, CreateIntermediateDirectory, true, true);
            }
            else
            {
                nodeContext = WalkPathToTarget(directoryPath, null, null, true, true);
            }

            if (nodeContext.Node is VirtualDirectory directory)
            {
                if (directory.NodeExists(newDirectoryName))
                {
                    throw new InvalidOperationException($"ディレクトリ '{newDirectoryName}' は既に存在します。");
                }

                // 新しいディレクトリを追加
                VirtualDirectory newDirectory = new(newDirectoryName);
                directory.Add(newDirectory);

                // 作成したノードがリンクターゲットとして登録されている場合、リンクターゲットのノードタイプを更新
                UpdateLinkTargetNodeTypes(path);
            }
            else
            {
                throw new VirtualNodeNotFoundException($"ノード '{directoryPath}' はディレクトリではありません。");
            }

            return;
        }

        private bool CreateIntermediateDirectory(VirtualDirectory directory, VirtualNodeName nodeName)
        {
            // 中間ディレクトリを追加
            VirtualDirectory newSubdirectory = new(nodeName);
            directory.Add(newSubdirectory);

            // 中間ディレクトリがリンクターゲットとして登録されている場合、リンクターゲットのノードタイプを更新
            UpdateLinkTargetNodeTypes(directory.Name + nodeName);

            return true;
        }

        public VirtualNodeContext WalkPathToTarget(
            VirtualPath targetPath,
            NotifyNodeDelegate? notifyNode = null,
            ActionNodeDelegate? actionNode = null,
            bool followLinks = true,
            bool exceptionEnabled = true)
        {
            targetPath = ConvertToAbsolutePath(targetPath).NormalizePath();
            VirtualNodeContext? nodeContext = WalkPathToTargetInternal(targetPath, 0, VirtualPath.Root, null, _root, notifyNode, actionNode, followLinks, exceptionEnabled, false);

            return nodeContext;
        }

        private VirtualNodeContext WalkPathToTargetInternal(
            VirtualPath targetPath,
            int traversalIndex,
            VirtualPath traversalPath,
            VirtualPath? resolvedPath,
            VirtualDirectory traversalDirectory,
            NotifyNodeDelegate? notifyNode,
            ActionNodeDelegate? actionNode,
            bool followLinks,
            bool exceptionEnabled,
            bool resolved)
        {
            // ターゲットがルートディレクトリの場合は、ルートノードを通知して終了
            if (targetPath.IsRoot)
            {
                notifyNode?.Invoke(VirtualPath.Root, _root, true);
                return new VirtualNodeContext(_root, VirtualPath.Root, null, 0, 0, VirtualPath.Root, resolved);
            }

            VirtualNodeName traversalNodeName = targetPath.PartsList[traversalIndex];

            while (!traversalDirectory.NodeExists(traversalNodeName))
            {
                if (actionNode != null)
                {
                    if (actionNode(traversalDirectory, traversalNodeName))
                    {
                        continue;
                    }
                }

                // 例外が有効な場合は例外をスロー
                if (exceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException($"ノード '{traversalPath + traversalNodeName}' が見つかりません。");
                }

                return new VirtualNodeContext(null, traversalPath, null, 0, 0, traversalPath, resolved);
            }

            VirtualNodeContext? nodeContext;

            // 探索ノードを取得
            VirtualNode? node = traversalDirectory[traversalNodeName];

            // 探索パスを更新
            traversalPath = traversalPath + traversalNodeName;

            // 次のノードへ
            traversalIndex++;

            if (node is VirtualDirectory)
            {
                // 最後のノードに到達したかチェック
                if (targetPath.PartsList.Count <= traversalIndex)
                {
                    // 末端のノードを通知
                    notifyNode?.Invoke(traversalPath, node, true);
                    resolvedPath ??= traversalPath;
                    return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
                }

                // 途中のノードを通知
                notifyNode?.Invoke(traversalPath, node, false);

                // 探索ディレクトリを取得
                traversalDirectory = (VirtualDirectory)node;

                // 再帰的に探索
                nodeContext = WalkPathToTargetInternal(targetPath, traversalIndex, traversalPath, resolvedPath, traversalDirectory, notifyNode, actionNode, followLinks, exceptionEnabled, resolved);
                node = nodeContext?.Node;
                traversalPath = nodeContext?.TraversalPath ?? traversalPath;
                resolvedPath = nodeContext?.ResolvedPath ?? resolvedPath;
            }
            else if (node is VirtualItem)
            {
                // 末端のノードを通知
                notifyNode?.Invoke(traversalPath, node, true);

                // 最後のノードに到達したかチェック
                if (targetPath.PartsList.Count <= traversalIndex)
                {
                    resolvedPath ??= traversalPath;
                    return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
                }

                resolvedPath ??= traversalPath;

                // 例外が有効な場合は例外をスロー
                if (exceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException($"ノード '{targetPath}' まで到達できません。ノード '{traversalPath}' はアイテムです。");
                }

                return new VirtualNodeContext(null, traversalPath, null, 0, 0, resolvedPath, resolved);
            }
            else if (node is VirtualSymbolicLink link)
            {
                if (!followLinks)
                {
                    // シンボリックリンクを通知
                    notifyNode?.Invoke(traversalPath, node, true);
                    resolvedPath ??= traversalPath;
                    return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
                }

                resolved = true;

                VirtualPath? linkTargetPath = link.TargetPath;
                VirtualPath parentTraversalPath = traversalPath.DirectoryPath;

                // シンボリックリンクのリンク先パスを絶対パスに変換
                linkTargetPath = ConvertToAbsolutePath(linkTargetPath, parentTraversalPath);

                // リンク先のパスを正規化する
                linkTargetPath = linkTargetPath.NormalizePath();

                // シンボリックリンクのリンク先パスを再帰的に探索
                nodeContext = WalkPathToTargetInternal(linkTargetPath, 0, VirtualPath.Root, null, _root, null, null, true, exceptionEnabled, resolved);

                node = nodeContext?.Node;
                //traversalPath = result?.TraversalPath ?? traversalPath;

                // 解決済みのパスに未探索のパスを追加
                resolvedPath = nodeContext?.ResolvedPath!.CombineFromIndex(targetPath, traversalIndex);

                // シンボリックリンクを通知
                notifyNode?.Invoke(traversalPath, node, true);

                if (node != null && (node is VirtualDirectory))
                {
                    // 探索ディレクトリを取得
                    traversalDirectory = (VirtualDirectory)node;

                    // 最後のノードに到達したかチェック
                    if (targetPath.PartsList.Count <= traversalIndex)
                    {
                        // 末端のノードを通知
                        resolvedPath ??= traversalPath;
                        return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
                    }

                    // 再帰的に探索
                    nodeContext = WalkPathToTargetInternal(targetPath, traversalIndex, traversalPath, resolvedPath, traversalDirectory, notifyNode, actionNode, followLinks, exceptionEnabled, resolved);
                    node = nodeContext?.Node;
                    traversalPath = nodeContext?.TraversalPath ?? traversalPath;
                    resolvedPath = nodeContext?.ResolvedPath ?? resolvedPath;
                }

                resolvedPath ??= traversalPath;
                return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
            }

            resolvedPath ??= traversalPath;
            return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
        }

        public IEnumerable<VirtualNodeContext> WalkPathTree(
            VirtualPath basePath,
            VirtualNodeTypeFilter filter = VirtualNodeTypeFilter.All,
            bool recursive = true,
            bool followLinks = true)
        {
            basePath = ConvertToAbsolutePath(basePath).NormalizePath();
            int baseDepth = GetBaseDepth(basePath);
            VirtualNode node = GetNode(basePath, followLinks);

            return WalkPathTreeInternal(basePath, basePath, node, null, baseDepth, 0, 0, filter, recursive, followLinks, null);
        }

        public IEnumerable<VirtualNodeContext> ResolvePathTree(
            VirtualPath path,
            VirtualNodeTypeFilter filter = VirtualNodeTypeFilter.All)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualPath basePath = ExtractBasePath(path);
            int baseDepth = GetBaseDepth(path);
            VirtualNode node = GetNode(basePath, true);

            List<string> patternList = path.PartsList.Select(node => node.Name).ToList();

            return WalkPathTreeInternal(basePath, basePath, node, null, baseDepth, 0, 0, filter, true, true, patternList);
        }

        // TODO: VirtualPathクラスに組み込むか検討する。
        private VirtualPath ExtractBasePath(VirtualPath path)
        {
            var wildcards = PowerShellWildcardDictionary.Wildcards;
            var parts = path.PartsList.TakeWhile(part => !wildcards.Any(wildcard => part.Name.Contains(wildcard))).ToList();
            return new VirtualPath(parts);
        }

        // TODO: VirtualPathクラスに組み込むか検討する。
        private static int GetBaseDepth(VirtualPath path)
        {
            var wildcards = PowerShellWildcardDictionary.Wildcards;
            var baseParts = path.PartsList.TakeWhile(part => !wildcards.Any(wildcard => part.Name.Contains(wildcard))).ToList();
            return baseParts.Count;
        }

        private IEnumerable<VirtualNodeContext> WalkPathTreeInternal(
            VirtualPath basePath,
            VirtualPath currentPath,
            VirtualNode baseNode,
            VirtualDirectory? parentDirectory,
            int baseDepth,
            int currentDepth,
            int currentIndex,
            VirtualNodeTypeFilter filter,
            bool recursive,
            bool followLinks,
            List<string>? patternList)
        {
            PatternMatcher? patternMatcher = VirtualStorageSettings.Settings.PatternMatcher ?? null;

            // ノードの種類に応じて処理を分岐
            if (baseNode is VirtualDirectory directory)
            {
                if (filter.HasFlag(VirtualNodeTypeFilter.Directory))
                {
                    if (patternMatcher != null && patternList != null)
                    {
                        if (baseDepth + currentDepth == patternList.Count)
                        {
                            if (MatchPatterns(currentPath.PartsList, patternList))
                            {
                                // ディレクトリを通知
                                yield return new VirtualNodeContext(directory, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                            }
                        }
                    }
                    else
                    {
                        // ディレクトリを通知
                        yield return new VirtualNodeContext(directory, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                    }
                }

                if (recursive || 0 == currentDepth)
                {
                    // ディレクトリ内のノードを再帰的に探索
                    int index = 0;
                    foreach (var node in directory.Nodes)
                    {
                        VirtualPath path = currentPath + node.Name;
                        foreach (var result in WalkPathTreeInternal(basePath, path, node, directory, baseDepth, currentDepth + 1, index, filter, recursive, followLinks, patternList))
                        {
                            yield return result;
                        }
                        index++;
                    }
                }
            }
            else if (baseNode is VirtualItem item)
            {
                if (filter.HasFlag(VirtualNodeTypeFilter.Item))
                {
                    if (patternMatcher != null && patternList != null)
                    {
                        if (baseDepth + currentDepth == patternList.Count)
                        {
                            if (MatchPatterns(currentPath.PartsList, patternList))
                            {
                                // アイテムを通知
                                yield return new VirtualNodeContext(item, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                            }
                        }
                    }
                    else
                    {
                        // アイテムを通知
                        yield return new VirtualNodeContext(item, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                    }
                }
            }
            else if (baseNode is VirtualSymbolicLink link)
            {
                if (followLinks)
                {
                    VirtualPath? linkTargetPath = link.TargetPath;

                    // シンボリックリンクのリンク先パスを絶対パスに変換
                    linkTargetPath = ConvertToAbsolutePath(linkTargetPath, currentPath).NormalizePath();

                    // リンク先のノードを取得
                    VirtualNode? linkTargetNode = GetNode(linkTargetPath, followLinks);

                    // リンク先のノードに対して再帰的に探索
                    foreach (var result in WalkPathTreeInternal(basePath, currentPath, linkTargetNode, parentDirectory, baseDepth, currentDepth, currentIndex, filter, recursive, followLinks, patternList))
                    {
                        yield return result;
                    }
                }
                else
                {
                    if (filter.HasFlag(VirtualNodeTypeFilter.SymbolicLink))
                    {
                        if (patternMatcher != null && patternList != null)
                        {
                            if (baseDepth + currentDepth == patternList.Count)
                            {
                                if (MatchPatterns(currentPath.PartsList, patternList))
                                {
                                    // シンボリックリンクを通知
                                    yield return new VirtualNodeContext(link, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                                }
                            }
                        }
                        else
                        {
                            // シンボリックリンクを通知
                            yield return new VirtualNodeContext(link, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                        }
                    }
                }
            }
        }

        static bool MatchPatterns(List<VirtualNodeName> parts, List<string> patternList)
        {
            PatternMatcher? patternMatcher = VirtualStorageSettings.Settings.PatternMatcher ?? null;

            if (patternMatcher == null)
            {
                return false;
            }

            if (parts.Count != patternList.Count)
            {
                return false;
            }

            for (int i = 0; i < parts.Count; i++)
            {
                if (!patternMatcher(parts[i], patternList[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public VirtualNode GetNode(VirtualPath path, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNodeContext nodeContext = WalkPathToTarget(path, null, null, followLinks, true);
            return nodeContext.Node!;
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

        public VirtualPath ResolveLinkTarget(VirtualPath path)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNodeContext nodeContext = WalkPathToTarget(path, null, null, true, true);
            return nodeContext.ResolvedPath!;
        }

        public VirtualPath? TryResolveLinkTarget(VirtualPath path)
        {
            try
            {
                return ResolveLinkTarget(path);
            }
            catch (VirtualNodeNotFoundException)
            {
                return null;
            }
        }

        public VirtualDirectory GetDirectory(VirtualPath path, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNode node = GetNode(path, followLinks);

            if (node is VirtualDirectory directory)
            {
                return directory;
            }
            else
            {
                throw new VirtualNodeNotFoundException($"ディレクトリ {path} は存在しません。");
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

        public VirtualItem<T> GetItem<T>(VirtualPath path, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNode node = GetNode(path, followLinks);

            if (node is VirtualItem<T> item)
            {
                return item;
            }
            else
            {
                throw new VirtualNodeNotFoundException($"アイテム {path} は存在しません。");
            }
        }

        public VirtualItem<T>? TryGetItem<T>(VirtualPath path)
        {
            try
            {
                return GetItem<T>(path);
            }
            catch (VirtualNodeNotFoundException)
            {
                return null;
            }
        }

        public VirtualSymbolicLink GetSymbolicLink(VirtualPath path, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNode node = GetNode(path, followLinks);

            if (node is VirtualSymbolicLink link)
            {
                return link;
            }
            else
            {
                throw new VirtualNodeNotFoundException($"シンボリックリンク {path} は存在しません。");
            }
        }

        public VirtualSymbolicLink? TryGetSymbolicLink(VirtualPath path)
        {
            try
            {
                return GetSymbolicLink(path);
            }
            catch (VirtualNodeNotFoundException)
            {
                return null;
            }
        }

        public VirtualNodeType GetNodeType(VirtualPath path, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNode? node = TryGetNode(path, followLinks);

            if (node is VirtualDirectory)
            {
                return VirtualNodeType.Directory;
            }
            else if (node is VirtualItem)
            {
                return VirtualNodeType.Item;
            }
            else if (node is VirtualSymbolicLink)
            {
                return VirtualNodeType.SymbolicLink;
            }

            return VirtualNodeType.None;
        }

        public IEnumerable<VirtualNode> GetNodes(VirtualPath basePath, VirtualNodeTypeFilter nodeType = VirtualNodeTypeFilter.All, bool recursive = false, bool followLinks = false)
        {
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(basePath, nodeType, recursive, followLinks);
            IEnumerable<VirtualNode> nodes = nodeContexts.Select(info => info.Node!);
            return nodes;
        }

        public IEnumerable<VirtualNode> GetNodes(VirtualNodeTypeFilter nodeType = VirtualNodeTypeFilter.All, bool recursive = false, bool followLinks = false)
        {
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(CurrentPath, nodeType, recursive, followLinks);
            IEnumerable<VirtualNode> nodes = nodeContexts.Select(info => info.Node!);
            return nodes;
        }

        public IEnumerable<VirtualPath> GetNodesWithPaths(VirtualPath basePath, VirtualNodeTypeFilter nodeType = VirtualNodeTypeFilter.All, bool recursive = false, bool followLinks = false)
        {
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(basePath, nodeType, recursive, followLinks);
            IEnumerable<VirtualPath> paths = nodeContexts.Select(info => info.TraversalPath);
            return paths;
        }

        public IEnumerable<VirtualPath> GetNodesWithPaths(VirtualNodeTypeFilter nodeType = VirtualNodeTypeFilter.All, bool recursive = false, bool followLinks = false)
        {
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(CurrentPath, nodeType, recursive, followLinks);
            IEnumerable<VirtualPath> paths = nodeContexts.Select(info => info.TraversalPath);
            return paths;
        }

        public IEnumerable<VirtualPath> ResolvePath(VirtualPath path)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualPath basePath = ExtractBasePath(path);
            IEnumerable<VirtualNodeContext> nodeContexts = ResolvePathTree(path);
            IEnumerable<VirtualPath> resolvedPaths = nodeContexts.Select(info => (basePath + info.TraversalPath).NormalizePath());

            return resolvedPaths;
        }

        // ワイルドカードの実装（デフォルト）
        public static bool RegexMatch(string nodeName, string pattern)
        {
            return Regex.IsMatch(nodeName, pattern);
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

            VirtualPath targetDirectoryPath;
            VirtualNodeName newNodeName;

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

        public IEnumerable<VirtualNodeContext> CopyNode(
            VirtualPath sourcePath,
            VirtualPath destinationPath,
            bool overwrite = false,
            bool followLinks = false,
            bool recursive = false)
        {
            sourcePath = ConvertToAbsolutePath(sourcePath).NormalizePath();
            destinationPath = ConvertToAbsolutePath(destinationPath).NormalizePath();

            // コピー元パスが存在しない場合
            if (!NodeExists(sourcePath, true))
            {
                throw new VirtualNodeNotFoundException($"コピー元のノード '{sourcePath}' が存在しません。");
            }

            // コピー元パスとコピー先パスが同じ場合
            if (sourcePath == destinationPath)
            {
                throw new InvalidOperationException("コピー元パスとコピー先パスが同じです。");
            }

            IEnumerable<VirtualNodeContext> destinationContexts = Enumerable.Empty<VirtualNodeContext>();

            // コピー元のツリーを取得
            IEnumerable<VirtualNodeContext> sourceContexts = WalkPathTree(sourcePath, VirtualNodeTypeFilter.All, recursive, followLinks);

            VirtualNode sourceNode = sourceContexts.First().Node!;

            IEnumerable<VirtualNodeContext> contexts = CopySingleInternal(sourcePath, sourceNode, destinationPath, destinationPath, overwrite, followLinks);
            destinationContexts = destinationContexts.Concat(contexts);

            return destinationContexts;
        }

        private IEnumerable<VirtualNodeContext> CopySingleInternal(
            VirtualPath sourcePath,
            VirtualNode sourceNode,
            VirtualPath destinationPath,
            VirtualPath originalDestinationPath,
            bool overwrite,
            bool followLinks)
        {
            VirtualNodeName? newNodeName = null;

            IEnumerable<VirtualNodeContext> contexts = Enumerable.Empty<VirtualNodeContext>();

            VirtualDirectory destinationDirectory = GetDirectory(destinationPath.DirectoryPath, true);
            VirtualNodeName destinationNodeName = destinationPath.NodeName;

            VirtualNode? destinationNode = destinationDirectory.Get(destinationNodeName, false);

            VirtualPath destinationDirectoryPath;

            bool overwriteDirectory = false;

            switch (destinationNode)
            {
                case VirtualDirectory directory:
                    destinationDirectory = directory;
                    newNodeName = sourceNode.Name;
                    VirtualNode? node = destinationDirectory.Get(newNodeName, false);
                    if (node != null)
                    {
                        if (overwrite && (node is VirtualDirectory))
                        {
                            overwriteDirectory = true;
                        }
                        else
                        {
                            throw new InvalidOperationException($"ノード '{newNodeName}' は既に存在します。上書きは許可されていません。");
                        }
                    }
                    destinationDirectoryPath = destinationPath;
                    destinationPath = destinationPath + newNodeName;
                    break;

                case VirtualItem _:
                    if (overwrite)
                    {
                        destinationDirectory.Remove(destinationNodeName);
                    }
                    else
                    {
                        throw new InvalidOperationException($"アイテム '{destinationNodeName}' は既に存在します。上書きは許可されていません。");
                    }
                    newNodeName = destinationPath.NodeName;
                    destinationDirectoryPath = destinationPath.DirectoryPath;
                    break;

                case VirtualSymbolicLink link:
                    VirtualPath targetPath = ConvertToAbsolutePath(link.TargetPath).NormalizePath();
                    return CopySingleInternal(sourcePath, sourceNode, targetPath, originalDestinationPath, overwrite, followLinks);

                default:
                    newNodeName = destinationPath.NodeName;
                    destinationDirectoryPath = destinationPath.DirectoryPath;
                    break;
            }

            // コピー先ノードを作成
            VirtualNode newNode = sourceNode.DeepClone();
            newNode.Name = newNodeName;

            // コピー元ノードをコピー先ディレクトリに追加 (ディレクトリの上書き以外の場合)
            if (!overwriteDirectory)
            {
                destinationDirectory.Add(newNode);
            }

            // パスの深さを計算
            int depth = originalDestinationPath.Depth - 1;
            if (depth < 0)
            {
                // デバッグ用
                throw new InvalidOperationException("深さが負の値になりました。");
            }

            // コピー操作の結果を表す VirtualNodeContext を生成して返却
            VirtualNodeContext context = new VirtualNodeContext(
                node: newNode,
                traversalPath: originalDestinationPath,
                parentNode: destinationDirectory,
                depth: depth,
                index: 0
            );

            contexts = contexts.Append(context);

            return contexts;
        }

        public void RemoveNode(VirtualPath path, bool recursive = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            path = ResolveLinkTarget(path);

            if (path.IsRoot)
            {
                throw new InvalidOperationException("ルートディレクトリを削除することはできません。");
            }

            VirtualNode node = GetNode(path, true);

            // ディレクトリを親ディレクトリから削除するための共通の親パスと親ディレクトリを取得
            VirtualPath parentPath = path.DirectoryPath;
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
                    VirtualPath subPath = path + subNode.Name;
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
            if (absoluteDestinationPath.StartsWith(new VirtualPath(absoluteSourcePath.Path + VirtualPath.Separator)))
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
                        VirtualNodeName destinationNodeName = absoluteDestinationPath.NodeName;
                        VirtualDirectory sourceDirectory = GetDirectory(absoluteSourcePath);

                        VirtualNodeName oldNodeName = sourceDirectory.Name;
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
                        VirtualNodeName destinationNodeName = absoluteDestinationPath.NodeName;
                        VirtualNode sourceNode = GetNode(absoluteSourcePath);

                        VirtualNodeName oldNodeName = sourceNode.Name;
                        sourceNode.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceNode);
                        VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                    else
                    {
                        // 存在する場合

                        VirtualDirectory destinationParentDirectory = GetDirectory(absoluteDestinationPath.GetParentPath());
                        VirtualNodeName destinationNodeName = absoluteDestinationPath.NodeName;
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

                        VirtualNodeName oldNodeName = sourceNode.Name;
                        sourceNode.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceNode);
                        VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                }
            }
        }

        // 仮想ストレージのツリー構造をテキストベースで作成し返却する
        // 返却するテストは以下の出力の形式とする。
        // 例: もし、/dir1/dir2/item1, /dir1/dir2/item2, /dir1/item3 が存在し、/dir1が basePath で指定された場合
        // 出力:
        // /
        // ├dir1/
        // │├item1
        // │└item2
        // └dir2/
        // 　├item3
        // 　├item4
        // 　└item5
        public string GenerateTextBasedTreeStructure(VirtualPath basePath, bool recursive = true, bool followLinks = true)
        {
            const char FullWidthSpaceChar = '\u3000';
            StringBuilder tree = new();

            basePath = ConvertToAbsolutePath(basePath).NormalizePath();
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(basePath, VirtualNodeTypeFilter.All, recursive, followLinks);
            StringBuilder line = new();
            string previous = string.Empty;

            VirtualNodeContext nodeContext = nodeContexts.First();
            VirtualNode baseNode = nodeContext.Node!;
            VirtualPath baseAbsolutePath = (basePath + nodeContext.TraversalPath).NormalizePath();
            if (baseNode is VirtualDirectory)
            {
                string baseNodeName;
                if (baseAbsolutePath == VirtualPath.Root)
                {
                    baseNodeName = baseAbsolutePath.NodeName;
                }
                else
                {
                    baseNodeName = baseAbsolutePath.NodeName + VirtualPath.Separator;
                }
                tree.AppendLine(baseNodeName);
            }
            else if (baseNode is VirtualItem)
            {
                tree.AppendLine(baseAbsolutePath.NodeName);
            }
            else if (baseNode is VirtualSymbolicLink link)
            {
                string baseNodeName;
                baseNodeName = (string)baseAbsolutePath.NodeName + " -> " + (string)link.TargetPath;
                tree.AppendLine(baseNodeName);
            }

            foreach (var nodeInfo in nodeContexts.Skip(1))
            {
                VirtualNode? node = nodeInfo.Node;
                string nodeName = nodeInfo.TraversalPath.NodeName;
                int depth = nodeInfo.Depth;
                int count = nodeInfo.ParentDirectory?.Count ?? 0;
                int index = nodeInfo.Index;

                line.Clear();

                if (depth > 0)
                {
                    for (int i = 0; i < depth - 1; i++)
                    {
                        if (i < previous.Length)
                        {
                            switch (previous[i])
                            {
                                case FullWidthSpaceChar:
                                    line.Append(FullWidthSpaceChar);
                                    break;
                                case '│':
                                    line.Append("│");
                                    break;
                                case '└':
                                    line.Append(FullWidthSpaceChar);
                                    break;
                                case '├':
                                    line.Append("│");
                                    break;
                                default:
                                    line.Append(FullWidthSpaceChar);
                                    break;
                            }
                        }
                        else
                        { 
                            line.Append(FullWidthSpaceChar);
                        }
                    }

                    if (index == count - 1)
                    {
                        line.Append("└");
                    }
                    else
                    {
                        line.Append("├");
                    }
                }

                if (node is VirtualDirectory)
                {
                    line.Append(nodeName + VirtualPath.Separator);
                }
                else if (node is VirtualSymbolicLink link)
                {
                    line.Append(nodeName + " -> " + (string)link.TargetPath);
                }
                else
                {
                    line.Append(nodeName);
                }
                previous = line.ToString();
                tree.AppendLine(line.ToString());
            }   

            return tree.ToString();
        }
    }

    public class GroupCondition<T, TKey>
    {
        public Expression<Func<T, TKey>> GroupBy { get; set; }
        public bool Ascending { get; set; } = true; // デフォルトは昇順

        public GroupCondition(Expression<Func<T, TKey>> groupBy, bool ascending = true)
        {
            GroupBy = groupBy;
            Ascending = ascending;
        }
    }

    public class SortCondition<T>
    {
        public Expression<Func<T, object>> SortBy { get; set; }
        public bool Ascending { get; set; } = true; // デフォルトは昇順

        public SortCondition(Expression<Func<T, object>> sortBy, bool ascending = true)
        {
            SortBy = sortBy;
            Ascending = ascending;
        }
    }

    public static class VirtualStorageLinqExtensions
    {
        public static IEnumerable<T> GroupAndSort<T>(
            this IEnumerable<T> source,
            GroupCondition<T, object>? groupCondition,
            List<SortCondition<T>>? sortConditions)
        {
            var query = source.AsQueryable();

            if (groupCondition != null)
            {
                var groupedData = groupCondition.Ascending
                    ? query.GroupBy(groupCondition.GroupBy).OrderBy(g => g.Key)
                    : query.GroupBy(groupCondition.GroupBy).OrderByDescending(g => g.Key);

                // グループ内のアイテムに対して追加のソート条件を適用
                return groupedData.SelectMany(group => group.ApplySortConditions(sortConditions));
            }
            else
            {
                // グルーピングなしで全体にソート条件を適用
                return query.ApplySortConditions(sortConditions);
            }
        }

        public static IEnumerable<T> ApplySortConditions<T>(
            this IEnumerable<T> source,
            List<SortCondition<T>>? sortConditions)
        {
            if (sortConditions == null || sortConditions.Count == 0)
            {
                return source;
            }

            IQueryable<T> sourceQuery = source.AsQueryable();
            IOrderedQueryable<T>? orderedQuery = null;

            for (int i = 0; i < sortConditions.Count; i++)
            {
                var condition = sortConditions[i];
                if (orderedQuery == null)
                {
                    // 最初のソート条件を適用
                    orderedQuery = condition.Ascending
                        ? sourceQuery.AsQueryable().OrderBy(condition.SortBy)
                        : sourceQuery.AsQueryable().OrderByDescending(condition.SortBy);
                }
                else
                {
                    // 2番目以降のソート条件を適用
                    orderedQuery = condition.Ascending
                        ? orderedQuery.ThenBy(condition.SortBy)
                        : orderedQuery.ThenByDescending(condition.SortBy);
                }
            }

            // ソートの指定がなかったらそのまま返す
            return orderedQuery ?? source;
        }
    }
}
