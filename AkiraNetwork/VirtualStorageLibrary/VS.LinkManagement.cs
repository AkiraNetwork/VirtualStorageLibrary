using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        /// <summary>
        /// A dictionary to hold link mappings.
        /// </summary>
        private readonly Dictionary<VirtualPath, HashSet<VirtualPath>> _linkDictionary;

        /// <summary>
        /// Gets the link dictionary.
        /// </summary>
        /// <value>A dictionary representing the link mappings.</value>
        public Dictionary<VirtualPath, HashSet<VirtualPath>> LinkDictionary => _linkDictionary;

        /// <summary>
        /// Adds a new link to the link dictionary.
        /// </summary>
        /// <param name="targetPath">The target path of the link.</param>
        /// <param name="linkPath">The path of the link.</param>
        /// <exception cref="ArgumentException">Thrown if the target path is not an absolute path.</exception>
        public void AddLinkToDictionary(VirtualPath targetPath, VirtualPath linkPath)
        {
            if (!targetPath.IsAbsolute)
            {
                throw new ArgumentException(string.Format(Resources.TargetPathIsNotAbsolutePath, targetPath), nameof(targetPath));
            }

            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            // Retrieve the link set from the dictionary, or create a new one
            HashSet<VirtualPath>? linkPathSet = GetLinksFromDictionary(targetPath);
            _linkDictionary[targetPath] = linkPathSet;

            // Add the new link path to the set
            linkPathSet.Add(linkPath);
        }

        /// <summary>
        /// Updates the target node types in the link dictionary.
        /// </summary>
        /// <param name="targetPath">The target path.</param>
        public void UpdateTargetNodeTypesInDictionary(VirtualPath targetPath)
        {
            HashSet<VirtualPath> linkPathSet = GetLinksFromDictionary(targetPath);
            if (linkPathSet.Count > 0)
            {
                VirtualNodeType targetType = GetNodeType(targetPath, true);
                SetLinkTargetNodeType(linkPathSet, targetType);
            }
        }

        /// <summary>
        /// Updates all target node types in the link dictionary.
        /// </summary>
        public void UpdateAllTargetNodeTypesInDictionary()
        {
            foreach (var targetPath in _linkDictionary.Keys)
            {
                UpdateTargetNodeTypesInDictionary(targetPath);
            }
        }

        /// <summary>
        /// Removes a link from the link dictionary.
        /// </summary>
        /// <param name="targetPath">The target path.</param>
        /// <param name="linkPath">The path of the link.</param>
        public void RemoveLinkFromDictionary(VirtualPath targetPath, VirtualPath linkPath)
        {
            // Retrieve the link set from the dictionary
            var links = GetLinksFromDictionary(targetPath);

            // Remove the specified link from the set
            if (links.Remove(linkPath))
            {
                // If the link set is empty, remove the entry from the dictionary
                if (links.Count == 0)
                {
                    _linkDictionary.Remove(targetPath);
                }
            }
        }

        /// <summary>
        /// Retrieves all link paths associated with the specified target path from the link dictionary.
        /// </summary>
        /// <param name="targetPath">The target path.</param>
        /// <returns>A set of link paths.</returns>
        public HashSet<VirtualPath> GetLinksFromDictionary(VirtualPath targetPath)
        {
            if (_linkDictionary.TryGetValue(targetPath, out HashSet<VirtualPath>? linkPathSet))
            {
                return linkPathSet;
            }
            return [];
        }

        /// <summary>
        /// Sets the target node type for all symbolic links in the specified list of link paths.
        /// </summary>
        /// <param name="linkPathSet">A set of link paths.</param>
        /// <param name="nodeType">The target node type.</param>
        public void SetLinkTargetNodeType(HashSet<VirtualPath> linkPathSet, VirtualNodeType nodeType)
        {
            foreach (var linkPath in linkPathSet)
            {
                VirtualSymbolicLink? link = TryGetSymbolicLink(linkPath);
                if (link != null)
                {
                    link.TargetNodeType = nodeType;
                }
            }
        }

        /// <summary>
        /// Updates the target path of a specific symbolic link to a new target path.
        /// </summary>
        /// <param name="linkPath">The path of the link.</param>
        /// <param name="newTargetPath">The new target path.</param>
        public void UpdateLinkInDictionary(VirtualPath linkPath, VirtualPath newTargetPath)
        {
            if (GetNode(linkPath) is VirtualSymbolicLink link)
            {
                if (link.TargetPath != null)
                {
                    // Retrieve the old target path
                    VirtualPath oldTargetPath = link.TargetPath;

                    // Remove the link from the old target path
                    RemoveLinkFromDictionary(oldTargetPath, linkPath);

                    // Set the new target path
                    link.TargetPath = newTargetPath;

                    // Add the link to the new target path
                    AddLinkToDictionary(newTargetPath, linkPath);
                }
            }
        }

        /// <summary>
        /// Updates the target path of links with a specific target path to a new target path.
        /// </summary>
        /// <param name="oldTargetPath">The old target path.</param>
        /// <param name="newTargetPath">The new target path.</param>
        public void UpdateLinksToTarget(VirtualPath oldTargetPath, VirtualPath newTargetPath)
        {
            var linkPathSet = GetLinksFromDictionary(oldTargetPath);

            foreach (var linkPath in linkPathSet)
            {
                // Update the target path of the link
                UpdateLinkInDictionary(linkPath, newTargetPath);
            }

            // Remove the links from the old target path
            _linkDictionary.Remove(oldTargetPath);
        }

        /// <summary>
        /// Updates the link name in the link dictionary.
        /// </summary>
        /// <param name="oldLinkPath">The old link path.</param>
        /// <param name="newLinkPath">The new link path.</param>
        private void UpdateLinkNameInDictionary(VirtualPath oldLinkPath, VirtualPath newLinkPath)
        {
            foreach (var entry in _linkDictionary)
            {
                var linkPaths = entry.Value;

                // If the old link path is successfully removed, add the new link path
                if (linkPaths.Remove(oldLinkPath))
                {
                    linkPaths.Add(newLinkPath);
                }
            }
        }

        /// <summary>
        /// Removes the link with the specified link path from the link dictionary.
        /// </summary>
        /// <param name="linkPath">The link path.</param>
        /// <exception cref="ArgumentException">Thrown if the link path is not an absolute path.</exception>
        public void RemoveLinkByLinkPath(VirtualPath linkPath)
        {
            if (!linkPath.IsAbsolute)
            {
                throw new ArgumentException(string.Format(Resources.LinkPathIsNotAbsolutePath, linkPath), nameof(linkPath));
            }

            List<VirtualPath> targetPathsToRemoveList = [];

            foreach (KeyValuePair<VirtualPath, HashSet<VirtualPath>> entry in _linkDictionary)
            {
                VirtualPath targetPath = entry.Key;
                HashSet<VirtualPath> linkPathSet = entry.Value;

                if (linkPathSet.Remove(linkPath))
                {
                    if (linkPathSet.Count == 0)
                    {
                        targetPathsToRemoveList.Add(targetPath);
                    }
                }
            }

            foreach (VirtualPath targetPath in targetPathsToRemoveList)
            {
                _linkDictionary.Remove(targetPath);
            }
        }
    }
}
