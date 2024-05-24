using System.Diagnostics;

namespace AkiraNet.VirtualStorageLibrary
{
    public class VirtualNodeContext
    {
        public VirtualNode? Node { [DebuggerStepThrough] get; }

        public VirtualPath TraversalPath { [DebuggerStepThrough] get; }

        public VirtualDirectory? ParentDirectory { [DebuggerStepThrough] get; }

        public int Depth { [DebuggerStepThrough] get; }

        public int Index { [DebuggerStepThrough] get; }

        public VirtualPath? ResolvedPath { [DebuggerStepThrough] get; }

        public bool Resolved { [DebuggerStepThrough] get; }

        [DebuggerStepThrough]
        public VirtualNodeContext(VirtualNode? node, VirtualPath traversalPath, VirtualDirectory? parentNode = null, int depth = 0, int index = 0, VirtualPath? resolvedPath = null, bool resolved = false)
        {
            Node = node;
            TraversalPath = traversalPath;
            ParentDirectory = parentNode;
            Depth = depth;
            Index = index;
            ResolvedPath = resolvedPath;
            Resolved = resolved;
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            List<string> parts = new()
            {
                $"NodeName: {Node?.Name}",
                $"TraversalPath: {TraversalPath}",
                $"ParentDirectory: {ParentDirectory?.Name}",
                $"Depth: {Depth}",
                $"Index: {Index}"
            };

            if (ResolvedPath != null)
            {
                parts.Add($"ResolvedPath: {ResolvedPath}");
            }

            parts.Add($"Resolved: {Resolved}");

            return string.Join(", ", parts);
        }
    }
}
