namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// A structure representing a unique identifier within the virtual storage library.
    /// This identifier is based on a randomly generated UUID version 4 (UUIDv4). UUIDv4
    /// is a 128-bit value that ensures high uniqueness and has strong cryptographic properties.
    /// </summary>
    public readonly record struct VirtualID
    {
        private readonly Guid _id;

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> value representing the unique identifier.
        /// </value>
        public Guid ID => _id;

        /// <summary>
        /// Returns a string representation of this instance's unique identifier.
        /// </summary>
        /// <returns>A string that represents the unique identifier.</returns>
        public override string ToString() => _id.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualID"/> structure,
        /// generating a unique identifier.
        /// </summary>
        public VirtualID()
        {
            _id = Guid.NewGuid();
        }
    }
}
