using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
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
    }
}
