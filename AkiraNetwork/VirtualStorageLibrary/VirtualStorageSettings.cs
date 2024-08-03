namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Manages settings for the virtual storage. This class implements the singleton pattern
    /// and holds default settings and various parameters.
    /// </summary>
    [DebuggerStepThrough]
    public class VirtualStorageSettings
    {
        private static VirtualStorageSettings _settings = new();

        /// <summary>
        /// Gets the current instance of the settings.
        /// </summary>
        public static VirtualStorageSettings Settings => _settings;

        private VirtualStorageSettings()
        {
            InvalidNodeNameCharacters = [PathSeparator];
            InvalidNodeNames = [PathDot, PathDotDot];

            WildcardMatcher = new PowerShellWildcardMatcher();

            NodeListConditions = new()
            {
                Filter = VirtualNodeTypeFilter.All,
                GroupCondition = new(node => node.ResolveNodeType(), true),
                SortConditions =
                [
                    new(node => node.Name, true)
                ]
            };
        }

        /// <summary>
        /// Initializes the settings. Existing settings are reset, and default settings
        /// are applied.
        /// </summary>
        public static void Initialize()
        {
            _settings = new();

            // Apply settings to state
            VirtualStorageState.InitializeFromSettings(_settings);
        }

        /// <summary>
        /// Gets or sets the path separator character. The default value is '/'.
        /// </summary>
        /// <value>
        /// The path separator character.
        /// </value>
        public char PathSeparator { get; set; } = '/';

        /// <summary>
        /// Gets or sets the root path. The default value is "/".
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        public string PathRoot { get; set; } = "/";

        /// <summary>
        /// Gets or sets the path representing the current directory. The default value
        /// is ".".
        /// </summary>
        /// <value>
        /// The path representing the current directory.
        /// </value>
        public string PathDot { get; set; } = ".";

        /// <summary>
        /// Gets or sets the path representing the parent directory. The default value
        /// is "..".
        /// </summary>
        /// <value>
        /// The path representing the parent directory.
        /// </value>
        public string PathDotDot { get; set; } = "..";

        /// <summary>
        /// Gets or sets an array of characters that are invalid in node names.
        /// </summary>
        /// <value>
        /// An array of characters that are invalid in node names.
        /// </value>
        public char[] InvalidNodeNameCharacters { get; set; }

        /// <summary>
        /// Gets or sets an array of invalid node names.
        /// </summary>
        /// <value>
        /// An array of invalid node names.
        /// </value>
        public string[] InvalidNodeNames { get; set; }

        /// <summary>
        /// Gets or sets the wildcard matcher.
        /// </summary>
        /// <value>
        /// The wildcard matcher.
        /// </value>
        public IVirtualWildcardMatcher? WildcardMatcher { get; set; }

        /// <summary>
        /// Gets or sets the conditions for listing nodes.
        /// </summary>
        /// <value>
        /// The conditions for listing nodes.
        /// </value>
        public VirtualNodeListConditions NodeListConditions { get; set; }

        /// <summary>
        /// Gets or sets the prefix used for item names. The default value is "item". 
        /// Used for auto-generating node names.
        /// </summary>
        /// <value>
        /// The prefix used for item names.
        /// </value>
        public string PrefixItem { get; set; } = "item";

        /// <summary>
        /// Gets or sets the prefix used for directory names. The default value is "dir".
        /// Used for auto-generating node names.
        /// </summary>
        /// <value>
        /// The prefix used for directory names.
        /// </value>
        public string PrefixDirectory { get; set; } = "dir";

        /// <summary>
        /// Gets or sets the prefix used for symbolic link names. The default value is
        /// "link". Used for auto-generating node names.
        /// </summary>
        /// <value>
        /// The prefix used for symbolic link names.
        /// </value>
        public string PrefixSymbolicLink { get; set; } = "link";
    }
}
