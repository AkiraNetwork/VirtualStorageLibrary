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
        /// Expands the virtual path containing wildcards.
        /// </summary>
        /// <param name="path">The virtual path containing wildcards</param>
        /// <param name="filter">The filter to apply for node types</param>
        /// <param name="followLinks">A flag indicating whether to follow
        /// symbolic links</param>
        /// <param name="resolveLinks">A flag indicating whether to resolve
        /// symbolic links</param>
        /// <returns>An enumerable of <see cref="VirtualNodeContext"/> containing
        /// information about the expanded nodes</returns>
        /// <remarks>
        /// <para>followLinks indicates whether to follow links during the
        /// recursive traversal process when the terminal node in the virtual
        /// path is a symbolic link.</para>
        /// <para>resolveLinks indicates whether to resolve links when nodes
        /// other than the terminal node in the virtual path are symbolic links.
        /// </para>
        /// </remarks>
        public IEnumerable<VirtualNodeContext> ExpandPathTree(
            VirtualPath path,
            VirtualNodeTypeFilter filter = VirtualNodeTypeFilter.All,
            bool followLinks = true,
            bool resolveLinks = true)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualPath fixedPath = path.FixedPath;
            int baseDepth = path.FixedDepth;
            VirtualNode node = GetNode(fixedPath, resolveLinks);

            List<string> patternList = path.PartsList.Select(node => node.Name).ToList();

            WalkPathTreeParameters p = new(
                fixedPath,
                fixedPath,
                node,
                null,
                baseDepth,
                0,
                0,
                filter,
                true,
                followLinks,
                patternList,
                null);

            // Clear cycle detection
            CycleDetectorForTree.Clear();

            foreach (var result in WalkPathTreeInternal(p))
            {
                yield return result;
            }
        }

        /// <summary>
        /// Traverses the path tree starting from the specified path.
        /// </summary>
        /// <param name="basePath">The base virtual path</param>
        /// <param name="filter">The filter to apply for node types</param>
        /// <param name="recursive">A flag indicating whether to traverse
        /// recursively</param>
        /// <param name="followLinks">A flag indicating whether to follow
        /// symbolic links</param>
        /// <param name="resolveLinks">A flag indicating whether to resolve
        /// symbolic links</param>
        /// <returns>An enumerable of <see cref="VirtualNodeContext"/> containing
        /// information about the traversed nodes</returns>
        /// <remarks>
        /// <para>followLinks indicates whether to follow links during the
        /// recursive traversal process when the terminal node in the virtual
        /// path is a symbolic link.</para>
        /// <para>resolveLinks indicates whether to resolve links when nodes
        /// other than the terminal node in the virtual path are symbolic links.
        /// </para>
        /// </remarks>
        public IEnumerable<VirtualNodeContext> WalkPathTree(
            VirtualPath basePath,
            VirtualNodeTypeFilter filter = VirtualNodeTypeFilter.All,
            bool recursive = true,
            bool followLinks = true,
            bool resolveLinks = true)
        {
            basePath = ConvertToAbsolutePath(basePath).NormalizePath();
            int baseDepth = basePath.FixedDepth;
            VirtualNode baseNode = GetNode(basePath, followLinks);

            VirtualNodeContext nodeContext = WalkPathToTarget(basePath, null, null, resolveLinks, true);
            VirtualPath traversalBasePath = nodeContext.TraversalPath;
            VirtualPath resolvedBasePath = nodeContext.ResolvedPath!;
            VirtualPath parentDirectoryPath = resolvedBasePath.DirectoryPath;
            VirtualDirectory parentDirectory = GetDirectory(parentDirectoryPath, followLinks);

            VirtualSymbolicLink? link = null;
            if (traversalBasePath != resolvedBasePath)
            {
                link = GetSymbolicLink(basePath);
            }

            WalkPathTreeParameters p = new(
                basePath,
                basePath,
                baseNode,
                parentDirectory,
                baseDepth,
                0,
                0,
                filter,
                recursive,
                followLinks,
                null,
                link);

            // Clear cycle detection
            CycleDetectorForTree.Clear();

            foreach (var result in WalkPathTreeInternal(p))
            {
                yield return result;
            }
        }

        /// <summary>
        /// Internal method for traversing the path tree.
        /// </summary>
        /// <param name="p">A structure containing parameters for the WalkPathTree
        /// method</param>
        /// <returns>An enumerable of <see cref="VirtualNodeContext"/> containing
        /// information about the traversed nodes</returns>
        private IEnumerable<VirtualNodeContext> WalkPathTreeInternal(WalkPathTreeParameters p)
        {
            IVirtualWildcardMatcher? wildcardMatcher = VirtualStorageState.State.WildcardMatcher;
            PatternMatch? patternMatcher;
            if (wildcardMatcher == null)
            {
                patternMatcher = null;
            }
            else
            {
                patternMatcher = wildcardMatcher.PatternMatcher;
            }

            // Process based on the node type
            if (p.BaseNode is VirtualDirectory directory)
            {
                if (p.Filter.HasFlag(VirtualNodeTypeFilter.Directory))
                {
                    if (patternMatcher != null && p.PatternList != null)
                    {
                        if (p.BaseDepth + p.CurrentDepth == p.PatternList.Count)
                        {
                            if (MatchPatterns(p.CurrentPath.PartsList, p.PatternList))
                            {
                                VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);

                                // Notify directory
                                yield return new VirtualNodeContext(directory, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                            }
                        }
                    }
                    else
                    {
                        VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);

                        // Notify directory
                        yield return new VirtualNodeContext(directory, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                    }
                }

                if (p.Recursive || 0 == p.CurrentDepth)
                {
                    // Recursively traverse nodes in the directory
                    int index = 0;
                    foreach (var node in directory.NodesView)
                    {
                        VirtualPath path = p.CurrentPath + node.Name;

                        WalkPathTreeParameters p2 = new(
                            p.BasePath,
                            path,
                            node,
                            directory,
                            p.BaseDepth,
                            p.CurrentDepth + 1,
                            index,
                            p.Filter,
                            p.Recursive,
                            p.FollowLinks,
                            p.PatternList,
                            null);

                        foreach (var result in WalkPathTreeInternal(p2))
                        {
                            yield return result;
                        }
                        index++;
                    }
                }
            }
            else if (p.BaseNode is VirtualItem item)
            {
                if (p.Filter.HasFlag(VirtualNodeTypeFilter.Item))
                {
                    if (patternMatcher != null && p.PatternList != null)
                    {
                        if (p.BaseDepth + p.CurrentDepth == p.PatternList.Count)
                        {
                            if (MatchPatterns(p.CurrentPath.PartsList, p.PatternList))
                            {
                                VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);

                                // Notify item
                                yield return new VirtualNodeContext(item, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                            }
                        }
                    }
                    else
                    {
                        VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);

                        // Notify item
                        yield return new VirtualNodeContext(item, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                    }
                }
            }
            else if (p.BaseNode is VirtualSymbolicLink link)
            {
                if (p.FollowLinks && link.TargetPath != null)
                {
                    VirtualPath? linkTargetPath = link.TargetPath;

                    // Convert the symbolic link's target path to an absolute path
                    linkTargetPath = ConvertToAbsolutePath(linkTargetPath, p.CurrentPath).NormalizePath();

                    // Retrieve the target node of the link
                    VirtualNode? linkTargetNode = GetNode(linkTargetPath, p.FollowLinks);

                    // Recursively traverse the target node of the link
                    WalkPathTreeParameters p2 = new(
                        p.BasePath,
                        p.CurrentPath,
                        linkTargetNode,
                        p.ParentDirectory,
                        p.BaseDepth,
                        p.CurrentDepth,
                        p.CurrentIndex,
                        p.Filter,
                        p.Recursive,
                        p.FollowLinks,
                        p.PatternList,
                        link);

                    VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);
                    VirtualPath traversalFullPath = p.BasePath + traversalPath;

                    // Cycle detection check
                    if (CycleDetectorForTree.IsNodeInCycle(link))
                    {
                        throw new InvalidOperationException(string.Format(Resources.CircularReferenceDetected, traversalFullPath, link));
                    }

                    foreach (var result in WalkPathTreeInternal(p2))
                    {
                        yield return result;
                    }
                }
                else
                {
                    if (p.Filter.HasFlag(VirtualNodeTypeFilter.SymbolicLink))
                    {
                        if (patternMatcher != null && p.PatternList != null)
                        {
                            if (p.BaseDepth + p.CurrentDepth == p.PatternList.Count)
                            {
                                if (MatchPatterns(p.CurrentPath.PartsList, p.PatternList))
                                {
                                    VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);

                                    // Notify symbolic link
                                    yield return new VirtualNodeContext(link, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                                }
                            }
                        }
                        else
                        {
                            VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);

                            // Notify symbolic link
                            yield return new VirtualNodeContext(link, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Matches the parts of the path with the pattern list.
        /// </summary>
        /// <param name="parts">The list of node parts</param>
        /// <param name="patternList">The list of patterns to match against</param>
        /// <returns>True if the parts match the patterns; otherwise, false</returns>
        static bool MatchPatterns(List<VirtualNodeName> parts, List<string> patternList)
        {
            IVirtualWildcardMatcher? wildcardMatcher = VirtualStorageState.State.WildcardMatcher!;

            PatternMatch? patternMatcher = wildcardMatcher.PatternMatcher;

            for (int i = 0; i < parts.Count; i++)
            {
                if (!patternMatcher(parts[i], patternList[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// A structure that holds parameters for the WalkPathTree method.
        /// </summary>
        private struct WalkPathTreeParameters
        {
            /// <summary>
            /// Gets or sets the base path.
            /// </summary>
            /// <value>The base path</value>
            public VirtualPath BasePath { get; set; }

            /// <summary>
            /// Gets or sets the current path being traversed.
            /// </summary>
            /// <value>The current path being traversed</value>
            public VirtualPath CurrentPath { get; set; }

            /// <summary>
            /// Gets or sets the base node.
            /// </summary>
            /// <value>The base node</value>
            public VirtualNode BaseNode { get; set; }

            /// <summary>
            /// Gets or sets the parent directory of the base node.
            /// </summary>
            /// <value>The parent directory of the base node</value>
            public VirtualDirectory? ParentDirectory { get; set; }

            /// <summary>
            /// Gets or sets the base depth.
            /// </summary>
            /// <value>The base depth</value>
            public int BaseDepth { get; set; }

            /// <summary>
            /// Gets or sets the current depth.
            /// </summary>
            /// <value>The current depth</value>
            public int CurrentDepth { get; set; }

            /// <summary>
            /// Gets or sets the current index.
            /// </summary>
            /// <value>The current index</value>
            public int CurrentIndex { get; set; }

            /// <summary>
            /// Gets or sets the filter for node types.
            /// </summary>
            /// <value>The filter for node types</value>
            public VirtualNodeTypeFilter Filter { get; set; }

            /// <summary>
            /// Gets or sets the flag indicating whether to traverse recursively.
            /// </summary>
            /// <value>The flag indicating whether to traverse recursively</value>
            public bool Recursive { get; set; }

            /// <summary>
            /// Gets or sets the flag indicating whether to follow symbolic links.
            /// </summary>
            /// <value>The flag indicating whether to follow symbolic links</value>
            public bool FollowLinks { get; set; }

            /// <summary>
            /// Gets or sets the pattern list.
            /// </summary>
            /// <value>The pattern list</value>
            public List<string>? PatternList { get; set; }

            /// <summary>
            /// Gets or sets the resolved symbolic link.
            /// </summary>
            /// <value>The resolved symbolic link</value>
            public VirtualSymbolicLink? ResolvedLink { get; set; }

            /// <summary>
            /// Initializes a new instance of the WalkPathTreeParameters structure.
            /// </summary>
            /// <param name="basePath">The base path</param>
            /// <param name="currentPath">The current path being traversed</param>
            /// <param name="baseNode">The base node</param>
            /// <param name="parentDirectory">The parent directory of the base node
            /// </param>
            /// <param name="baseDepth">The base depth</param>
            /// <param name="currentDepth">The current depth</param>
            /// <param name="currentIndex">The current index</param>
            /// <param name="filter">The filter for node types</param>
            /// <param name="recursive">The flag indicating whether to traverse
            /// recursively</param>
            /// <param name="followLinks">The flag indicating whether to follow
            /// symbolic links</param>
            /// <param name="patternList">The pattern list</param>
            /// <param name="resolvedLink">The resolved symbolic link</param>
            public WalkPathTreeParameters(
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
                List<string>? patternList,
                VirtualSymbolicLink? resolvedLink)
            {
                BasePath = basePath;
                CurrentPath = currentPath;
                BaseNode = baseNode;
                ParentDirectory = parentDirectory;
                BaseDepth = baseDepth;
                CurrentDepth = currentDepth;
                CurrentIndex = currentIndex;
                Filter = filter;
                Recursive = recursive;
                FollowLinks = followLinks;
                PatternList = patternList;
                ResolvedLink = resolvedLink;
            }
        }
    }
}
