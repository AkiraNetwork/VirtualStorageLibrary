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

using System.Collections.ObjectModel;

namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Provides functionality to create deep clones of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to clone.</typeparam>
    public interface IVirtualDeepCloneable<T>
    {
        /// <summary>
        /// Creates a deep clone of the object.
        /// </summary>
        /// <param name="recursive">
        /// Indicates whether to recursively clone nested objects. If true, a recursive
        /// clone is performed; if false, only the object itself is cloned. This parameter
        /// is only relevant for VirtualDirectory, and is ignored for VirtualItem{T} and
        /// VirtualSymbolicLink.
        /// </param>
        /// <returns>The cloned object.</returns>
        T DeepClone(bool recursive = false);
    }

    /// <summary>
    /// Provides functionality for wildcard matching.
    /// </summary>
    public interface IVirtualWildcardMatcher
    {
        /// <summary>
        /// Gets a dictionary of wildcards and their corresponding regex patterns.
        /// </summary>
        /// <value>
        /// A read-only dictionary containing wildcards as keys and their corresponding
        /// regex patterns as values.
        /// </value>
        ReadOnlyDictionary<string, string> WildcardDictionary { get; }

        /// <summary>
        /// Gets a collection of available wildcards.
        /// </summary>
        /// <value>
        /// An enumerable collection of strings representing the available wildcards.
        /// </value>
        IEnumerable<string> Wildcards { get; }

        /// <summary>
        /// Gets a collection of available patterns.
        /// </summary>
        /// <value>
        /// An enumerable collection of strings representing the available patterns.
        /// </value>
        IEnumerable<string> Patterns { get; }

        /// <summary>
        /// Gets the number of wildcards.
        /// </summary>
        /// <value>
        /// An integer representing the number of wildcards available.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Determines whether the specified node name matches the specified pattern.
        /// </summary>
        /// <param name="nodeName">The node name to check.</param>
        /// <param name="pattern">The pattern to compare against.</param>
        /// <returns>True if the node name matches the pattern; otherwise, false.</returns>
        bool PatternMatcher(string nodeName, string pattern);

        /// <summary>
        /// Checks if the provided wildcard pattern is valid.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to validate.</param>
        /// <returns>True if the pattern is valid; otherwise, false.</returns>
        bool IsValidWildcardPattern(string pattern);
    }
}
