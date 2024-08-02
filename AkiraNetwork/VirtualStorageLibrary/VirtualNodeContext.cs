namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// A class that holds context information for a node.
    /// It is returned during or after path traversal, providing information about the node, path,
    /// parent directory, depth, index, resolved path, and symbolic link.
    /// </summary>
    [DebuggerStepThrough]
    public class VirtualNodeContext
    {
        /// <summary>
        /// Gets the node and can only be set within the assembly.
        /// </summary>
        /// <value>
        /// The current node.
        /// </value>
        public VirtualNode? Node { get; internal set; }

        /// <summary>
        /// Gets the traversal path and can only be set within the assembly.
        /// </summary>
        /// <value>
        /// The path used for node traversal.
        /// </value>
        public VirtualPath TraversalPath { get; internal set; }

        /// <summary>
        /// Gets the parent directory of the node and can only be set within the assembly.
        /// </summary>
        /// <value>
        /// The instance of the parent directory.
        /// </value>
        public VirtualDirectory? ParentDirectory { get; internal set; }

        /// <summary>
        /// Gets the depth of the node and can only be set within the assembly.
        /// The root is considered 0.
        /// </summary>
        /// <value>
        /// The depth of the node.
        /// </value>
        public int Depth { get; internal set; }

        /// <summary>
        /// Gets the index of the node and can only be set within the assembly.
        /// This is the index corresponding to the enumeration order within the directory.
        /// </summary>
        /// <value>
        /// The index of the node.
        /// </value>
        public int Index { get; internal set; }

        /// <summary>
        /// Gets the resolved path and can only be set within the assembly.
        /// This represents the result of resolving a symbolic link.
        /// </summary>
        /// <value>
        /// The resolved path, or null.
        /// </value>
        public VirtualPath? ResolvedPath { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the symbolic link has been resolved and can only
        /// be set within the assembly.
        /// </summary>
        /// <value>
        /// True if the link is resolved; otherwise, false.
        /// </value>
        public bool Resolved { get; internal set; }

        /// <summary>
        /// Gets the resolved symbolic link and can only be set within the assembly.
        /// </summary>
        /// <value>
        /// The resolved symbolic link, or null.
        /// </value>
        public VirtualSymbolicLink? ResolvedLink { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNodeContext"/> class.
        /// </summary>
        /// <param name="node">The node being traversed.</param>
        /// <param name="traversalPath">The path used for node traversal.</param>
        /// <param name="parentNode">The parent directory of the node.</param>
        /// <param name="depth">The depth of the node. The root is considered 0.</param>
        /// <param name="index">The index of the node. Corresponds to the enumeration order within the directory.</param>
        /// <param name="resolvedPath">The result of resolving a symbolic link.</param>
        /// <param name="resolved">Indicates whether the symbolic link has been resolved.</param>
        /// <param name="resolvedLink">The resolved symbolic link.</param>
        public VirtualNodeContext(
            VirtualNode? node,
            VirtualPath traversalPath,
            VirtualDirectory? parentNode = null,
            int depth = 0,
            int index = 0,
            VirtualPath? resolvedPath = null,
            bool resolved = false,
            VirtualSymbolicLink? resolvedLink = null)
        {
            Node = node;
            TraversalPath = traversalPath;
            ParentDirectory = parentNode;
            Depth = depth;
            Index = index;
            ResolvedPath = resolvedPath;
            Resolved = resolved;
            ResolvedLink = resolvedLink;
        }

        /// <summary>
        /// Returns a string representation of this instance's information.
        /// </summary>
        /// <returns>A string representing this instance's information.</returns>
        public override string ToString()
        {
            List<string> parts =
            [
                $"Node: {Node}",
                $"TraversalPath: {TraversalPath}",
                $"ParentDirectory: {ParentDirectory}",
                $"Depth: {Depth}",
                $"Index: {Index}"
            ];

            if (ResolvedPath != null)
            {
                parts.Add($"ResolvedPath: {ResolvedPath}");
            }

            parts.Add($"Resolved: {Resolved}");
            parts.Add($"ResolvedLink: {ResolvedLink}");

            return string.Join(", ", parts);
        }
    }
}
