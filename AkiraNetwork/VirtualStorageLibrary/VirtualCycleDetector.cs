namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualCycleDetector
    {
        private readonly Dictionary<VirtualID, VirtualSymbolicLink> _cycleDictionary;

        public Dictionary<VirtualID, VirtualSymbolicLink> CycleDictionary => _cycleDictionary;

        public int Count => _cycleDictionary.Count;

        public VirtualCycleDetector()
        {
            _cycleDictionary = [];
        }

        public void Clear()
        {
            _cycleDictionary.Clear();
        }

        public bool IsNodeInCycle(VirtualSymbolicLink link)
        {
            if (_cycleDictionary.ContainsKey(link.VID))
            {
                // 循環が発生している
                return true;
            }

            _cycleDictionary.Add(link.VID, link);

            // 循環は発生していない
            return false;
        }
    }
}
