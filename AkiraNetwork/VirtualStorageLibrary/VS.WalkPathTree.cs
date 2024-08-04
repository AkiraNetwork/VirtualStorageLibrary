using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
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

            // 循環参照のクリア
            CycleDetectorForTree.Clear();

            foreach (var result in WalkPathTreeInternal(p))
            {
                yield return result;
            }
        }

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

            // 循環参照のクリア
            CycleDetectorForTree.Clear();

            foreach (var result in WalkPathTreeInternal(p))
            {
                yield return result;
            }
        }

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

            // ノードの種類に応じて処理を分岐
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

                                // ディレクトリを通知
                                yield return new VirtualNodeContext(directory, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                            }
                        }
                    }
                    else
                    {
                        VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);

                        // ディレクトリを通知
                        yield return new VirtualNodeContext(directory, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                    }
                }

                if (p.Recursive || 0 == p.CurrentDepth)
                {
                    // ディレクトリ内のノードを再帰的に探索
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

                                // アイテムを通知
                                yield return new VirtualNodeContext(item, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                            }
                        }
                    }
                    else
                    {
                        VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);

                        // アイテムを通知
                        yield return new VirtualNodeContext(item, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                    }
                }
            }
            else if (p.BaseNode is VirtualSymbolicLink link)
            {
                if (p.FollowLinks && link.TargetPath != null)
                {
                    VirtualPath? linkTargetPath = link.TargetPath;

                    // シンボリックリンクのリンク先パスを絶対パスに変換
                    linkTargetPath = ConvertToAbsolutePath(linkTargetPath, p.CurrentPath).NormalizePath();

                    // リンク先のノードを取得
                    VirtualNode? linkTargetNode = GetNode(linkTargetPath, p.FollowLinks);

                    // リンク先のノードに対して再帰的に探索
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

                    // 循環参照チェック
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

                                    // シンボリックリンクを通知
                                    yield return new VirtualNodeContext(link, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                                }
                            }
                        }
                        else
                        {
                            VirtualPath traversalPath = p.CurrentPath.GetRelativePath(p.BasePath);

                            // シンボリックリンクを通知
                            yield return new VirtualNodeContext(link, traversalPath, p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                        }
                    }
                }
            }
        }

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

        [DebuggerStepThrough]
        private struct WalkPathTreeParameters(
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
            public VirtualPath BasePath { get; set; } = basePath;
            public VirtualPath CurrentPath { get; set; } = currentPath;
            public VirtualNode BaseNode { get; set; } = baseNode;
            public VirtualDirectory? ParentDirectory { get; set; } = parentDirectory;
            public int BaseDepth { get; set; } = baseDepth;
            public int CurrentDepth { get; set; } = currentDepth;
            public int CurrentIndex { get; set; } = currentIndex;
            public VirtualNodeTypeFilter Filter { get; set; } = filter;
            public bool Recursive { get; set; } = recursive;
            public bool FollowLinks { get; set; } = followLinks;
            public List<string>? PatternList { get; set; } = patternList;
            public VirtualSymbolicLink? ResolvedLink { get; set; } = resolvedLink;
        }
    }
}
