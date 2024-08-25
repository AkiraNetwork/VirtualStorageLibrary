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
        /// Changes the current working directory.
        /// </summary>
        /// <param name="path">The path of the new working directory</param>
        public void ChangeDirectory(VirtualPath path)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();
            VirtualNodeContext? nodeContext = WalkPathToTarget(path, null, null, true, true);
            CurrentPath = nodeContext.TraversalPath;

            return;
        }

        /// <summary>
        /// Sets a node at the specified path.
        /// </summary>
        /// <param name="destinationPath">The path where the node is set</param>
        /// <param name="node">The <see cref="VirtualNode"/> to set</param>
        public void SetNode(VirtualPath destinationPath, VirtualNode node)
        {
            destinationPath = ConvertToAbsolutePath(destinationPath).NormalizePath();

            switch (node)
            {
                case VirtualDirectory directory:
                    UpdateDirectory(destinationPath, directory);
                    break;

                case VirtualSymbolicLink symbolicLink:
                    UpdateSymbolicLInk(destinationPath, symbolicLink);
                    break;

                case VirtualItem<T> item:
                    UpdateItem(destinationPath, item);
                    break;
            }
        }

        /// <summary>
        /// Updates an item.
        /// </summary>
        /// <param name="itemPath">The path of the item to update</param>
        /// <param name="newItem">The new data for the item</param>
        public void UpdateItem(VirtualPath itemPath, VirtualItem<T> newItem)
        {
            // Convert to absolute path
            itemPath = ConvertToAbsolutePath(itemPath).NormalizePath();

            // Retrieve the existing item
            VirtualItem<T> item = GetItem(itemPath, true);

            // Update the data of the existing item
            item.Update(newItem);
        }

        /// <summary>
        /// Updates a directory.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to update</param>
        /// <param name="newDirectory">The new data for the directory</param>
        public void UpdateDirectory(VirtualPath directoryPath, VirtualDirectory newDirectory)
        {
            // Convert to absolute path
            directoryPath = ConvertToAbsolutePath(directoryPath).NormalizePath();

            // Retrieve the existing directory
            VirtualDirectory directory = GetDirectory(directoryPath, true);

            // Update the nodes of the existing directory
            directory.Update(newDirectory);
        }

        /// <summary>
        /// Updates a symbolic link.
        /// </summary>
        /// <param name="linkPath">The path of the symbolic link to update</param>
        /// <param name="newLink">The new data for the symbolic link</param>
        public void UpdateSymbolicLInk(VirtualPath linkPath, VirtualSymbolicLink newLink)
        {
            // Convert to absolute path
            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            // Retrieve the existing symbolic link
            // For symbolic links, resolving the path is meaningless, so followLinks is set to false
            VirtualSymbolicLink link = GetSymbolicLink(linkPath);

            // Update the nodes of the existing symbolic link
            link.Update(newLink);
        }

        /// <summary>
        /// Adds a node to the specified directory.
        /// </summary>
        /// <param name="nodeDirectoryPath">The path of the directory to add the node to</param>
        /// <param name="node">The <see cref="VirtualNode"/> to add</param>
        /// <param name="overwrite">Indicates whether to overwrite an existing node</param>
        public void AddNode(VirtualPath nodeDirectoryPath, VirtualNode node, bool overwrite = false)
        {
            nodeDirectoryPath = ConvertToAbsolutePath(nodeDirectoryPath).NormalizePath();

            switch (node)
            {
                case VirtualDirectory directory:
                    AddDirectory(nodeDirectoryPath, directory);
                    break;

                case VirtualSymbolicLink symbolicLink:
                    AddSymbolicLink(nodeDirectoryPath, symbolicLink, overwrite);
                    break;

                case VirtualItem<T> item:
                    AddItem(nodeDirectoryPath, item, overwrite);
                    break;
            }
        }

        /// <summary>
        /// Adds a directory.
        /// </summary>
        /// <param name="directoryPath">The path of the directory to add</param>
        /// <param name="directory">The <see cref="VirtualDirectory"/> to add</param>
        /// <param name="createSubdirectories">Indicates whether to create intermediate directories</param>
        public void AddDirectory(VirtualPath directoryPath, VirtualDirectory directory, bool createSubdirectories = false)
        {
            directoryPath = ConvertToAbsolutePath(directoryPath).NormalizePath();

            VirtualNodeContext nodeContext;

            if (createSubdirectories)
            {
                nodeContext = WalkPathToTarget(directoryPath, null, CreateIntermediateDirectory, true, true);
            }
            else
            {
                nodeContext = WalkPathToTarget(directoryPath, null, null, true, true);
            }

            if (nodeContext.Node is VirtualDirectory parentDirectory)
            {
                if (parentDirectory.NodeExists(directory.Name))
                {
                    throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, directory.Name));
                }

                // Add the new directory
                directory = (VirtualDirectory)parentDirectory.Add(directory);

                // Update all target node types in the dictionary
                UpdateAllTargetNodeTypesInDictionary();
            }
            else
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeIsNotVirtualDirectory, nodeContext.Node!.Name));
            }

            return;
        }

        /// <summary>
        /// Adds a directory.
        /// </summary>
        /// <param name="path">The path of the directory to add</param>
        /// <param name="createSubdirectories">Indicates whether to create intermediate directories</param>
        public void AddDirectory(VirtualPath path, bool createSubdirectories = false)
        {
            path = ConvertToAbsolutePath(path).NormalizePath();

            if (path.IsRoot)
            {
                throw new InvalidOperationException(Resources.RootAlreadyExists);
            }

            VirtualPath directoryPath = path.DirectoryPath;
            VirtualNodeName newDirectoryName = path.NodeName;

            VirtualDirectory newDirectory = new(newDirectoryName);
            AddDirectory(directoryPath, newDirectory, createSubdirectories);
        }

        /// <summary>
        /// Creates intermediate directories.
        /// </summary>
        /// <param name="parentDirectory">The parent directory</param>
        /// <param name="nodeName">The name of the new node</param>
        /// <param name="nodePath">The path of the new node</param>
        /// <returns>True if the intermediate directory was successfully created</returns>
        private bool CreateIntermediateDirectory(VirtualDirectory parentDirectory, VirtualNodeName nodeName, VirtualPath nodePath)
        {
            VirtualDirectory newSubdirectory = new(nodeName);

            // Add the intermediate directory
            parentDirectory.Add(newSubdirectory);

            // Update all target node types in the dictionary
            UpdateAllTargetNodeTypesInDictionary();

            return true;
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="itemDirectoryPath">The path of the directory to add the item to</param>
        /// <param name="item">The <see cref="VirtualItem{T}"/> to add</param>
        /// <param name="overwrite">Indicates whether to overwrite an existing node</param>
        public void AddItem(VirtualPath itemDirectoryPath, VirtualItem<T> item, bool overwrite = false)
        {
            // Convert to absolute path
            itemDirectoryPath = ConvertToAbsolutePath(itemDirectoryPath).NormalizePath();

            // Retrieve the directory where the item will be added
            VirtualDirectory directory = GetDirectory(itemDirectoryPath, true);

            // Check if the item already exists
            if (directory.NodeExists(item.Name))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, item.Name));
                }
                else
                {
                    // Throw an exception if the target is not an item
                    if (!ItemExists(itemDirectoryPath + item.Name))
                    {
                        throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualItem, item.Name, typeof(T).Name));
                    }
                    // Remove the existing item
                    directory.Remove(item);
                }
            }

            // Add the new item
            directory.Add(item, overwrite);

            // Update all target node types in the dictionary
            UpdateAllTargetNodeTypesInDictionary();
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="itemPath">The path of the item to add</param>
        /// <param name="data">The data for the item</param>
        /// <param name="overwrite">Indicates whether to overwrite an existing node</param>
        public void AddItem(VirtualPath itemPath, T? data = default, bool overwrite = false)
        {
            // Convert to absolute path
            itemPath = ConvertToAbsolutePath(itemPath).NormalizePath();

            // Separate the directory path and item name
            VirtualPath directoryPath = itemPath.DirectoryPath;
            VirtualNodeName itemName = itemPath.NodeName;

            // Create the item
            VirtualItem<T> item = new(itemName, data);

            // Call the AddItem method
            AddItem(directoryPath, item, overwrite);
        }

        /// <summary>
        /// Adds a symbolic link.
        /// </summary>
        /// <param name="linkDirectoryPath">The path of the directory to add the symbolic link to</param>
        /// <param name="link">The <see cref="VirtualSymbolicLink"/> to add</param>
        /// <param name="overwrite">Indicates whether to overwrite an existing node</param>
        public void AddSymbolicLink(VirtualPath linkDirectoryPath, VirtualSymbolicLink link, bool overwrite = false)
        {
            // Convert to absolute path and normalize linkDirectoryPath
            linkDirectoryPath = ConvertToAbsolutePath(linkDirectoryPath).NormalizePath();

            // Retrieve the directory where the symbolic link will be added
            VirtualDirectory directory = GetDirectory(linkDirectoryPath, true);

            // Check if the node already exists
            if (directory.NodeExists(link.Name))
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, link.Name));
                }
                else
                {
                    // Throw an exception if the existing node is not a symbolic link
                    if (!SymbolicLinkExists(linkDirectoryPath + link.Name))
                    {
                        throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualSymbolicLink, link.Name));
                    }
                    // Remove the existing symbolic link
                    directory.Remove(link);
                }
            }

            // Add the new symbolic link
            link = (VirtualSymbolicLink)directory.Add(link);

            if (link.TargetPath != null)
            {
                // Use the directory path where the symbolic link was created as the base
                VirtualPath absoluteTargetPath = ConvertToAbsolutePath(link.TargetPath!, linkDirectoryPath).NormalizePath();

                // Add the link information to the dictionary
                AddLinkToDictionary(absoluteTargetPath, linkDirectoryPath + link.Name);
            }

            // Update all target node types in the dictionary
            UpdateAllTargetNodeTypesInDictionary();
        }

        /// <summary>
        /// Adds a symbolic link.
        /// </summary>
        /// <param name="linkPath">The path of the symbolic link</param>
        /// <param name="targetPath">The path of the target</param>
        /// <param name="overwrite">Indicates whether to overwrite an existing node</param>
        public void AddSymbolicLink(VirtualPath linkPath, VirtualPath? targetPath = null, bool overwrite = false)
        {
            // Convert to absolute path and normalize linkPath
            linkPath = ConvertToAbsolutePath(linkPath).NormalizePath();

            // Separate the directory path and link name
            VirtualPath directoryPath = linkPath.DirectoryPath;
            VirtualNodeName linkName = linkPath.NodeName;

            // Create the symbolic link
            VirtualSymbolicLink link = new(linkName, targetPath);

            // Call the AddSymbolicLink method
            AddSymbolicLink(directoryPath, link, overwrite);
        }

        /// <summary>
        /// Changes the name of a node.
        /// </summary>
        /// <param name="nodePath">The path of the node to rename</param>
        /// <param name="newName">The new name</param>
        /// <param name="resolveLinks">Indicates whether to resolve symbolic links</param>
        public void SetNodeName(VirtualPath nodePath, VirtualNodeName newName, bool resolveLinks = true)
        {
            VirtualPath oldAbsolutePath = ConvertToAbsolutePath(nodePath);
            VirtualPath newAbsolutePath = oldAbsolutePath.DirectoryPath + newName;

            // Check if the new name is the same as the current name
            if (oldAbsolutePath == newAbsolutePath)
            {
                throw new InvalidOperationException(string.Format(Resources.NewNameSameAsCurrent, newAbsolutePath));
            }

            // Retrieve the node (resolving links as needed)
            VirtualNodeContext nodeContext = WalkPathToTarget(oldAbsolutePath, null, null, resolveLinks, true);
            VirtualNode node = nodeContext.Node!;

            // Check if a node with the new name already exists
            if (NodeExists(newAbsolutePath))
            {
                throw new InvalidOperationException(string.Format(Resources.NewNameNodeAlreadyExists, newAbsolutePath));
            }

            // Retrieve the parent directory
            VirtualDirectory parentDirectory = nodeContext.ParentDirectory!;

            // Update the link dictionary (if the node is a symbolic link)
            if (node is VirtualSymbolicLink)
            {
                UpdateLinkNameInDictionary(oldAbsolutePath, newAbsolutePath);
            }

            // Update the link dictionary (change the target path)
            UpdateLinksToTarget(oldAbsolutePath, newAbsolutePath);

            // Remove the old node from the parent directory and add the new named node
            parentDirectory.SetNodeName(node.Name, newName);
        }
    }
}
