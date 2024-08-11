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
    /// Specifies the types of virtual nodes.
    /// </summary>
    public enum VirtualNodeType
    {
        /// <summary>Represents a directory node.</summary>
        Directory = 0,

        /// <summary>Represents an item node with a user-defined type T.</summary>
        Item = 1,

        /// <summary>Represents a symbolic link node.</summary>
        SymbolicLink = 2,

        /// <summary>Represents an unspecified or unknown node type.</summary>
        None = 3
    }

    /// <summary>
    /// Specifies the filters for virtual node types.
    /// </summary>
    public enum VirtualNodeTypeFilter
    {
        /// <summary>No specific node type is specified.</summary>
        None = 0x00,

        /// <summary>Filter for item nodes with a user-defined type T.</summary>
        Item = 0x01,

        /// <summary>Filter for directory nodes.</summary>
        Directory = 0x02,

        /// <summary>Filter for symbolic link nodes.</summary>
        SymbolicLink = 0x04,

        /// <summary>Filter for all node types.</summary>
        All = Item | Directory | SymbolicLink
    }
}
