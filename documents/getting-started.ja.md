---
title: "Getting Started"
---

![burner.png](images/burner.png)

<details>
  <summary>Language: Japanese</summary>
  <ul>
    <li><a href="getting-started.md">English</a></li>
    <li><a href="getting-started.ja.md">Japanese</a></li>
  </ul>
</details>

# Getting Started

VirtualStorageLibraryへようこそ！このガイドでは、ライブラリを迅速かつ効果的にセットアップし、使用を開始する方法を説明します。

## 前提条件

使用を開始する前に、以下のソフトウェアがインストールされていることを確認してください。

- .NET 8 以降

- C# 12 以降

- Visual Studio 2022 または Visual Studio Code

- 基本的なC#と.NETの知識

## インストール方法

### Visual Studio 2022 でのインストール

#### **NuGet パッケージマネージャーからインストール**する方法:

- Visual Studio 2022 のソリューションエクスプローラーで、プロジェクトを右クリックし、「NuGet パッケージの管理」を選択します。
- 「参照」タブで `VirtualStorageLibrary` を検索し、選択してインストールします。
  
  #### **パッケージマネージャーコンソールからインストール**する方法:
- Visual Studio 2022 のメニューから「ツール」>「NuGet パッケージマネージャー」>「パッケージマネージャーコンソール」を選択します。
- パッケージマネージャーコンソールで以下のコマンドを入力し、インストールします。
  
  ```powershell
  Install-Package VirtualStorageLibrary -Version 0.8.0
  ```

### .NET CLIを使用したインストール

- コマンドラインで、プロジェクトファイル (`.csproj`) があるディレクトリに移動します。
- Visual Studio 2022が起動していないことを確認してください。
- 以下のコマンドを入力して、`VirtualStorageLibrary` をインストールします。  
  この方法でプロジェクトファイルに自動的にパッケージが追加されます。
  
  ```bash
  dotnet add package VirtualStorageLibrary --version 0.8.0
  ```

### インストールの確認

インストールが成功すると、プロジェクトの依存関係に `VirtualStorageLibrary` が追加され、使用できるようになります。  
インストール後、必要に応じて `using` ディレクティブを追加してライブラリを参照してください。



![tree_256x256.svg](images/tree_256x256.svg)



## クイックスタート

まずは簡単なサンプルから始めましょう。  
VSLSample01という名前のソリューション、プロジェクトを作成して、以下のソースコードを入力して実行してください。  
このサンプルは仮想ストレージの初期化と作成、ノードの作成と取得を実行するサンプルです。

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

このコードは、以下のことを実行します。

1. 仮想ストレージを初期化します。

2. 仮想ストレージを作成します。この時、ルートディレクトリは自動的に作成された状態です。

3. ユーザー定義オブジェクト`person1`を作成します。

4. ユーザー定義オブジェクト`person1`を含むアイテム`item1`を作成します。

5. ディレクトリ`/home1`を作成します。

6. ディレクトリ`/home1`にアイテム`item1`を追加します。

7. パス`/home1/item1`からユーザー定義オブジェクト`result`を取得します。

8.  ユーザー定義オブジェクト`result`の`Name`プロパティと`Age`プロパティを表示します。

## 次のステップ

基本を理解したら、次のステップとして以下を探索してみてください。

- [APIリファレンス](xref:AkiraNetwork.VirtualStorageLibrary)

- [Introduction](introduction.md)

- チュートリアル (執筆予定)

## サポートとフィードバック

問題が発生したり、提案がある場合は、[Issue](https://github.com/shimodateakira/VirtualStorageLibrary/issues)を作成するか、[ディスカッション](https://github.com/shimodateakira/VirtualStorageLibrary/discussions)に参加してください。
