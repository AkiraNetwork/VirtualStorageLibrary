using AkiraNetwork.VirtualStorageLibrary.Localization;
using AkiraNetwork.VirtualStorageLibrary.Utilities;
using System.Runtime.CompilerServices;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualStorage<T>
    {
        private readonly VirtualDirectory _root;

        public VirtualDirectory Root => _root;

        public VirtualPath CurrentPath { get; private set; }

        private readonly Dictionary<VirtualPath, HashSet<VirtualPath>> _linkDictionary;

        public Dictionary<VirtualPath, HashSet<VirtualPath>> LinkDictionary => _linkDictionary;

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

        // リンク辞書に新しいリンクを追加します。
        public void AddLinkToDictionary(VirtualPath targetPath, VirtualPath linkPath)
        {
            if (!targetPath.IsAbsolute)
            {
                throw new ArgumentException(string.Format(Resources.TargetPathIsNotAbsolutePath, targetPath), nameof(targetPath));
            }

            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            HashSet<VirtualPath>? linkPathSet = GetLinksFromDictionary(targetPath);
            _linkDictionary[targetPath] = linkPathSet;

            linkPathSet.Add(linkPath);
        }

        // リンク辞書内のリンクターゲットノードのタイプを更新します。
        public void UpdateTargetNodeTypesInDictionary(VirtualPath targetPath)
        {
            HashSet<VirtualPath> linkPathSet = GetLinksFromDictionary(targetPath);
            if (linkPathSet.Count > 0)
            {
                VirtualNodeType targetType = GetNodeType(targetPath, true);
                SetLinkTargetNodeType(linkPathSet, targetType);
            }
        }

        // リンク辞書内の全てのリンクのターゲットノードタイプを更新します。
        public void UpdateAllTargetNodeTypesInDictionary()
        {
            foreach (var targetPath in _linkDictionary.Keys)
            {
                UpdateTargetNodeTypesInDictionary(targetPath);
            }
        }

        // リンク辞書からリンクを削除します。
        public void RemoveLinkFromDictionary(VirtualPath targetPath, VirtualPath linkPath)
        {
            // GetLinksFromDictionaryを使用してリンクセットを取得
            var links = GetLinksFromDictionary(targetPath);

            // リンクセットから指定されたリンクを削除
            if (links.Remove(linkPath))
            {
                // リンクセットが空になった場合、リンク辞書からエントリを削除
                if (links.Count == 0)
                {
                    _linkDictionary.Remove(targetPath);
                }
            }
        }

        // 指定されたターゲットパスに関連するすべてのリンクパスをリンク辞書から取得します。
        public HashSet<VirtualPath> GetLinksFromDictionary(VirtualPath targetPath)
        {
            if (_linkDictionary.TryGetValue(targetPath, out HashSet<VirtualPath>? linkPathSet))
            {
                return linkPathSet;
            }
            return [];
        }

        // 指定されたリンクパスリスト内の全てのシンボリックリンクのターゲットノードタイプを設定します。
        public void SetLinkTargetNodeType(HashSet<VirtualPath> linkPathSet, VirtualNodeType nodeType)
        {
            foreach (var linkPath in linkPathSet)
            {
                VirtualSymbolicLink? link = TryGetSymbolicLink(linkPath);
                if (link != null)
                {
                    link.TargetNodeType = nodeType;
                }
            }
        }

        // 特定のシンボリックリンクのターゲットパスを新しいターゲットパスに更新します。
        public void UpdateLinkInDictionary(VirtualPath linkPath, VirtualPath newTargetPath)
        {
            if (GetNode(linkPath) is VirtualSymbolicLink link)
            {
                if (link.TargetPath != null)
                {
                    // 古いターゲットパスを取得
                    VirtualPath oldTargetPath = link.TargetPath;

                    // 古いターゲットパスからリンクを削除
                    RemoveLinkFromDictionary(oldTargetPath, linkPath);

                    // 新しいターゲットパスを設定
                    link.TargetPath = newTargetPath;

                    // 新しいターゲットパスにリンクを追加
                    AddLinkToDictionary(newTargetPath, linkPath);
                }
            }
        }

        // 特定のターゲットパスを持つリンクのターゲットパスを新しいターゲットパスに更新します。
        public void UpdateLinksToTarget(VirtualPath oldTargetPath, VirtualPath newTargetPath)
        {
            var linkPathSet = GetLinksFromDictionary(oldTargetPath);

            foreach (var linkPath in linkPathSet)
            {
                // UpdateLinkInDictionaryを使用してリンクのターゲットパスを更新
                UpdateLinkInDictionary(linkPath, newTargetPath);
            }

            // 古いターゲットパスからリンクを削除
            _linkDictionary.Remove(oldTargetPath);
        }

        // リンク辞書内のリンク名を更新します。
        private void UpdateLinkNameInDictionary(VirtualPath oldLinkPath, VirtualPath newLinkPath)
        {
            foreach (var entry in _linkDictionary)
            {
                var linkPaths = entry.Value;

                // Remove が成功した場合、新しいリンクパスを追加
                if (linkPaths.Remove(oldLinkPath))
                {
                    linkPaths.Add(newLinkPath);
                }
            }
        }

        public void RemoveLinkByLinkPath(VirtualPath linkPath)
        {
            if (!linkPath.IsAbsolute)
            {
                throw new ArgumentException(string.Format(Resources.LinkPathIsNotAbsolutePath, linkPath), nameof(linkPath));
            }

            List<VirtualPath> targetPathsToRemoveList = [];

            foreach (KeyValuePair<VirtualPath, HashSet<VirtualPath>> entry in _linkDictionary)
            {
                VirtualPath targetPath = entry.Key;
                HashSet<VirtualPath> linkPathSet = entry.Value;

                if (linkPathSet.Remove(linkPath))
                {
                    if (linkPathSet.Count == 0)
                    {
                        targetPathsToRemoveList.Add(targetPath);
                    }
                }
            }

            foreach (VirtualPath targetPath in targetPathsToRemoveList)
            {
                _linkDictionary.Remove(targetPath);
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
                throw new ArgumentException(string.Format(Resources.ParameterIsNullOrEmpty, relativePath), nameof(relativePath));
            }

            // relativePathが既に絶対パスである場合は、そのまま使用
            if (relativePath.IsAbsolute)
            {
                return relativePath;
            }

            // basePathが空文字列の場合、ArgumentExceptionをスロー
            if (basePath.IsEmpty)
            {
                throw new ArgumentException(string.Format(Resources.ParameterIsEmpty, basePath), nameof(basePath));
            }

            // relativePathを effectiveBasePath に基づいて絶対パスに変換
            var absolutePath = basePath + relativePath;

            return absolutePath;
        }

        // パスを受け取るインデクサ
        [IndexerName("Indexer")]
        public VirtualNode this[VirtualPath path, bool followLinks = true]
        {
            get => GetNode(path, followLinks);
            set => SetNode(path, value);
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
                // GetNodeメソッドは、ノードが見つからない場合に null を返すか、例外をスローするように実装されていると仮定
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
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, node.Name));
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

            return node?.NodeType ?? VirtualNodeType.None;
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

        public IEnumerable<VirtualPath> ExpandPath(VirtualPath path, VirtualNodeTypeFilter filter = VirtualNodeTypeFilter.All, bool followLinks = true, bool resolveLinks = true)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualPath fixedPath = path.FixedPath;
            IEnumerable<VirtualNodeContext> nodeContexts = ExpandPathTree(path, filter, followLinks, resolveLinks);
            IEnumerable<VirtualPath> resolvedPaths = nodeContexts.Select(info => (fixedPath + info.TraversalPath).NormalizePath());

            return resolvedPaths;
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
