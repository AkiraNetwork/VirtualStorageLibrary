using AkiraNetwork.VirtualStorageLibrary.Localization;
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
