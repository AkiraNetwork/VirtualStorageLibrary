// This file is part of VirtualStorageLibrary.
//
// Copyright (C) 2024 Akira Shimodate
//
// VirtualStorageLibrary is free software, and it is distributed under the terms of 
// the GNU Lesser General Public License (version 3, or at your option, any later 
// version). This license is published by the Free Software Foundation.
//
// VirtualStorageLibrary is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for 
// more details.
//
// You should have received a copy of the GNU Lesser General Public License along 
// with VirtualStorageLibrary. If not, see https://www.gnu.org/licenses/.

namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Adapter class to simplify operations on virtual items.
    /// Reduces the need for casting and simplifies code, allowing
    /// users to easily manage specific node types.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the virtual storage.</typeparam>
    public class VirtualItemAdapter<T>
    {
        private readonly VirtualStorage<T> _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualItemAdapter{T}"/> class.
        /// </summary>
        /// <param name="storage">The virtual storage instance.</param>
        public VirtualItemAdapter(VirtualStorage<T> storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Gets or sets the virtual item corresponding to the specified path.
        /// </summary>
        /// <param name="path">The path of the item.</param>
        /// <param name="followLinks">A flag indicating whether to follow links.</param>
        /// <value>The <see cref="VirtualItem{T}"/> corresponding to the specified path.</value>
        public VirtualItem<T> this[VirtualPath path, bool followLinks = true]
        {
            get => _storage.GetItem(path, followLinks);
            set => _storage.SetNode(path, value);
        }
    }

    /// <summary>
    /// Adapter class to simplify operations on virtual symbolic links.
    /// Reduces the need for casting and simplifies code, allowing
    /// users to easily manage specific node types.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the virtual storage.</typeparam>
    public class VirtualSymbolicLinkAdapter<T>
    {
        private readonly VirtualStorage<T> _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualSymbolicLinkAdapter{T}"/> class.
        /// </summary>
        /// <param name="storage">The virtual storage instance.</param>
        public VirtualSymbolicLinkAdapter(VirtualStorage<T> storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Gets or sets the virtual symbolic link corresponding to the specified path.
        /// </summary>
        /// <param name="path">The path of the symbolic link.</param>
        /// <value>The <see cref="VirtualSymbolicLink"/> corresponding to the specified path.</value>
        /// <remarks>
        /// This adapter directly operates on the symbolic link itself and does not resolve the link to its target. 
        /// Ensure that this behavior is desired, as any operations will affect the symbolic link rather than the linked item.
        /// </remarks>
        public VirtualSymbolicLink this[VirtualPath path]
        {
            get => _storage.GetSymbolicLink(path);
            set => _storage.SetNode(path, value);
        }
    }
}
