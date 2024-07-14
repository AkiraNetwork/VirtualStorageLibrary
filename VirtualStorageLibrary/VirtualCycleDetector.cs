namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualCycleDetector
    {
        private readonly HashSet<VirtualID> _checkedNodeSet;

        public HashSet<VirtualID> CheckedNodeSet => _checkedNodeSet;

        public VirtualCycleDetector()
        {
            _checkedNodeSet = [];
        }

        public void Clear()
        {
            _checkedNodeSet.Clear();
        }

        public bool IsNodeInCycle(VirtualNode node)
        {
            if (!_checkedNodeSet.Add(node.VID))
            {
                // 循環が発生している
                return true;
            }

            // 循環は発生していない
            return false;
        }
    }
}
