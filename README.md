
![burner.png](https://raw.githubusercontent.com/AkiraNetwork/VirtualStorageLibrary/master/docs/images/burner.png)

<details>
  <summary>Language: English</summary>
  <ul>
    <li><a href="README.md">English</a></li>
    <li><a href="README.ja.md">Japanese</a></li>
  </ul>
</details>

![Version: 0.9.0](https://img.shields.io/badge/version-0.9.0-pink.svg)
[![License: LGPL-3.0-or-later](https://img.shields.io/badge/License-LGPL%20v3.0%20or%20later-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0.html)
[![Platform: .NET 8](https://img.shields.io/badge/platform-.NET%208-green)](https://dotnet.microsoft.com/en-us/download/dotnet)
[![Documentation: online](https://img.shields.io/badge/docs-online-purple.svg)](https://AkiraNetwork.github.io/VirtualStorageLibrary/api/AkiraNetwork.VirtualStorageLibrary.html)
[![Maintenance: active](https://img.shields.io/badge/maintenance-active-blue.svg)](https://github.com/users/AkiraNetwork/projects/1)

# Welcome to VirtualStorageLibrary!

## Project Overview and Purpose
`VirtualStorageLibrary` is a .NET library that operates entirely in-memory and 
provides a **tree-structured collection**. This library offers a foundation 
for managing **hierarchical data structures**, supporting items, directories, 
and symbolic links that encapsulate user-defined types \<T\>.  
This library **is not a file system**.  
Instead, it was **redesigned from scratch** to create a more 
flexible and user-friendly tree structure. The library aims to make it 
**intuitive** to reference, traverse, and manipulate nodes **by specifying 
paths**.

![VirtualStorageLibraryLogo](https://raw.githubusercontent.com/AkiraNetwork/VirtualStorageLibrary/master/tree_256x256.svg)

## Project Background
The collections provided by .NET are linear, including types like hash sets, 
arrays, lists, and dictionaries, which inherently have a linear structure. In 
contrast, common file systems can be viewed as tree-shaped collections, where 
elements are managed as nodes in a hierarchical structure. While there are 
existing libraries that support tree-shaped collections, I couldn’t find one 
that models a file system-like structure. Therefore, I conceptualized a 
logical interpretation of a file system and asked, **"Can we implement a tree 
collection purely as objects?"** The goal was to create a system that can 
flexibly manage hierarchical  and allow intuitive access.

## Table of Contents

- [Key Features](#key-features)
- [Anticipated Use Cases](#anticipated-use-cases)
- [Technology Stack](#technology-stack)
- [Target Users](#target-users)
- [Installation Instructions](#installation-instructions)
- [Usage](#usage)
- [Documentation](#documentation)
- [Configuration and Customization](#configuration-and-customization)
- [License](#license)
- [Contribution Guidelines](#contribution-guidelines)
- [Author and Acknowledgments](#author-and-acknowledgments)

[[▲](#table-of-contents)]
## Key Features

#### Flexible Tree Structure
  Provides a hierarchical  structure based on parent-child
  relationships, allowing flexible node management.

#### Support for Various Nodes
  Supports items, directories, and symbolic links, including
  user-defined types \<T\>.

#### Intuitive Node Operations via Paths
  Offers an intuitive API for referencing, searching, adding,
  deleting, renaming, copying, and moving nodes using paths.

#### Link Management
  Manages symbolic links with a link dictionary, tracking
  target path changes.

#### Circular Reference Prevention
  Throws an exception when detecting circular references in
  paths involving symbolic links.

#### Flexible Node List Retrieval
  Retrieves node lists within directories, filtered, grouped, and
  sorted by specified node types and attributes.

[[▲](#table-of-contents)]
## Anticipated Use Cases

### Natural Language Processing (NLP)
In natural language processing, tree structures are often used to
analyze and parse text data. For instance, parsing results can be
represented as syntax trees, visualizing the relationships between
elements of a sentence. Tree structures are highly effective for
managing such structured data.

The Virtual Storage Library supports managing tree-structured data
and accessing nodes via paths, enabling efficient data analysis. 
Specific scenarios where it is useful include:

- **Managing Syntax Trees**: Stores syntax trees resulting from
  grammar parsing and manages relationships hierarchically.
- **Managing Entity Links**: Represents relationships of entities 
  (e.g., names, places) within text as a tree structure, supporting 
  quick search and access.
- **Visualizing Topic Models**: Models hierarchical relationships 
  between topics, efficiently displaying multiple topics and 
  subtopics.

This facilitates easier management of complex data structures in NLP 
tasks and enhances analysis efficiency.

### Knowledge Base Systems
In knowledge base systems, it is essential to organize large volumes 
of documents and information hierarchically, providing efficient 
searchability. The Virtual Storage Library helps manage such 
hierarchical structures, enabling users to quickly access the 
information they need.

Specific scenarios include:

- **Technical Document Management**: Categorizes product technical 
  documents and manuals, allowing users to quickly find specific 
  information.
- **FAQ Systems**: Organizes frequently asked questions and their 
  answers hierarchically, enhancing searchability and helping users 
  find answers easily.
- **Building Knowledge Bases**: Documents organizational expertise 
  and manages it in a tree structure, providing a learning 
  environment for new members.

This enables knowledge base systems to efficiently organize and 
access information, maximizing the utility of information.

### Game Development
In game development, managing in-game objects and scenes is 
important. Particularly when managing hierarchical relationships 
between objects, the Virtual Storage Library supports dynamic scene 
changes, contributing to efficient development processes.

Specific scenarios include:

- **Scene Management**: Manages objects in different levels or areas 
  of the game hierarchically, dynamically changing them according to 
  player actions or events.
- **Character Equipment Management**: Manages items and weapons that 
  a character equips in a tree structure, making real-time equipment 
  changes easy.
- **Streamlining Level Design**: Allows level designers to visually 
  manage scenes and object hierarchies, enabling quick placement and 
  changes.

This gives the game development process flexibility and speed, 
maximizing creative elements.

### Hierarchical Clustering
Hierarchical clustering involves grouping and classifying data in a 
hierarchical manner, managing clustering results as a tree structure 
and supporting data analysis and visualization. The Virtual Storage 
Library supports this analysis process.

Specific scenarios include:

- **Customer Segmentation**: Classifies customer data hierarchically 
  based on purchasing behavior and demographic information, 
  optimizing marketing strategies.
- **Biological Taxonomy**: Manages biological classification 
  information (species, genus, family, etc.) in a tree structure, 
  visualizing characteristics and relationships at each classification 
  level.
- **Hierarchical Data Analysis**: Groups large datasets hierarchically 
  and conducts exploratory data analysis to discover patterns and 
  trends.

This allows hierarchical clustering results to be efficiently managed 
and visual insights to be gained. The Virtual Storage Library supports 
this process of data visualization and analysis, aiding data-driven 
decision-making.

### Education and Learning
The source code of the Virtual Storage Library can be used in education 
and learning, particularly for deepening understanding of programming 
and data structures.

Specific scenarios include:

- **Programming Education**: Used as a practical teaching material for 
  students to learn how to manage and manipulate tree-structured data. 
  By manipulating virtual directory structures and data trees through 
  specific tasks, students deepen their understanding of data structures 
  and algorithms.
- **Visualizing Data Structures**: Visually representing data structures 
  allows students to intuitively understand hierarchical structures and 
  relationships. This makes it easier to learn complex data structure 
  concepts.
- **Learning Recursion, Collection Operations, and Lazy Evaluation**: 
  Students can use the Virtual Storage Library to practically learn 
  important programming concepts such as designing recursive algorithms, 
  manipulating collection data, and lazy evaluation techniques. This 
  helps them acquire skills ranging from the basics to the advanced 
  level of programming.

Thus, the Virtual Storage Library helps students and learners deepen 
their understanding of practical programming skills and data structures 
in an educational setting.

[[▲](#table-of-contents)]
## Technology Stack

### Overview
`VirtualStorageLibrary` is developed on the .NET 8 platform using the C# language.  
This library **is not a file system**.  
It operates entirely in-memory and provides a flexible foundation for managing hierarchical data structures.  

### Programming Language
- **C#**: The primary language used in this project. C# version 12 is utilized.

### Frameworks and Libraries
- **.NET 8**: The foundation framework of the project, enabling high-performance applications.  
  It also supports multiple platforms that are compatible with .NET 8.

### Tools and Services
- **Visual Studio 2022**: The main development environment for this project.  
  For more details, visit [Visual Studio's official website](https://visualstudio.microsoft.com/vs/).

- **MSTest**: The testing framework used in the project.  
  For more details, visit [MSTest](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-intro).

- **GitHub**: The platform used for managing development resources.  
  For more details, visit [GitHub Documentation](https://docs.github.com/).

- **DocFX**: A powerful tool for generating documentation.  
  For more details, visit the [DocFX repository](https://dotnet.github.io/docfx/).

- **DocFX Material**: Provides stylesheets and templates for DocFX, enhancing the appearance of the documentation.  
  For more details, visit the [DocFX Material repository](https://github.com/ovasquez/docfx-material).

[[▲](#table-of-contents)]
## Target Users

### Primary User Groups
The Virtual Storage Library is designed for a broad range of users, including:

- **Developers**: Software developers using .NET and C#, especially those interested in managing tree-structured data. This library serves as a tool to easily handle complex data structures.
- **Data Scientists**: Professionals engaged in analyzing and modeling complex data structures, where efficient management and analysis of tree-structured data are required.
- **Educators and Students**: Those interested in programming education or learning about data structures. It can be used as a teaching resource to deepen practical programming skills and understanding of data structures.

### Use Cases
The Virtual Storage Library can be used for the following purposes:

- **Data Management and Analysis**: A tool for efficiently managing and analyzing tree-structured data, enabling hierarchical organization and search capabilities.
- **Data Organization and Structuring**: Organizing large amounts of data hierarchically and structuring it for better visualization and understanding of data relationships.
- **Education and Learning**: A resource for learning the basic concepts of programming and data structures, including recursive programming, collection manipulation, and lazy evaluation.

### Target Industries
The Virtual Storage Library is particularly useful in the following industries:

- **IT**: In software development and data science, where efficient data management and analysis are essential for project success.
- **Education**: In programming education and learning, where it serves as a tool for educators and students to acquire practical skills.

## Development Status and Future Plans
As of 2024/08/09, all essential features for version 1.0.0 have been implemented.  
However, some bug fixes, around 30 feature improvements, and refactoring tasks remain.  
With version 0.8.0, we aim to gather user feedback, including bug reports and feature enhancement suggestions.  
Simultaneously, we plan to work through the remaining tasks for version 0.9.0, targeted for release in October 2024.  
During this period, class names, method names, property names, and other elements in the library may change, merge, or be deprecated without notice.  
Details will be provided in the release notes, so please check them.  
For more information, please refer to [Current Issues and Improvement Plans](https://github.com/users/AkiraNetwork/projects/3/views/3) (Japanese).

[[▲](#table-of-contents)]
## Installation Instructions

### Installing with Visual Studio 2022
#### **Using the NuGet Package Manager:**
   - Right-click on your project in the Solution Explorer in Visual Studio 2022 and select "Manage NuGet Packages."
   - In the "Browse" tab, search for `AkiraNetwork.VirtualStorageLibrary`, select it, and install.

#### **Using the Package Manager Console:**
   - In Visual Studio 2022, go to "Tools" > "NuGet Package Manager" > "Package Manager Console."
   - Enter the following command in the console to install the package:
```powershell
Install-Package AkiraNetwork.VirtualStorageLibrary
```

### Installing with .NET CLI
- Navigate to the directory containing your project file (`.csproj`) via the command line.
- Ensure that Visual Studio 2022 is not running.
- Run the following command to install `VirtualStorageLibrary`, which will be automatically added to your project file:
```bash
dotnet add package AkiraNetwork.VirtualStorageLibrary
```

### Verifying the Installation
Once installed, `VirtualStorageLibrary` will be added to your project's dependencies, and you can begin using it.  
After installation, add the necessary `using` directives to reference the library in your code.

[[▲](#table-of-contents)]
## Usage

### Simple Example
```csharp
using AkiraNetwork.VirtualStorageLibrary;

namespace VSLSample01
{
    // User-defined class
    public class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; } = 0;
    }

    internal class Program
    {
        static void Main()
        {
            // Initialize the VirtualStorageSettings
            VirtualStorageSettings.Initialize();

            // Create a VirtualStorage instance
            VirtualStorage<Person> vs = new();

            // Create a Person object
            Person person1 = new() { Name = "John", Age = 20 };

            // Create a VirtualItem instance
            VirtualItem<Person> item1 = new("item1", person1);

            // Add the directory and item to the VirtualStorage instance
            vs.AddDirectory("/home1");
            vs.AddItem("/home1", item1);

            // Retrieve the Person object from the VirtualStorage instance
            Person result = vs.GetItem("/home1/item1").ItemData!;

            // Display the retrieved Person object
            Console.WriteLine($"Name: {result.Name}, Age: {result.Age}");
        }
    }
}
```

### Setting up `using`
```csharp
using AkiraNetwork.VirtualStorageLibrary;
```
This directive is needed to reference the VirtualStorageLibrary namespace.
For most features, this is sufficient, but depending on the functionality you want to use,
you may also need to include the following namespaces:
```csharp
using AkiraNetwork.VirtualStorageLibrary.Utilities;
using AkiraNetwork.VirtualStorageLibrary.WildcardMatchers;
```

### Library Initialization
```csharp
VirtualStorageSettings.Initialize();
```
This initializes VirtualStorageLibrary. It should be called once at the start of your application code.
This initialization configures various settings, such as characters used in 
paths (delimiters, root, dot, dot-dot),
prohibited characters for paths and node names, wildcard matchers, node list display conditions,
prefixes for node name generation, etc.
Without this initialization, subsequent operations may not function correctly.

### Defining a User-Defined Class
```csharp
public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; } = 0;
}
```
This defines the user-defined class to be used with VirtualStorageLibrary.
VirtualStorageLibrary is a generic collection that can manage instances of user-defined classes 
in a tree structure.
Therefore, in your application, you need to define the class you want to manage.
In this simple example, a Person class with name and age properties is defined.

### Creating an Instance of the `VirtualStorage` Class
```csharp
VirtualStorage<Person> vs = new();
```
This creates an instance of the VirtualStorage class.
Upon creation, only the root directory exists.

### Creating an Instance of the User-Defined Class
```csharp
Person person1 = new() { Name = "John", Age = 20 };
```
This creates an instance of the user-defined class.

### Creating an Instance of the `VirtualItem` Class
```csharp
VirtualItem<Person> item1 = new("item1", person1);
```
This creates an instance of the VirtualItem class.
The first parameter of the constructor specifies the node name, and the second parameter specifies 
the instance of the user-defined class.

### Adding a Directory
```csharp
vs.AddDirectory("/home1");
```
This adds a directory named "home1" to the root directory.
To add subdirectories at once, specify the second parameter (createSubdirectories) as true.
The default value of createSubdirectories is false.
```csharp
vs.AddDirectory("/home1/subdir1/subdir2", true);
```

### Adding an Item
```csharp
vs.AddItem("/home1", item1);
```
This adds an instance of the VirtualItem class to the "/home1" directory.
The node name for this item will be the name specified when the VirtualItem instance was created.
As a result, a node named "/home1/item1" will be created.
If a node with the same name already exists in the same directory, an exception will be thrown.
However, if the third parameter (overwrite) is set to true, the existing item will be overwritten.
The default value of overwrite is false.
```csharp
vs.AddItem("/home1", item1, true);
```

### Retrieving an Item
```csharp
Person result = vs.GetItem("/home1/item1").ItemData!;
```
The GetItem() method retrieves an instance of VirtualItem corresponding to the specified path.
The ItemData property of VirtualItem exposes the instance of the user-defined class.
As a result, the result will be set to the Person class instance stored at "/home1/item1".

### Displaying the `Person`
```csharp
Console.WriteLine($"Name: {result.Name}, Age: {result.Age}");
```
This displays the retrieved Person.
The result will be displayed as follows:
```
Name: John, Age: 20
```

[[▲](#table-of-contents)]
## Documentation
For detailed usage instructions and reference information on this library, please refer to the following 
documentation:

- [Introduction](https://AkiraNetwork.github.io/VirtualStorageLibrary/introduction.html)  
  Provides an overview of the library and its design philosophy. It introduces the primary purpose, basic 
  features, and characteristics, serving as an introductory guide for new users.

- [Getting Started](https://AkiraNetwork.github.io/VirtualStorageLibrary/getting-started.html)  
  A step-by-step guide to start using the library, including installation, initial setup, and basic sample 
  code.

- [API Reference](https://AkiraNetwork.github.io/VirtualStorageLibrary/api/AkiraNetwork.VirtualStorageLibrary)  
  Detailed information on all classes, methods, and properties included in the library, helping users 
  understand the specific usage of each member.

- Tutorials (Coming Soon)  
  Planned to provide detailed examples based on real-world use cases, guiding users in the advanced usage 
  of the library.

[[▲](#table-of-contents)]
## Configuration and Customization
The initial settings of this library are automatically configured by calling the 
`VirtualStorageSettings.Initialize()` method. This applies all default settings such as path delimiters, 
root directory names, and prohibited characters. While manual configuration is not required, you can modify 
settings during application runtime through the `VirtualStorageState.State` property. 
For more details, refer to the [API Reference](https://akiranetwork.github.io/VirtualStorageLibrary/).

[[▲](#table-of-contents)]
## License
This project is licensed under the 
[GNU Lesser General Public License v3.0 or later (LGPL-3.0-or-later)](https://www.gnu.org/licenses/lgpl-3.0.html).

Copyright (C) 2024 Akira Shimodate

VirtualStorageLibrary is free software. This software is distributed under the terms of the GNU Lesser General 
Public License, version 3, or (at your option) any later version. VirtualStorageLibrary is distributed in 
the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
A copy of the GNU Lesser General Public License is saved in the LICENSE file at the root of the repository. If 
not provided, you can check it [here](https://www.gnu.org/licenses/lgpl-3.0.html).

### Future Commercial License
We are considering offering a commercial license for VirtualStorageLibrary in the future. For more details or to inquire about the commercial license, please contact us at [akiranetwork211@gmail.com](mailto:akiranetwork211@gmail.com).

[[▲](#table-of-contents)]
## Contribution Guidelines

Thank you for your interest in contributing to `VirtualStorageLibrary`. As it is currently in the pre-release stage, we encourage you to try it out. Your feedback and suggestions are invaluable in helping us improve the library.

Here are some ways you can contribute:

- **Feedback**: If you have any opinions or suggestions about the functionality, please let us know. Your insights help shape the future of the project.
- **Bug Reports**: If you find any bugs, please report them on our [Issues](https://github.com/AkiraNetwork/VirtualStorageLibrary/issues) page.

For technical questions and feature suggestions, please use our [Discussions](https://github.com/AkiraNetwork/VirtualStorageLibrary/discussions) forum. Choose the appropriate category:

- `Q&A`: Ask for help from the community.
- `Ideas`: Suggest new features or improvements. Adopted ideas will be added as Issues by the maintainer.
- `General`: Engage in casual discussions about anything related to VirtualStorageLibrary.
- `Show and tell`: Share and showcase what you've created using VirtualStorageLibrary.

We are currently not accepting pull requests as the multi-person development system is not yet in place. We look forward to your feedback and suggestions.

**We welcome your feedback and look forward to hearing from you!**

[[▲](#table-of-contents)]
## Author and Acknowledgments

### Author
This project was developed by Akira Shimodate. It started as a personal project to realize the idea 
of a virtual storage library, with Akira responsible for the design and implementation of 
`VirtualStorageLibrary`.

### Acknowledgments
This project heavily relies on the following tools and resources, and we are deeply grateful to their 
contributors.

- [DocFX](https://github.com/dotnet/docfx):
  A powerful tool that supports the generation of project documentation.
  
- [DocFX Material](https://github.com/ovasquez/docfx-material):
  A beautiful Material design theme for DocFX.
