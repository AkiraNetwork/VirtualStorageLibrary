---
title: "Getting Started"
---

![burner.png](images/burner.png)

<details>
  <summary>Language: English</summary>
  <ul>
    <li><a href="getting-started.md">English</a></li>
    <li><a href="getting-started.ja.md">Japanese</a></li>
  </ul>
</details>

# Getting Started

Welcome to the VirtualStorageLibrary! This guide will help you quickly and effectively set up and start using the library.

## Prerequisites

Before getting started, make sure you have the following software installed:

- .NET 8 or later

- C# 12 or later

- Visual Studio 2022 or Visual Studio Code

- Basic knowledge of C# and .NET

## Installation

### Installing with Visual Studio 2022

#### **Installing via the NuGet Package Manager**:

- In Visual Studio 2022, right-click on your project in the Solution Explorer and select "Manage NuGet Packages".
- In the "Browse" tab, search for `AkiraNetwork.VirtualStorageLibrary`, select it, and install it.

#### **Installing via the Package Manager Console**:
- From the menu in Visual Studio 2022, go to "Tools" > "NuGet Package Manager" > "Package Manager Console".
- In the Package Manager Console, enter the following command to install the package:

  ```powershell
  Install-Package AkiraNetwork.VirtualStorageLibrary
  ```

### Installing with .NET CLI

- Navigate to the directory containing your project file (`.csproj`) in the command line.
- Ensure that Visual Studio 2022 is not running.
- Enter the following command to install `VirtualStorageLibrary`.  
  This will automatically add the package to your project file.

  ```bash
  dotnet add package AkiraNetwork.VirtualStorageLibrary
  ```

### Verifying the Installation

Once the installation is successful, the `VirtualStorageLibrary` will be added to your project's dependencies and will be ready to use.  
After installation, make sure to add the necessary `using` directives to reference the library.

![tree_256x256.svg](images/tree_256x256.svg)

## Quick Start

Let's start with a simple example.  
Create a solution and project named `VSLSample01`, and enter the following source code to run it.  
This example demonstrates the initialization and creation of virtual storage, as well as the creation and retrieval of nodes.

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

This code performs the following steps:

1. Initializes the virtual storage.

2. Creates the virtual storage, with the root directory automatically created.

3. Creates a user-defined object `person1`.

4. Creates an item `item1` containing the user-defined object `person1`.

5. Creates the directory `/home1`.

6. Adds the item `item1` to the directory `/home1`.

7. Retrieves the user-defined object `result` from the path `/home1/item1`.

8. Displays the `Name` and `Age` properties of the user-defined object `result`.

## Next Steps

Once you've understood the basics, explore the following:

- [API Reference](xref:AkiraNetwork.VirtualStorageLibrary)

- [Introduction](introduction.md)

- Tutorial (Coming Soon)

## Support and Feedback

If you encounter any issues or have suggestions, please [create an issue](https://github.com/shimodateakira/VirtualStorageLibrary/issues) or join the [discussion](https://github.com/shimodateakira/VirtualStorageLibrary/discussions).
