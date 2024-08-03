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
        /// Returns a string that represents the instance's path.
        /// </summary>
        /// <returns>A string that represents the instance's path.</returns>
        public override string ToString() => _path;

        /// <summary>
        /// Gets a value indicating whether the instance's path is empty.
        /// </summary>
        /// <value><c>true</c> if the instance's path is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get => _path == string.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether the instance's path is the root path.
        /// </summary>
        /// <value><c>true</c> if the instance's path is the root path; otherwise, <c>false</c>.</value>
        public bool IsRoot
        {
            get => _path == Separator.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether the instance's path is absolute.
        /// </summary>
        /// <value><c>true</c> if the instance's path is absolute; otherwise, <c>false</c>.</value>
        public bool IsAbsolute
        {
            get => _path.StartsWith(Separator);
        }

        /// <summary>
        /// Gets a value indicating whether the instance's path ends with a slash.
        /// </summary>
        /// <value><c>true</c> if the instance's path ends with a slash; otherwise, <c>false</c>.</value>
        public bool IsEndsWithSlash
        {
            get => _path.EndsWith(Separator);
        }

        /// <summary>
        /// Gets a value indicating whether the instance's path is a dot.
        /// </summary>
        /// <value><c>true</c> if the instance's path is a dot; otherwise, <c>false</c>.</value>
        public bool IsDot
        {
            get => _path == Dot;
        }

        /// <summary>
        /// Gets a value indicating whether the instance's path is a double dot.
        /// </summary>
        /// <value><c>true</c> if the instance's path is a double dot; otherwise, <c>false</c>.</value>
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
        /// <param name="obj">The object to compare with the instance's path.</param>
        /// <returns>true if the specified object is equal to the instance's path; otherwise, false.</returns>
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
        /// <param name="other">The <see cref="VirtualPath"/> object to compare with the instance's path.</param>
        /// <returns>true if the specified <see cref="VirtualPath"/> is equal to the instance's path; otherwise, false.</returns>
        public bool Equals(VirtualPath? other)
        {
            return _path == other?._path;
        }

        /// <summary>
        /// Determines whether two <see cref="VirtualPath"/> instances are equal.
        /// </summary>
        /// <param name="left">The left <see cref="VirtualPath"/>.</param>
        /// <param name="right">The right <see cref="VirtualPath"/>.</param>
        /// <returns>true if the paths are equal; otherwise, false.</returns>
        public static bool operator ==(VirtualPath? left, VirtualPath? right)
        {
            // Returns true if both are null
            if (left is null && right is null)
            {
                return true;
            }

            // Returns false if one is null
            if (left is null || right is null)
            {
                return false;
            }

            // Compare the actual paths
            return left._path == right._path;
        }

        /// <summary>
        /// Determines whether two <see cref="VirtualPath"/> instances are not equal.
        /// </summary>
        /// <param name="left">The left <see cref="VirtualPath"/>.</param>
        /// <param name="right">The right <see cref="VirtualPath"/>.</param>
        /// <returns>true if the paths are not equal; otherwise, false.</returns>
        public static bool operator !=(VirtualPath? left, VirtualPath? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Combines two <see cref="VirtualPath"/> instances.
        /// </summary>
        /// <param name="path1">The first <see cref="VirtualPath"/>.</param>
        /// <param name="path2">The second <see cref="VirtualPath"/>.</param>
        /// <returns>A new <see cref="VirtualPath"/> representing the combined path.</returns>
        public static VirtualPath operator +(VirtualPath path1, VirtualPath path2)
        {
            return path1.Combine(path2).NormalizePath();
        }

        /// <summary>
        /// Combines a <see cref="VirtualPath"/> with a <see cref="VirtualNodeName"/>.
        /// </summary>
        /// <param name="path">The <see cref="VirtualPath"/>.</param>
        /// <param name="nodeName">The <see cref="VirtualNodeName"/>.</param>
        /// <returns>A new <see cref="VirtualPath"/> representing the combined path.</returns>
        public static VirtualPath operator +(VirtualPath path, VirtualNodeName nodeName)
        {
            string combinedPath = VirtualPath.Combine(path.Path, nodeName.Name);
            return new VirtualPath(combinedPath).NormalizePath();
        }

        /// <summary>
        /// Combines a <see cref="VirtualPath"/> with a string.
        /// </summary>
        /// <param name="path">The <see cref="VirtualPath"/>.</param>
        /// <param name="str">The string to combine with the path.</param>
        /// <returns>A new <see cref="VirtualPath"/> representing the combined path.</returns>
        public static VirtualPath operator +(VirtualPath path, string str)
        {
            string combinedPath = VirtualPath.Combine(path.Path, str);
            return new VirtualPath(combinedPath).NormalizePath();
        }

        /// <summary>
        /// Combines a string with a <see cref="VirtualPath"/>.
        /// </summary>
        /// <param name="str">The string to combine with the path.</param>
        /// <param name="path">The <see cref="VirtualPath"/>.</param>
        /// <returns>A new <see cref="VirtualPath"/> representing the combined path.</returns>
        public static VirtualPath operator +(string str, VirtualPath path)
        {
            string combinedPath = VirtualPath.Combine(str, path.Path);
            return new VirtualPath(combinedPath).NormalizePath();
        }

        /// <summary>
        /// Combines a <see cref="VirtualPath"/> with a character.
        /// </summary>
        /// <param name="path">The <see cref="VirtualPath"/>.</param>
        /// <param name="chr">The character to combine with the path.</param>
        /// <returns>A new <see cref="VirtualPath"/> representing the combined path.</returns>
        public static VirtualPath operator +(VirtualPath path, char chr)
        {
            string combinedPath = path.Path + chr;
            return new VirtualPath(combinedPath); // Combine without normalization
        }

        /// <summary>
        /// Combines a character with a <see cref="VirtualPath"/>.
        /// </summary>
        /// <param name="chr">The character to combine with the path.</param>
        /// <param name="path">The <see cref="VirtualPath"/>.</param>
        /// <returns>A new <see cref="VirtualPath"/> representing the combined path.</returns>
        public static VirtualPath operator +(char chr, VirtualPath path)
        {
            string combinedPath = chr + path.Path;
            return new VirtualPath(combinedPath); // Combine without normalization
        }

        /// <summary>
        /// Removes the trailing slash from the path.
        /// </summary>
        /// <returns>A new <see cref="VirtualPath"/> without the trailing slash.</returns>
        public VirtualPath TrimEndSlash()
        {
            if (_path.EndsWith(Separator))
            {
                return new VirtualPath(_path[..^1]);
            }
            return this;
        }

        /// <summary>
        /// Adds a trailing slash to the path if it doesn't already have one.
        /// </summary>
        /// <returns>A new <see cref="VirtualPath"/> with a trailing slash.</returns>
        public VirtualPath AddEndSlash()
        {
            if (!_path.EndsWith(Separator))
            {
                return new VirtualPath(_path + Separator);
            }
            return this;
        }

        /// <summary>
        /// Adds a leading slash to the path if it doesn't already have one.
        /// </summary>
        /// <returns>A new <see cref="VirtualPath"/> with a leading slash.</returns>
        public VirtualPath AddStartSlash()
        {
            if (!_path.StartsWith(Separator))
            {
                return new VirtualPath(Separator + _path);
            }
            return this;
        }

        /// <summary>
        /// Determines whether the instance's path starts with the specified path.
        /// </summary>
        /// <param name="path">The <see cref="VirtualPath"/> to compare with.</param>
        /// <returns>true if the instance's path starts with the specified path; otherwise, false.</returns>
        public bool StartsWith(VirtualPath path)
        {
            return _path.StartsWith(path.Path);
        }

        /// <summary>
        /// Normalizes the instance's path.
        /// </summary>
        /// <returns>A new <see cref="VirtualPath"/> instance with the normalized path.</returns>
        public VirtualPath NormalizePath()
        {
            // Call the static method to normalize the path as a string
            string normalizedPathString = NormalizePath(_path);

            // Create and return a new VirtualPath instance using the normalized path string
            return new VirtualPath(normalizedPathString);
        }

        /// <summary>
        /// Normalizes the specified path string.
        /// </summary>
        /// <param name="path">The path string to normalize.</param>
        /// <returns>A normalized path string.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to navigate above the root directory.
        /// </exception>
        public static string NormalizePath(string path)
        {
            // Return as is if the path is an empty string or the PathSeparator
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

        /// <summary>
        /// Retrieves the directory part of the instance's path.
        /// </summary>
        /// <returns>A string representing the directory part of the path.</returns>
        private string GetDirectoryPath()
        {
            // Return as is if the path does not start with PathSeparator, indicating it is a relative path
            if (!_path.StartsWith(Separator))
            {
                return _path;
            }

            int lastSlashIndex = _path.LastIndexOf(Separator);

            // Return PathSeparator if it is not found, indicating the root directory
            if (lastSlashIndex <= 0)
            {
                return Separator.ToString();
            }
            else
            {
                // Extract and return the part up to the last PathSeparator in the full path
                return _path[..lastSlashIndex];
            }
        }

        /// <summary>
        /// Retrieves the node name from the instance's path.
        /// </summary>
        /// <returns>A <see cref="VirtualNodeName"/> object representing the node name.</returns>
        public VirtualNodeName GetNodeName()
        {
            if (_path == VirtualPath.Separator.ToString())
            {
                // Return as is if the path represents the root directory
                return new VirtualNodeName(VirtualPath.Separator.ToString());
            }

            StringBuilder path = new(_path);

            if (path.Length > 0 && path[^1] == VirtualPath.Separator)
            {
                // Remove the trailing PathSeparator
                path.Remove(path.Length - 1, 1);
            }

            int lastSlashIndex = path.ToString().LastIndexOf(VirtualPath.Separator);
            if (lastSlashIndex < 0)
            {
                // Return as is if no PathSeparator is found
                return new VirtualNodeName(_path);
            }
            else
            {
                // Extract and return the part after the last PathSeparator
                return new VirtualNodeName(path.ToString()[(lastSlashIndex + 1)..]);
            }
        }

        /// <summary>
        /// Combines the instance's path with the specified paths.
        /// </summary>
        /// <param name="paths">An array of <see cref="VirtualPath"/> objects to combine.</param>
        /// <returns>A new <see cref="VirtualPath"/> representing the combined path.</returns>
        public VirtualPath Combine(params VirtualPath[] paths)
        {
            string[] currentPathArray = [_path];
            string[] pathStrings = paths.Select(p => p.Path).ToArray();
            string[] allPaths = [.. currentPathArray, .. pathStrings];
            string combinedPathString = Combine(allPaths);

            return new VirtualPath(combinedPathString);
        }

        /// <summary>
        /// Combines the specified paths into a single path.
        /// </summary>
        /// <param name="paths">An array of path strings to combine.</param>
        /// <returns>A combined path string.</returns>
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

            // Return as is if the result is an empty string
            if (string.IsNullOrEmpty(combinedPath))
            {
                return string.Empty;
            }

            // Add a leading separator if it is an absolute path
            if (isAbsolutePath)
            {
                combinedPath = VirtualPath.Separator + combinedPath;
            }

            // Remove the trailing PathSeparator
            if (combinedPath.EndsWith(VirtualPath.Separator))
            {
                combinedPath = combinedPath[..^1];
            }

            return combinedPath;
        }

        /// <summary>
        /// Gets the parent path of the instance's path.
        /// </summary>
        /// <returns>A <see cref="VirtualPath"/> representing the parent path.</returns>
        public VirtualPath GetParentPath()
        {
            // Remove the trailing PathSeparator
            string trimmedPath = _path.TrimEnd(VirtualPath.Separator);
            // Split the path by PathSeparator
            string[] pathParts = trimmedPath.Split(VirtualPath.Separator);
            // Remove the last part
            string[] parentPathParts = pathParts.Take(pathParts.Length - 1).ToArray();
            // Reconstruct the path
            string parentPath = string.Join(VirtualPath.Separator, parentPathParts);

            // Return the root if the path is empty
            if (string.IsNullOrEmpty(parentPath))
            {
                return Root;
            }

            return new VirtualPath(parentPath);
        }

        /// <summary>
        /// Gets the parts of the path as a linked list.
        /// </summary>
        /// <returns>A <see cref="LinkedList{T}"/> of <see cref="VirtualNodeName"/> objects representing the path parts.</returns>
        public LinkedList<VirtualNodeName> GetPartsLinkedList()
        {
            LinkedList<VirtualNodeName> parts = new();
            foreach (var part in _path.Split(VirtualPath.Separator, StringSplitOptions.RemoveEmptyEntries))
            {
                parts.AddLast(new VirtualNodeName(part));
            }

            return parts;
        }

        /// <summary>
        /// Retrieves the node parts of the path as a list.
        /// This method converts the node parts stored in a linked list structure
        /// into a list structure, making it more convenient for sequential access
        /// and operations that require indexing.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="VirtualNodeName"/>
        /// objects representing the node parts of the path.</returns>
        public List<VirtualNodeName> GetPartsList()
        {
            return [.. GetPartsLinkedList()];
        }

        /// <summary>
        /// Combines the instance's path with the parts of another path starting from a specified index.
        /// </summary>
        /// <param name="path">The <see cref="VirtualPath"/> whose parts to combine.</param>
        /// <param name="index">The starting index from which to combine the parts.</param>
        /// <returns>A new <see cref="VirtualPath"/> representing the combined path.</returns>
        public VirtualPath CombineFromIndex(VirtualPath path, int index)
        {
            // Get the parts of the path from the specified index
            var partsFromIndex = path.PartsList.Skip(index).ToList();

            // Combine the instance's path (this) with the parts from the specified index
            VirtualPath combinedPath = this;
            foreach (var part in partsFromIndex)
            {
                combinedPath += part;
            }

            return combinedPath;
        }

        /// <summary>
        /// Compares the instance's path with another <see cref="VirtualPath"/>.
        /// </summary>
        /// <param name="other">The other <see cref="VirtualPath"/> to compare with.</param>
        /// <returns>An integer indicating the relative order of the paths.</returns>
        public int CompareTo(VirtualPath? other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(_path, other._path, StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares the instance's path with another object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>An integer indicating the relative order of the paths.</returns>
        /// <exception cref="ArgumentException">Thrown if the object is not a <see cref="VirtualPath"/>.</exception>
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

        /// <summary>
        /// Gets the relative path from the specified base path.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <returns>A <see cref="VirtualPath"/> representing the relative path.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the instance's path is not absolute: This exception occurs when
        /// the method is called on a <see cref="VirtualPath"/> instance that does
        /// not start with the root path separator, indicating that it is not an
        /// absolute path.
        /// Thrown if the base path is not absolute: This exception occurs when
        /// the <paramref name="basePath"/> argument does not start with the root
        /// path separator, indicating that it is not an absolute path.
        /// Both the instance's path and the base path must be absolute paths to
        /// compute a relative path.
        /// </exception>
        public VirtualPath GetRelativePath(VirtualPath basePath)
        {
            // Throws an exception if the instance's path is not absolute
            if (!IsAbsolute)
            {
                throw new InvalidOperationException(string.Format(Resources.PathIsNotAbsolutePath, _path));
            }

            // Throws an exception if the base path is not absolute
            if (!basePath.IsAbsolute)
            {
                throw new InvalidOperationException(string.Format(Resources.BasePathIsNotAbsolute, basePath.Path));
            }

            // Return "." if both paths are equal
            if (_path == basePath.Path)
            {
                return VirtualPath.Dot;
            }

            // Remove the leading slash if the base path is the root
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

            // Return the absolute path if there are no common parts between the base and target paths
            if (commonLength == 0)
            {
                return new VirtualPath(_path);
            }

            // Add ".." for the remaining parts in the base path
            IEnumerable<string> relativePath = Enumerable.Repeat("..", baseParts.Count - commonLength);

            // Add the remaining parts from the target path
            relativePath = relativePath.Concat(targetParts.Skip(commonLength).Select(p => p.Name));

            return new VirtualPath(string.Join(VirtualPath.Separator, relativePath));
        }

        /// <summary>
        /// Gets the fixed path and depth after applying wildcard matching.
        /// </summary>
        /// <returns>A tuple containing the <see cref="VirtualPath"/> representing the fixed path and an integer representing the depth.</returns>
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

        /// <summary>
        /// Determines whether one path is a subdirectory of another path.
        /// </summary>
        /// <param name="path1">The first <see cref="VirtualPath"/>.</param>
        /// <param name="path2">The second <see cref="VirtualPath"/>.</param>
        /// <returns>true if one path is a subdirectory of the other; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown if either path is empty.</exception>
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

            // Check if either path is the root
            if (path1.IsRoot || path2.IsRoot)
            {
                return true;
            }

            var sourceParts = path1.PartsList;
            var destinationParts = path2.PartsList;

            // Check if the paths are identical
            if (sourceParts.SequenceEqual(destinationParts))
            {
                return true;
            }

            // Check if path1 is a subdirectory of path2
            if (sourceParts.Count <= destinationParts.Count &&
                sourceParts.SequenceEqual(destinationParts.Take(sourceParts.Count)))
            {
                return true;
            }

            // Check if path2 is a subdirectory of path1
            if (destinationParts.Count <= sourceParts.Count &&
                destinationParts.SequenceEqual(sourceParts.Take(destinationParts.Count)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether this instance's path is a subdirectory of the specified parent path.
        /// </summary>
        /// <param name="parentPath">The parent <see cref="VirtualPath"/>.</param>
        /// <returns>true if this instance's path is a subdirectory of the parent path; otherwise, false.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if this instance's path or the parent path is empty.
        /// </exception>
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

            // Check if the paths are identical
            if (sourceParts.SequenceEqual(destinationParts))
            {
                return false;
            }

            // Check if the parentPath is the root directory and the destinationParts are under the root
            if (parentPath.IsRoot)
            {
                return true;
            }

            // Check if the parentPath is the parent directory of the instance's path
            if (sourceParts.Count <= destinationParts.Count &&
                sourceParts.SequenceEqual(destinationParts.Take(sourceParts.Count)))
            {
                return true;
            }

            return false;
        }
    }
}
