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
