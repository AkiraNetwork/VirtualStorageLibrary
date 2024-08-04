using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
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
    }
}
