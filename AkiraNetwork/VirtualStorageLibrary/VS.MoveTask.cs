// This file is part of VirtualStorageLibrary.
//
// Copyright (C) 2024 Akira Shimodate
//
// VirtualStorageLibrary is free software, and it is distributed under the terms of 
// the GNU General Public License (version 3, or at your option, any later version). 
// This license is published by the Free Software Foundation.
//
// VirtualStorageLibrary is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along 
// with VirtualStorageLibrary. If not, see https://www.gnu.org/licenses/.

using AkiraNetwork.VirtualStorageLibrary.Localization;

namespace AkiraNetwork.VirtualStorageLibrary
{
    public partial class VirtualStorage<T>
    {
        /// <summary>
        /// Moves the node at the specified path to a new location.
        /// </summary>
        /// <param name="sourcePath">The source path of the node.</param>
        /// <param name="destinationPath">The destination path for the node.</param>
        /// <param name="overwrite">Whether to overwrite existing nodes.</param>
        /// <param name="resolveLinks">Whether to resolve symbolic links.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the source and destination paths are the same, if attempting to 
        /// move the root directory, or if the destination is a subdirectory of the source.
        /// </exception>
        /// <exception cref="VirtualNodeNotFoundException">
        /// Thrown if the source node is not found.
        /// </exception>
        /// <remarks>
        /// <para>
        /// resolveLinks indicates whether to resolve links when non-terminal nodes 
        /// (from the top node to the parent of the terminal node) in the path are symbolic links.
        /// </para>
        /// </remarks>
        public void MoveNode(VirtualPath sourcePath, VirtualPath destinationPath, bool overwrite = false, bool resolveLinks = true)
        {
            sourcePath = ConvertToAbsolutePath(sourcePath);
            destinationPath = ConvertToAbsolutePath(destinationPath);

            // Resolve the source path if resolveLinks is true
            if (resolveLinks)
            {
                // If resolveLinks is specified, resolve the entire source path
                sourcePath = ResolveLinkTarget(sourcePath);
            }
            else
            {
                // If not resolving links, resolve only the parent of the source path
                sourcePath = ResolveLinkTarget(sourcePath.DirectoryPath) + sourcePath.NodeName;
            }

            // Resolve the destination path
            VirtualPath? path = TryResolveLinkTarget(destinationPath);
            if (path != null)
            {
                // If the entire path is resolved, use it as the destination path
                destinationPath = path;
            }
            else
            {
                // If not resolved, resolve only the directory path
                destinationPath = ResolveLinkTarget(destinationPath.DirectoryPath) + destinationPath.NodeName;
            }

            // Check if the source and destination paths are the same
            if (sourcePath == destinationPath)
            {
                throw new InvalidOperationException(string.Format(Resources.SourceAndDestinationPathSameForMove, sourcePath, destinationPath));
            }

            // Check if the source is the root directory
            if (sourcePath.IsRoot)
            {
                // Throw an exception if the root directory is being moved
                throw new InvalidOperationException(Resources.CannotMoveRootDirectory);
            }

            // Check if the destination is a subdirectory of the source
            if (destinationPath.IsSubdirectory(sourcePath))
            {
                throw new InvalidOperationException(string.Format(Resources.DestinationIsSubdirectoryOfSource, sourcePath, destinationPath));
            }

            // Check if the source node exists
            if (!NodeExists(sourcePath))
            {
                // Throw an exception if the source node does not exist
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, sourcePath.NodeName));
            }

            // Check if the source path is the current directory or a parent directory, and throw an exception if it is.
            if (sourcePath == CurrentPath || CurrentPath.IsSubdirectory(sourcePath))
            {
                throw new InvalidOperationException(Resources.CannotMoveCurrentOrParentDirectory);
            }

            // Move the node
            if (DirectoryExists(sourcePath))
            {
                MoveDirectoryInternal(sourcePath, destinationPath);
            }
            else
            {
                MoveItemOrLinkInternal(sourcePath, destinationPath, overwrite);
            }
        }

        /// <summary>
        /// Internally moves the directory from the source path to the destination path.
        /// </summary>
        /// <param name="sourcePath">The source path of the directory.</param>
        /// <param name="destinationPath">The destination path for the directory.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a node with the same name exists at the destination.
        /// </exception>
        private void MoveDirectoryInternal(VirtualPath sourcePath, VirtualPath destinationPath)
        {
            VirtualDirectory sourceDirectory = GetDirectory(sourcePath);
            VirtualDirectory sourceParentDirectory = GetDirectory(sourcePath.GetParentPath());

            VirtualDirectory destinationParentDirectory;
            VirtualNodeName destinationNodeName;

            VirtualPath destinationBasePath;

            if (DirectoryExists(destinationPath))
            {
                destinationParentDirectory = GetDirectory(destinationPath);
                destinationNodeName = sourceDirectory.Name;
                destinationBasePath = destinationPath + sourceDirectory.Name;
            }
            else if (!NodeExists(destinationPath))
            {
                VirtualPath destinationParentPath = destinationPath.GetParentPath();

                destinationParentDirectory = GetDirectory(destinationParentPath);
                destinationNodeName = destinationPath.NodeName;
                destinationBasePath = destinationPath;
            }
            else
            {
                throw new InvalidOperationException(string.Format(Resources.DestinationNodeIsItemOrSymbolicLink, destinationPath));
            }

            if (destinationParentDirectory.NodeExists(destinationNodeName))
            {
                throw new InvalidOperationException(string.Format(Resources.NodeWithSameNameAtDestination, destinationPath, destinationNodeName));
            }

            // Get the list of paths before moving
            var sourceNodes = WalkPathTree(sourcePath, VirtualNodeTypeFilter.All, true, true)
                                .Select(context => context.TraversalPath)
                                .ToList();

            // Create a list of pairs of source and destination paths after moving
            var nodePairs = sourceNodes
                            .Select(path =>
                                (Source: sourcePath + path,
                                 Destination: destinationBasePath + path))
                            .ToList();

            // Update link dictionary (if symbolic links are present)
            foreach (var (sourceNodePath, destinationNodePath) in nodePairs)
            {
                if (GetNode(sourceNodePath) is VirtualSymbolicLink)
                {
                    UpdateLinkNameInDictionary(sourceNodePath, destinationNodePath);
                }

                // Update link dictionary (target path changes)
                UpdateLinksToTarget(sourceNodePath, destinationNodePath);
            }

            // Move the node
            sourceDirectory.Name = destinationNodeName;
            destinationParentDirectory.Add(sourceDirectory);
            sourceParentDirectory.Remove(sourceDirectory);

            // Update all target node types in the dictionary
            UpdateAllTargetNodeTypesInDictionary();
        }

        /// <summary>
        /// Internally moves the item or link from the source path to the destination path.
        /// </summary>
        /// <param name="sourcePath">The source path of the item or link.</param>
        /// <param name="destinationPath">The destination path for the item or link.</param>
        /// <param name="overwrite">Whether to overwrite existing nodes.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a node with the same name exists at the destination.
        /// </exception>
        private void MoveItemOrLinkInternal(VirtualPath sourcePath, VirtualPath destinationPath, bool overwrite)
        {
            VirtualNode sourceNode = GetNode(sourcePath);
            VirtualDirectory sourceParentDirectory = GetDirectory(sourcePath.GetParentPath());

            VirtualDirectory destinationParentDirectory;
            VirtualNodeName destinationNodeName;
            VirtualNode? destinationNode;

            if (DirectoryExists(destinationPath))
            {
                destinationParentDirectory = GetDirectory(destinationPath);
                destinationNodeName = sourceNode.Name;
                destinationPath += sourceNode.Name;
                destinationNode = TryGetNode(destinationPath);
            }
            else if (!NodeExists(destinationPath))
            {
                VirtualPath destinationParentPath = destinationPath.GetParentPath();

                destinationParentDirectory = GetDirectory(destinationParentPath);
                destinationNodeName = destinationPath.NodeName;
                destinationNode = TryGetNode(destinationPath);
            }
            else
            {
                destinationParentDirectory = GetDirectory(destinationPath.GetParentPath());
                destinationNodeName = destinationPath.NodeName;
                destinationNode = TryGetNode(destinationPath);
            }

            if (destinationNode != null)
            {
                if (overwrite)
                {
                    destinationParentDirectory.Remove(destinationNode);
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.NodeWithSameNameAtDestination, destinationPath, destinationNodeName));
                }
            }

            // Update link dictionary (if symbolic links are present)
            if (sourceNode is VirtualSymbolicLink)
            {
                UpdateLinkNameInDictionary(sourcePath, destinationPath);
            }

            // Update link dictionary (target path changes)
            UpdateLinksToTarget(sourcePath, destinationPath);

            // Move the node
            sourceParentDirectory.Remove(sourceNode);
            sourceNode.Name = destinationNodeName;
            destinationParentDirectory.Add(sourceNode);

            // Update all target node types in the dictionary
            UpdateAllTargetNodeTypesInDictionary();
        }
    }
}
