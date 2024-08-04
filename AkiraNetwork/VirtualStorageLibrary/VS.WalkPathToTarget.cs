using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        /// <summary>
        /// Traverses the path to the target node specified by the virtual path.
        /// </summary>
        /// <param name="targetPath">The target virtual path</param>
        /// <param name="notifyNode">A delegate to notify information about the node when each node is reached</param>
        /// <param name="actionNode">A delegate to perform actions when each node is reached</param>
        /// <param name="followLinks">A flag indicating whether to follow symbolic links</param>
        /// <param name="exceptionEnabled">A flag indicating whether to enable exceptions. If true, exceptions are thrown; if false, the Node in <see cref="VirtualNodeContext"/> will be returned as null.</param>
        /// <returns>An instance of <see cref="VirtualNodeContext"/> containing information about the target node</returns>
        public VirtualNodeContext WalkPathToTarget(
            VirtualPath targetPath,
            NotifyNodeDelegate? notifyNode = null,
            ActionNodeDelegate? actionNode = null,
            bool followLinks = true,
            bool exceptionEnabled = true)
        {
            targetPath = ConvertToAbsolutePath(targetPath).NormalizePath();

            WalkPathToTargetParameters p = new(
                targetPath,
                0,
                VirtualPath.Root,
                null,
                _root,
                notifyNode,
                actionNode,
                followLinks,
                exceptionEnabled,
                false);

            // Clear cycle detection
            CycleDetectorForTarget.Clear();

            VirtualNodeContext? nodeContext = WalkPathToTargetInternal(p);

            return nodeContext;
        }

        /// <summary>
        /// Internal method for traversing to the target node.
        /// </summary>
        /// <param name="p">A structure containing parameters for the WalkPathToTarget method</param>
        /// <returns>An instance of <see cref="VirtualNodeContext"/> containing information about the target node</returns>
        private VirtualNodeContext WalkPathToTargetInternal(WalkPathToTargetParameters p)
        {
            // If the target is the root directory, notify the root node and finish
            if (p.TargetPath.IsRoot)
            {
                p.NotifyNode?.Invoke(VirtualPath.Root, _root);
                return new VirtualNodeContext(_root, VirtualPath.Root, null, -1, -1, VirtualPath.Root, p.Resolved);
            }

            VirtualNodeName traversalNodeName = p.TargetPath.PartsList[p.TraversalIndex];

            while (!p.TraversalDirectory.NodeExists(traversalNodeName))
            {
                VirtualPath traversalPath = p.TraversalPath + traversalNodeName;

                if (p.ActionNode != null)
                {
                    if (p.ActionNode(p.TraversalDirectory, traversalNodeName, traversalPath))
                    {
                        continue;
                    }
                }

                // If exceptions are enabled, throw an exception
                if (p.ExceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, traversalPath));
                }

                return new VirtualNodeContext(null, traversalPath, p.TraversalDirectory, -1, -1, traversalPath, p.Resolved);
            }

            VirtualNodeContext nodeContext;

            // Retrieve the traversal node
            VirtualNode node = p.TraversalDirectory[traversalNodeName];

            // Update the traversal path
            p.TraversalPath += traversalNodeName;

            // Move to the next node
            p.TraversalIndex++;

            if (node is VirtualDirectory directory)
            {
                // If the node is a directory

                // Check if the last node has been reached
                if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                {
                    // Notify the terminal node
                    p.NotifyNode?.Invoke(p.TraversalPath, directory);
                    p.ResolvedPath ??= p.TraversalPath;
                    return new VirtualNodeContext(directory, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved);
                }

                // Notify the intermediate node
                p.NotifyNode?.Invoke(p.TraversalPath, directory);

                // Retrieve the traversal directory
                p.TraversalDirectory = directory;

                // Recursively traverse
                nodeContext = WalkPathToTargetInternal(p);

                return nodeContext;
            }
            else if (node is VirtualItem<T> item)
            {
                // If the node is an item

                // Notify the terminal node
                p.NotifyNode?.Invoke(p.TraversalPath, item);

                // Check if the last node has been reached
                if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                {
                    p.ResolvedPath ??= p.TraversalPath;
                    return new VirtualNodeContext(item, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved);
                }

                p.ResolvedPath ??= p.TraversalPath;

                // If exceptions are enabled, throw an exception
                if (p.ExceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException(string.Format(Resources.CannotReachBecauseNodeItem, p.TargetPath, p.TraversalPath));
                }

                return new VirtualNodeContext(null, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved);
            }
            else
            {
                // If the node is a symbolic link

                VirtualSymbolicLink link = (VirtualSymbolicLink)node;
                if (!p.FollowLinks || link.TargetPath == null)
                {
                    // Notify the symbolic link
                    p.NotifyNode?.Invoke(p.TraversalPath, link);

                    // Check if the last node has been reached
                    if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                    {
                        p.ResolvedPath ??= p.TargetPath;
                        return new VirtualNodeContext(link, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved, link);
                    }

                    p.ResolvedPath ??= p.TraversalPath;

                    // If exceptions are enabled, throw an exception
                    if (p.ExceptionEnabled)
                    {
                        throw new VirtualNodeNotFoundException(string.Format(Resources.CannotReachBecauseNodeSymbolicLink, p.TargetPath, p.TraversalPath));
                    }

                    return new VirtualNodeContext(null, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved, link);
                }

                // Set to true if a symbolic link was resolved during path traversal
                p.Resolved = true;

                VirtualPath? linkTargetPath = link.TargetPath;
                VirtualPath parentTraversalPath = p.TraversalPath.DirectoryPath;

                // Convert the symbolic link's target path to an absolute path
                linkTargetPath = ConvertToAbsolutePath(linkTargetPath, parentTraversalPath);

                // Normalize the symbolic link's target path
                linkTargetPath = linkTargetPath.NormalizePath();

                // Recursively traverse the symbolic link's target path
                WalkPathToTargetParameters p2 = new(
                    linkTargetPath,
                    0,
                    VirtualPath.Root,
                    null,
                    _root,
                    null,
                    null,
                    true,
                    p.ExceptionEnabled,
                    p.Resolved);

                // Cycle detection check
                if (CycleDetectorForTarget.IsNodeInCycle(link))
                {
                    throw new InvalidOperationException(string.Format(Resources.CircularReferenceDetected, p.TraversalPath, link));
                }

                nodeContext = WalkPathToTargetInternal(p2);

                VirtualNode? resolvedNode = nodeContext.Node;

                // Add the unexplored path to the resolved path
                p.ResolvedPath = nodeContext.ResolvedPath!.CombineFromIndex(p.TargetPath, p.TraversalIndex);

                // Notify the symbolic link
                p.NotifyNode?.Invoke(p.TraversalPath, resolvedNode);

                if (resolvedNode != null && resolvedNode is VirtualDirectory linkDirectory)
                {
                    // Check if the last node has been reached
                    if (p.TargetPath.PartsList.Count <= p.TraversalIndex)
                    {
                        // Notify the terminal node
                        nodeContext.TraversalPath = p.TraversalPath;
                        nodeContext.ResolvedPath = p.ResolvedPath;

                        return nodeContext;
                    }

                    // Retrieve the traversal directory
                    p.TraversalDirectory = linkDirectory;

                    // Recursively traverse
                    nodeContext = WalkPathToTargetInternal(p);

                    return nodeContext;
                }

                return new VirtualNodeContext(resolvedNode, p.TraversalPath, p.TraversalDirectory, -1, -1, p.ResolvedPath, p.Resolved, link);
            }
        }

        /// <summary>
        /// A structure that holds parameters for the WalkPathToTarget method.
        /// </summary>
        private struct WalkPathToTargetParameters
        {
            /// <summary>
            /// Gets or sets the target path.
            /// </summary>
            /// <value>The target path</value>
            public VirtualPath TargetPath { get; set; }

            /// <summary>
            /// Gets or sets the current traversal index.
            /// </summary>
            /// <value>The current traversal index</value>
            public int TraversalIndex { get; set; }

            /// <summary>
            /// Gets or sets the current traversal path.
            /// </summary>
            /// <value>The current traversal path</value>
            public VirtualPath TraversalPath { get; set; }

            /// <summary>
            /// Gets or sets the resolved path.
            /// </summary>
            /// <value>The resolved path</value>
            public VirtualPath? ResolvedPath { get; set; }

            /// <summary>
            /// Gets or sets the current traversal directory.
            /// </summary>
            /// <value>The current traversal directory</value>
            public VirtualDirectory TraversalDirectory { get; set; }

            /// <summary>
            /// Gets or sets the delegate for notifying information about the node when each node is reached.
            /// </summary>
            /// <value>The notify delegate</value>
            public NotifyNodeDelegate? NotifyNode { get; set; }

            /// <summary>
            /// Gets or sets the delegate for performing actions when each node is reached.
            /// </summary>
            /// <value>The action delegate</value>
            public ActionNodeDelegate? ActionNode { get; set; }

            /// <summary>
            /// Gets or sets the flag indicating whether to follow symbolic links.
            /// </summary>
            /// <value>The flag indicating whether to follow symbolic links</value>
            public bool FollowLinks { get; set; }

            /// <summary>
            /// Gets or sets the flag indicating whether to enable exceptions.
            /// If true, exceptions are thrown; if false, the Node in <see cref="VirtualNodeContext"/> will be returned as null.
            /// </summary>
            /// <value>The flag indicating whether to enable exceptions</value>
            public bool ExceptionEnabled { get; set; }

            /// <summary>
            /// Gets or sets the flag indicating whether a symbolic link was resolved during path traversal.
            /// </summary>
            /// <value>The flag indicating whether a symbolic link was resolved</value>
            public bool Resolved { get; set; }

            /// <summary>
            /// Initializes a new instance of the WalkPathToTargetParameters structure.
            /// </summary>
            /// <param name="targetPath">The target path</param>
            /// <param name="traversalIndex">The current traversal index</param>
            /// <param name="traversalPath">The current traversal path</param>
            /// <param name="resolvedPath">The resolved path</param>
            /// <param name="traversalDirectory">The current traversal directory</param>
            /// <param name="notifyNode">The notify delegate</param>
            /// <param name="actionNode">The action delegate</param>
            /// <param name="followLinks">The flag indicating whether to follow symbolic links</param>
            /// <param name="exceptionEnabled">The flag indicating whether to enable exceptions</param>
            /// <param name="resolved">The flag indicating whether a symbolic link was resolved</param>
            public WalkPathToTargetParameters(
                VirtualPath targetPath,
                int traversalIndex,
                VirtualPath traversalPath,
                VirtualPath? resolvedPath,
                VirtualDirectory traversalDirectory,
                NotifyNodeDelegate? notifyNode,
                ActionNodeDelegate? actionNode,
                bool followLinks,
                bool exceptionEnabled,
                bool resolved)
            {
                TargetPath = targetPath;
                TraversalIndex = traversalIndex;
                TraversalPath = traversalPath;
                ResolvedPath = resolvedPath;
                TraversalDirectory = traversalDirectory;
                NotifyNode = notifyNode;
                ActionNode = actionNode;
                FollowLinks = followLinks;
                ExceptionEnabled = exceptionEnabled;
                Resolved = resolved;
            }
        }
    }
}
