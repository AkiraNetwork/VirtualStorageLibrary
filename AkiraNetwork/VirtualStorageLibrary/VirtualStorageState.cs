namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Manages the state of the virtual storage. This class implements the singleton pattern
    /// and maintains the current settings and state.
    /// </summary>
    public class VirtualStorageState
    {
        private static VirtualStorageState _state = new();

        /// <summary>
        /// Gets the current instance of the state.
        /// </summary>
        public static VirtualStorageState State => _state;

        /// <summary>
        /// Initializes a new instance of the <c>VirtualStorageState</c> class with default settings.
        /// </summary>
        private VirtualStorageState()
        {
            InvalidNodeNameCharacters = [PathSeparator];
            InvalidNodeNames = [PathDot, PathDotDot];
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
        public VirtualNodeListConditions NodeListConditions { get; set; } = new();

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
        /// Gets or sets the prefix used for symbolic link names. The default value is "link".
        /// Used for auto-generating node names.
        /// </summary>
        /// <value>
        /// The prefix used for symbolic link names.
        /// </value>
        public string PrefixSymbolicLink { get; set; } = "link";

        /// <summary>
        /// Initializes the state from <c>VirtualStorageSettings</c>.
        /// </summary>
        /// <param name="settings">The settings to apply.</param>
        internal static void InitializeFromSettings(VirtualStorageSettings settings)
        {
            _state = new VirtualStorageState
            {
                PathSeparator = settings.PathSeparator,
                PathRoot = settings.PathRoot,
                PathDot = settings.PathDot,
                PathDotDot = settings.PathDotDot,
                InvalidNodeNameCharacters = (char[])settings.InvalidNodeNameCharacters.Clone(),
                InvalidNodeNames = (string[])settings.InvalidNodeNames.Clone(),
                WildcardMatcher = settings.WildcardMatcher,
                NodeListConditions = settings.NodeListConditions,
                PrefixItem = settings.PrefixItem,
                PrefixDirectory = settings.PrefixDirectory,
                PrefixSymbolicLink = settings.PrefixSymbolicLink
            };
        }

        /// <summary>
        /// Sets the conditions for listing nodes.
        /// </summary>
        /// <param name="conditions">The conditions for listing nodes.</param>
        public static void SetNodeListConditions(VirtualNodeListConditions conditions)
        {
            _state.NodeListConditions = conditions;
        }

        /// <summary>
        /// Sets the conditions for listing nodes.
        /// </summary>
        /// <param name="filter">The filter for node types.</param>
        /// <param name="groupCondition">The condition for grouping nodes.</param>
        /// <param name="sortConditions">The conditions for sorting nodes.</param>
        public static void SetNodeListConditions(
            VirtualNodeTypeFilter filter,
            VirtualGroupCondition<VirtualNode, object>? groupCondition = null,
            List<VirtualSortCondition<VirtualNode>>? sortConditions = null)
        {
            _state.NodeListConditions = new VirtualNodeListConditions(filter, groupCondition, sortConditions);
        }
    }
}
