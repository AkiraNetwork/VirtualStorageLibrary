using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        private readonly Dictionary<VirtualPath, HashSet<VirtualPath>> _linkDictionary;

        public Dictionary<VirtualPath, HashSet<VirtualPath>> LinkDictionary => _linkDictionary;

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
    }
}
