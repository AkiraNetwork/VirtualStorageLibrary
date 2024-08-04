using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
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
    }
}
