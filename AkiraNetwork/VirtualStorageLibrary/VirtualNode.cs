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
    /// Represents an abstract class for nodes.
    /// </summary>
    public abstract class VirtualNode : IVirtualDeepCloneable<VirtualNode>
    {
        /// <summary>
        /// Gets the name of node.
        /// </summary>
        public VirtualNodeName Name { get; internal set; }

        /// <summary>
        /// Gets the created date of the node. This date represents when the node was first created.
        /// </summary>
        public DateTime CreatedDate { get; internal set; }

        /// <summary>
        /// Gets the updated date of the node. This date represents the last time the node was modified.
        /// It is set to the current date and time at the moment of instantiation or cloning.
        /// </summary>
        public DateTime UpdatedDate { get; internal set; }

        /// <summary>
        /// Gets the node type of node.
        /// </summary>
        public abstract VirtualNodeType NodeType { get; }

        /// <summary>
        /// Gets the VID of node.
        /// </summary>
        public VirtualID VID { get; private set; } = new VirtualID();

        /// <summary>
        /// Creates a deep clone of the entity. However, the CreatedDate and UpdatedDate 
        /// should not be cloned as they are set to the current date and time at the time
        /// of cloning.
        /// </summary>
        /// <param name="recursive">
        /// When true, all child nodes are also cloned, creating a
        /// deep copy of the entire tree. The default is false.
        /// The CreatedDate and UpdatedDate properties are not preserved.
        /// They are set to the current date and time at the moment of instantiation or cloning.
        /// </param>
        /// <returns>Cloned <see cref="VirtualNode"/> instance</returns>
        public abstract VirtualNode DeepClone(bool recursive = false);

        /// <summary>
        /// Updates the <see cref="VirtualNode"/>.
        /// </summary>
        /// <param name="node">Value to update</param>
        public abstract void Update(VirtualNode node);

        /// <summary>
        /// Gets a value indicating whether referenced in storage.
        /// If this property is true, the node is referenced from storage. Otherwise, it is not.
        /// </summary>
        public bool IsReferencedInStorage { get; internal set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNode"/> class.
        /// </summary>
        /// <param name="name">The name of node.</param>
        protected VirtualNode(VirtualNodeName name)
        {
            Name = name;
            CreatedDate = UpdatedDate = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNode"/> class.
        /// </summary>
        /// <param name="name">The name of node.</param>
        /// <param name="createdDate">The created date of node.</param>
        protected VirtualNode(VirtualNodeName name, DateTime createdDate)
        {
            Name = name;
            CreatedDate = UpdatedDate = createdDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNode"/> class.
        /// </summary>
        /// <param name="name">The name of node.</param>
        /// <param name="createdDate">The created date of node.</param>
        /// <param name="updatedDate">The updated date of node.</param>
        protected VirtualNode(VirtualNodeName name, DateTime createdDate, DateTime updatedDate)
        {
            Name = name;
            CreatedDate = createdDate;
            UpdatedDate = updatedDate;
        }
    }
}
