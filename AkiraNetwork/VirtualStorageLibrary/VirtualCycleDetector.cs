// This file is part of VirtualStorageLibrary.
//
// Copyright (C) 2024 Akira Shimodate
//
// VirtualStorageLibrary is free software, and it is distributed under the terms of 
// the GNU General Public License (version 3, or at your option, any later version). 
// This license is published by the Free Software Foundation.
//
// VirtualStorageLibrary is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along 
// with VirtualStorageLibrary. If not, see https://www.gnu.org/licenses/.

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
            _cycleDictionary = [];
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
