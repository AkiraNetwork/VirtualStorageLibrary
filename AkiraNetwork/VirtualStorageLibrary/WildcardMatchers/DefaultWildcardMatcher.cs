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

using System.Collections.ObjectModel;

namespace AkiraNetwork.VirtualStorageLibrary.WildcardMatchers
{
    /// <summary>
    /// Implements default wildcard matching using regular expression patterns.
    /// Provides a straightforward mapping of regular expression symbols.
    /// </summary>
    public class DefaultWildcardMatcher : IVirtualWildcardMatcher
    {
        /// <summary>
        /// Dictionary mapping regular expression symbols to their literal equivalents.
        /// </summary>
        /// <value>
        /// A read-only dictionary where the keys are regular expression symbols and the values
        /// are their literal equivalents.
        /// </value>
        private static readonly ReadOnlyDictionary<string, string> _wildcardDictionary = new(
            new Dictionary<string, string>
            {
                { @".", @"." },   // Matches any single character
                { @"*", @"*" },   // Matches zero or more characters
                { @"+", @"+" },   // Matches one or more characters
                { @"?", @"?" },   // Matches zero or one character
                { @"^", @"^" },   // Start of line
                { @"$", @"$" },   // End of line
                { @"|", @"|" },   // OR condition
                { @"(", @"(" },   // Start of group
                { @")", @")" },   // End of group
                { @"[", @"[" },   // Start of character class
                { @"]", @"]" },   // End of character class
                { @"{", @"{" },   // Start of repetition
                { @"}", @"}" },   // End of repetition
                { @"\", @"\" }    // Escape character
            });

        /// <summary>
        /// Gets the dictionary of regular expression symbols and their corresponding literals.
        /// </summary>
        /// <value>
        /// A read-only dictionary where the keys are regular expression symbols and the values
        /// are their literal equivalents.
        /// </value>
        public ReadOnlyDictionary<string, string> WildcardDictionary => _wildcardDictionary;

        /// <summary>
        /// Gets the list of supported wildcard characters.
        /// </summary>
        /// <value>
        /// A collection of supported wildcard characters.
        /// </value>
        public IEnumerable<string> Wildcards => _wildcardDictionary.Keys;

        /// <summary>
        /// Gets the list of regular expression patterns corresponding to the wildcard characters.
        /// </summary>
        /// <value>
        /// A collection of regular expression patterns corresponding to the wildcard characters.
        /// </value>
        public IEnumerable<string> Patterns => _wildcardDictionary.Values;

        /// <summary>
        /// Gets the number of supported wildcard characters.
        /// </summary>
        /// <value>
        /// The number of supported wildcard characters.
        /// </value>
        public int Count => _wildcardDictionary.Count;

        /// <summary>
        /// Performs simple regular expression-based wildcard matching.
        /// </summary>
        /// <param name="nodeName">The name of the node to match against the pattern.</param>
        /// <param name="pattern">The regular expression pattern to use for matching.</param>
        /// <returns>True if the node name matches the pattern; otherwise, false.</returns>
        public bool PatternMatcher(string nodeName, string pattern)
        {
            // Uses the pattern string as is for regular expression matching
            return Regex.IsMatch(nodeName, pattern);
        }

        /// <summary>
        /// Determines whether the specified wildcard pattern is valid according to the rules 
        /// defined by the wildcard matcher implementation. 
        /// This method checks if the wildcard pattern adheres to the syntax rules defined 
        /// by the wildcard matcher implementation. If the pattern is not valid, 
        /// this method returns <c>false</c>.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to validate.</param>
        /// <returns>
        /// <c>true</c> if the pattern is a valid wildcard pattern; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidWildcardPattern(string pattern)
        {
            try
            {
                // Uses the pattern string as is for regular expression matching
                _ = new Regex(pattern);
                return true;
            }
            catch (ArgumentException)
            {
                // If the regex pattern is not valid
                return false;
            }
        }
    }
}
