namespace AkiraNetwork.VirtualStorageLibrary
{
    [DebuggerStepThrough]
    public class VirtualNodeContext(
        VirtualNode? node,
        VirtualPath traversalPath,
        VirtualDirectory? parentNode = null,
        int depth = 0,
        int index = 0,
        VirtualPath? resolvedPath = null,
        bool resolved = false,
        VirtualSymbolicLink? resolvedLink = null)
    {
        public VirtualNode? Node { get; set; } = node;

        public VirtualPath TraversalPath { get; set; } = traversalPath;

        public VirtualDirectory? ParentDirectory { get; set; } = parentNode;

        public int Depth { get; set; } = depth;

        public int Index { get; set; } = index;

        public VirtualPath? ResolvedPath { get; set; } = resolvedPath;

        public bool Resolved { get; set; } = resolved;

        public VirtualSymbolicLink? ResolvedLink { get; set; } = resolvedLink;

        public override string ToString()
        {
            List<string> parts =
            [
                $"Node: {Node}",
                $"TraversalPath: {TraversalPath}",
                $"ParentDirectory: {ParentDirectory?.Name}",
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
