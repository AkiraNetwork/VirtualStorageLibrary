﻿using AkiraNetwork.VirtualStorageLibrary.Localization;
using System.Runtime.CompilerServices;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        private readonly VirtualDirectory _root;

        public VirtualDirectory Root => _root;

        public VirtualPath CurrentPath { get; private set; }

        // Adapterインスタンスをプロパティとして保持
        public VirtualItemAdapter<T> Item { get; }
        public VirtualDirectoryAdapter<T> Dir { get; }
        public VirtualSymbolicLinkAdapter<T> Link { get; }

        // 循環参照検出クラス(WalkPathToTargetメソッド用)
        public VirtualCycleDetector CycleDetectorForTarget { get; } = new();

        // 循環参照検出クラス(WalkPathTreeメソッド用)
        public VirtualCycleDetector CycleDetectorForTree { get; } = new();

        public VirtualStorage()
        {
            _root = new(VirtualPath.Root)
            {
                IsReferencedInStorage = true
            };

            CurrentPath = VirtualPath.Root;

            _linkDictionary = [];

            Item = new(this);
            Dir = new(this);
            Link = new(this);
        }

        public void ChangeDirectory(VirtualPath path)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNodeContext? nodeContext = WalkPathToTarget(path, null, null, true, true);
            CurrentPath = nodeContext.TraversalPath;

            return;
        }

        public void SetNode(VirtualPath destinationPath, VirtualNode node)
        {
            destinationPath = ConvertToAbsolutePath(destinationPath).NormalizePath();

            switch (node)
            {
                case VirtualDirectory directory:
                    UpdateDirectory(destinationPath, directory);
                    break;

                case VirtualSymbolicLink symbolicLink:
                    UpdateSymbolicLInk(destinationPath, symbolicLink);
                    break;

                case VirtualItem<T> item:
                    UpdateItem(destinationPath, item);
                    break;
            }
        }

        public void UpdateItem(VirtualPath itemPath, VirtualItem<T> newItem)
        {
            // 絶対パスに変換
            itemPath = ConvertToAbsolutePath(itemPath).NormalizePath();

            // 既存のアイテムを取得
            VirtualItem<T> item = GetItem(itemPath, true);

            // 既存アイテムのデータを更新
            item.Update(newItem);
        }

        public void UpdateDirectory(VirtualPath directoryPath, VirtualDirectory newDirectory)
        {
            // 絶対パスに変換
            directoryPath = ConvertToAbsolutePath(directoryPath).NormalizePath();

            // 既存のディレクトリを取得
            VirtualDirectory directory = GetDirectory(directoryPath, true);

            // 既存ディレクトリのノードを更新
            directory.Update(newDirectory);
        }

        public void UpdateSymbolicLInk(VirtualPath linkPath, VirtualSymbolicLink newLink)
        {
            // 絶対パスに変換
            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            // 既存のシンボリックリンクを取得
            // シンボリックリンクの場合、パス解決をするのは無意味なので followLinks は false固定
            VirtualSymbolicLink link = GetSymbolicLink(linkPath);

            // 既存シンボリックリンクのノードを更新
            link.Update(newLink);
        }

        public void AddNode(VirtualPath nodeDirectoryPath, VirtualNode node, bool overwrite = false)
        {
            nodeDirectoryPath = ConvertToAbsolutePath(nodeDirectoryPath).NormalizePath();

            switch (node)
            {
                case VirtualDirectory directory:
                    AddDirectory(nodeDirectoryPath, directory);
                    break;

                case VirtualSymbolicLink symbolicLink:
                    AddSymbolicLink(nodeDirectoryPath, symbolicLink, overwrite);
                    break;

                case VirtualItem<T> item:
                    AddItem(nodeDirectoryPath, item, overwrite);
                    break;
            }
        }

        public void AddDirectory(VirtualPath directoryPath, VirtualDirectory directory, bool createSubdirectories = false)
        {
            directoryPath = ConvertToAbsolutePath(directoryPath).NormalizePath();

            VirtualNodeContext nodeContext;

            if (createSubdirectories)
            {
                nodeContext = WalkPathToTarget(directoryPath, null, CreateIntermediateDirectory, true, true);
            }
            else
            {
                nodeContext = WalkPathToTarget(directoryPath, null, null, true, true);
            }

            if (nodeContext.Node is VirtualDirectory parentDirectory)
            {
                if (parentDirectory.NodeExists(directory.Name))
                {
                    throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, directory.Name));
                }

                // 新しいディレクトリを追加
                directory = (VirtualDirectory)parentDirectory.Add(directory);

                // 全てのターゲットノードタイプを更新
                UpdateAllTargetNodeTypesInDictionary();
            }
            else
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeIsNotVirtualDirectory, nodeContext.Node!.Name));
            }

            return;
        }

        public void AddDirectory(VirtualPath path, bool createSubdirectories = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();

            if (path.IsRoot)
            {
                throw new InvalidOperationException(Resources.RootAlreadyExists);
            }

            VirtualPath directoryPath = path.DirectoryPath;
            VirtualNodeName newDirectoryName = path.NodeName;

            VirtualDirectory newDirectory = new(newDirectoryName);
            AddDirectory(directoryPath, newDirectory, createSubdirectories);
        }

        private bool CreateIntermediateDirectory(VirtualDirectory parentDirectory, VirtualNodeName nodeName, VirtualPath nodePath)
        {
            VirtualDirectory newSubdirectory = new(nodeName);

            // 中間ディレクトリを追加
            parentDirectory.Add(newSubdirectory);

            // 全てのターゲットノードタイプを更新
            UpdateAllTargetNodeTypesInDictionary();

            return true;
        }

        public void AddItem(VirtualPath itemDirectoryPath, VirtualItem<T> item, bool overwrite = false)
        {
            // 絶対パスに変換
            itemDirectoryPath = ConvertToAbsolutePath(itemDirectoryPath).NormalizePath();

            // アイテム追加対象ディレクトリを取得
            VirtualDirectory directory = GetDirectory(itemDirectoryPath, true);

            // 既存のアイテムの存在チェック
            if (directory.NodeExists(item.Name))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, item.Name));
                }
                else
                {
                    // 上書き対象がアイテムでない場合は例外をスロー
                    if (!ItemExists(itemDirectoryPath + item.Name))
                    {
                        throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualItem, item.Name, typeof(T).Name));
                    }
                    // 既存アイテムの削除
                    directory.Remove(item);
                }
            }

            // 新しいアイテムを追加
            directory.Add(item, overwrite);

            // 全てのターゲットノードタイプを更新
            UpdateAllTargetNodeTypesInDictionary();
        }

        public void AddItem(VirtualPath itemPath, T? data = default, bool overwrite = false)
        {
            // 絶対パスに変換
            itemPath = ConvertToAbsolutePath(itemPath).NormalizePath();

            // ディレクトリパスとアイテム名を分離
            VirtualPath directoryPath = itemPath.DirectoryPath;
            VirtualNodeName itemName = itemPath.NodeName;

            // アイテムを作成
            VirtualItem<T> item = new(itemName, data);

            // AddItemメソッドを呼び出し
            AddItem(directoryPath, item, overwrite);
        }

        public void AddSymbolicLink(VirtualPath linkDirectoryPath, VirtualSymbolicLink link, bool overwrite = false)
        {
            // linkDirectoryPathを絶対パスに変換し正規化も行う
            linkDirectoryPath = ConvertToAbsolutePath(linkDirectoryPath).NormalizePath();

            // シンボリックリンク追加対象ディレクトリを取得
            VirtualDirectory directory = GetDirectory(linkDirectoryPath, true);

            // 既存のノードの存在チェック
            if (directory.NodeExists(link.Name))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, link.Name));
                }
                else
                {
                    // 既存のノードがシンボリックリンクでない場合は例外をスロー
                    if (!SymbolicLinkExists(linkDirectoryPath + link.Name))
                    {
                        throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualSymbolicLink, link.Name));
                    }
                    // 既存のシンボリックリンクを削除
                    directory.Remove(link);
                }
            }

            // 新しいシンボリックリンクを追加
            link = (VirtualSymbolicLink)directory.Add(link);

            if (link.TargetPath != null)
            {
                // シンボリックリンクを作成したディレクトリパスを基準とする
                VirtualPath absoluteTargetPath = ConvertToAbsolutePath(link.TargetPath!, linkDirectoryPath).NormalizePath();

                // リンク辞書にリンク情報を追加
                AddLinkToDictionary(absoluteTargetPath, linkDirectoryPath + link.Name);
            }

            // 全てのターゲットノードタイプを更新
            UpdateAllTargetNodeTypesInDictionary();
        }

        public void AddSymbolicLink(VirtualPath linkPath, VirtualPath? targetPath = null, bool overwrite = false)
        {
            // linkPathを絶対パスに変換し正規化も行う
            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            // ディレクトリパスとリンク名を分離
            VirtualPath directoryPath = linkPath.DirectoryPath;
            VirtualNodeName linkName = linkPath.NodeName;

            // シンボリックリンクを作成
            VirtualSymbolicLink link = new(linkName, targetPath);

            // AddSymbolicLinkメソッドを呼び出し
            AddSymbolicLink(directoryPath, link, overwrite);
        }

        public VirtualNodeContext WalkPathToTarget(
            VirtualPath targetPath,
            NotifyNodeDelegate? notifyNode = null,
            ActionNodeDelegate? actionNode = null,
            bool followLinks = true,
            bool exceptionEnabled = true)
        {
            targetPath = ConvertToAbsolutePath(targetPath).NormalizePath();

            WalkPathToTargetParameters p = new(
                targetPath,
                0,
                VirtualPath.Root,
                null,
                _root,
                notifyNode,
                actionNode,
                followLinks,
                exceptionEnabled,
                false);

            // 循環参照のクリア
            CycleDetectorForTarget.Clear();

            VirtualNodeContext? nodeContext = WalkPathToTargetInternal(p);

            return nodeContext;
        }

        private VirtualNodeContext WalkPathToTargetInternal(WalkPathToTargetParameters p)
        {
            // ターゲットがルートディレクトリの場合は、ルートノードを通知して終了
            if (p.TargetPath.IsRoot)
            {
                p.NotifyNode?.Invoke(VirtualPath.Root, _root);
                return new VirtualNodeContext(_root, VirtualPath.Root, null, -1, -1, VirtualPath.Root, p.Resolved);
            }

            VirtualNodeName traversalNodeName = p.TargetPath.PartsList[p.TraversalIndex];

            while (!p.TraversalDirectory.NodeExists(traversalNodeName))
            {
                VirtualPath traversalPath = p.TraversalPath + traversalNodeName;

                if (p.ActionNode != null)
                {
                    if (p.ActionNode(p.TraversalDirectory, traversalNodeName, traversalPath))
                    {
                        continue;
                    }
                }

                // 例外が有効な場合は例外をスロー
                if (p.ExceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, traversalPath));
                }

                return new VirtualNodeContext(null, traversalPath, p.TraversalDirectory, -1, -1, traversalPath, p.Resolved);
            }

            VirtualNodeContext nodeContext;

            // 探索ノードを取得
            VirtualNode node = p.TraversalDirectory[traversalNodeName];

            // 探索パスを更新
            p.TraversalPath += traversalNodeName;

            // 次のノードへ
            p.TraversalIndex++;

            if (node is VirtualDirectory directory)
            {
                // ディレクトリの場合

                // 最後のノードに到達したかチェック
                if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                {
                    // 末端のノードを通知
                    p.NotifyNode?.Invoke(p.TraversalPath, directory);
                    p.ResolvedPath ??= p.TraversalPath;
                    return new VirtualNodeContext(directory, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved);
                }

                // 途中のノードを通知
                p.NotifyNode?.Invoke(p.TraversalPath, directory);

                // 探索ディレクトリを取得
                p.TraversalDirectory = directory;

                // 再帰的に探索
                nodeContext = WalkPathToTargetInternal(p);

                return nodeContext;

            }
            else if (node is VirtualItem<T> item)
            {
                // アイテムの場合

                // 末端のノードを通知
                p.NotifyNode?.Invoke(p.TraversalPath, item);

                // 最後のノードに到達したかチェック
                if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                {
                    p.ResolvedPath ??= p.TraversalPath;
                    return new VirtualNodeContext(item, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved);
                }

                p.ResolvedPath ??= p.TraversalPath;

                // 例外が有効な場合は例外をスロー
                if (p.ExceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException(string.Format(Resources.CannotReachBecauseNodeItem, p.TargetPath, p.TraversalPath));
                }

                return new VirtualNodeContext(null, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved);
            }
            else
            {
                // シンボリックリンクの場合

                VirtualSymbolicLink link = (VirtualSymbolicLink)node;
                if (!p.FollowLinks || link.TargetPath == null)
                {
                    // シンボリックリンクを通知
                    p.NotifyNode?.Invoke(p.TraversalPath, link);

                    // 最後のノードに到達したかチェック
                    if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                    {
                        p.ResolvedPath ??= p.TargetPath;
                        return new VirtualNodeContext(link, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved, link);
                    }

                    p.ResolvedPath ??= p.TraversalPath;

                    // 例外が有効な場合は例外をスロー
                    if (p.ExceptionEnabled)
                    {
                        throw new VirtualNodeNotFoundException(string.Format(Resources.CannotReachBecauseNodeSymbolicLink, p.TargetPath, p.TraversalPath));
                    }

                    return new VirtualNodeContext(null, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved, link);
                }

                // パス探索中、一度でもシンボリックリンクを解決したら true を設定
                p.Resolved = true;

                VirtualPath? linkTargetPath = link.TargetPath;
                VirtualPath parentTraversalPath = p.TraversalPath.DirectoryPath;

                // シンボリックリンクのリンク先パスを絶対パスに変換
                linkTargetPath = ConvertToAbsolutePath(linkTargetPath, parentTraversalPath);

                // リンク先のパスを正規化する
                linkTargetPath = linkTargetPath.NormalizePath();

                // シンボリックリンクのリンク先パスを再帰的に探索
                WalkPathToTargetParameters p2 = new(
                    linkTargetPath,
                    0,
                    VirtualPath.Root,
                    null,
                    _root,
                    null,
                    null,
                    true,
                    p.ExceptionEnabled,
                    p.Resolved);

                // 循環参照チェック
                if (CycleDetectorForTarget.IsNodeInCycle(link))
                {
                    throw new InvalidOperationException(string.Format(Resources.CircularReferenceDetected, p.TraversalPath, link));
                }

                nodeContext = WalkPathToTargetInternal(p2);

                VirtualNode? resolvedNode = nodeContext.Node;

                // 解決済みのパスに未探索のパスを追加
                p.ResolvedPath = nodeContext.ResolvedPath!.CombineFromIndex(p.TargetPath, p.TraversalIndex);

                // シンボリックリンクを通知
                p.NotifyNode?.Invoke(p.TraversalPath, resolvedNode);

                if (resolvedNode != null && resolvedNode is VirtualDirectory linkDirectory)
                {
                    // 最後のノードに到達したかチェック
                    if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                    {
                        // 末端のノードを通知
                        nodeContext.TraversalPath = p.TraversalPath;
                        nodeContext.ResolvedPath = p.ResolvedPath;

                        return nodeContext;
                    }

                    // 探索ディレクトリを取得
                    p.TraversalDirectory = linkDirectory;

                    // 再帰的に探索
                    nodeContext = WalkPathToTargetInternal(p);

                    return nodeContext;
                }

                return new VirtualNodeContext(resolvedNode, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved, link);
            }
        }

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

        private void CheckCopyPreconditions(VirtualPath sourcePath, VirtualPath destinationPath, bool followLinks, bool recursive)
        {
            VirtualPath absoluteSourcePath = ConvertToAbsolutePath(sourcePath).NormalizePath();
            VirtualPath absoluteDestinationPath = ConvertToAbsolutePath(destinationPath).NormalizePath();

            // コピー元の存在確認
            if (!NodeExists(absoluteSourcePath, true))
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, absoluteSourcePath.NodeName));
            }

            // コピー元とコピー先が同じ場合は例外をスロー
            if (absoluteSourcePath == absoluteDestinationPath)
            {
                throw new InvalidOperationException(string.Format(Resources.SourceAndDestinationPathSameForCopy, absoluteSourcePath, absoluteDestinationPath));
            }

            // 循環参照チェック
            if (recursive)
            {
                if (VirtualPath.ArePathsSubdirectories(absoluteSourcePath, absoluteDestinationPath))
                {
                    throw new InvalidOperationException(string.Format(Resources.RecursiveSubdirectoryConflict, absoluteSourcePath, absoluteDestinationPath));
                }
            }

            // コピー元ツリーの探索 (コピー元の存在確認を行い、問題があれば例外をスロー。返却値は破棄)
            IEnumerable<VirtualNodeContext> _ = WalkPathTree(absoluteSourcePath, VirtualNodeTypeFilter.All, recursive, followLinks).ToList();
        }

        public void CopyNode(
            VirtualPath sourcePath,
            VirtualPath destinationPath,
            bool overwrite = false,
            bool recursive = false,
            bool followLinks = false,
            List<VirtualNodeContext>? destinationContextList = null)
        {
            CheckCopyPreconditions(sourcePath, destinationPath, followLinks, recursive);

            sourcePath = ConvertToAbsolutePath(sourcePath).NormalizePath();
            destinationPath = ConvertToAbsolutePath(destinationPath).NormalizePath();

            // コピー元のツリーを取得
            IEnumerable<VirtualNodeContext> sourceContexts = WalkPathTree(sourcePath, VirtualNodeTypeFilter.All, recursive, followLinks);

            VirtualNode sourceNode = sourceContexts.First().Node!;

            CopySingleInternal(sourcePath, sourceNode, destinationPath, null, overwrite, followLinks, destinationContextList);

            if (recursive && sourceNode is VirtualDirectory sourceDirectory)
            {
                foreach (var sourceContext in sourceContexts.Skip(1))
                {
                    VirtualPath sourceSubPath = sourcePath + sourceContext.TraversalPath;
                    VirtualPath destinationSubPath = destinationPath + sourceDirectory.Name + sourceSubPath.GetRelativePath(sourcePath);
                    VirtualNode sourceSubNode = sourceContext.Node!;

                    CopySingleInternal(sourceSubPath, sourceSubNode, destinationSubPath, null, overwrite, followLinks, destinationContextList);
                }
            }

            return;
        }

        private void CopySingleInternal(
            VirtualPath sourcePath,
            VirtualNode sourceNode,
            VirtualPath destinationPath,
            VirtualPath? linkOriginalPath,
            bool overwrite,
            bool followLinks,
            List<VirtualNodeContext>? destinationContextList)
        {
            VirtualNodeName? newNodeName;
            VirtualDirectory destinationDirectory;
            VirtualNode? destinationNode;

            if (destinationPath.IsRoot)
            {
                destinationDirectory = _root;
                destinationNode = _root;
            }
            else
            {
                destinationDirectory = GetDirectory(destinationPath.DirectoryPath, true);
                destinationNode = destinationDirectory.Get(destinationPath.NodeName, false);
            }

            bool overwriteDirectory = false;

            // コピー先ノードが存在する場合
            switch (destinationNode)
            {
                case VirtualDirectory directory:
                    destinationDirectory = directory;
                    newNodeName = sourceNode.Name;
                    VirtualNode? node = destinationDirectory.Get(newNodeName, false);
                    if (node != null)
                    {
                        if (overwrite && (node is VirtualDirectory existingDirectory))
                        {
                            overwriteDirectory = true;

                            // 同名ディレクトリへの上書きの場合、実質、上書きはしないが更新日付は更新する。
                            existingDirectory.UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, newNodeName));
                        }
                    }
                    destinationPath += newNodeName;
                    break;

                case VirtualItem<T> item:
                    if (overwrite)
                    {
                        destinationDirectory.Remove(item);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, item.Name));
                    }
                    newNodeName = destinationPath.NodeName;
                    break;

                case VirtualSymbolicLink link:
                    VirtualPath targetPath = ConvertToAbsolutePath(link.TargetPath).NormalizePath();
                    CopySingleInternal(sourcePath, sourceNode, targetPath, destinationPath, overwrite, followLinks, destinationContextList);
                    return;

                default:
                    newNodeName = destinationPath.NodeName;
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

            // リンクオリジナルパスが指定されている場合は設定
            destinationPath = linkOriginalPath ?? destinationPath;

            // パスの深さを計算
            int depth = destinationPath.Depth - 1;

            if (destinationContextList != null)
            {
                // コピー操作の結果を表す VirtualNodeContext を生成して返却
                VirtualNodeContext context = new(
                    newNode,
                    destinationPath,
                    destinationDirectory,
                    depth,
                    0
                );
                destinationContextList.Add(context);
            }

            return;
        }

        public void RemoveNode(VirtualPath nodePath, bool recursive = false, bool followLinks = false, bool resolveLinks = true)
        {
            nodePath = ConvertToAbsolutePath(nodePath).NormalizePath();

            if (nodePath.IsRoot)
            {
                throw new InvalidOperationException(Resources.CannotRemoveRoot);
            }

            VirtualPath resolvedNodePath = nodePath;
            if (resolveLinks)
            {
                resolvedNodePath = ResolveLinkTarget(nodePath);
            }

            IEnumerable<VirtualNodeContext> contexts = WalkPathTree(nodePath, VirtualNodeTypeFilter.All, recursive, followLinks, resolveLinks);

            if (recursive)
            {
                // ノードコンテキストを逆順に処理するためにリストに変換して逆順にソート
                IEnumerable<VirtualNodeContext> reversedContexts = contexts.Reverse();

                foreach (VirtualNodeContext context in reversedContexts)
                {
                    VirtualDirectory? parentDir = context.ParentDirectory;

                    // シンボリックリンクか判定
                    if (context.ResolvedLink != null)
                    {
                        // シンボリックリンクの場合はリンクノードを削除する
                        VirtualSymbolicLink link = context.ResolvedLink;
                        VirtualPath linkPath = nodePath + context.TraversalPath;
                        VirtualPath linkParentPath = linkPath.DirectoryPath;
                        parentDir = GetDirectory(linkParentPath, true);

                        // 削除するリンクのストレージ参照フラグをリセット
                        link.IsReferencedInStorage = false;

                        // 辞書からノードを削除
                        parentDir?.Remove(link);

                        // 全てのターゲットノードタイプを更新
                        UpdateAllTargetNodeTypesInDictionary();

                        // リンク辞書からリンクを削除
                        if (link.TargetPath != null)
                        {
                            VirtualPath? resolvedTargetPath = TryResolveLinkTarget(link.TargetPath);
                            if (resolvedTargetPath != null)
                            {
                                RemoveLinkFromDictionary(resolvedTargetPath, linkPath);
                            }
                        }

                        // リンクターゲットも削除する
                        VirtualPath targetPath = ConvertToAbsolutePath(link.TargetPath).NormalizePath();
                        RemoveNode(targetPath, recursive, followLinks);
                    }
                    else
                    {
                        // 通常のノードの場合はそのまま削除

                        VirtualNode node = context.Node!;
                        if (node is IDisposable disposableNode)
                        {
                            // 削除対象ノードがIDisposableを実装している場合はDisposeメソッドを呼び出す
                            disposableNode.Dispose();
                        }

                        // 削除するノードのストレージ参照フラグをリセット
                        node.IsReferencedInStorage = false;

                        // 辞書からノードを削除
                        parentDir?.Remove(node);

                        // 全てのターゲットノードタイプを更新
                        UpdateAllTargetNodeTypesInDictionary();
                    }
                }
            }
            else
            {
                VirtualNodeContext context = contexts.First();
                VirtualNode node = context.Node!;

                if (node is VirtualDirectory directory)
                {
                    if (directory.Count > 0)
                    {
                        throw new InvalidOperationException(string.Format(Resources.CannotRemoveNonEmptyDirectory, node.Name));
                    }
                }
                VirtualDirectory? parentDir = context.ParentDirectory;

                if (node is IDisposable disposableNode)
                {
                    // 削除対象ノードがIDisposableを実装している場合はDisposeメソッドを呼び出す
                    disposableNode.Dispose();
                }

                // 削除するノードのストレージ参照フラグをリセット
                node.IsReferencedInStorage = false;

                // 辞書からノードを削除
                parentDir?.Remove(node);

                // 全てのターゲットノードタイプを更新
                UpdateAllTargetNodeTypesInDictionary();

                // ノードがリンクの場合、リンク辞書からリンクを削除
                if (node is VirtualSymbolicLink link)
                {
                    if (link.TargetPath != null)
                    {
                        VirtualPath? resolvedTargetPath = TryResolveLinkTarget(link.TargetPath);
                        if (resolvedTargetPath != null)
                        {
                            RemoveLinkFromDictionary(resolvedTargetPath, resolvedNodePath);
                        }
                        else
                        {
                            RemoveLinkByLinkPath(resolvedNodePath);
                        }
                    }
                }
            }
        }

        public void SetNodeName(VirtualPath nodePath, VirtualNodeName newName, bool resolveLinks = true)
        {
            VirtualPath oldAbsolutePath = ConvertToAbsolutePath(nodePath);
            VirtualPath newAbsolutePath = oldAbsolutePath.DirectoryPath + newName;

            // 新しい名前が現在の名前と同じかどうかのチェック
            if (oldAbsolutePath == newAbsolutePath)
            {
                throw new InvalidOperationException(string.Format(Resources.NewNameSameAsCurrent, newAbsolutePath));
            }

            // ノードの取得（リンクを解決しながら）
            VirtualNodeContext nodeContext = WalkPathToTarget(oldAbsolutePath, null, null, resolveLinks, true);
            VirtualNode node = nodeContext.Node!;

            // 新しい名前のノードが既に存在するかどうかのチェック
            if (NodeExists(newAbsolutePath))
            {
                throw new InvalidOperationException(string.Format(Resources.NewNameNodeAlreadyExists, newAbsolutePath));
            }

            // 親ディレクトリの取得
            VirtualDirectory parentDirectory = nodeContext.ParentDirectory!;

            // リンク辞書の更新（シンボリックリンクの場合）
            if (node is VirtualSymbolicLink)
            {
                UpdateLinkNameInDictionary(oldAbsolutePath, newAbsolutePath);
            }

            // リンク辞書の更新（ターゲットパスの変更）
            UpdateLinksToTarget(oldAbsolutePath, newAbsolutePath);

            // 親ディレクトリから古いノードを削除し、新しい名前のノードを追加
            parentDirectory.SetNodeName(node.Name, newName);
        }

        public void MoveNode(VirtualPath sourcePath, VirtualPath destinationPath, bool overwrite = false, bool resolveLinks = true)
        {
            sourcePath = ConvertToAbsolutePath(sourcePath);
            destinationPath = ConvertToAbsolutePath(destinationPath);

            // 移動元パスのパス解決をする
            if (resolveLinks)
            {
                // パス解決を指定した場合は、移動元パスの全体をパス解決する
                sourcePath = ResolveLinkTarget(sourcePath);
            }
            else
            {
                // パス解決を指定しない場合は、移動元パスの親のパスだけをパス解決する
                sourcePath = ResolveLinkTarget(sourcePath.DirectoryPath) + sourcePath.NodeName;
            }

            // 移動先パスのパス解決をする
            VirtualPath? path = TryResolveLinkTarget(destinationPath);
            if (path != null)
            {
                // 全体がパス解決された場合は、移動先パスをそのまま使う
                destinationPath = path;
            }
            else
            {
                // パス解決されなかった場合は、ディレクトリパスだけをパス解決する
                destinationPath = ResolveLinkTarget(destinationPath.DirectoryPath) + destinationPath.NodeName;
            }

            // 移動先と移動元が同じかどうかのチェック
            if (sourcePath == destinationPath)
            {
                throw new InvalidOperationException(string.Format(Resources.SourceAndDestinationPathSameForMove, sourcePath, destinationPath));
            }

            // 移動元のルートディレクトリチェック
            if (sourcePath.IsRoot)
            {
                // ルートディレクトリの場合は例外をスロー
                throw new InvalidOperationException(Resources.CannotMoveRootDirectory);
            }

            // 移動先が移動元のサブディレクトリになっていないかどうかのチェック
            if (destinationPath.IsSubdirectory(sourcePath))
            {
                throw new InvalidOperationException(string.Format(Resources.DestinationIsSubdirectoryOfSource, sourcePath, destinationPath));
            }

            // 移動元の存在チェック
            if (!NodeExists(sourcePath))
            {
                // 存在しない場合は例外をスロー
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, sourcePath.NodeName));
            }

            // 移動処理
            if (DirectoryExists(sourcePath))
            {
                MoveDirectoryInternal(sourcePath, destinationPath);
            }
            else
            {
                MoveItemOrLinkInternal(sourcePath, destinationPath, overwrite);
            }
        }

        private void MoveDirectoryInternal(VirtualPath sourcePath, VirtualPath destinationPath)
        {
            VirtualDirectory sourceDirectory = GetDirectory(sourcePath);
            VirtualDirectory sourceParentDirectory = GetDirectory(sourcePath.GetParentPath());

            VirtualDirectory destinationParentDirectory;
            VirtualNodeName destinationNodeName;

            VirtualPath destinationBasePath;

            if (DirectoryExists(destinationPath))
            {
                destinationParentDirectory = GetDirectory(destinationPath);
                destinationNodeName = sourceDirectory.Name;
                destinationBasePath = destinationPath + sourceDirectory.Name;
            }
            else if (!NodeExists(destinationPath))
            {
                VirtualPath destinationParentPath = destinationPath.GetParentPath();

                destinationParentDirectory = GetDirectory(destinationParentPath);
                destinationNodeName = destinationPath.NodeName;
                destinationBasePath = destinationPath;
            }
            else
            {
                throw new InvalidOperationException(string.Format(Resources.DestinationNodeIsItemOrSymbolicLink, destinationPath));
            }

            if (destinationParentDirectory.NodeExists(destinationNodeName))
            {
                throw new InvalidOperationException(string.Format(Resources.NodeWithSameNameAtDestination, destinationPath, destinationNodeName));
            }

            // 移動前のパスリストを取得
            var sourceNodes = WalkPathTree(sourcePath, VirtualNodeTypeFilter.All, true, true)
                                .Select(context => context.TraversalPath)
                                .ToList();

            // 移動後のパスリストを作成し、タプルとして管理
            var nodePairs = sourceNodes
                            .Select(path =>
                                (Source: sourcePath + path,
                                 Destination: destinationBasePath + path))
                            .ToList();

            // リンク辞書の更新
            foreach (var (sourceNodePath, destinationNodePath) in nodePairs)
            {
                // リンク辞書の更新（シンボリックリンクの場合）
                if (GetNode(sourceNodePath) is VirtualSymbolicLink)
                {
                    UpdateLinkNameInDictionary(sourceNodePath, destinationNodePath);
                }

                // リンク辞書の更新（ターゲットパスの変更）
                UpdateLinksToTarget(sourceNodePath, destinationNodePath);
            }

            // ノードを移動
            sourceDirectory.Name = destinationNodeName;
            destinationParentDirectory.Add(sourceDirectory);
            sourceParentDirectory.Remove(sourceDirectory);

            // 全てのターゲットノードタイプを更新
            UpdateAllTargetNodeTypesInDictionary();
        }

        private void MoveItemOrLinkInternal(VirtualPath sourcePath, VirtualPath destinationPath, bool overwrite)
        {
            VirtualNode sourceNode = GetNode(sourcePath);
            VirtualDirectory sourceParentDirectory = GetDirectory(sourcePath.GetParentPath());

            VirtualDirectory destinationParentDirectory;
            VirtualNodeName destinationNodeName;
            VirtualNode? destinationNode;

            if (DirectoryExists(destinationPath))
            {
                destinationParentDirectory = GetDirectory(destinationPath);
                destinationNodeName = sourceNode.Name;
                destinationNode = TryGetNode(destinationPath + sourceNode.Name);
            }
            else if (!NodeExists(destinationPath))
            {
                VirtualPath destinationParentPath = destinationPath.GetParentPath();

                destinationParentDirectory = GetDirectory(destinationParentPath);
                destinationNodeName = destinationPath.NodeName;
                destinationNode = TryGetNode(destinationPath);
            }
            else
            {
                destinationParentDirectory = GetDirectory(destinationPath.GetParentPath());
                destinationNodeName = destinationPath.NodeName;
                destinationNode = TryGetNode(destinationPath);
            }

            if (destinationNode != null)
            {
                if (overwrite)
                {
                    destinationParentDirectory.Remove(destinationNode);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.NodeWithSameNameAtDestination, destinationPath, destinationNodeName));
                }
            }

            // リンク辞書の更新（シンボリックリンクの場合）
            if (sourceNode is VirtualSymbolicLink)
            {
                UpdateLinkNameInDictionary(sourcePath, destinationPath);
            }

            // リンク辞書の更新（ターゲットパスの変更）
            UpdateLinksToTarget(sourcePath, destinationPath);

            // ノードを移動
            sourceParentDirectory.Remove(sourceNode);
            sourceNode.Name = destinationNodeName;
            destinationParentDirectory.Add(sourceNode);

            // 全てのターゲットノードタイプを更新
            UpdateAllTargetNodeTypesInDictionary();
        }

        [DebuggerStepThrough]
        private struct WalkPathToTargetParameters(
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
            public VirtualPath TargetPath { get; set; } = targetPath;
            public int TraversalIndex { get; set; } = traversalIndex;
            public VirtualPath TraversalPath { get; set; } = traversalPath;
            public VirtualPath? ResolvedPath { get; set; } = resolvedPath;
            public VirtualDirectory TraversalDirectory { get; set; } = traversalDirectory;
            public NotifyNodeDelegate? NotifyNode { get; set; } = notifyNode;
            public ActionNodeDelegate? ActionNode { get; set; } = actionNode;
            public bool FollowLinks { get; set; } = followLinks;
            public bool ExceptionEnabled { get; set; } = exceptionEnabled;
            public bool Resolved { get; set; } = resolved;
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
