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
    }
}
