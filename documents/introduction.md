---
title: "Introduction"
---

![burner.png](images/burner.png)

<details>
  <summary>Language: English</summary>
  <ul>
    <li><a href="introduction.md">English</a></li>
    <li><a href="introduction.ja.md">Japanese</a></li>
  </ul>
</details>

# Introduction

## Background and Motivation

.NET collections are linear collections. While there are various types such as hash sets, arrays, lists, and dictionaries, they fundamentally have a linear structure.  
On the other hand, general file systems can be considered tree collections. Elements are managed as nodes, forming a hierarchical structure.  
While there are existing libraries that support such tree collections, I couldn’t find one that models a file system.  
So, I conceptualized and developed a **tree collection that can be treated as pure objects by logically interpreting the file system**.  
The aim is to create a system that allows flexible management of hierarchical data and intuitive access.

## Use Cases

`VirtualStorageLibrary` can be utilized in a variety of use cases, such as:

- Natural Language Processing (NLP)
- Knowledge-based systems
- Game development
- Hierarchical clustering
- Education and learning

For more detailed use cases and specific applications, please refer to the [README](https://github.com/shimodateakira/VirtualStorageLibrary/blob/master/README.md).

## Design Philosophy

`VirtualStorageLibrary` is designed to provide a tree structure-based collection that operates in-memory, enabling application developers to efficiently manage data. The design emphasizes an intuitive API with flexibility and extensibility in mind.

#### Simplicity

- **Support for Node Types**  
  In `VirtualStorageLibrary`, three node types have been defined according to their roles: items, directories, and symbolic links. By making the node roles somewhat specific and limited, it allows for role-specific implementations. This approach is simpler and more efficient than a method where a single node can have multiple roles.
  
  - `VirtualItem<T>`: A node representing an item that encapsulates a user-defined type `T`.
  
  - `VirtualDirectory`: A node representing a directory.
  
  - `VirtualSymbolicLink`: A node representing a symbolic link.

- **Loosely Coupled Nodes**  
  Loosely coupled nodes mean minimizing dependencies between nodes, which enhances the flexibility and efficiency of node management. Generally, tree structures have nodes that maintain references to other nodes, supporting node traversal. In `VirtualStorageLibrary`, only directories hold references to their children nodes, meaning directories only care about their children. This allows for efficient execution of operations like copying or moving the node tree.

#### Flexibility

- **Support for User-defined Types**  
  The main `VirtualStorage` class in `VirtualStorageLibrary` is a generic class that handles user-defined types.  
  Users can define any class they wish.  
  This allows for flexible handling of application data.
- **Support for Deep Cloning**  
  Similar to a general file system, each node in `VirtualStorageLibrary` has its own instance.  
  This means that multiple nodes do not reference the same instance.  
  When a node is copied, a deep clone of the node is performed, creating a new node with the same content as the original.  
  The user-defined type classes encapsulated within the items can support deep cloning by implementing the `IVirtualDeepCloneable` interface.
- **Definition of Path Characters**  
  The path characters defined by default are initially set to follow common file system separators `/`, dot `.`, dot-dot `..`, and root character `/`.  
  These are initially set in the `VirtualStorageSettings` class, and after library initialization, they can be modified in the `VirtualStorageState` class.
- **Definition of Prohibited Node Name Characters and Strings**  
  The prohibited node name characters defined by default are dot `.`, dot-dot `..`, and the prohibited node name string is separator `/`.  
  These are used in the node name validity check when adding nodes to a directory.  
  These are initially set in the `VirtualStorageSettings` class, and after library initialization, they can be modified in the `VirtualStorageState` class.
- **Definition of Node List Display Conditions**  
  The node list can be retrieved according to pre-set display conditions.  
  The display conditions consist of the following elements:  
  The default is no filtering, grouping enabled (ascending), and sorting by node name property (ascending).  
  These are initially set in the `VirtualStorageSettings` class, and after library initialization, they can be modified in the `VirtualStorageState` class.
  - Filtering  
    You can filter by specifying the node type (directory, item, symbolic link) with OR conditions.
  - Grouping  
    You can specify whether to group by node type, and when grouping, you can specify the order of the groups (ascending/descending).
  - Sorting  
    You can sort within the group when grouped or the entire list when not grouped, by the specified property. You can specify the order (ascending/descending) at that time.

#### Extensibility

- **Support for Wildcards**  
  Paths can include wildcards.  
  Paths containing wildcards can be expanded into multiple specific paths by calling the wildcard expansion API.  
  Each API that operates on nodes can perform node operations by passing the expanded paths.
- **Definition of Wildcard Matchers**  
  Wildcards are processed by a wildcard matcher class that implements the `IVirtualWildcardMatcher` interface.  
  `VirtualStorageLibrary` provides the following two classes in advance.  
  The default is `PowerShellWildcardMatcher`.  
  These are initially set in the `VirtualStorageSettings` class, and after library initialization, they can be modified in the `VirtualStorageState` class.  
  It is also possible to use a custom wildcard matcher by implementing the `IVirtualWildcardMatcher` interface.
  - `DefaultWildcardMatcher` class  
    Wildcards using regular expressions as-is.
  - `PowerShellWildcardMatcher` class  
    Wildcards adopted in PowerShell.

## Current Status and Future Plans

As of (2024/08), all the features to be implemented in V1.0.0 have been completed.  
In V0.8.0, a few bug fixes, nearly 30 feature improvements, and refactoring remain.  
After completing these tasks, the remaining tasks for V0.9.0 will be addressed, aiming for a stable release in V1.0.0.

During this period, class names, method names, property names, etc., provided by the library may be changed, merged, or deprecated without notice.  
In such cases, details will be included in the release notes, so please check them.  
For more details, refer to the [Current Issues and Improvement Proposals](https://github.com/users/shimodateakira/projects/3/views/7) (Japanese).

## Expansion Possibilities

#### Roadmap

- **V0.9.0**: Focus on bug fixes, feature improvements, and refactoring that remain.
- **V1.0.0**: Plan for a stable release based on user feedback.
- **Long-term Plan**: Plans include improving the indexer functionality, derived node classes, and smarter exception handling.

#### Feedback and Community Collaboration

`VirtualStorageLibrary` is a project that grows through feedback and collaboration from everyone. We welcome your suggestions on the features and improvements that should be prioritized in the next version, through [Issues](https://github.com/shimodateakira/VirtualStorageLibrary/issues) or [Discussions](https://github.com/shimodateakira/VirtualStorageLibrary/discussions).

We also look forward to various forms of collaboration, such as contributing to the code, improving documentation, and assisting with translation. As the project is still in the pre-release stage, the participation of users is crucial. We ask for your active participation.

#### How You Can Help

The growth of `VirtualStorageLibrary` relies on your feedback and collaboration. Here are ways you can contribute to the project:

- **Feedback**: Share your opinions on usability and functionality.
- **Bug Reporting**: Report any bugs you find.
- **Feature Improvement and Addition Requests**: We welcome suggestions for new features and improvements to existing ones.

These can be submitted through [Issues](https://github.com/shimodateakira/VirtualStorageLibrary/issues). We ask for your active participation.

#### Technical Questions

For technical questions, please use [StackOverflow](https://stackoverflow.com/).  
Using tags like `c#`, `.net`, `tree`, `shared-libraries`, and `generic-collections` will make your questions easier to find.

#### Pull Request Policy

Currently, due to the lack of a multi-person development structure, we are not accepting pull requests for the time being.  
We appreciate your understanding and will proceed with this once a proper structure is in place.

## Long-term Vision

`VirtualStorageLibrary` aims to simplify and make the management of tree structures more intuitive, strongly supporting the creative activities of application developers. Some additional features have already been identified for future implementation, and potential use cases have been mentioned. However, the ways to utilize this library are limitless, and new ideas will likely emerge as it is used.  
We hope that in the near future, this library will be incorporated into applications across various fields, contributing as part of numerous solutions.

![tree_256x256.svg](images/tree_256x256.svg)
