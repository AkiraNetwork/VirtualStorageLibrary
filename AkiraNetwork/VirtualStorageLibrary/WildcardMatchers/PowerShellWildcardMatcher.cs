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

namespace AkiraNetwork.VirtualStorageLibrary.WildcardMatchers
{
    /// <summary>
    /// Implements wildcard matching based on PowerShell's wildcard patterns.
    /// Provides pattern matching with support for specific escape characters.
    /// </summary>
    public class PowerShellWildcardMatcher : IVirtualWildcardMatcher
    {
        /// <summary>
        /// Dictionary mapping wildcard characters to regular expression patterns.
        /// </summary>
        /// <value>
        /// A read-only dictionary where the keys are wildcard characters and the values
        /// are their corresponding regular expression patterns.
        /// </value>
        private static readonly ReadOnlyDictionary<string, string> _wildcardDictionary = new(
            new Dictionary<string, string>
            {
                { "*", ".*" },  // Matches zero or more characters
                { "?", "." },   // Matches any single character
                { "[", "[" },   // Start of character class
                { "]", "]" }    // End of character class
            });

        /// <summary>
        /// Gets the dictionary of wildcard characters and their corresponding regular expression patterns.
        /// </summary>
        /// <value>
        /// A read-only dictionary where the keys are wildcard characters and the values
        /// are their corresponding regular expression patterns.
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
        /// Performs pattern matching using the specified PowerShell wildcard pattern.
        /// </summary>
        /// <param name="nodeName">The name of the node to match against the pattern.</param>
        /// <param name="pattern">The PowerShell wildcard pattern to use for matching.</param>
        /// <returns>True if the node name matches the pattern; otherwise, false.</returns>
        /// <remarks>
        /// If a wildcard character is escaped using a backtick (\`), the character is treated as a literal.
        /// The escape character itself is represented by the backtick (\`), and the character following it
        /// is the one being escaped.
        /// </remarks>
        public bool PatternMatcher(string nodeName, string pattern)
        {
            // Construct the regular expression pattern with escape handling
            string regexPattern = "^";
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == '`' && i + 1 < pattern.Length && Wildcards.Contains(pattern[i + 1].ToString()))
                {
                    // Treat the escaped wildcard character as a literal
                    regexPattern += Regex.Escape(pattern[i + 1].ToString());
                    i++; // Skip the escaped character
                }
                else
                {
                    string currentChar = pattern[i].ToString();
                    if (WildcardDictionary.TryGetValue(currentChar, out string? wildcard))
                    {
                        regexPattern += wildcard;
                    }
                    else
                    {
                        regexPattern += Regex.Escape(currentChar);
                    }
                }
            }
            regexPattern += "$";

            // Perform matching using the constructed regular expression
            return Regex.IsMatch(nodeName, regexPattern);
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
                string regexPattern = "^";
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (pattern[i] == '`' && i + 1 < pattern.Length && Wildcards.Contains(pattern[i + 1].ToString()))
                    {
                        regexPattern += Regex.Escape(pattern[i + 1].ToString());
                        i++; // Skip the escaped character
                    }
                    else
                    {
                        string currentChar = pattern[i].ToString();
                        if (WildcardDictionary.TryGetValue(currentChar, out string? value))
                        {
                            regexPattern += value;
                        }
                        else
                        {
                            regexPattern += Regex.Escape(currentChar);
                        }
                    }
                }
                regexPattern += "$";

                // Compiling the regex pattern to ensure it is valid
                _ = new Regex(regexPattern);
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
