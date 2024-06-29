using System.Runtime.CompilerServices;

namespace AkiraNet.VirtualStorageLibrary
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
                throw new ArgumentException("リンク先のパスは絶対パスである必要があります。", nameof(targetPath));
            }

            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            HashSet<VirtualPath>? linkPathSet = GetLinksFromDictionary(targetPath);
            _linkDictionary[targetPath] = linkPathSet;

            linkPathSet.Add(linkPath);

            VirtualSymbolicLink link = GetSymbolicLink(linkPath);
            link.TargetNodeType = GetNodeType(targetPath);
        }

        // リンク辞書内のリンクターゲットノードのタイプを更新します。
        public void UpdateLinkTypesInDictionary(VirtualPath targetPath)
        {
            HashSet<VirtualPath> linkPathSet = GetLinksFromDictionary(targetPath);
            if (linkPathSet.Count > 0)
            {
                VirtualNodeType targetType = GetNodeType(targetPath);
                SetLinkTargetNodeType(linkPathSet, targetType);
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
                VirtualPath oldTargetPath = link.TargetPath ?? string.Empty;

                // 古いターゲットパスからリンクを削除
                RemoveLinkFromDictionary(oldTargetPath, linkPath);

                // 新しいターゲットパスを設定
                link.TargetPath = newTargetPath;

                // 新しいターゲットパスにリンクを追加
                AddLinkToDictionary(newTargetPath, linkPath);

                // 新しいターゲットノードのタイプを更新
                link.TargetNodeType = GetNodeType(newTargetPath);
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
                throw new ArgumentException("relativePathがnullまたは空です。", nameof(relativePath));
            }

            // relativePathが既に絶対パスである場合は、そのまま使用
            if (relativePath.IsAbsolute)
            {
                return relativePath;
            }

            // basePathが空文字列の場合、ArgumentExceptionをスロー
            if (basePath.IsEmpty)
            {
                throw new ArgumentException("basePathが空です。", nameof(basePath));
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
            set => SetNode(path, value, followLinks);
        }

        public void SetNode(VirtualPath destinationPath, VirtualNode node, bool followLinks = false)
        {
            destinationPath = ConvertToAbsolutePath(destinationPath).NormalizePath();

            switch (node)
            {
                case VirtualDirectory directory:
                    UpdateDirectory(destinationPath, directory);
                    break;

                case VirtualSymbolicLink symbolicLink:
                    UpdateSymbolicLInk(destinationPath, symbolicLink, followLinks);
                    break;

                case VirtualItem<T> item:
                    UpdateItem(destinationPath, item);
                    break;

                default:
                    throw new InvalidOperationException($"ノードの種類 '{node.GetType().Name}' はサポートされていません。");
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

        public void UpdateSymbolicLInk(VirtualPath linkPath, VirtualSymbolicLink newLink, bool followLinks = false)
        {
            // 絶対パスに変換
            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            // 既存のシンボリックリンクを取得
            VirtualSymbolicLink link = GetSymbolicLink(linkPath, followLinks);

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

                default:
                    throw new InvalidOperationException($"ノードの種類 '{node.GetType().Name}' はサポートされていません。");
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
                    throw new InvalidOperationException($"ディレクトリ '{directory.Name}' は既に存在します。");
                }

                // 新しいディレクトリを追加
                directory = (VirtualDirectory)parentDirectory.Add(directory);

                // 作成したノードがリンクターゲットとして登録されている場合、リンクターゲットのノードタイプを更新
                UpdateLinkTypesInDictionary(directoryPath + directory.Name);
            }
            else
            {
                throw new VirtualNodeNotFoundException($"ノード '{directoryPath}' はディレクトリではありません。");
            }

            return;
        }

        public void AddDirectory(VirtualPath path, bool createSubdirectories = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();

            if (path.IsRoot)
            {
                throw new InvalidOperationException("ルートディレクトリは既に存在します。");
            }

            VirtualPath directoryPath = path.DirectoryPath;
            VirtualNodeName newDirectoryName = path.NodeName;

            VirtualDirectory newDirectory = new(newDirectoryName);
            AddDirectory(directoryPath, newDirectory, createSubdirectories);
        }

        private bool CreateIntermediateDirectory(VirtualDirectory directory, VirtualNodeName nodeName, VirtualPath nodePath)
        {
            VirtualDirectory newSubdirectory = new(nodeName);

            // 中間ディレクトリを追加
            directory.Add(newSubdirectory);

            // 中間ディレクトリをリンク辞書に追加
            UpdateLinkTypesInDictionary(nodePath);

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
                    throw new InvalidOperationException($"ノード '{item.Name}' は既に存在します。上書きは許可されていません。");
                }
                else
                {
                    // 上書き対象がアイテムであることを確認
                    if (!ItemExists(itemDirectoryPath + item.Name))
                    {
                        throw new InvalidOperationException($"'{item.Name}' はアイテム以外のノードです。アイテムの上書きはできません。");
                    }
                    // 既存アイテムの削除
                    directory.Remove(item);
                }
            }

            // 新しいアイテムを追加
            item = (VirtualItem<T>)directory.Add(item, overwrite);

            // リンク辞書を更新
            UpdateLinkTypesInDictionary(itemDirectoryPath + item.Name);
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
                    throw new InvalidOperationException($"ノード '{link.Name}' は既に存在します。上書きは許可されていません。");
                }
                else
                {
                    // 既存のノードがシンボリックリンクであるかどうかチェック
                    if (!SymbolicLinkExists(linkDirectoryPath + link.Name))
                    {
                        throw new InvalidOperationException($"既存のノード '{link.Name}' はシンボリックリンクではありません。シンボリックリンクのみ上書き可能です。");
                    }
                    // 既存のシンボリックリンクを削除
                    directory.Remove(link);
                }
            }

            // 新しいシンボリックリンクを追加
            link = (VirtualSymbolicLink)directory.Add(link);

            if (link.TargetPath != null)
            {
                // targetPathを絶対パスに変換して正規化する必要がある。その際、シンボリックリンクを作成したディレクトリパスを基準とする
                VirtualPath absoluteTargetPath = ConvertToAbsolutePath(link.TargetPath!, linkDirectoryPath).NormalizePath();

                // リンク辞書にリンク情報を追加
                AddLinkToDictionary(absoluteTargetPath, linkDirectoryPath + link.Name);

                // 作成したノードがリンクターゲットとして登録されている場合、リンクターゲットのノードタイプを更新
                UpdateLinkTypesInDictionary(linkDirectoryPath + link.Name);
            }
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

            VirtualNodeContext? nodeContext = WalkPathToTargetInternal(p);

            return nodeContext;
        }

        private VirtualNodeContext WalkPathToTargetInternal(WalkPathToTargetParameters p)
        {
            // ターゲットがルートディレクトリの場合は、ルートノードを通知して終了
            if (p.TargetPath.IsRoot)
            {
                p.NotifyNode?.Invoke(VirtualPath.Root, _root, true);
                return new VirtualNodeContext(_root, VirtualPath.Root, null, 0, 0, VirtualPath.Root, p.Resolved);
            }

            VirtualNodeName traversalNodeName = p.TargetPath.PartsList[p.TraversalIndex];

            while (!p.TraversalDirectory.NodeExists(traversalNodeName))
            {
                if (p.ActionNode != null)
                {
                    if (p.ActionNode(p.TraversalDirectory, traversalNodeName, p.TraversalPath + traversalNodeName))
                    {
                        continue;
                    }
                }

                // 例外が有効な場合は例外をスロー
                if (p.ExceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException($"ノード '{p.TraversalPath + traversalNodeName}' が見つかりません。");
                }

                return new VirtualNodeContext(null, p.TraversalPath, null, 0, 0, p.TraversalPath, p.Resolved);
            }

            VirtualNodeContext? nodeContext;

            // 探索ノードを取得
            VirtualNode? node = p.TraversalDirectory[traversalNodeName];

            // 探索パスを更新
            p.TraversalPath += traversalNodeName;

            // 次のノードへ
            p.TraversalIndex++;

            if (node is VirtualDirectory directory)
            {
                // 最後のノードに到達したかチェック
                if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                {
                    // 末端のノードを通知
                    p.NotifyNode?.Invoke(p.TraversalPath, node, true);
                    p.ResolvedPath ??= p.TraversalPath;
                    return new VirtualNodeContext(node, p.TraversalPath, p.TraversalDirectory, 0, 0, p.ResolvedPath, p.Resolved);
                }

                // 途中のノードを通知
                p.NotifyNode?.Invoke(p.TraversalPath, node, false);

                // 探索ディレクトリを取得
                p.TraversalDirectory = directory;

                // 再帰的に探索
                nodeContext = WalkPathToTargetInternal(p);
                node = nodeContext?.Node;
                p.TraversalPath = nodeContext?.TraversalPath ?? p.TraversalPath;
                p.ResolvedPath = nodeContext?.ResolvedPath ?? p.ResolvedPath;
            }
            else if (node is VirtualItem)
            {
                // 末端のノードを通知
                p.NotifyNode?.Invoke(p.TraversalPath, node, true);

                // 最後のノードに到達したかチェック
                if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                {
                    p.ResolvedPath ??= p.TraversalPath;
                    return new VirtualNodeContext(node, p.TraversalPath, p.TraversalDirectory, 0, 0, p.ResolvedPath, p.Resolved);
                }

                p.ResolvedPath ??= p.TraversalPath;

                // 例外が有効な場合は例外をスロー
                if (p.ExceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException($"ノード '{p.TargetPath}' まで到達できません。ノード '{p.TraversalPath}' はアイテムです。");
                }

                return new VirtualNodeContext(null, p.TraversalPath, null, 0, 0, p.ResolvedPath, p.Resolved);
            }
            else if (node is VirtualSymbolicLink link)
            {
                if (!p.FollowLinks || link.TargetPath == null)
                {
                    // シンボリックリンクを通知
                    p.NotifyNode?.Invoke(p.TraversalPath, node, true);
                    p.ResolvedPath ??= p.TraversalPath;
                    return new VirtualNodeContext(node, p.TraversalPath, p.TraversalDirectory, 0, 0, p.ResolvedPath, p.Resolved);
                }

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
                nodeContext = WalkPathToTargetInternal(p2);

                node = nodeContext?.Node;
                //deletePath = result?.TraversalPath ?? deletePath;

                // 解決済みのパスに未探索のパスを追加
                p.ResolvedPath = nodeContext?.ResolvedPath!.CombineFromIndex(p.TargetPath, p.TraversalIndex);

                // シンボリックリンクを通知
                p.NotifyNode?.Invoke(p.TraversalPath, node, true);

                if (node != null && (node is VirtualDirectory linkDirectory))
                {
                    // 最後のノードに到達したかチェック
                    if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                    {
                        // 末端のノードを通知
                        p.ResolvedPath ??= p.TraversalPath;
                        return new VirtualNodeContext(node, p.TraversalPath, p.TraversalDirectory, 0, 0, p.ResolvedPath, p.Resolved);
                    }

                    // 探索ディレクトリを取得
                    p.TraversalDirectory = linkDirectory;

                    // 再帰的に探索
                    nodeContext = WalkPathToTargetInternal(p);
                    node = nodeContext?.Node;
                    p.TraversalPath = nodeContext?.TraversalPath ?? p.TraversalPath;
                    p.ResolvedPath = nodeContext?.ResolvedPath ?? p.ResolvedPath;
                }

                p.ResolvedPath ??= p.TraversalPath;
                return new VirtualNodeContext(node, p.TraversalPath, p.TraversalDirectory, 0, 0, p.ResolvedPath, p.Resolved);
            }

            p.ResolvedPath ??= p.TraversalPath;
            return new VirtualNodeContext(node, p.TraversalPath, p.TraversalDirectory, 0, 0, p.ResolvedPath, p.Resolved);
        }

        public IEnumerable<VirtualNodeContext> ResolvePathTree(
            VirtualPath path,
            VirtualNodeTypeFilter filter = VirtualNodeTypeFilter.All)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualPath basePath = path.ExtractBasePath();
            int baseDepth = path.GetBaseDepth();
            VirtualNode node = GetNode(basePath, true);

            List<string> patternList = path.PartsList.Select(node => node.Name).ToList();

            WalkPathTreeParameters p = new(
                basePath,
                basePath,
                node,
                null,
                baseDepth,
                0,
                0,
                filter,
                true,
                true,
                patternList,
                null);

            return WalkPathTreeInternal(p);
        }

        public IEnumerable<VirtualNodeContext> WalkPathTree(
            VirtualPath basePath,
            VirtualNodeTypeFilter filter = VirtualNodeTypeFilter.All,
            bool recursive = true,
            bool followLinks = true)
        {
            basePath = ConvertToAbsolutePath(basePath).NormalizePath();
            int baseDepth = basePath.GetBaseDepth();
            VirtualNode baseNode = GetNode(basePath, followLinks);

            // TODO: リンクのターゲット先が存在しない場合の仕様を検討する。
            VirtualNodeContext nodeContext = WalkPathToTarget(basePath, null, null, true, true);
            VirtualPath traversalBasePath = nodeContext.TraversalPath;
            VirtualPath resolvedBasePath = nodeContext.ResolvedPath!;
            VirtualPath parentDirectoryPath = resolvedBasePath.DirectoryPath;
            VirtualDirectory parentDirectory = GetDirectory(parentDirectoryPath, followLinks);

            VirtualSymbolicLink? link = null;
            if (traversalBasePath != resolvedBasePath)
            {
                link = GetSymbolicLink(basePath, false);
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

            return WalkPathTreeInternal(p);
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
                                // ディレクトリを通知
                                yield return new VirtualNodeContext(directory, p.CurrentPath.GetRelativePath(p.BasePath), p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                            }
                        }
                    }
                    else
                    {
                        // ディレクトリを通知
                        yield return new VirtualNodeContext(directory, p.CurrentPath.GetRelativePath(p.BasePath), p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                    }
                }

                if (p.Recursive || 0 == p.CurrentDepth)
                {
                    // ディレクトリ内のノードを再帰的に探索
                    int index = 0;
                    foreach (var node in directory.Nodes)
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
                                // アイテムを通知
                                yield return new VirtualNodeContext(item, p.CurrentPath.GetRelativePath(p.BasePath), p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                            }
                        }
                    }
                    else
                    {
                        // アイテムを通知
                        yield return new VirtualNodeContext(item, p.CurrentPath.GetRelativePath(p.BasePath), p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
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
                                    // シンボリックリンクを通知
                                    yield return new VirtualNodeContext(link, p.CurrentPath.GetRelativePath(p.BasePath), p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                                }
                            }
                        }
                        else
                        {
                            // シンボリックリンクを通知
                            yield return new VirtualNodeContext(link, p.CurrentPath.GetRelativePath(p.BasePath), p.ParentDirectory, p.CurrentDepth, p.CurrentIndex, null, false, p.ResolvedLink);
                        }
                    }
                }
            }
        }

        static bool MatchPatterns(List<VirtualNodeName> parts, List<string> patternList)
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

            if (patternMatcher == null)
            {
                return false;
            }

            if (parts.Count != patternList.Count)
            {
                return false;
            }

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
                throw new VirtualNodeNotFoundException($"ディレクトリ {path} は存在しません。");
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
                throw new VirtualNodeNotFoundException($"アイテム {path} は存在しません。");
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

        public VirtualSymbolicLink GetSymbolicLink(VirtualPath path, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNode node = GetNode(path, followLinks);

            if (node is VirtualSymbolicLink link)
            {
                return link;
            }
            else
            {
                throw new VirtualNodeNotFoundException($"シンボリックリンク {path} は存在しません。");
            }
        }

        public VirtualSymbolicLink? TryGetSymbolicLink(VirtualPath path, bool followLinks = false)
        {
            try
            {
                return GetSymbolicLink(path, followLinks);
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

        public IEnumerable<VirtualPath> ResolvePath(VirtualPath path)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualPath basePath = path.ExtractBasePath();
            IEnumerable<VirtualNodeContext> nodeContexts = ResolvePathTree(path);
            IEnumerable<VirtualPath> resolvedPaths = nodeContexts.Select(info => (basePath + info.TraversalPath).NormalizePath());

            return resolvedPaths;
        }

        // ワイルドカードの実装（デフォルト）
        public static bool RegexMatch(string nodeName, string pattern)
        {
            return Regex.IsMatch(nodeName, pattern);
        }

        private void CheckCopyPreconditions(VirtualPath sourcePath, VirtualPath destinationPath, bool followLinks, bool recursive)
        {
            VirtualPath absoluteSourcePath = ConvertToAbsolutePath(sourcePath).NormalizePath();
            VirtualPath absoluteDestinationPath = ConvertToAbsolutePath(destinationPath).NormalizePath();

            // ルートディレクトリのコピーを禁止
            if (absoluteSourcePath.IsRoot)
            {
                throw new InvalidOperationException("ルートディレクトリのコピーは禁止されています。");
            }

            // コピー元の存在確認
            if (!NodeExists(absoluteSourcePath, true))
            {
                throw new VirtualNodeNotFoundException($"コピー元ノード '{absoluteSourcePath}' は存在しません。");
            }

            // コピー元とコピー先が同じ場合は例外をスロー
            if (absoluteSourcePath == absoluteDestinationPath)
            {
                throw new InvalidOperationException("コピー元とコピー先が同じです。");
            }

            // 循環参照チェック
            if (absoluteDestinationPath.StartsWith(absoluteSourcePath.AddEndSlash()) || absoluteSourcePath.StartsWith(absoluteDestinationPath.AddEndSlash()))
            {
                throw new InvalidOperationException("コピー元またはコピー先が互いのサブディレクトリになっています。");
            }

            // コピー元ツリーの探索
            IEnumerable<VirtualNodeContext> sourceContexts = WalkPathTree(absoluteSourcePath, VirtualNodeTypeFilter.All, recursive, followLinks);

            // 各ノードに対する存在確認
            foreach (var context in sourceContexts)
            {
                VirtualPath currentSourcePath = absoluteSourcePath + context.TraversalPath;

                // コピー元ノードの存在確認
                if (!NodeExists(currentSourcePath, true))
                {
                    throw new VirtualNodeNotFoundException($"コピー元ノード '{currentSourcePath}' は存在しません。");
                }
            }
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

            // コピー元パスが存在しない場合
            if (!NodeExists(sourcePath, true))
            {
                throw new VirtualNodeNotFoundException($"コピー元のノード '{sourcePath}' が存在しません。");
            }

            // コピー元パスとコピー先パスが同じ場合
            if (sourcePath == destinationPath)
            {
                throw new InvalidOperationException("コピー元パスとコピー先パスが同じです。");
            }

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

            VirtualDirectory destinationDirectory = GetDirectory(destinationPath.DirectoryPath, true);
            //VirtualNodeName destinationNodeName = destinationPath.NodeName;

            VirtualNode? destinationNode = destinationDirectory.Get(destinationPath.NodeName, false);

            bool overwriteDirectory = false;

            switch (destinationNode)
            {
                case VirtualDirectory directory:
                    destinationDirectory = directory;
                    newNodeName = sourceNode.Name;
                    VirtualNode? node = destinationDirectory.Get(newNodeName, false);
                    if (node != null)
                    {
                        if (overwrite && (node is VirtualDirectory))
                        {
                            overwriteDirectory = true;
                        }
                        else
                        {
                            throw new InvalidOperationException($"ノード '{newNodeName}' は既に存在します。上書きは許可されていません。");
                        }
                    }
                    destinationPath += newNodeName;
                    break;

                case VirtualItem _:
                    if (overwrite)
                    {
                        VirtualItem<T> item = GetItem(destinationPath, true);
                        destinationDirectory.Remove(item);
                    }
                    else
                    {
                        throw new InvalidOperationException($"アイテム '{destinationPath.NodeName}' は既に存在します。上書きは許可されていません。");
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
            if (depth < 0)
            {
                // デバッグ用
                throw new InvalidOperationException("深さが負の値になりました。");
            }

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

        public void RemoveNode(VirtualPath path, bool recursive = false, bool followLinks = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();

            if (path.IsRoot)
            {
                throw new InvalidOperationException("ルートディレクトリを削除することはできません。");
            }

            // TODO: ToList
            IEnumerable<VirtualNodeContext> contexts = WalkPathTree(path, VirtualNodeTypeFilter.All, recursive, followLinks).ToList();

            if (recursive)
            {
                // TODO: ToList
                // ノードコンテキストを逆順に処理するためにリストに変換して逆順にソート
                List<VirtualNodeContext> reversedContexts = contexts.Reverse().ToList();

                foreach (VirtualNodeContext context in reversedContexts)
                {
                    VirtualDirectory? parentDir = context.ParentDirectory;

                    // シンボリックリンクか判定
                    if (context.ResolvedLink != null)
                    {
                        // シンボリックリンクの場合はリンクノードを削除する
                        VirtualSymbolicLink link = context.ResolvedLink;
                        VirtualPath linkPath = path + context.TraversalPath;
                        VirtualPath linkParentPath = linkPath.DirectoryPath;
                        parentDir = GetDirectory(linkParentPath, true);

                        // 削除するリンクのストレージ参照フラグをリセット
                        link.IsReferencedInStorage = false;

                        // 辞書からノードを削除
                        parentDir?.Remove(link);

                        // ターゲットノードタイプを更新
                        VirtualPath deletePath = linkParentPath + link.Name;
                        UpdateLinkTypesInDictionary(deletePath);

                        // リンク辞書からリンクを削除
                        if (link.TargetPath != null)
                        {
                            RemoveLinkFromDictionary(link.TargetPath, linkPath);
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

                        // ターゲットノードタイプを更新
                        VirtualPath deletePath = path + context.TraversalPath;
                        UpdateLinkTypesInDictionary(deletePath);
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
                        throw new InvalidOperationException("ディレクトリが空ではなく、再帰フラグが設定されていません。");
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

                // ターゲットノードタイプを更新
                UpdateLinkTypesInDictionary(path);

                // ノードがリンクの場合、リンク辞書からリンクを削除
                if (node is VirtualSymbolicLink link)
                {
                    if (link.TargetPath != null)
                    {
                        RemoveLinkFromDictionary(link.TargetPath, path);
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

        public void SetNodeName(VirtualPath oldPath, VirtualNodeName newName, bool resolveLinks = true)
        {
            VirtualPath oldAbsolutePath = ConvertToAbsolutePath(oldPath);
            VirtualPath newAbsolutePath = oldAbsolutePath.DirectoryPath + newName;

            // 移動先と移動元が同じかどうかのチェック
            if (oldAbsolutePath == newAbsolutePath)
            {
                throw new InvalidOperationException("新しい名前が現在の名前と同じです。");
            }

            // ノードの取得（リンクを解決しながら）
            VirtualNodeContext nodeContext = WalkPathToTarget(oldAbsolutePath, null, null, resolveLinks, true);
            VirtualNode node = nodeContext.Node!;

            // 新しい名前のノードが既に存在するかどうかのチェック
            if (NodeExists(newAbsolutePath))
            {
                throw new InvalidOperationException($"指定された新しい名前のノード '{newAbsolutePath}' は既に存在します。");
            }

            // 親ディレクトリの取得
            VirtualDirectory parentDirectory = nodeContext.ParentDirectory!;

            // リンク辞書の更新（シンボリックリンクの場合）
            if (node is VirtualSymbolicLink symbolicLink)
            {
                UpdateLinkNameInDictionary(oldAbsolutePath, newAbsolutePath);
            }

            // リンク辞書の更新（ターゲットパスの変更）
            UpdateLinksToTarget(oldAbsolutePath, newAbsolutePath);

            // 親ディレクトリから古いノードを削除し、新しい名前のノードを追加
            parentDirectory.Remove(node);
            node.Name = newName;
            parentDirectory.Add(node);
        }

        public void MoveNode(VirtualPath sourcePath, VirtualPath destinationPath, bool overwrite = false)
        {
            VirtualPath absoluteSourcePath = ConvertToAbsolutePath(sourcePath);
            VirtualPath absoluteDestinationPath = ConvertToAbsolutePath(destinationPath);

            // 循環参照チェック
            if (absoluteDestinationPath.StartsWith(new VirtualPath(absoluteSourcePath.Path + VirtualPath.Separator)))
            {
                throw new InvalidOperationException("移動先が移動元のサブディレクトリになっています。");
            }

            // 移動先と移動元が同じかどうかのチェック
            if (absoluteSourcePath == absoluteDestinationPath)
            {
                throw new InvalidOperationException("移動元と移動先が同じです。");
            }

            // 移動元の存在チェック
            if (!NodeExists(absoluteSourcePath))
            {
                // 存在しない場合は例外をスロー
                throw new VirtualNodeNotFoundException($"指定されたノード '{absoluteSourcePath}' は存在しません。");
            }

            // 移動元のルートディレクトリチェック
            if (absoluteSourcePath.IsRoot)
            {
                // ルートディレクトリの場合は例外をスロー
                throw new InvalidOperationException("ルートディレクトリを移動することはできません。");
            }

            // 移動処理
            if (DirectoryExists(absoluteSourcePath))
            {
                MoveDirectoryInternal(absoluteSourcePath, absoluteDestinationPath);
            }
            else
            {
                MoveItemOrLinkInternal(absoluteSourcePath, absoluteDestinationPath, overwrite);
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
                if (!DirectoryExists(destinationParentPath))
                {
                    throw new VirtualNodeNotFoundException($"指定されたノード '{destinationParentPath}' は存在しません。");
                }

                destinationParentDirectory = GetDirectory(destinationParentPath);
                destinationNodeName = destinationPath.NodeName;
                destinationBasePath = destinationPath;
            }
            else
            {
                throw new InvalidOperationException($"移動先ノード '{destinationPath}' はアイテムまたはシンボリックリンクです。");
            }

            if (destinationParentDirectory.NodeExists(destinationNodeName))
            {
                throw new InvalidOperationException($"移動先ディレクトリ '{destinationPath}' に同名のノード '{destinationNodeName}' が存在します。");
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
                destinationNodeName = sourceNode.Name; // TODO: NodeExistsのシグネチャを node に変更する予定
                destinationNode = TryGetNode(destinationPath + sourceNode.Name);
            }
            else if (!NodeExists(destinationPath))
            {
                VirtualPath destinationParentPath = destinationPath.GetParentPath();
                if (!DirectoryExists(destinationParentPath))
                {
                    throw new VirtualNodeNotFoundException($"指定されたノード '{destinationParentPath}' は存在しません。");
                }

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
                    throw new InvalidOperationException($"移動先ディレクトリ '{destinationPath}' に同名のノード '{destinationNodeName}' が存在します。");
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
        }

        // 仮想ストレージのツリー構造をテキストベースで作成し返却する
        // 返却するテストは以下の出力の形式とする。
        // 例: もし、/dir1/dir2/item1, /dir1/dir2/item2, /dir1/item3 が存在し、/dir1が basePath で指定された場合
        // 出力:
        // /
        // ├dir1/
        // │├item1
        // │└item2
        // └dir2/
        // 　├item3
        // 　├item4
        // 　└item5
        public string GenerateTextBasedTreeStructure(VirtualPath basePath, bool recursive = true, bool followLinks = true)
        {
            const char FullWidthSpaceChar = '\u3000';
            StringBuilder tree = new();

            basePath = ConvertToAbsolutePath(basePath).NormalizePath();
            IEnumerable<VirtualNodeContext> nodeContexts = WalkPathTree(basePath, VirtualNodeTypeFilter.All, recursive, followLinks);
            StringBuilder line = new();
            string previous = string.Empty;

            VirtualNodeContext nodeFirstContext = nodeContexts.First();
            VirtualPath baseAbsolutePath = basePath + nodeFirstContext.TraversalPath;

            if (basePath.IsRoot)
            {
                tree.AppendLine("/");
            }
            else
            {
                tree.AppendLine(baseAbsolutePath);
            }

            foreach (var nodeInfo in nodeContexts.Skip(1))
            {
                VirtualNode? node = nodeInfo.Node;
                string nodeName = nodeInfo.TraversalPath.NodeName;
                int depth = nodeInfo.Depth;
                int count = nodeInfo.ParentDirectory?.Count ?? 0;
                int index = nodeInfo.Index;

                line.Clear();

                if (depth > 0)
                {
                    for (int i = 0; i < depth - 1; i++)
                    {
                        if (i < previous.Length)
                        {
                            switch (previous[i])
                            {
                                case FullWidthSpaceChar:
                                    line.Append(FullWidthSpaceChar);
                                    break;
                                case '│':
                                    line.Append('│');
                                    break;
                                case '└':
                                    line.Append(FullWidthSpaceChar);
                                    break;
                                case '├':
                                    line.Append('│');
                                    break;
                                default:
                                    line.Append(FullWidthSpaceChar);
                                    break;
                            }
                        }
                        else
                        { 
                            line.Append(FullWidthSpaceChar);
                        }
                    }

                    if (index == count - 1)
                    {
                        line.Append('└');
                    }
                    else
                    {
                        line.Append('├');
                    }
                }

                line.Append(node?.ToString());

                previous = line.ToString();
                tree.AppendLine(line.ToString());
            }   

            return tree.ToString();
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
