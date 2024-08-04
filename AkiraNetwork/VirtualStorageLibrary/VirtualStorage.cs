using AkiraNetwork.VirtualStorageLibrary.Localization;
using System.Runtime.CompilerServices;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        private readonly VirtualDirectory _root;

        public VirtualDirectory Root => _root;

        public VirtualPath CurrentPath { get; private set; }

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
    }
}
