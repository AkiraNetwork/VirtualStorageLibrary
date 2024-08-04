using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        /// <summary>
        /// Removes the node at the specified path.
        /// </summary>
        /// <param name="nodePath">The path of the node to remove.</param>
        /// <param name="recursive">Whether to remove recursively.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <param name="resolveLinks">Whether to resolve link targets.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to remove the root node or a non-empty directory.
        /// </exception>
        /// <remarks>
        /// <para>
        /// If followLinks is true and the terminal node in the path is a 
        /// symbolic link, the path tree after resolving the path will be 
        /// removed, and the symbolic link will also be removed. If false, 
        /// only the symbolic link itself is removed.
        /// </para>
        /// <para>
        /// resolveLinks indicates whether to resolve links when 
        /// non-terminal nodes (from the top node to the parent of the 
        /// terminal node) in the path are symbolic links.
        /// </para>
        /// </remarks>
        public void RemoveNode(VirtualPath nodePath, bool recursive = false, bool followLinks = false, bool resolveLinks = true)
        {
            nodePath = ConvertToAbsolutePath(nodePath).NormalizePath();

            if (nodePath.IsRoot)
            {
                throw new InvalidOperationException(Resources.CannotRemoveRoot);
            }

            VirtualPath resolvedNodePath = nodePath;
            if (resolveLinks)
            {
                resolvedNodePath = ResolveLinkTarget(nodePath);
            }

            IEnumerable<VirtualNodeContext> contexts = WalkPathTree(nodePath, VirtualNodeTypeFilter.All, recursive, followLinks, resolveLinks);

            if (recursive)
            {
                // Convert the node contexts to a list and process in reverse order
                IEnumerable<VirtualNodeContext> reversedContexts = contexts.Reverse();

                foreach (VirtualNodeContext context in reversedContexts)
                {
                    VirtualDirectory? parentDir = context.ParentDirectory;

                    // Check if it's a symbolic link
                    if (context.ResolvedLink != null)
                    {
                        // If it's a symbolic link, remove the link node
                        VirtualSymbolicLink link = context.ResolvedLink;
                        VirtualPath linkPath = nodePath + context.TraversalPath;
                        VirtualPath linkParentPath = linkPath.DirectoryPath;
                        parentDir = GetDirectory(linkParentPath, true);

                        // Reset storage reference flag for the link to be removed
                        link.IsReferencedInStorage = false;

                        // Remove node from dictionary
                        parentDir?.Remove(link);

                        // Update all target node types in the dictionary
                        UpdateAllTargetNodeTypesInDictionary();

                        // Remove link from dictionary
                        if (link.TargetPath != null)
                        {
                            VirtualPath? resolvedTargetPath = TryResolveLinkTarget(link.TargetPath);
                            if (resolvedTargetPath != null)
                            {
                                RemoveLinkFromDictionary(resolvedTargetPath, linkPath);
                            }
                        }

                        // Remove the target of the link
                        VirtualPath targetPath = ConvertToAbsolutePath(link.TargetPath).NormalizePath();
                        RemoveNode(targetPath, recursive, followLinks);
                    }
                    else
                    {
                        // If it's a regular node, remove it

                        VirtualNode node = context.Node!;
                        if (node is IDisposable disposableNode)
                        {
                            // If the node implements IDisposable, call Dispose method
                            disposableNode.Dispose();
                        }

                        // Reset storage reference flag for the node to be removed
                        node.IsReferencedInStorage = false;

                        // Remove node from dictionary
                        parentDir?.Remove(node);

                        // Update all target node types in the dictionary
                        UpdateAllTargetNodeTypesInDictionary();
                    }
                }
            }
            else
            {
                VirtualNodeContext context = contexts.First();
                VirtualNode node = context.Node!;

                if (node is VirtualDirectory directory)
                {
                    if (directory.Count > 0)
                    {
                        throw new InvalidOperationException(string.Format(Resources.CannotRemoveNonEmptyDirectory, node.Name));
                    }
                }
                VirtualDirectory? parentDir = context.ParentDirectory;

                if (node is IDisposable disposableNode)
                {
                    // If the node implements IDisposable, call Dispose method
                    disposableNode.Dispose();
                }

                // Reset storage reference flag for the node to be removed
                node.IsReferencedInStorage = false;

                // Remove node from dictionary
                parentDir?.Remove(node);

                // Update all target node types in the dictionary
                UpdateAllTargetNodeTypesInDictionary();

                // If the node is a link, remove the link from the dictionary
                if (node is VirtualSymbolicLink link)
                {
                    if (link.TargetPath != null)
                    {
                        VirtualPath? resolvedTargetPath = TryResolveLinkTarget(link.TargetPath);
                        if (resolvedTargetPath != null)
                        {
                            RemoveLinkFromDictionary(resolvedTargetPath, resolvedNodePath);
                        }
                        else
                        {
                            RemoveLinkByLinkPath(resolvedNodePath);
                        }
                    }
                }
            }
        }
    }
}
