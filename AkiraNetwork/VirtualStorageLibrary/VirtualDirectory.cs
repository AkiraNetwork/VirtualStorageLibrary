// This file is part of VirtualStorageLibrary.
//
// Copyright (C) 2024 Akira Shimodate
//
// VirtualStorageLibrary is free software, and it is distributed under the terms of 
// the GNU Lesser General Public License (version 3, or at your option, any later 
// version). This license is published by the Free Software Foundation.
//
// VirtualStorageLibrary is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY, without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for 
// more details.
//
// You should have received a copy of the GNU Lesser General Public License along 
// with VirtualStorageLibrary. If not, see https://www.gnu.org/licenses/.

using AkiraNetwork.VirtualStorageLibrary.Localization;
using System.Runtime.CompilerServices;

namespace AkiraNetwork.VirtualStorageLibrary
{
    /// <summary>
    /// Represents a virtual directory. A virtual directory functions as a container
    /// for other nodes. It can contain <see cref="VirtualItem{T}"/>, 
    /// <see cref="VirtualDirectory"/>, and <see cref="VirtualSymbolicLink"/>, 
    /// providing a centralized way to manage these entities.
    ///
    /// This class supports operations such as adding, removing, updating, 
    /// checking the existence of, and enumerating nodes within the directory. 
    /// It serves as a fundamental component for efficiently managing the structure 
    /// of the virtual storage.
    ///
    /// Additionally, nodes can be enumerated according to the specified display conditions.
    /// </summary>
    public class VirtualDirectory : VirtualNode, IEnumerable<VirtualNode>
    {
        /// <summary>
        /// A dictionary for managing nodes within the directory.
        /// </summary>
        private readonly Dictionary<VirtualNodeName, VirtualNode> _nodes = [];

        /// <summary>
        /// Gets the type of the node. This class always returns Directory.
        /// </summary>
        public override VirtualNodeType NodeType => VirtualNodeType.Directory;

        /// <summary>
        /// Enumerates all node names within the directory.
        /// </summary>
        public IEnumerable<VirtualNodeName> NodeNames => _nodes.Keys;

        /// <summary>
        /// Enumerates all nodes within the directory.
        /// </summary>
        public IEnumerable<VirtualNode> Nodes => _nodes.Values;

        /// <summary>
        /// Gets the total number of nodes within the directory.
        /// </summary>
        public int Count => _nodes.Count;

        /// <summary>
        /// Gets the number of directories within the directory.
        /// </summary>
        public int DirectoryCount => Nodes.Count(n => n is VirtualDirectory);

        /// <summary>
        /// Gets the number of items within the directory.
        /// </summary>
        public int ItemCount => Nodes.Count(n => n is VirtualItem);

        /// <summary>
        /// Gets the number of symbolic links within the directory.
        /// </summary>
        public int SymbolicLinkCount => Nodes.Count(n => n is VirtualSymbolicLink);

        /// <summary>
        /// Gets a view of nodes based on the current display conditions.
        /// </summary>
        public IEnumerable<VirtualNode> NodesView => GetNodesView();

        /// <summary>
        /// Gets the number of nodes based on the current display conditions.
        /// </summary>
        public int NodesViewCount => NodesView.Count();

        /// <summary>
        /// Gets the number of directories based on the current display conditions.
        /// </summary>
        public int DirectoryViewCount => NodesView.Count(n => n is VirtualDirectory);

        /// <summary>
        /// Gets the number of items based on the current display conditions.
        /// </summary>
        public int ItemViewCount => NodesView.Count(n => n is VirtualItem);

        /// <summary>
        /// Gets the number of symbolic links based on the current display conditions.
        /// </summary>
        public int SymbolicLinkViewCount => NodesView.Count(n => n is VirtualSymbolicLink);

        /// <summary>
        /// A dictionary that holds node types and their corresponding filters.
        /// </summary>
        private static readonly Dictionary<VirtualNodeType, VirtualNodeTypeFilter> _nodeTypeToFilterMap = new()
        {
            { VirtualNodeType.Directory, VirtualNodeTypeFilter.Directory },
            { VirtualNodeType.Item, VirtualNodeTypeFilter.Item },
            { VirtualNodeType.SymbolicLink, VirtualNodeTypeFilter.SymbolicLink }
        };

        /// <summary>
        /// Checks whether a node with the specified name exists.
        /// </summary>
        /// <param name="name">The name of the node to check.</param>
        /// <returns>True if the node exists; otherwise, false.</returns>
        public bool NodeExists(VirtualNodeName name) => _nodes.ContainsKey(name);

        /// <summary>
        /// Checks whether an item with the specified name exists.
        /// </summary>
        /// <param name="name">The name of the item to check.</param>
        /// <returns>True if the item exists; otherwise, false.</returns>
        public bool ItemExists(VirtualNodeName name)
        {
            // Check for the existence of the node using NodeExists
            if (!NodeExists(name))
            {
                return false; // Return false if the node does not exist
            }

            var node = _nodes[name];
            var nodeType = node.GetType();
            // Check if the node type is generic and its generic type definition is VirtualItem<T>
            return nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeof(VirtualItem<>);
        }

        /// <summary>
        /// Checks whether a directory with the specified name exists.
        /// </summary>
        /// <param name="name">The name of the directory to check.</param>
        /// <returns>True if the directory exists; otherwise, false.</returns>
        public bool DirectoryExists(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                return false;
            }

            return _nodes[name] is VirtualDirectory;
        }

        /// <summary>
        /// Checks whether a symbolic link with the specified name exists.
        /// </summary>
        /// <param name="name">The name of the symbolic link to check.</param>
        /// <returns>True if the symbolic link exists; otherwise, false.</returns>
        public bool SymbolicLinkExists(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                return false;
            }

            var node = _nodes[name];
            return node is VirtualSymbolicLink;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualDirectory"/> class.
        /// The node name is automatically generated by the <see cref="VirtualNodeName.GenerateNodeName"/> method.
        /// </summary>
        public VirtualDirectory()
            : base(VirtualNodeName.GenerateNodeName(VirtualStorageState.State.PrefixDirectory))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualDirectory"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        public VirtualDirectory(VirtualNodeName name) : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualDirectory"/> class with the specified name, creation date, and update date.
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        /// <param name="createdDate">The creation date.</param>
        public VirtualDirectory(VirtualNodeName name, DateTime createdDate) : base(name, createdDate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualDirectory"/> class with the specified name, creation date, and update date.
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        /// <param name="createdDate">The creation date.</param>
        /// <param name="updatedDate">The update date.</param>
        public VirtualDirectory(VirtualNodeName name, DateTime createdDate, DateTime updatedDate) : base(name, createdDate, updatedDate)
        {
        }

        /// <summary>
        /// Implicitly converts the specified node name to a <see cref="VirtualDirectory"/>.
        /// </summary>
        /// <param name="nodeName">The node name to convert.</param>
        public static implicit operator VirtualDirectory(VirtualNodeName nodeName)
        {
            return new VirtualDirectory(nodeName);
        }

        /// <summary>
        /// Returns a string representation of the directory.
        /// </summary>
        /// <returns>A string representation of the directory.</returns>
        public override string ToString() => (Name == VirtualPath.Root) ? VirtualPath.Root : $"{Name}{VirtualPath.Separator}";

        /// <summary>
        /// Creates a deep clone of the directory.
        /// </summary>
        /// <param name="recursive">If true, all nodes within the directory are recursively cloned.</param>
        /// <returns>A deep clone of the directory.</returns>
        public override VirtualNode DeepClone(bool recursive = false)
        {
            VirtualDirectory newDirectory = new(Name);
            if (recursive)
            {
                foreach (VirtualNode node in Nodes)
                {
                    newDirectory.Add(node.DeepClone(true));
                }
            }
            return newDirectory;
        }

        /// <summary>
        /// Gets a view of nodes based on the current display conditions.
        /// </summary>
        /// <returns>An enumeration of nodes based on the display conditions.</returns>
        public IEnumerable<VirtualNode> GetNodesView()
        {
            VirtualNodeTypeFilter filter = VirtualStorageState.State.NodeListConditions.Filter;
            VirtualGroupCondition<VirtualNode, object>? groupCondition = VirtualStorageState.State.NodeListConditions.GroupCondition;
            List<VirtualSortCondition<VirtualNode>>? sortConditions = VirtualStorageState.State.NodeListConditions.SortConditions;

            IEnumerable<VirtualNode> nodes = Nodes;

            switch (filter)
            {
                case VirtualNodeTypeFilter.None:
                    return [];
                case VirtualNodeTypeFilter.All:
                    break;
                default:
                    nodes = nodes.Where(node => IsNodeMatchingFilter(node, filter));
                    break;
            }

            return nodes.GroupAndSort(groupCondition, sortConditions);
        }

        /// <summary>
        /// Checks if the node matches the specified filter.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <param name="filter">The filter to apply.</param>
        /// <returns>True if the node matches the filter; otherwise, false.</returns>
        private static bool IsNodeMatchingFilter(VirtualNode node, VirtualNodeTypeFilter filter)
        {
            if (filter.HasFlag(VirtualNodeTypeFilter.Directory) && node is VirtualDirectory)
            {
                return true;
            }

            if (filter.HasFlag(VirtualNodeTypeFilter.Item) && node is VirtualItem)
            {
                return true;
            }

            if (filter.HasFlag(VirtualNodeTypeFilter.SymbolicLink) && node is VirtualSymbolicLink link)
            {
                // Return true if only the SymbolicLink flag is specified
                if (filter == VirtualNodeTypeFilter.SymbolicLink)
                {
                    return true;
                }

                VirtualNodeTypeFilter targetFilter = _nodeTypeToFilterMap[link.TargetNodeType];

                return filter.HasFlag(targetFilter);
            }

            return false;
        }

        /// <summary>
        /// Adds a node to the directory.
        /// </summary>
        /// <param name="node">The node to add.</param>
        /// <param name="allowOverwrite">If true, allows overwriting an existing node with the same name.</param>
        /// <returns>The added node.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid node name is specified.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a node with the same name already exists.</exception>
        public VirtualNode Add(VirtualNode node, bool allowOverwrite = false)
        {
            if (node.Name.IsRoot)
            {
                throw new ArgumentException(Resources.CannotAddRoot, nameof(node));
            }

            if (!VirtualNodeName.IsValidNodeName(node.Name))
            {
                throw new ArgumentException(string.Format(Resources.InvalidNodeName, node.Name), nameof(node));
            }

            VirtualNodeName key = node.Name;

            if (_nodes.ContainsKey(key) && !allowOverwrite)
            {
                throw new InvalidOperationException(string.Format(Resources.NodeAlreadyExists, key));
            }

            // If the node is already added to storage, create a clone
            if (node.IsReferencedInStorage)
            {
                if (node is VirtualDirectory)
                {
                    node = node.DeepClone(true);
                }
                else
                {
                    node = node.DeepClone(false);
                }
            }

            _nodes[key] = node;

            // Update the update date
            UpdatedDate = DateTime.Now;

            // Propagate IsReferencedInStorage to the child nodes
            SetIsReferencedInStorageRecursively(node, IsReferencedInStorage);

            return node;
        }

        /// <summary>
        /// Adds multiple nodes to the directory.
        /// </summary>
        /// <param name="nodes">The nodes to add.</param>
        /// <param name="allowOverwrite">If true, allows overwriting existing nodes with the same names.</param>
        /// <returns>A list of the added nodes. If a node was cloned, the cloned instance is returned in the list.</returns>
        /// <exception cref="ArgumentException">Thrown if an invalid node name is specified in any of the nodes.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a node with the same name already exists and overwriting is not allowed.</exception>
        public IList<VirtualNode> AddRange(IEnumerable<VirtualNode> nodes, bool allowOverwrite = false)
        {
            var addedNodes = new List<VirtualNode>();

            foreach (var node in nodes)
            {
                // Add the node and collect the returned node (original or cloned)
                var addedNode = Add(node, allowOverwrite);
                addedNodes.Add(addedNode);
            }

            return addedNodes;
        }

        /// <summary>
        /// Adds an item with the specified name.
        /// </summary>
        /// <typeparam name="T">The data type of the item.</typeparam>
        /// <param name="name">The name of the item.</param>
        /// <param name="itemData">The data of the item.</param>
        /// <param name="allowOverwrite">If true, allows overwriting an existing node with the same name.</param>
        /// <returns>The added item.</returns>
        public VirtualItem<T> AddItem<T>(VirtualNodeName name, T? itemData = default, bool allowOverwrite = false)
        {
            VirtualItem<T> item = (VirtualItem<T>)Add(new VirtualItem<T>(name, itemData), allowOverwrite);
            return item;
        }

        /// <summary>
        /// Adds a symbolic link with the specified name.
        /// </summary>
        /// <param name="name">The name of the symbolic link.</param>
        /// <param name="targetPath">The target path of the symbolic link.</param>
        /// <param name="allowOverwrite">If true, allows overwriting an existing node with the same name.</param>
        /// <returns>The added symbolic link.</returns>
        public VirtualSymbolicLink AddSymbolicLink(VirtualNodeName name, VirtualPath targetPath, bool allowOverwrite = false)
        {
            VirtualSymbolicLink link = (VirtualSymbolicLink)Add(new VirtualSymbolicLink(name, targetPath), allowOverwrite);
            return link;
        }

        /// <summary>
        /// Adds a directory with the specified name.
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        /// <param name="allowOverwrite">If true, allows overwriting an existing node with the same name.</param>
        /// <returns>The added directory.</returns>
        public VirtualDirectory AddDirectory(VirtualNodeName name, bool allowOverwrite = false)
        {
            VirtualDirectory directory = (VirtualDirectory)Add(new VirtualDirectory(name), allowOverwrite);
            return directory;
        }

        /// <summary>
        /// Gets or sets the node with the specified name using the indexer.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <returns>The node with the specified name.</returns>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node is not found.</exception>
        [IndexerName("Indexer")]
        public VirtualNode this[VirtualNodeName name]
        {
            get
            {
                if (!NodeExists(name))
                {
                    throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, name));
                }
                return _nodes[name];
            }
            set
            {
                if (!NodeExists(name))
                {
                    _nodes[name] = value;
                }
                else
                {
                    _nodes[name].Update(value);
                }
            }
        }

        /// <summary>
        /// Changes the name of a node.
        /// </summary>
        /// <param name="oldName">The old node name.</param>
        /// <param name="newName">The new node name.</param>
        internal void SetNodeName(VirtualNodeName oldName, VirtualNodeName newName)
        {
            VirtualNode node = _nodes[oldName];
            _nodes.Remove(oldName);
            _nodes[newName] = node;

            // Update the update date
            DateTime now = DateTime.Now;
            _nodes[newName].UpdatedDate = now;
            UpdatedDate = now;
        }

        /// <summary>
        /// Removes the specified node from the directory.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node is not found.</exception>
        public void Remove(VirtualNode node)
        {
            if (!NodeExists(node.Name))
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, node.Name));
            }

            _nodes.Remove(node.Name);

            // Update the update date
            UpdatedDate = DateTime.Now;

            // Propagate IsReferencedInStorage = false to the child nodes
            SetIsReferencedInStorageRecursively(node, false);
        }

        /// <summary>
        /// Gets the node with the specified name.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="exceptionEnabled">If true, throws an exception if the node is not found.</param>
        /// <returns>The node with the specified name, or null if the node does not exist.</returns>
        public VirtualNode? Get(VirtualNodeName name, bool exceptionEnabled = true)
        {
            if (!NodeExists(name))
            {
                if (exceptionEnabled)
                {
                    throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, name));
                }
                else
                {
                    return null;
                }
            }
            return _nodes[name];
        }

        /// <summary>
        /// Gets an enumerator for nodes based on the current display conditions.
        /// </summary>
        /// <returns>An enumerator for filtered nodes.</returns>
        public IEnumerator<VirtualNode> GetEnumerator() => NodesView.GetEnumerator();

        /// <summary>
        /// Gets an enumerator for nodes based on the current display conditions.
        /// </summary>
        /// <returns>An enumerator for filtered nodes.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the item with the specified name.
        /// </summary>
        /// <typeparam name="T">The data type of the item.</typeparam>
        /// <param name="name">The name of the item.</param>
        /// <returns>The item with the specified name.</returns>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified node is not an item.</exception>
        public VirtualItem<T> GetItem<T>(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, name));
            }

            var node = _nodes[name];
            if (node is VirtualItem<T> item)
            {
                return item;
            }

            throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualItem, name, typeof(T).Name));
        }

        /// <summary>
        /// Gets the directory with the specified name.
        /// </summary>
        /// <param name="name">The name of the directory.</param>
        /// <returns>The directory with the specified name.</returns>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified node is not a directory.</exception>
        public VirtualDirectory GetDirectory(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, name));
            }

            if (_nodes[name] is VirtualDirectory directory)
            {
                return directory;
            }

            throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualDirectory, name));
        }

        /// <summary>
        /// Gets the symbolic link with the specified name.
        /// </summary>
        /// <param name="name">The name of the symbolic link.</param>
        /// <returns>The symbolic link with the specified name.</returns>
        /// <exception cref="VirtualNodeNotFoundException">Thrown if the node is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the specified node is not a symbolic link.</exception>
        public VirtualSymbolicLink GetSymbolicLink(VirtualNodeName name)
        {
            if (!NodeExists(name))
            {
                throw new VirtualNodeNotFoundException(string.Format(Resources.NodeNotFound, name));
            }

            var node = _nodes[name];
            if (node is VirtualSymbolicLink link)
            {
                return link;
            }

            throw new InvalidOperationException(string.Format(Resources.NodeIsNotVirtualSymbolicLink, name));
        }

        /// <summary>
        /// Overloads the addition operator to add a node to the directory.
        /// </summary>
        /// <param name="directory">The directory to which the node is added.</param>
        /// <param name="node">The node to add.</param>
        /// <returns>The directory with the added node.</returns>
        public static VirtualDirectory operator +(VirtualDirectory directory, VirtualNode node)
        {
            node = directory.Add(node);
            if (directory.IsReferencedInStorage)
            {
                SetIsReferencedInStorageRecursively(node, true);
            }
            return directory;
        }

        /// <summary>
        /// Overloads the addition operator to add multiple nodes to the directory using AddRange.
        /// </summary>
        /// <param name="directory">The directory to which the nodes are added.</param>
        /// <param name="nodes">The nodes to add.</param>
        /// <returns>The directory with the added nodes.</returns>
        public static VirtualDirectory operator +(VirtualDirectory directory, IList<VirtualNode> nodes)
        {
            // Use AddRange to add multiple nodes
            var addedNodes = directory.AddRange(nodes, allowOverwrite: false);

            // If the directory is referenced in storage, propagate IsReferencedInStorage to each added node
            if (directory.IsReferencedInStorage)
            {
                foreach (var node in addedNodes)
                {
                    SetIsReferencedInStorageRecursively(node, true);
                }
            }

            return directory;
        }

        /// <summary>
        /// Overloads the addition operator to add a node to the directory.
        /// </summary>
        /// <param name="directory">The directory to which the node is added.</param>
        /// <param name="nodeName">The node name of directory to add.</param>
        /// <returns>The directory with the added node.</returns>
        public static VirtualDirectory operator +(VirtualDirectory directory, VirtualNodeName nodeName)
        {
            VirtualNode node = directory.Add(new VirtualDirectory(nodeName));
            if (directory.IsReferencedInStorage)
            {
                SetIsReferencedInStorageRecursively(node, true);
            }
            return directory;
        }

        /// <summary>
        /// Overloads the subtraction operator to remove a node from the directory.
        /// </summary>
        /// <param name="directory">The directory from which the node is removed.</param>
        /// <param name="node">The node to remove.</param>
        /// <returns>The directory with the removed node.</returns>
        public static VirtualDirectory operator -(VirtualDirectory directory, VirtualNode node)
        {
            directory.Remove(node);
            SetIsReferencedInStorageRecursively(node, false);
            return directory;
        }

        /// <summary>
        /// Recursively sets the IsReferencedInStorage property for the node and its child nodes.
        /// </summary>
        /// <param name="node">The node to set.</param>
        /// <param name="value">The value to set.</param>
        private static void SetIsReferencedInStorageRecursively(VirtualNode node, bool value)
        {
            node.IsReferencedInStorage = value;
            if (node is VirtualDirectory subDirectory)
            {
                foreach (var subNode in subDirectory.Nodes)
                {
                    SetIsReferencedInStorageRecursively(subNode, value);
                }
            }
        }

        /// <summary>
        /// Updates the current directory with the data from the specified node.
        /// </summary>
        /// <param name="node">The node used for the update.</param>
        /// <exception cref="ArgumentException">Thrown if the specified node is not a VirtualDirectory.</exception>
        public override void Update(VirtualNode node)
        {
            if (node is not VirtualDirectory newDirectory)
            {
                throw new ArgumentException(string.Format(Resources.NodeIsNotVirtualDirectory, node.Name), nameof(node));
            }

            if (newDirectory.IsReferencedInStorage)
            {
                newDirectory = (VirtualDirectory)newDirectory.DeepClone(true);
            }

            CreatedDate = newDirectory.CreatedDate;
            UpdatedDate = DateTime.Now;

            foreach (VirtualNode subNode in newDirectory.Nodes)
            {
                if (_nodes.TryGetValue(subNode.Name, out VirtualNode? existingNode))
                {
                    existingNode.Update(subNode.DeepClone(true));
                }
                else
                {
                    Add(subNode.DeepClone(true));
                }
            }
        }
    }
}
