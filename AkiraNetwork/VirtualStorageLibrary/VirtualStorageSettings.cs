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
        /// <value>
        /// The current instance of <see cref="VirtualStorageSettings"/>.
        /// </value>
        public static VirtualStorageSettings Settings => _settings;

        /// <summary>
        /// Initializes a new instance of the <c>VirtualStorageSettings</c> class with default settings.
        /// </summary>
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
        /// The character used to separate path segments.
        /// </value>
        public char PathSeparator { get; set; } = '/';

        /// <summary>
        /// Gets or sets the root path. The default value is "/".
        /// </summary>
        /// <value>
        /// The root directory path in the virtual storage.
        /// </value>
        public string PathRoot { get; set; } = "/";

        /// <summary>
        /// Gets or sets the path representing the current directory. The default value
        /// is ".".
        /// </summary>
        /// <value>
        /// The representation of the current directory.
        /// </value>
        public string PathDot { get; set; } = ".";

        /// <summary>
        /// Gets or sets the path representing the parent directory. The default value
        /// is "..".
        /// </summary>
        /// <value>
        /// The representation of the parent directory.
        /// </value>
        public string PathDotDot { get; set; } = "..";

        /// <summary>
        /// Gets or sets an array of characters that are invalid in node names.
        /// </summary>
        /// <value>
        /// An array of characters that cannot be used in node names.
        /// </value>
        public char[] InvalidNodeNameCharacters { get; set; }

        /// <summary>
        /// Gets or sets an array of invalid node names.
        /// </summary>
        /// <value>
        /// An array of node names that are not allowed in the virtual storage.
        /// </value>
        public string[] InvalidNodeNames { get; set; }

        /// <summary>
        /// Gets or sets the wildcard matcher.
        /// </summary>
        /// <value>
        /// The implementation of wildcard matching used in the storage.
        /// </value>
        public IVirtualWildcardMatcher? WildcardMatcher { get; set; }

        /// <summary>
        /// Gets or sets the conditions for listing nodes.
        /// </summary>
        /// <value>
        /// The conditions used to filter, group, and sort nodes.
        /// </value>
        public VirtualNodeListConditions NodeListConditions { get; set; }

        /// <summary>
        /// Gets or sets the prefix used for item names. The default value is "item".
        /// Used for auto-generating node names.
        /// </summary>
        /// <value>
        /// The prefix added to auto-generated item names.
        /// </value>
        public string PrefixItem { get; set; } = "item";

        /// <summary>
        /// Gets or sets the prefix used for directory names. The default value is "dir".
        /// Used for auto-generating node names.
        /// </summary>
        /// <value>
        /// The prefix added to auto-generated directory names.
        /// </value>
        public string PrefixDirectory { get; set; } = "dir";

        /// <summary>
        /// Gets or sets the prefix used for symbolic link names. The default value is
        /// "link". Used for auto-generating node names.
        /// </summary>
        /// <value>
        /// The prefix added to auto-generated symbolic link names.
        /// </value>
        public string PrefixSymbolicLink { get; set; } = "link";
    }
}
