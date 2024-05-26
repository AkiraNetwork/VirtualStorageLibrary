namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualStorageSettings
    {
        private static VirtualStorageSettings _settings = new();

        public static VirtualStorageSettings Settings => _settings;

        private VirtualStorageSettings()
        {
            InvalidNodeNameCharacters = [ PathSeparator ];
            InvalidFullNodeNames = [ PathDot, PathDotDot ];

            WildcardMatcher = new PowerShellWildcardMatcher();

            NodeListConditions = new()
            {
                Filter = VirtualNodeTypeFilter.All,
                GroupCondition = new(node => node.NodeType, true),
                SortConditions =
                [
                    new(node => node.Name, true)
                ]
            };
        }

        public static void Initialize()
        {
            _settings = new();
            VirtualStorageState.InitializeFromSettings(_settings);
        }

        public char PathSeparator { get; set; } = '/';

        public string PathRoot { get; set; } = "/";
        
        public string PathDot { get; set; } = ".";

        public string PathDotDot { get; set; } = "..";

        public char[] InvalidNodeNameCharacters { get; set; }

        public string[] InvalidFullNodeNames { get; set; }

        public IVirtualWildcardMatcher? WildcardMatcher { get; set; }

        public VirtualNodeListConditions NodeListConditions { get; set; }
    }
}
