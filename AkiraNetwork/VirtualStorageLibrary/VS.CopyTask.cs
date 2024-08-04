using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
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
    }
}
