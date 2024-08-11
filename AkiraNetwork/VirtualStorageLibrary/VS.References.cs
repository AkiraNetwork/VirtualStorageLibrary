// This file is part of VirtualStorageLibrary.
//
// Copyright (C) 2024 Akira Shimodate
//
// VirtualStorageLibrary is free software, and it is distributed under the terms of 
// the GNU General Public License (version 3, or at your option, any later version). 
// This license is published by the Free Software Foundation.
//
// VirtualStorageLibrary is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along 
// with VirtualStorageLibrary. If not, see https://www.gnu.org/licenses/.

using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        /// <summary>
        /// Converts a relative path to an absolute path.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="basePath">The base path. If not specified, CurrentPath is used.</param>
        /// <returns>The absolute path represented by <see cref="VirtualPath"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if relativePath is null or an empty string.</exception>
        public VirtualPath ConvertToAbsolutePath(VirtualPath? relativePath, VirtualPath? basePath = null)
        {
            basePath ??= CurrentPath;

            // Throw ArgumentException if relativePath is null or empty
            if (relativePath == null || relativePath.IsEmpty)
            {
                throw new ArgumentException(string.Format(Resources.ParameterIsNullOrEmpty, relativePath), nameof(relativePath));
            }

            // Use relativePath as-is if it is already an absolute path
            if (relativePath.IsAbsolute)
            {
                return relativePath;
            }

            // Throw ArgumentException if basePath is empty
            if (basePath.IsEmpty)
            {
                throw new ArgumentException(string.Format(Resources.ParameterIsEmpty, basePath), nameof(basePath));
            }

            // Convert relativePath to absolute path based on basePath
            var absolutePath = basePath + relativePath;

            return absolutePath;
        }

        /// <summary>
        /// Gets the node at the specified path.
        /// </summary>
        /// <param name="path">The node's path.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The <see cref="VirtualNode"/> at the specified path.</returns>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node cannot be found.</exception>
        public VirtualNode GetNode(VirtualPath path, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNodeContext nodeContext = WalkPathToTarget(path, null, null, followLinks, true);
            return nodeContext.Node!;
        }

        /// <summary>
        /// Tries to get the node at the specified path.
        /// </summary>
        /// <param name="path">The node's path.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The found <see cref="VirtualNode"/>, or null if not found.</returns>
        public VirtualNode? TryGetNode(VirtualPath path, bool followLinks = false)
        {
            VirtualPath absolutePath = ConvertToAbsolutePath(path);
            try
            {
                // Assume GetNode returns null or throws an exception if not found
                return GetNode(absolutePath, followLinks);
            }
            catch (VirtualNodeNotFoundException)
            {
                return null; // Return null if the node does not exist
            }
        }

        /// <summary>
        /// Resolves the target of the symbolic link at the specified path.
        /// </summary>
        /// <param name="path">The path of the symbolic link.</param>
        /// <returns>The target path.</returns>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node cannot be found.</exception>
        public VirtualPath ResolveLinkTarget(VirtualPath path)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNodeContext nodeContext = WalkPathToTarget(path, null, null, true, true);
            return nodeContext.ResolvedPath!;
        }

        /// <summary>
        /// Tries to resolve the target of the symbolic link at the specified path.
        /// </summary>
        /// <param name="path">The path of the symbolic link.</param>
        /// <returns>The target path, or null if the node cannot be found.</returns>
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

        /// <summary>
        /// Gets the directory at the specified path.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The <see cref="VirtualDirectory"/> at the specified path.</returns>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node cannot be found.</exception>
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
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, node.Name));
            }
        }

        /// <summary>
        /// Tries to get the directory at the specified path.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The found <see cref="VirtualDirectory"/>, or null if not found.</returns>
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

        /// <summary>
        /// Gets the item at the specified path.
        /// </summary>
        /// <param name="path">The path of the item.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The <see cref="VirtualItem{T}"/> at the specified path.</returns>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node cannot be found.</exception>
        public VirtualItem<T> GetItem(VirtualPath path, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNode node = GetNode(path, followLinks);

            if (node is VirtualItem<T> item)
            {
                return item;
            }
            else
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, node.Name));
            }
        }

        /// <summary>
        /// Tries to get the item at the specified path.
        /// </summary>
        /// <param name="path">The path of the item.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The found <see cref="VirtualItem{T}"/>, or null if not found.</returns>
        public VirtualItem<T>? TryGetItem(VirtualPath path, bool followLinks = false)
        {
            try
            {
                return GetItem(path, followLinks);
            }
            catch (VirtualNodeNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the symbolic link at the specified path.
        /// </summary>
        /// <param name="path">The path of the symbolic link.</param>
        /// <returns>The <see cref="VirtualSymbolicLink"/> at the specified path.</returns>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node cannot be found.</exception>
        /// <remarks>This method does not have a followLinks parameter because it retrieves the symbolic link itself.</remarks>
        public VirtualSymbolicLink GetSymbolicLink(VirtualPath path)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();

            VirtualPath directoryPath = ResolveLinkTarget(path.DirectoryPath);
            VirtualNodeName nodeName = path.NodeName;

            VirtualNode node = GetNode(directoryPath + nodeName);

            if (node is VirtualSymbolicLink link)
            {
                return link;
            }
            else
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, node.Name));
            }
        }

        /// <summary>
        /// Tries to get the symbolic link at the specified path.
        /// </summary>
        /// <param name="path">The path of the symbolic link.</param>
        /// <returns>The found <see cref="VirtualSymbolicLink"/>, or null if not found.</returns>
        /// <remarks>
        /// This method does not have a followLinks parameter because it retrieves the symbolic link itself.
        /// </remarks>
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

        /// <summary>
        /// Gets the node type at the specified path.
        /// </summary>
        /// <param name="path">The node's path.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The <see cref="VirtualNodeType"/> of the node.</returns>
        public VirtualNodeType GetNodeType(VirtualPath path, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNode? node = TryGetNode(path, followLinks);

            return node?.NodeType ?? VirtualNodeType.None;
        }

        /// <summary>
        /// Gets the nodes in the path tree starting from the specified path.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="nodeType">The node type filter.</param>
        /// <param name="recursive">Whether to retrieve nodes recursively.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The nodes in the path tree starting from the specified path <see cref="IEnumerable{T}"/>.</returns>
        /// <remarks>This method may be integrated, reorganized, or deprecated in the near future.</remarks>
        public IEnumerable<VirtualNode> GetNodes(VirtualPath basePath, VirtualNodeTypeFilter nodeType = VirtualNodeTypeFilter.All, bool recursive = false, bool followLinks = false)
        {
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(basePath, nodeType, recursive, followLinks);
            IEnumerable<VirtualNode> nodes = nodeContexts.Select(info => info.Node!);
            return nodes;
        }

        /// <summary>
        /// Gets the nodes in the path tree starting from the current path.
        /// </summary>
        /// <param name="nodeType">The node type filter.</param>
        /// <param name="recursive">Whether to retrieve nodes recursively.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The nodes in the path tree starting from the current path <see cref="IEnumerable{T}"/>.</returns>
        /// <remarks>This method may be integrated, reorganized, or deprecated in the near future.</remarks>
        public IEnumerable<VirtualNode> GetNodes(VirtualNodeTypeFilter nodeType = VirtualNodeTypeFilter.All, bool recursive = false, bool followLinks = false)
        {
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(CurrentPath, nodeType, recursive, followLinks);
            IEnumerable<VirtualNode> nodes = nodeContexts.Select(info => info.Node!);
            return nodes;
        }

        /// <summary>
        /// Gets the paths in the path tree starting from the specified path.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="nodeType">The node type filter.</param>
        /// <param name="recursive">Whether to retrieve paths recursively.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The paths in the path tree starting from the specified path <see cref="IEnumerable{T}"/>.</returns>
        /// <remarks>This method may be integrated, reorganized, or deprecated in the near future.</remarks>
        public IEnumerable<VirtualPath> GetNodesWithPaths(VirtualPath basePath, VirtualNodeTypeFilter nodeType = VirtualNodeTypeFilter.All, bool recursive = false, bool followLinks = false)
        {
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(basePath, nodeType, recursive, followLinks);
            IEnumerable<VirtualPath> paths = nodeContexts.Select(info => info.TraversalPath);
            return paths;
        }

        /// <summary>
        /// Gets the paths in the path tree starting from the current path.
        /// </summary>
        /// <param name="nodeType">The node type filter.</param>
        /// <param name="recursive">Whether to retrieve paths recursively.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>The paths in the path tree starting from the current path <see cref="IEnumerable{T}"/>.</returns>
        /// <remarks>This method may be integrated, reorganized, or deprecated in the near future.</remarks>
        public IEnumerable<VirtualPath> GetNodesWithPaths(VirtualNodeTypeFilter nodeType = VirtualNodeTypeFilter.All, bool recursive = false, bool followLinks = false)
        {
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(CurrentPath, nodeType, recursive, followLinks);
            IEnumerable<VirtualPath> paths = nodeContexts.Select(info => info.TraversalPath);
            return paths;
        }

        /// <summary>
        /// Expands a path that contains wildcards.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="filter">The node type filter.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <param name="resolveLinks">Whether to resolve symbolic links.</param>
        /// <returns>The expanded paths <see cref="IEnumerable{T}"/>.</returns>
        /// <remarks>
        /// <para>followLinks indicates whether to follow links during the recursive traversal of nodes when the terminal node of the path is a symbolic link.</para>
        /// <para>resolveLinks indicates whether to resolve links when non-terminal nodes of the path are symbolic links.</para>
        /// </remarks>
        public IEnumerable<VirtualPath> ExpandPath(VirtualPath path, VirtualNodeTypeFilter filter = VirtualNodeTypeFilter.All, bool followLinks = true, bool resolveLinks = true)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualPath fixedPath = path.FixedPath;
            IEnumerable<VirtualNodeContext> nodeContexts = ExpandPathTree(path, filter, followLinks, resolveLinks);
            IEnumerable<VirtualPath> resolvedPaths = nodeContexts.Select(info => (fixedPath + info.TraversalPath).NormalizePath());

            return resolvedPaths;
        }

        /// <summary>
        /// Checks if a node exists at the specified path.
        /// </summary>
        /// <param name="path">The node's path.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>True if the node exists; otherwise, false.</returns>
        public bool NodeExists(VirtualPath path, bool followLinks = false)
        {
            VirtualPath absolutePath = ConvertToAbsolutePath(path);
            var node = TryGetNode(absolutePath, followLinks);
            return node != null; // Determine existence if the node is not null
        }

        /// <summary>
        /// Checks if a directory exists at the specified path.
        /// </summary>
        /// <param name="path">The directory's path.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>True if the directory exists; otherwise, false.</returns>
        public bool DirectoryExists(VirtualPath path, bool followLinks = false)
        {
            if (path.IsRoot)
            {
                return true; // The root directory always exists
            }

            VirtualPath absolutePath = ConvertToAbsolutePath(path);
            VirtualDirectory? directory = TryGetDirectory(absolutePath, followLinks);
            return directory != null;
        }

        /// <summary>
        /// Checks if an item exists at the specified path.
        /// </summary>
        /// <param name="path">The item's path.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <returns>True if the item exists; otherwise, false.</returns>
        public bool ItemExists(VirtualPath path, bool followLinks = false)
        {
            VirtualPath absolutePath = ConvertToAbsolutePath(path);
            var node = TryGetNode(absolutePath, followLinks);
            if (node == null) return false;
            var nodeType = node.GetType();
            return nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeof(VirtualItem<>);
        }

        /// <summary>
        /// Checks if a symbolic link exists at the specified path.
        /// </summary>
        /// <param name="path">The symbolic link's path.</param>
        /// <returns>True if the symbolic link exists; otherwise, false.</returns>
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
    }
}
