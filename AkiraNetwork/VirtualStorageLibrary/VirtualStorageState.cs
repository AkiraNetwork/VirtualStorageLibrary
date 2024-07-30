namespace AkiraNetwork.VirtualStorageLibrary
{
    public class VirtualStorageState
    {
        private static VirtualStorageState _state = new();

        public static VirtualStorageState State => _state;

        private VirtualStorageState()
        {
            InvalidNodeNameCharacters = [PathSeparator];
            InvalidFullNodeNames = [PathDot, PathDotDot];
        }

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

        internal static void InitializeFromSettings(VirtualStorageSettings settings)
        {
            _state = new VirtualStorageState
            {
                PathSeparator = settings.PathSeparator,
                PathRoot = settings.PathRoot,
                PathDot = settings.PathDot,
                PathDotDot = settings.PathDotDot,
                InvalidNodeNameCharacters = (char[])settings.InvalidNodeNameCharacters.Clone(),
                InvalidFullNodeNames = (string[])settings.InvalidFullNodeNames.Clone(),
                WildcardMatcher = settings.WildcardMatcher,
                NodeListConditions = settings.NodeListConditions,
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
            VirtualGroupCondition<VirtualNode, object>? groupCondition = null,
            List<VirtualSortCondition<VirtualNode>>? sortConditions = null)
        {
            _state.NodeListConditions = new VirtualNodeListConditions(filter, groupCondition, sortConditions);
        }
    }
}
