namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualStorageState
    {
        private static VirtualStorageState _state = new();

        public static VirtualStorageState State => _state;

        private VirtualStorageState()
        {
            InvalidNodeNameCharacters = [ PathSeparator ];
            InvalidFullNodeNames = [ PathDot, PathDotDot ];
        }

        public static void Initialize() => _state = new();

        public char PathSeparator { get; set; } = '/';

        public string PathRoot { get; set; } = "/";

        public string PathDot { get; set; } = ".";

        public string PathDotDot { get; set; } = "..";

        public char[] InvalidNodeNameCharacters { get; set; }

        public string[] InvalidFullNodeNames { get; set; }

        public IVirtualWildcardMatcher? WildcardMatcher { get; set; }

        public VirtualNodeListConditions NodeListConditions { get; set; } = new();

        public string PrefixItem { get; set; } = "item";

        public string PrefixDirectory { get; set; } = "dir";

        public string PrefixSymbolicLink { get; set; } = "link";

        public static void InitializeFromSettings(VirtualStorageSettings settings)
        {
            VirtualNodeListConditions settingsConditions = settings.NodeListConditions;
            VirtualNodeListConditions stateConditions = new()
            {
                Filter = settingsConditions.Filter,
                GroupCondition = settingsConditions.GroupCondition != null
                    ? new VirtualGroupCondition<VirtualNode, object>(settingsConditions.GroupCondition.GroupBy, settingsConditions.GroupCondition.Ascending)
                    : null,
                SortConditions = settingsConditions.SortConditions?.Select(c => new VirtualSortCondition<VirtualNode>(c.SortBy, c.Ascending)).ToList()
            };

            _state = new VirtualStorageState
            {
                PathSeparator = settings.PathSeparator,
                PathRoot = settings.PathRoot,
                PathDot = settings.PathDot,
                PathDotDot = settings.PathDotDot,
                InvalidNodeNameCharacters = (char[])settings.InvalidNodeNameCharacters.Clone(),
                InvalidFullNodeNames = (string[])settings.InvalidFullNodeNames.Clone(),
                WildcardMatcher = settings.WildcardMatcher?.DeepClone(),
                NodeListConditions = stateConditions,
                PrefixItem = settings.PrefixItem,
                PrefixDirectory = settings.PrefixDirectory,
                PrefixSymbolicLink = settings.PrefixSymbolicLink
            };
        }

        public static void SetNodeListConditions(VirtualNodeListConditions conditions)
        {
            _state.NodeListConditions = conditions;
        }

        public static void SetNodeListConditions(
            VirtualNodeTypeFilter filter,
            VirtualGroupCondition<VirtualNode, object>? groupCondition,
            List<VirtualSortCondition<VirtualNode>>? sortConditions)
        {
            _state.NodeListConditions = new VirtualNodeListConditions(filter, groupCondition, sortConditions);
        }
    }
}
