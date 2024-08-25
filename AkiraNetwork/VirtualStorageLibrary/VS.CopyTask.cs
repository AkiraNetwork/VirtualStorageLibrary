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
        /// Checks preconditions before copying.
        /// </summary>
        /// <param name="sourcePath">The source path of the copy.</param>
        /// <param name="destinationPath">The destination path of the copy.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <param name="recursive">Whether to copy recursively.</param>
        /// <exception cref="VirtualNodeNotFoundException">
        /// Thrown if the source node is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the source and destination paths are the same or if a recursive copy
        /// would result in a circular reference.
        /// </exception>
        private void CheckCopyPreconditions(VirtualPath sourcePath, VirtualPath destinationPath, bool followLinks, bool recursive)
        {
            VirtualPath absoluteSourcePath = ConvertToAbsolutePath(sourcePath).NormalizePath();
            VirtualPath absoluteDestinationPath = ConvertToAbsolutePath(destinationPath).NormalizePath();

            // Check if the source node exists
            if (!NodeExists(absoluteSourcePath, true))
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, absoluteSourcePath.NodeName));
            }

            // Throw an exception if the source and destination paths are the same
            if (absoluteSourcePath == absoluteDestinationPath)
            {
                throw new InvalidOperationException(string.Format(Resources.SourceAndDestinationPathSameForCopy, absoluteSourcePath, absoluteDestinationPath));
            }

            // Check for circular references
            if (recursive)
            {
                if (VirtualPath.ArePathsSubdirectories(absoluteSourcePath, absoluteDestinationPath))
                {
                    throw new InvalidOperationException(string.Format(Resources.RecursiveSubdirectoryConflict, absoluteSourcePath, absoluteDestinationPath));
                }
            }

            // Traverse the source tree (check for source existence and throw an exception if necessary. Discard the return value)
            IEnumerable<VirtualNodeContext> _ = WalkPathTree(absoluteSourcePath, VirtualNodeTypeFilter.All, recursive, followLinks).ToList();
        }

        /// <summary>
        /// Copies the node at the specified path.
        /// </summary>
        /// <param name="sourcePath">The source path of the copy.</param>
        /// <param name="destinationPath">The destination path of the copy.</param>
        /// <param name="overwrite">Whether to overwrite existing nodes.</param>
        /// <param name="recursive">Whether to copy recursively.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <param name="destinationContextList">List of destination contexts.</param>
        /// <exception cref="VirtualNodeNotFoundException">
        /// Thrown if the source node is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the source and destination paths are the same or if a recursive copy
        /// would result in a circular reference.
        /// </exception>
        public void CopyNode(
            VirtualPath sourcePath,
            VirtualPath destinationPath,
            bool overwrite = false,
            bool recursive = false,
            bool followLinks = false,
            List<VirtualNodeContext>? destinationContextList = null)
        {
            CheckCopyPreconditions(sourcePath, destinationPath, followLinks, recursive);

            sourcePath = ConvertToAbsolutePath(sourcePath).NormalizePath();
            destinationPath = ConvertToAbsolutePath(destinationPath).NormalizePath();

            // Retrieve the source tree
            IEnumerable<VirtualNodeContext> sourceContexts = WalkPathTree(sourcePath, VirtualNodeTypeFilter.All, recursive, followLinks);

            VirtualNode sourceNode = sourceContexts.First().Node!;

            CopySingleInternal(sourcePath, sourceNode, destinationPath, null, overwrite, followLinks, destinationContextList);

            if (recursive && sourceNode is VirtualDirectory sourceDirectory)
            {
                foreach (var sourceContext in sourceContexts.Skip(1))
                {
                    VirtualPath sourceSubPath = sourcePath + sourceContext.TraversalPath;
                    VirtualPath destinationSubPath = destinationPath + sourceDirectory.Name + sourceSubPath.GetRelativePath(sourcePath);
                    VirtualNode sourceSubNode = sourceContext.Node!;

                    CopySingleInternal(sourceSubPath, sourceSubNode, destinationSubPath, null, overwrite, followLinks, destinationContextList);
                }
            }

            return;
        }

        /// <summary>
        /// Copies a single node internally.
        /// </summary>
        /// <param name="sourcePath">The source path of the node.</param>
        /// <param name="sourceNode">The source node.</param>
        /// <param name="destinationPath">The destination path of the node.</param>
        /// <param name="linkOriginalPath">The original path of the link.</param>
        /// <param name="overwrite">Whether to overwrite existing nodes.</param>
        /// <param name="followLinks">Whether to follow symbolic links.</param>
        /// <param name="destinationContextList">List of destination contexts.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a node with the same name exists at the destination.
        /// </exception>
        private void CopySingleInternal(
            VirtualPath sourcePath,
            VirtualNode sourceNode,
            VirtualPath destinationPath,
            VirtualPath? linkOriginalPath,
            bool overwrite,
            bool followLinks,
            List<VirtualNodeContext>? destinationContextList)
        {
            VirtualNodeName? newNodeName;
            VirtualDirectory destinationDirectory;
            VirtualNode? destinationNode;

            if (destinationPath.IsRoot)
            {
                destinationDirectory = _root;
                destinationNode = _root;
            }
            else
            {
                destinationDirectory = GetDirectory(destinationPath.DirectoryPath, true);
                destinationNode = destinationDirectory.Get(destinationPath.NodeName, false);
            }

            bool overwriteDirectory = false;

            // If the destination node exists
            switch (destinationNode)
            {
                case VirtualDirectory directory:
                    destinationDirectory = directory;
                    newNodeName = sourceNode.Name;
                    VirtualNode? node = destinationDirectory.Get(newNodeName, false);
                    if (node != null)
                    {
                        if (overwrite && (node is VirtualDirectory existingDirectory))
                        {
                            overwriteDirectory = true;

                            // In the case of overwriting a directory with the same name, do not actually overwrite but update the modification date.
                            existingDirectory.UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, newNodeName));
                        }
                    }
                    destinationPath += newNodeName;
                    break;

                case VirtualItem<T> item:
                    if (overwrite)
                    {
                        destinationDirectory.Remove(item);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, item.Name));
                    }
                    newNodeName = destinationPath.NodeName;
                    break;

                case VirtualSymbolicLink link:
                    VirtualPath targetPath = ConvertToAbsolutePath(link.TargetPath).NormalizePath();
                    CopySingleInternal(sourcePath, sourceNode, targetPath, destinationPath, overwrite, followLinks, destinationContextList);
                    return;

                default:
                    newNodeName = destinationPath.NodeName;
                    break;
            }

            // Create the destination node
            VirtualNode newNode = sourceNode.DeepClone();
            newNode.Name = newNodeName;

            // Add the source node to the destination directory (unless overwriting a directory)
            if (!overwriteDirectory)
            {
                destinationDirectory.Add(newNode);
            }

            // Set the link original path if specified
            destinationPath = linkOriginalPath ?? destinationPath;

            // Calculate the depth of the path
            int depth = destinationPath.Depth - 1;

            if (destinationContextList != null)
            {
                // Generate a VirtualNodeContext representing the result of the copy operation and return it
                VirtualNodeContext context = new(
                    newNode,
                    destinationPath,
                    destinationDirectory,
                    depth,
                    0
                );
                destinationContextList.Add(context);
            }

            return;
        }
    }
}
