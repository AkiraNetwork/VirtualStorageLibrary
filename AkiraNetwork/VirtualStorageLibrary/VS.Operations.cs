using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
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
    }
}
