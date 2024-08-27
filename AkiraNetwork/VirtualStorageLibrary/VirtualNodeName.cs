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

using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Represents the name of a virtual node. This class handles the generation,
    /// validation, and comparison of node names.
    /// </summary>
    [DebuggerStepThrough]
    public class VirtualNodeName : IEquatable<VirtualNodeName>, IComparable<VirtualNodeName>, IComparable
    {
        private readonly string _name;

        // Counter appended to generated node names
        private static long _counter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNodeName"/> class
        /// with the specified name.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        public VirtualNodeName(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <value>
        /// The name of this node.
        /// </value>
        public string Name => _name;

        /// <summary>
        /// Returns the name of the node as a string.
        /// </summary>
        /// <returns>The name of the node as a string.</returns>
        public override string ToString() => _name;

        /// <summary>
        /// Indicates whether the node is the root node.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is the root node; otherwise, <c>false</c>.
        /// </value>
        public bool IsRoot => _name == VirtualStorageState.State.PathRoot;

        /// <summary>
        /// Indicates whether the node is a single dot representing the current directory.
        /// </summary>
        /// <value>
        /// <c>true</c> if this node is dot; otherwise, <c>false</c>.
        /// </value>
        public bool IsDot => _name == VirtualStorageState.State.PathDot;

        /// <summary>
        /// Indicates whether the node is a double dot representing the parent directory.
        /// </summary>
        /// <value>
        /// <c>true</c> if this node is double dot"; otherwise, <c>false</c>.
        /// </value>
        public bool IsDotDot => _name == VirtualStorageState.State.PathDotDot;

        /// <summary>
        /// Generates a new node name using the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix for the node name.</param>
        /// <returns>The generated node name.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the prefix is an empty string.
        /// </exception>
        /// <remarks>
        /// The current implementation may be improved for better usability, including
        /// a possible re-implementation of the node name generation. Users are advised
        /// to check for updates in the documentation regularly.
        /// </remarks>
        public static VirtualNodeName GenerateNodeName(string prefix)
        {
            if (prefix == string.Empty)
            {
                throw new ArgumentException(Resources.PrefixIsEmpty, nameof(prefix));
            }

            long currentNumber = Interlocked.Increment(ref _counter);
            string generatedName = $"{prefix}{currentNumber}";

            return new VirtualNodeName(generatedName);
        }

        /// <summary>
        /// Resets the counter used for node name generation.
        /// </summary>
        public static void ResetCounter()
        {
            Interlocked.Exchange(ref _counter, 0);
        }

        /// <summary>
        /// Checks if a node name is valid.
        /// </summary>
        /// <param name="nodeName">The node name to check.</param>
        /// <returns><c>true</c> if the node name is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValidNodeName(VirtualNodeName nodeName)
        {
            if (nodeName.Name == string.Empty)
            {
                return false;
            }

            foreach (char c in VirtualStorageState.State.InvalidNodeNameCharacters)
            {
                if (nodeName.Name.Contains(c))
                {
                    return false;
                }
            }

            foreach (string invalidFullNodeName in VirtualStorageState.State.InvalidNodeNames)
            {
                if (nodeName.Name == invalidFullNodeName)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Implicitly converts a string to a <see cref="VirtualNodeName"/>.
        /// </summary>
        /// <param name="name">The string to convert.</param>
        public static implicit operator VirtualNodeName(string name)
        {
            return new VirtualNodeName(name);
        }

        /// <summary>
        /// Implicitly converts a <see cref="VirtualNodeName"/> to a string.
        /// </summary>
        /// <param name="nodeName">The <see cref="VirtualNodeName"/> to convert.</param>
        public static implicit operator string(VirtualNodeName nodeName)
        {
            return nodeName._name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="VirtualNodeName"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="VirtualNodeName"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="VirtualNodeName"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(VirtualNodeName? other)
        {
            return _name == other?._name;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (obj is VirtualNodeName other)
            {
                return _name == other._name;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode() => _name.GetHashCode();

        /// <summary>
        /// Compares this instance with a specified <see cref="VirtualNodeName"/>.
        /// </summary>
        /// <param name="other">The <see cref="VirtualNodeName"/> to compare with.</param>
        /// <returns>
        /// A value indicating the relative order of the instances being compared.
        /// </returns>
        public int CompareTo(VirtualNodeName? other)
        {
            if (other == null)
            {
                return 1;
            }

            return string.Compare(_name, other._name, StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares this instance with a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>
        /// A value indicating the relative order of the instances being compared.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the object is not a <see cref="VirtualNodeName"/>.
        /// </exception>
        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is not VirtualNodeName)
            {
                throw new ArgumentException(Resources.ParameterIsNotVirtualNodeName,
                    nameof(obj));
            }

            return CompareTo((VirtualNodeName)obj);
        }

        /// <summary>
        /// Determines whether two <see cref="VirtualNodeName"/> instances are equal.
        /// </summary>
        /// <param name="left">The left <see cref="VirtualNodeName"/> to compare.</param>
        /// <param name="right">The right <see cref="VirtualNodeName"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the specified instances are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(VirtualNodeName? left, VirtualNodeName? right)
        {
            // Both are null
            if (left is null && right is null)
            {
                return true;
            }

            // One is null
            if (left is null || right is null)
            {
                return false;
            }

            // Compare actual node names
            return left._name == right._name;
        }

        /// <summary>
        /// Determines whether two <see cref="VirtualNodeName"/> instances are not equal.
        /// </summary>
        /// <param name="left">The left <see cref="VirtualNodeName"/> to compare.</param>
        /// <param name="right">The right <see cref="VirtualNodeName"/> to compare.</param>
        /// <returns>
        /// <c>true</c> if the specified instances are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(VirtualNodeName? left, VirtualNodeName? right)
        {
            return !(left == right);
        }
    }
}
