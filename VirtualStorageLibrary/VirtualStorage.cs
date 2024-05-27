namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualStorage
    {
        private readonly VirtualDirectory _root;

        public VirtualDirectory Root => _root;

        public VirtualPath CurrentPath { get; private set; }

        private readonly Dictionary<VirtualPath, List<VirtualPath>> _linkDictionary;

        public Dictionary<VirtualPath, List<VirtualPath>> LinkDictionary => _linkDictionary;

        public VirtualStorage()
        {
            _root = new VirtualDirectory(VirtualPath.Root);
            CurrentPath = VirtualPath.Root;
            _linkDictionary = [];
        }

        public void UpdateLinkTargetNodeTypes(VirtualPath targetPath)
        {
            if (_linkDictionary.TryGetValue(targetPath, out List<VirtualPath>? linkPathList))
            {
                VirtualNodeType targetType = GetNodeType(targetPath, true);
                foreach (VirtualPath linkPath in linkPathList)
                {
                    VirtualSymbolicLink symbolicLink = (VirtualSymbolicLink)GetNode(linkPath);
                    symbolicLink.TargetNodeType = targetType;
                }
            }
        }

        public void AddLinkToDictionary(VirtualPath targetPath, VirtualPath linkPath)
        {
            if (!targetPath.IsAbsolute)
            {
                throw new ArgumentException("リンク先のパスは絶対パスである必要があります。", nameof(targetPath));
            }

            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            if (!_linkDictionary.TryGetValue(targetPath, out List<VirtualPath>? linkPathList))
            {
                linkPathList = [];
                _linkDictionary[targetPath] = linkPathList;
            }

            linkPathList.Add(linkPath);

            VirtualSymbolicLink symbolicLink = (VirtualSymbolicLink)GetNode(linkPath);
            symbolicLink.TargetNodeType = GetNodeType(targetPath, true);
        }

        public void RemoveLinkToDictionary(VirtualPath targetPath, VirtualPath linkPath)
        {
            if (!targetPath.IsAbsolute)
            {
                throw new ArgumentException("リンク先のパスは絶対パスである必要があります。", nameof(targetPath));
            }

            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            if (_linkDictionary.TryGetValue(targetPath, out List<VirtualPath>? linkPathsList))
            {
                linkPathsList.Remove(linkPath);

                if (linkPathsList.Count == 0)
                {
                    _linkDictionary.Remove(targetPath);
                }

                VirtualSymbolicLink symbolicLink = (VirtualSymbolicLink)GetNode(linkPath);
                symbolicLink.TargetNodeType = VirtualNodeType.None;
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

            // relativePathをeffectiveBasePathに基づいて絶対パスに変換
            var absolutePath = basePath + relativePath;

            return absolutePath;
        }

        public void AddSymbolicLink(VirtualPath path, VirtualPath targetPath, bool overwrite = false)
        {
            // linkPathを絶対パスに変換し正規化も行う
            path = ConvertToAbsolutePath(path).NormalizePath();

            // directoryPath（ディレクトリパス）と linkName（リンク名）を分離
            VirtualPath directoryPath = path.DirectoryPath;
            VirtualNodeName linkName = path.NodeName;

            // 対象ディレクトリを安全に取得
            VirtualDirectory? directory = TryGetDirectory(directoryPath, followLinks: true) ??
                throw new VirtualNodeNotFoundException($"ディレクトリ '{directoryPath}' が存在しません。");

            // 既存のノードの存在チェック
            if (directory.NodeExists(linkName))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException($"ノード '{linkName}' は既に存在します。上書きは許可されていません。");
                }
                else
                {
                    // 既存のノードがシンボリックリンクであるかどうかチェック
                    if (!directory.SymbolicLinkExists(linkName))
                    {
                        throw new InvalidOperationException($"既存のノード '{linkName}' はシンボリックリンクではありません。シンボリックリンクのみ上書き可能です。");
                    }
                    // 既存のシンボリックリンクを削除（上書きフラグが true の場合）
                    directory.Remove(linkName);
                }
            }

            // シンボリックリンクを作成
            VirtualSymbolicLink symbolicLink = new(linkName, targetPath);
            
            // 新しいシンボリックリンクを追加
            directory.Add(symbolicLink);

            // targetPathを絶対パスに変換して正規化する必要がある。その際、シンボリックリンクを作成したディレクトリパスを基準とする
            VirtualPath absoluteTargetPath = ConvertToAbsolutePath(targetPath, directoryPath).NormalizePath();

            // リンク辞書にリンク情報を追加
            AddLinkToDictionary(absoluteTargetPath, path);
        }

        public void AddItem<T>(VirtualPath path, VirtualItem<T> item, bool overwrite = false)
        {
            // 絶対パスに変換
            path = ConvertToAbsolutePath(path).NormalizePath();

            // 対象ディレクトリを取得
            VirtualDirectory directory = GetDirectory(path, true);

            // 既存のアイテムの存在チェック
            if (directory.NodeExists(item.Name))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException($"アイテム '{item.Name}' は既に存在します。上書きは許可されていません。");
                }
                else
                {
                    // 上書き対象がアイテムであることを確認
                    if (!ItemExists(path + item.Name))
                    {
                        throw new InvalidOperationException($"'{item.Name}' はアイテム以外のノードです。アイテムの上書きはできません。");
                    }
                    // 既存アイテムの削除
                    directory.Remove(item.Name);
                }
            }

            // 新しいアイテムを追加
            directory.Add(item, overwrite);

            // 作成したノードがリンクターゲットとして登録されている場合、リンクターゲットのノードタイプを更新
            UpdateLinkTargetNodeTypes(path + item.Name);
        }

        public void AddItem<T>(VirtualPath path, T data, bool overwrite = false)
        {
            // 絶対パスに変換
            path = ConvertToAbsolutePath(path).NormalizePath();

            // ディレクトリパスとアイテム名を分離
            VirtualPath directoryPath = path.DirectoryPath;
            VirtualNodeName itemName = path.NodeName;

            // アイテムを作成
            VirtualItem<T> item = new(itemName, data);

            // AddItemメソッドを呼び出し
            AddItem(directoryPath, item, overwrite);
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
            VirtualNodeContext nodeContext;

            if (createSubdirectories)
            {
                nodeContext = WalkPathToTarget(directoryPath, null, CreateIntermediateDirectory, true, true);
            }
            else
            {
                nodeContext = WalkPathToTarget(directoryPath, null, null, true, true);
            }

            if (nodeContext.Node is VirtualDirectory directory)
            {
                if (directory.NodeExists(newDirectoryName))
                {
                    throw new InvalidOperationException($"ディレクトリ '{newDirectoryName}' は既に存在します。");
                }

                // 新しいディレクトリを追加
                VirtualDirectory newDirectory = new(newDirectoryName);
                directory.Add(newDirectory);

                // 作成したノードがリンクターゲットとして登録されている場合、リンクターゲットのノードタイプを更新
                UpdateLinkTargetNodeTypes(path);
            }
            else
            {
                throw new VirtualNodeNotFoundException($"ノード '{directoryPath}' はディレクトリではありません。");
            }

            return;
        }

        private bool CreateIntermediateDirectory(VirtualDirectory directory, VirtualNodeName nodeName)
        {
            // 中間ディレクトリを追加
            VirtualDirectory newSubdirectory = new(nodeName);
            directory.Add(newSubdirectory);

            // 中間ディレクトリがリンクターゲットとして登録されている場合、リンクターゲットのノードタイプを更新
            UpdateLinkTargetNodeTypes(directory.Name + nodeName);

            return true;
        }

        public VirtualNodeContext WalkPathToTarget(
            VirtualPath targetPath,
            NotifyNodeDelegate? notifyNode = null,
            ActionNodeDelegate? actionNode = null,
            bool followLinks = true,
            bool exceptionEnabled = true)
        {
            targetPath = ConvertToAbsolutePath(targetPath).NormalizePath();
            VirtualNodeContext? nodeContext = WalkPathToTargetInternal(targetPath, 0, VirtualPath.Root, null, _root, notifyNode, actionNode, followLinks, exceptionEnabled, false);

            return nodeContext;
        }

        private VirtualNodeContext WalkPathToTargetInternal(
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
            // ターゲットがルートディレクトリの場合は、ルートノードを通知して終了
            if (targetPath.IsRoot)
            {
                notifyNode?.Invoke(VirtualPath.Root, _root, true);
                return new VirtualNodeContext(_root, VirtualPath.Root, null, 0, 0, VirtualPath.Root, resolved);
            }

            VirtualNodeName traversalNodeName = targetPath.PartsList[traversalIndex];

            while (!traversalDirectory.NodeExists(traversalNodeName))
            {
                if (actionNode != null)
                {
                    if (actionNode(traversalDirectory, traversalNodeName))
                    {
                        continue;
                    }
                }

                // 例外が有効な場合は例外をスロー
                if (exceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException($"ノード '{traversalPath + traversalNodeName}' が見つかりません。");
                }

                return new VirtualNodeContext(null, traversalPath, null, 0, 0, traversalPath, resolved);
            }

            VirtualNodeContext? nodeContext;

            // 探索ノードを取得
            VirtualNode? node = traversalDirectory[traversalNodeName];

            // 探索パスを更新
            traversalPath += traversalNodeName;

            // 次のノードへ
            traversalIndex++;

            if (node is VirtualDirectory directory)
            {
                // 最後のノードに到達したかチェック
                if (targetPath.PartsList.Count <= traversalIndex)
                {
                    // 末端のノードを通知
                    notifyNode?.Invoke(traversalPath, node, true);
                    resolvedPath ??= traversalPath;
                    return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
                }

                // 途中のノードを通知
                notifyNode?.Invoke(traversalPath, node, false);

                // 探索ディレクトリを取得
                traversalDirectory = directory;

                // 再帰的に探索
                nodeContext = WalkPathToTargetInternal(targetPath, traversalIndex, traversalPath, resolvedPath, traversalDirectory, notifyNode, actionNode, followLinks, exceptionEnabled, resolved);
                node = nodeContext?.Node;
                traversalPath = nodeContext?.TraversalPath ?? traversalPath;
                resolvedPath = nodeContext?.ResolvedPath ?? resolvedPath;
            }
            else if (node is VirtualItem)
            {
                // 末端のノードを通知
                notifyNode?.Invoke(traversalPath, node, true);

                // 最後のノードに到達したかチェック
                if (targetPath.PartsList.Count <= traversalIndex)
                {
                    resolvedPath ??= traversalPath;
                    return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
                }

                resolvedPath ??= traversalPath;

                // 例外が有効な場合は例外をスロー
                if (exceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException($"ノード '{targetPath}' まで到達できません。ノード '{traversalPath}' はアイテムです。");
                }

                return new VirtualNodeContext(null, traversalPath, null, 0, 0, resolvedPath, resolved);
            }
            else if (node is VirtualSymbolicLink link)
            {
                if (!followLinks)
                {
                    // シンボリックリンクを通知
                    notifyNode?.Invoke(traversalPath, node, true);
                    resolvedPath ??= traversalPath;
                    return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
                }

                resolved = true;

                VirtualPath? linkTargetPath = link.TargetPath;
                VirtualPath parentTraversalPath = traversalPath.DirectoryPath;

                // シンボリックリンクのリンク先パスを絶対パスに変換
                linkTargetPath = ConvertToAbsolutePath(linkTargetPath, parentTraversalPath);

                // リンク先のパスを正規化する
                linkTargetPath = linkTargetPath.NormalizePath();

                // シンボリックリンクのリンク先パスを再帰的に探索
                nodeContext = WalkPathToTargetInternal(linkTargetPath, 0, VirtualPath.Root, null, _root, null, null, true, exceptionEnabled, resolved);

                node = nodeContext?.Node;
                //traversalPath = result?.TraversalPath ?? traversalPath;

                // 解決済みのパスに未探索のパスを追加
                resolvedPath = nodeContext?.ResolvedPath!.CombineFromIndex(targetPath, traversalIndex);

                // シンボリックリンクを通知
                notifyNode?.Invoke(traversalPath, node, true);

                if (node != null && (node is VirtualDirectory linkDirectory))
                {
                    // 探索ディレクトリを取得
                    traversalDirectory = linkDirectory;

                    // 最後のノードに到達したかチェック
                    if (targetPath.PartsList.Count <= traversalIndex)
                    {
                        // 末端のノードを通知
                        resolvedPath ??= traversalPath;
                        return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
                    }

                    // 再帰的に探索
                    nodeContext = WalkPathToTargetInternal(targetPath, traversalIndex, traversalPath, resolvedPath, traversalDirectory, notifyNode, actionNode, followLinks, exceptionEnabled, resolved);
                    node = nodeContext?.Node;
                    traversalPath = nodeContext?.TraversalPath ?? traversalPath;
                    resolvedPath = nodeContext?.ResolvedPath ?? resolvedPath;
                }

                resolvedPath ??= traversalPath;
                return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
            }

            resolvedPath ??= traversalPath;
            return new VirtualNodeContext(node, traversalPath, null, 0, 0, resolvedPath, resolved);
        }

        public IEnumerable<VirtualNodeContext> WalkPathTree(
            VirtualPath basePath,
            VirtualNodeTypeFilter filter = VirtualNodeTypeFilter.All,
            bool recursive = true,
            bool followLinks = true)
        {
            basePath = ConvertToAbsolutePath(basePath).NormalizePath();
            int baseDepth = basePath.GetBaseDepth();
            VirtualNode node = GetNode(basePath, followLinks);

            return WalkPathTreeInternal(basePath, basePath, node, null, baseDepth, 0, 0, filter, recursive, followLinks, null);
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

            return WalkPathTreeInternal(basePath, basePath, node, null, baseDepth, 0, 0, filter, true, true, patternList);
        }

        private IEnumerable<VirtualNodeContext> WalkPathTreeInternal(
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
            List<string>? patternList)
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
            if (baseNode is VirtualDirectory directory)
            {
                if (filter.HasFlag(VirtualNodeTypeFilter.Directory))
                {
                    if (patternMatcher != null && patternList != null)
                    {
                        if (baseDepth + currentDepth == patternList.Count)
                        {
                            if (MatchPatterns(currentPath.PartsList, patternList))
                            {
                                // ディレクトリを通知
                                yield return new VirtualNodeContext(directory, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                            }
                        }
                    }
                    else
                    {
                        // ディレクトリを通知
                        yield return new VirtualNodeContext(directory, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                    }
                }

                if (recursive || 0 == currentDepth)
                {
                    // ディレクトリ内のノードを再帰的に探索
                    int index = 0;
                    foreach (var node in directory.Nodes)
                    {
                        VirtualPath path = currentPath + node.Name;
                        foreach (var result in WalkPathTreeInternal(basePath, path, node, directory, baseDepth, currentDepth + 1, index, filter, recursive, followLinks, patternList))
                        {
                            yield return result;
                        }
                        index++;
                    }
                }
            }
            else if (baseNode is VirtualItem item)
            {
                if (filter.HasFlag(VirtualNodeTypeFilter.Item))
                {
                    if (patternMatcher != null && patternList != null)
                    {
                        if (baseDepth + currentDepth == patternList.Count)
                        {
                            if (MatchPatterns(currentPath.PartsList, patternList))
                            {
                                // アイテムを通知
                                yield return new VirtualNodeContext(item, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                            }
                        }
                    }
                    else
                    {
                        // アイテムを通知
                        yield return new VirtualNodeContext(item, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                    }
                }
            }
            else if (baseNode is VirtualSymbolicLink link)
            {
                if (followLinks)
                {
                    VirtualPath? linkTargetPath = link.TargetPath;

                    // シンボリックリンクのリンク先パスを絶対パスに変換
                    linkTargetPath = ConvertToAbsolutePath(linkTargetPath, currentPath).NormalizePath();

                    // リンク先のノードを取得
                    VirtualNode? linkTargetNode = GetNode(linkTargetPath, followLinks);

                    // リンク先のノードに対して再帰的に探索
                    foreach (var result in WalkPathTreeInternal(basePath, currentPath, linkTargetNode, parentDirectory, baseDepth, currentDepth, currentIndex, filter, recursive, followLinks, patternList))
                    {
                        yield return result;
                    }
                }
                else
                {
                    if (filter.HasFlag(VirtualNodeTypeFilter.SymbolicLink))
                    {
                        if (patternMatcher != null && patternList != null)
                        {
                            if (baseDepth + currentDepth == patternList.Count)
                            {
                                if (MatchPatterns(currentPath.PartsList, patternList))
                                {
                                    // シンボリックリンクを通知
                                    yield return new VirtualNodeContext(link, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
                                }
                            }
                        }
                        else
                        {
                            // シンボリックリンクを通知
                            yield return new VirtualNodeContext(link, currentPath.GetRelativePath(basePath), parentDirectory, currentDepth, currentIndex);
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
                // GetNodeメソッドは、ノードが見つからない場合にnullを返すか、例外をスローするように実装されていると仮定
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

        public VirtualItem<T> GetItem<T>(VirtualPath path, bool followLinks = false)
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

        public VirtualItem<T>? TryGetItem<T>(VirtualPath path)
        {
            try
            {
                return GetItem<T>(path);
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

            if (node is VirtualDirectory)
            {
                return VirtualNodeType.Directory;
            }
            else if (node is VirtualItem)
            {
                return VirtualNodeType.Item;
            }
            else if (node is VirtualSymbolicLink)
            {
                return VirtualNodeType.SymbolicLink;
            }

            return VirtualNodeType.None;
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

        public IEnumerable<VirtualNodeContext> CopyNode(
            VirtualPath sourcePath,
            VirtualPath destinationPath,
            bool overwrite = false,
            bool recursive = false,
            bool followLinks = false)
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

            IEnumerable<VirtualNodeContext> destinationContexts = [];

            // コピー元のツリーを取得
            IEnumerable<VirtualNodeContext> sourceContexts = WalkPathTree(sourcePath, VirtualNodeTypeFilter.All, recursive, followLinks);

            VirtualNode sourceNode = sourceContexts.First().Node!;

            IEnumerable<VirtualNodeContext> contexts = CopySingleInternal(sourcePath, sourceNode, destinationPath, null, overwrite, followLinks);
            destinationContexts = destinationContexts.Concat(contexts);

            if (recursive && sourceNode is VirtualDirectory sourceDirectory)
            {
                foreach (var sourceContext in sourceContexts.Skip(1))
                {
                    VirtualPath sourceSubPath = sourcePath + sourceContext.TraversalPath;
                    VirtualPath destinationSubPath = destinationPath + sourceDirectory.Name + sourceSubPath.GetRelativePath(sourcePath);
                    VirtualNode sourceSubNode = sourceContext.Node!;

                    contexts = CopySingleInternal(sourceSubPath, sourceSubNode, destinationSubPath, null, overwrite, followLinks);
                    destinationContexts = destinationContexts.Concat(contexts);
                }
            }

            return destinationContexts;
        }

        private IEnumerable<VirtualNodeContext> CopySingleInternal(
            VirtualPath sourcePath,
            VirtualNode sourceNode,
            VirtualPath destinationPath,
            VirtualPath? linkOriginalPath,
            bool overwrite,
            bool followLinks)
        {
            VirtualNodeName? newNodeName;

            IEnumerable<VirtualNodeContext> contexts = [];

            VirtualDirectory destinationDirectory = GetDirectory(destinationPath.DirectoryPath, true);
            VirtualNodeName destinationNodeName = destinationPath.NodeName;

            VirtualNode? destinationNode = destinationDirectory.Get(destinationNodeName, false);

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
                        destinationDirectory.Remove(destinationNodeName);
                    }
                    else
                    {
                        throw new InvalidOperationException($"アイテム '{destinationNodeName}' は既に存在します。上書きは許可されていません。");
                    }
                    newNodeName = destinationPath.NodeName;
                    break;

                case VirtualSymbolicLink link:
                    VirtualPath targetPath = ConvertToAbsolutePath(link.TargetPath).NormalizePath();
                    return CopySingleInternal(sourcePath, sourceNode, targetPath, destinationPath, overwrite, followLinks);

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

            // コピー操作の結果を表す VirtualNodeContext を生成して返却
            VirtualNodeContext context = new(
                newNode,
                destinationPath,
                destinationDirectory,
                depth,
                0
            );

            contexts = contexts.Append(context);

            return contexts;
        }

        public void RemoveNode(VirtualPath path, bool recursive = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();

            if (path.IsRoot)
            {
                throw new InvalidOperationException("ルートディレクトリを削除することはできません。");
            }

            VirtualNode node = GetNode(path, true);

            // ディレクトリを親ディレクトリから削除するための共通の親パスと親ディレクトリを取得
            VirtualPath parentPath = path.DirectoryPath;
            VirtualDirectory parentDirectory = GetDirectory(parentPath, true);

            if (node is VirtualDirectory directory)
            {
                if (!recursive && directory.Count > 0)
                {
                    throw new InvalidOperationException("ディレクトリが空ではなく、再帰フラグが設定されていません。");
                }

                // directory.Nodes コレクションのスナップショットを作成
                var nodesSnapshot = directory.Nodes.ToList();

                // スナップショットを反復処理して、各ノードを削除
                foreach (var subNode in nodesSnapshot)
                {
                    VirtualPath subPath = path + subNode.Name;
                    RemoveNode(subPath, recursive);
                }

                // ここで親ディレクトリからディレクトリを削除
                parentDirectory.Remove(directory.Name);
            }
            else
            {
                // ここで親ディレクトリからアイテム（ノード）を削除
                parentDirectory.Remove(node.Name);
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

            // 移動元のディレクトリ存在チェック
            if (DirectoryExists(absoluteSourcePath))
            {
                // ディレクトリの場合

                // 移動先の種類チェック
                if (DirectoryExists(absoluteDestinationPath))
                {
                    // ディレクトリの場合
                    VirtualDirectory destinationDirectory = GetDirectory(absoluteDestinationPath);
                    VirtualDirectory sourceDirectory = GetDirectory(absoluteSourcePath);
                    
                    // 移動先ディレクトリの同名チェック
                    if (destinationDirectory.NodeExists(sourceDirectory.Name))
                    {
                        // 同名のディレクトリが存在する場合
                        throw new InvalidOperationException($"移動先ディレクトリ '{absoluteDestinationPath}' に同名のノード '{sourceDirectory.Name}' が存在します。");
                    }
                    destinationDirectory.Add(sourceDirectory);
                    VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                    sourceParentDirectory.Remove(sourceDirectory.Name);
                }
                else
                {
                    // ディレクトリでない、または存在しない場合

                    // 移動先の存在チェック
                    if (!NodeExists(absoluteDestinationPath))
                    {
                        VirtualPath destinationParentPath = absoluteDestinationPath.GetParentPath();
                        if (!DirectoryExists(destinationParentPath))
                        {
                            // 移動先の親ディレクトリが存在しない場合
                            throw new VirtualNodeNotFoundException($"指定されたノード '{destinationParentPath}' は存在しません。");
                        }
                        VirtualDirectory destinationParentDirectory = GetDirectory(destinationParentPath);
                        VirtualNodeName destinationNodeName = absoluteDestinationPath.NodeName;
                        VirtualDirectory sourceDirectory = GetDirectory(absoluteSourcePath);

                        VirtualNodeName oldNodeName = sourceDirectory.Name;
                        sourceDirectory.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceDirectory);
                        VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                    else
                    {
                        // 存在する場合（移動先ノードがアイテム）
                        throw new InvalidOperationException($"移動先ノード '{absoluteDestinationPath}' はアイテムです。");
                    }
                }
            }
            else
            {
                // アイテムの場合

                // 移動先のディレクトリ存在チェック
                if (DirectoryExists(absoluteDestinationPath))
                {
                    // 存在する

                    VirtualDirectory destinationDirectory = GetDirectory(absoluteDestinationPath);
                    VirtualNode sourceNode = GetNode(absoluteSourcePath);

                    // 移動先ディレクトリの同名チェック
                    if (destinationDirectory.NodeExists(sourceNode.Name))
                    {
                        // 同名のノードが存在する場合

                        // 上書きチェック
                        if (overwrite)
                        {
                            // trueの場合、同名のノードを削除
                            destinationDirectory.Remove(sourceNode.Name);
                        }
                        else
                        {
                            // falseの場合、例外をスロー
                            throw new InvalidOperationException($"移動先ディレクトリ '{absoluteDestinationPath}' に同名のノード '{sourceNode.Name}' が存在します。");
                        }
                    }
                    destinationDirectory.Add(sourceNode);
                    VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                    sourceParentDirectory.Remove(sourceNode.Name);
                }
                else
                {
                    // 存在しない

                    // 移動先の存在チェック
                    if (!NodeExists(absoluteDestinationPath))
                    {
                        VirtualPath destinationParentPath = absoluteDestinationPath.GetParentPath();
                        if (!DirectoryExists(destinationParentPath))
                        {
                            // 移動先の親ディレクトリが存在しない場合
                            throw new VirtualNodeNotFoundException($"指定されたノード '{destinationParentPath}' は存在しません。");
                        }
                        VirtualDirectory destinationParentDirectory = GetDirectory(destinationParentPath);
                        VirtualNodeName destinationNodeName = absoluteDestinationPath.NodeName;
                        VirtualNode sourceNode = GetNode(absoluteSourcePath);

                        VirtualNodeName oldNodeName = sourceNode.Name;
                        sourceNode.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceNode);
                        VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                    else
                    {
                        // 存在する場合

                        VirtualDirectory destinationParentDirectory = GetDirectory(absoluteDestinationPath.GetParentPath());
                        VirtualNodeName destinationNodeName = absoluteDestinationPath.NodeName;
                        VirtualNode sourceNode = GetNode(absoluteSourcePath);

                        // 移動先ディレクトリの同名チェック
                        if (destinationParentDirectory.NodeExists(destinationNodeName))
                        {
                            // 同名のノードが存在する場合

                            // 上書きチェック
                            if (overwrite)
                            {
                                // trueの場合、同名のノードを削除
                                destinationParentDirectory.Remove(destinationNodeName);
                            }
                            else
                            {
                                // falseの場合、例外をスロー
                                throw new InvalidOperationException($"移動先ディレクトリ '{absoluteDestinationPath}' に同名のノード '{destinationNodeName}' が存在します。");
                            }
                        }

                        VirtualNodeName oldNodeName = sourceNode.Name;
                        sourceNode.Name = destinationNodeName;
                        destinationParentDirectory.Add(sourceNode);
                        VirtualDirectory sourceParentDirectory = GetDirectory(absoluteSourcePath.GetParentPath());
                        sourceParentDirectory.Remove(oldNodeName);
                    }
                }
            }
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

            VirtualNodeContext nodeContext = nodeContexts.First();
            VirtualNode baseNode = nodeContext.Node!;
            VirtualPath baseAbsolutePath = (basePath + nodeContext.TraversalPath).NormalizePath();
            if (baseNode is VirtualDirectory)
            {
                string baseNodeName;
                if (baseAbsolutePath == VirtualPath.Root)
                {
                    baseNodeName = baseAbsolutePath.NodeName;
                }
                else
                {
                    baseNodeName = baseAbsolutePath.NodeName + VirtualPath.Separator;
                }
                tree.AppendLine(baseNodeName);
            }
            else if (baseNode is VirtualItem)
            {
                tree.AppendLine(baseAbsolutePath.NodeName);
            }
            else if (baseNode is VirtualSymbolicLink link)
            {
                string baseNodeName;
                baseNodeName = (string)baseAbsolutePath.NodeName + " -> " + (string)link.TargetPath;
                tree.AppendLine(baseNodeName);
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

                if (node is VirtualDirectory)
                {
                    line.Append(nodeName + VirtualPath.Separator);
                }
                else if (node is VirtualSymbolicLink link)
                {
                    line.Append(nodeName + " -> " + (string)link.TargetPath);
                }
                else
                {
                    line.Append(nodeName);
                }
                previous = line.ToString();
                tree.AppendLine(line.ToString());
            }   

            return tree.ToString();
        }
    }
}
