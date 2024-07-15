namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualCycleDetector
    {
        private readonly HashSet<VirtualPath> _checkedNodeSet;

        public HashSet<VirtualPath> CheckedNodeSet => _checkedNodeSet;

        public VirtualCycleDetector()
        {
            _checkedNodeSet = [];
        }

        public void Clear()
        {
            _checkedNodeSet.Clear();
        }

        public bool IsNodeInCycle(VirtualPath path)
        {
            if (!_checkedNodeSet.Add(path))
            {
                // 循環が発生している
                return true;
            }

            // 循環は発生していない
            return false;
        }
    }
}
