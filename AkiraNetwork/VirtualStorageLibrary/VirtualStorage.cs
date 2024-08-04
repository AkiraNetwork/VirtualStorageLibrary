namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Manages virtual storage containing user-defined type T item data.
    /// </summary>
    /// <typeparam name="T">The user-defined type within the virtual storage</typeparam>
    public partial class VirtualStorage<T>
    {
        private readonly VirtualDirectory _root;

        /// <summary>
        /// Gets the root directory of the virtual storage.
        /// </summary>
        /// <value>The root directory of the virtual storage</value>
        public VirtualDirectory Root => _root;

        /// <summary>
        /// Gets or sets the current virtual path.
        /// </summary>
        /// <value>The current virtual path</value>
        public VirtualPath CurrentPath { get; private set; }

        /// <summary>
        /// Gets the cycle detector class used for the WalkPathToTarget method.
        /// </summary>
        /// <value>An instance of the cycle detector class</value>
        public VirtualCycleDetector CycleDetectorForTarget { get; } = new();

        /// <summary>
        /// Gets the cycle detector class used for the WalkPathTree method.
        /// </summary>
        /// <value>An instance of the cycle detector class</value>
        public VirtualCycleDetector CycleDetectorForTree { get; } = new();

        /// <summary>
        /// Initializes a new instance of the VirtualStorage class.
        /// </summary>
        public VirtualStorage()
        {
            _root = new(VirtualPath.Root)
            {
                IsReferencedInStorage = true
            };

            CurrentPath = VirtualPath.Root;

            _linkDictionary = [];

            Item = new(this);
            Dir = new(this);
            Link = new(this);
        }
    }
}
