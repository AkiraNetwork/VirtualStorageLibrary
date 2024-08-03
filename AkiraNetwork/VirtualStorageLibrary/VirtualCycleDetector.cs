namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Class for detecting cycles in virtual symbolic links.
    /// Determines if a given link is part of a cycle.
    /// </summary>
    public class VirtualCycleDetector
    {
        /// <summary>
        /// Dictionary of detected cycles. The keys are <see cref="VirtualID"/>s, and the values are the 
        /// corresponding <see cref="VirtualSymbolicLink"/>s.
        /// </summary>
        private readonly Dictionary<VirtualID, VirtualSymbolicLink> _cycleDictionary;

        /// <summary>
        /// Gets the dictionary of detected cycles.
        /// </summary>
        /// <value>
        /// A dictionary where the keys are <see cref="VirtualID"/>s and the values are the corresponding 
        /// <see cref="VirtualSymbolicLink"/>s.
        /// </value>
        public Dictionary<VirtualID, VirtualSymbolicLink> CycleDictionary => _cycleDictionary;

        /// <summary>
        /// Gets the number of detected cycles.
        /// </summary>
        /// <value>
        /// The number of detected cycles.
        /// </value>
        public int Count => _cycleDictionary.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualCycleDetector"/> class.
        /// </summary>
        public VirtualCycleDetector()
        {
            _cycleDictionary = new Dictionary<VirtualID, VirtualSymbolicLink>();
        }

        /// <summary>
        /// Clears the dictionary of detected cycles.
        /// </summary>
        public void Clear()
        {
            _cycleDictionary.Clear();
        }

        /// <summary>
        /// Determines whether the specified <see cref="VirtualSymbolicLink"/> is part of a cycle.
        /// </summary>
        /// <param name="link">The <see cref="VirtualSymbolicLink"/> to check.</param>
        /// <returns>True if the link is part of a cycle; otherwise, false.</returns>
        public bool IsNodeInCycle(VirtualSymbolicLink link)
        {
            if (_cycleDictionary.ContainsKey(link.VID))
            {
                // A cycle has been detected
                return true;
            }

            _cycleDictionary.Add(link.VID, link);

            // No cycle detected
            return false;
        }
    }
}
