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

using System.Runtime.CompilerServices;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        /// <summary>
        /// Gets or sets the virtual node corresponding to the specified virtual path.
        /// </summary>
        /// <param name="path">The virtual path</param>
        /// <param name="followLinks">Flag indicating whether to follow symbolic links</param>
        /// <value>The virtual node corresponding to the specified virtual path</value>
        [IndexerName("Indexer")]
        public VirtualDirectory this[VirtualPath path, bool followLinks = true]
        {
            get => GetDirectory(path, followLinks);
            set
            {
                // Compare the node ID of the target with the source.
                if (GetNodeID(path) != value.VID)
                {
                    // If they are different, set it.
                    SetNode(path, value);
                }
            }
        }
    }
}
