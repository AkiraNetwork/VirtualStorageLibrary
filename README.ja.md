
![burner.png](https://raw.githubusercontent.com/shimodateakira/VirtualStorageLibrary/master/docs/images/burner.png)

<details>
  <summary>Language: Japanese</summary>
  <ul>
    <li><a href="README.md">English</a></li>
    <li><a href="README.ja.md">Japanese</a></li>
  </ul>
</details>

![Version: 0.8.0](https://img.shields.io/badge/version-0.8.0-pink.svg)
[![License: LGPL-3.0-or-later](https://img.shields.io/badge/License-LGPL%20v3.0%20or%20later-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0.html)
[![Platform: .NET 8](https://img.shields.io/badge/platform-.NET%208-green)](https://dotnet.microsoft.com/en-us/download/dotnet)
[![Documentation: online](https://img.shields.io/badge/docs-online-purple.svg)](https://shimodateakira.github.io/VirtualStorageLibrary/api/AkiraNetwork.VirtualStorageLibrary.html)
[![Maintenance: active](https://img.shields.io/badge/maintenance-active-blue.svg)](https://github.com/users/shimodateakira/projects/3)

# VirtualStorageLibraryへようこそ！

## プロジェクトの概要と目的
`VirtualStorageLibrary`は、完全にオンメモリで動作し、**ツリー構造コレクション**を提供する.NETライブラリです。
このライブラリは、**データの階層的な構造を管理するための基盤**を提供し、 ユーザー定義型<T>を内包するアイテム、ディレクトリ、シンボリックリンクをサポートします。
このライブラリは **ファイルシステムではありません。** 
従来のファイルシステムの概念を参考にしつつ、より柔軟で使いやすいツリー構造を実現するために **ゼロから再設計** しました。
このライブラリは、ユーザーが **パスの指定による** ノードの参照、探索、操作を **直感的** に行えるようにすることを目的としています。

![VirtualStorageLibraryLogo](https://raw.githubusercontent.com/shimodateakira/VirtualStorageLibrary/master/tree_256x256.svg)

## プロジェクトの背景
.NETが備えているコレクションは線形コレクションです。コレクションはハッシュセット型、配列型、リスト型、辞書型など様々ありますが、本質的には線の構造となっています。
一方、一般のファイルシステムは木形コレクションと捉える事ができます。要素はノードで管理され、階層構造になっています。
この様な木形コレクションをサポートするライブラリは既存でいくつか存在しますが、ファイルシステムのようなモデルのライブラリを見つけることができませんでした。
そこで私はファイルシステムを論理的に解釈し、**純粋なオブジェクトとして扱える木形のコレクションとして実装できないか**と考えアイディアをまとめました。
階層構造のデータを柔軟に管理し、直感的にアクセスできる仕組みを作ろうと考えたのです。

## 目次

- [主な機能](#主な機能)
- [想定されるユースケース](#想定されるユースケース)
- [技術スタック](#技術スタック)
- [ターゲットユーザー](#ターゲットユーザー)
- [インストール方法](#インストール方法)
- [使用方法](#使用方法)
- [ドキュメント](#ドキュメント)
- [設定とカスタマイズ](#設定とカスタマイズ)
- [ライセンス](#ライセンス)
- [貢献ガイドライン](#貢献ガイドライン)
- [著者と謝辞](#著者と謝辞)

[[▲](#目次)]
## 主な機能

#### 柔軟なツリー構造
  親子関係に基づく階層的なデータ構造を提供し、柔軟なノード管理が可能です。

#### 多様なノードのサポート
  ユーザー定義型\<T\>を含むアイテム、ディレクトリ、シンボリックリンクをサポートします。

#### パスによる直感的なノード操作
  パスを指定することでノードの参照、探索、追加、削除、変名、コピーおよび、移動が容易に行え、使いやすいAPIを提供します。

#### リンク管理
  リンク辞書を使ったシンボリックリンクの変更を管理し、ターゲットパスの変更を追跡します。

#### 循環参照防止
  シンボリックリンクを含んだパスを解決時、ディレクトリを循環参照するような構造を検出した場合、例外をスローします。

#### 柔軟なノードリストの取得
  ディレクトリ内のノードのリストを取得する際、指定されたノードタイプでフィルタ、グルーピングし、指定された属性でソートした結果を取得します。

[[▲](#目次)]
## 想定されるユースケース

### 自然言語処理(NLP)
自然言語処理の分野では、テキストデータの解析や文書の構文解析に**ツリー構造**が頻繁に利用されます。
例えば、文章の構文解析結果を**構文木**として表現し、文の各要素の関係性を視覚化します。
このような**構造化データの管理**にはツリー構造が非常に有効です。

仮想ストレージライブラリは、こうした**ツリー構造データの管理**や**パスを使ったノードアクセス**をサポートし、
**効率的なデータ解析**を可能にします。具体的には、以下のようなシナリオで役立ちます：

- **構文木の管理**: 文法解析結果の構文木をノードとして保存し、ノード間の関係を階層的に管理します。
- **エンティティリンクの管理**: テキスト内のエンティティ（人名、地名など）の関係をツリー構造で表現し、迅速な検索とアクセスをサポートします。
- **トピックモデルの可視化**: トピック間の階層的な関係をモデル化し、複数のトピックやサブトピックを効率的に表示します。

これにより、NLPタスクにおいて**複雑なデータ構造の管理**が容易になり、**解析効率が向上**します。

### ナレッジベースシステム
ナレッジベースシステムでは、大量の文書や情報を**階層的に整理**し、**効率的な検索性**を提供することが求められます。
仮想ストレージライブラリは、こうした情報の**階層構造を管理**し、ユーザーが**必要な情報に迅速にアクセス**できるようにします。

具体的なシナリオとしては、以下のようなものがあります：
- **技術文書の管理**: 製品の技術文書やマニュアルをカテゴリごとに分類し、ユーザーが特定の情報を素早く見つけられるようにします。
- **FAQシステム**: よくある質問とその回答を階層的に整理し、検索性を高め、ユーザーが簡単に回答を見つけられるようにします。
- **知識ベースの構築**: 組織内の専門知識を文書化し、ツリー構造で管理することで、新しいメンバーが素早く学習できる環境を提供します。

これにより、ナレッジベースシステムは**情報の整理とアクセスの効率化**を実現し、**情報の利活用を最大化**します。

### ゲーム開発
ゲーム開発において、ゲーム内の**オブジェクトやシーンの管理**は重要です。
特に、オブジェクトの**階層関係の管理**が必要な場合、仮想ストレージライブラリは、シーンの**ダイナミックな変更**をサポートし、
**開発プロセスの効率化**に寄与します。

具体的なシナリオとしては、以下のようなものがあります：
- **シーン管理**: ゲーム内の異なるレベルやエリアのオブジェクトを階層的に管理し、プレイヤーの行動やイベントに応じて動的に変更します。
- **キャラクターの装備管理**: キャラクターが装備するアイテムや武器をツリー構造で管理し、リアルタイムでの装備変更を容易に行います。
- **レベルデザインの効率化**: レベルデザイナーがシーンやオブジェクトの階層を視覚的に管理し、配置や変更を迅速に行えるようにします。

これにより、ゲーム開発プロセスは**柔軟性とスピード**を持ち、**クリエイティブな要素**を最大限に活かすことができます。

### 階層型クラスタリング
データの**階層的なグループ化や分類**を行う階層型クラスタリングでは、クラスタリング結果を**ツリー構造で管理**し、
**データの分析と可視化**をサポートします。仮想ストレージライブラリは、このような分析プロセスを支援します。

具体的なシナリオとしては、以下のようなものがあります：
- **顧客セグメンテーション**: 顧客データを購買行動やデモグラフィック情報に基づいて階層的に分類し、マーケティング戦略の最適化を図ります。
- **生物分類学**: 種や属、科などの生物の分類情報をツリー構造で管理し、各レベルの分類における特性や関係を可視化します。
- **階層型データ分析**: 大規模データセットを階層的にグループ化し、データのパターンやトレンドを発見するための探索的データ解析を行います。

これにより、階層型クラスタリングの結果を**効率的に管理**し、**視覚的な洞察**を得ることが可能になります。仮想ストレージライブラリは、
このような**データの可視化と分析のプロセス**を支援し、データドリブンな意思決定をサポートします。

### 教育と学習
仮想ストレージライブラリのソースコードは、教育と学習の分野で活用できます。
特に、プログラミング教育やデータ構造の理解を深めるために効果的です。

具体的なシナリオとしては、以下のようなものがあります：

- **プログラミング教育**:
  学生がツリー構造データの管理や操作方法を学ぶための実習教材として使用できます。
  具体的な課題を通じて、仮想のディレクトリ構造やデータツリーを操作することで、データ構造やアルゴリズムの理解を深めます。

- **データ構造の可視化**:
  データ構造を視覚的に表現することで、学生はデータの階層構造や関係性を直感的に理解できます。
  これにより、複雑なデータ構造の概念をより容易に学ぶことが可能です。

- **再帰プログラミング、コレクションの操作、遅延評価の学習**:
  学生は仮想ストレージライブラリを使用して、再帰的なアルゴリズムの設計、コレクションデータの操作、遅延評価のテクニックなど、
  重要なプログラミングの概念を実践的に学ぶことができます。
  これにより、プログラミングの基礎から応用までのスキルを身につけることができます。

これにより、仮想ストレージライブラリは教育現場において、学生や学習者が実践的なプログラミングスキルやデータ構造の理解を深める手助けをします。

[[▲](#目次)]
## 技術スタック

### 概要
`VirtualStorageLibrary`は、.NET 8プラットフォーム上で開発され、C#言語を使用しています。  
このライブラリは**ファイルシステムではありません**。  
完全にオンメモリで動作し、データの階層的な構造を管理するための柔軟な基盤を提供します。  

### プログラミング言語
- **C#**: プロジェクトの主要な開発言語です。C#のバージョンは12を使用しています。

### フレームワークとライブラリ
- **.NET 8**: プロジェクトの基盤となるフレームワークで、高性能なアプリケーションを構築します。  
              また、.NET 8をサポートする複数のプラットフォームで動作する事が可能です。

### ツールとサービス
- **Visual Studio 2022**: プロジェクトの主要な開発環境です。  
  詳細は、[Visual Studioの公式サイト](https://visualstudio.microsoft.com/vs/)で確認できます。

- **MSTest**: プロジェクトで使用しているテストフレームワークです。  
  詳細は、[MSTset](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-intro)で確認できます。

- **GitHub**: 開発リソースを管理するプラットフォームです。  
  詳細は、[GitHub ドキュメント](https://docs.github.com/)で確認できます。

- **DocFX**: ドキュメントを生成する高機能なツールです。  
  詳細は、[DocFXのリポジトリ](https://dotnet.github.io/docfx/)で確認できます。

- **DocFX Material**: DocFX用のスタイルシートとテンプレートを提供し、ドキュメントの見栄えを向上させるために使用しています。  
  詳細は、[DocFX Materialのリポジトリ](https://github.com/ovasquez/docfx-material)で確認できます。

[[▲](#目次)]
## ターゲットユーザー

### 主なユーザー層
仮想ストレージライブラリは、幅広いユーザー層を対象としています。以下は、主なユーザー層の例です：

- **開発者**: .NETやC#を使用しているソフトウェア開発者。特にツリー構造データの管理に関心がある方が対象です。このライブラリは、複雑なデータ構造を簡単に扱うためのツールとして利用できます。
- **データサイエンティスト**: 複雑なデータ構造の解析やモデリングに従事する専門家。特に、ツリー構造データの効率的な管理や分析が求められる状況で役立ちます。
- **教育関係者と学生**: プログラミング教育やデータ構造の学習に興味がある方。実践的なプログラミングスキルやデータ構造の理解を深めるための教材として使用できます。

### 使用目的
仮想ストレージライブラリは、以下のような目的で使用されます：

- **データの管理と解析**: ツリー構造データを効率的に管理し、解析するためのツールとして。データの階層的な組織化や検索が可能です。
- **データの組織化と整理**: 大量のデータを階層的に整理し、視覚的に理解しやすい構造にするため。データの関係性を明確にするのに役立ちます。
- **教育と学習**: プログラミングやデータ構造の基本概念を学ぶ教材として。再帰プログラミング、コレクションの操作、遅延評価などの概念を学ぶことができます。

### 対象とする業界
仮想ストレージライブラリは、特に以下の業界での利用を想定しています：

- **IT**: ソフトウェア開発やデータサイエンス分野での利用。効率的なデータ管理と解析が求められるプロジェクトで役立ちます。
- **教育**: プログラミング教育や学習の分野。教育関係者や学生が実践的なスキルを習得するためのツールとして使用できます。

## 開発状況と今後の予定
現在 (2024/08/09) 、V1.0.0で実装すべき必要な機能は全て実装済みです。  
しかし、数件のバグ修正と、30件近い機能改善、リファクタリングが残っている状況です。  
V0.8.0では、この状態でユーザーの皆様に試用して頂き、フィードバックを頂きたいと考えています。  
フィードバックには、バグ報告や機能改善の提案などが含まれます。  
それと同時に、V0.9.0に向けて残作業を消化していく予定です。  
V0.9.0のリリースは2024年10月を予定しています。  
なお、この期間中、ライブラリで提供している機能のクラス名、メソッド名、プロパティ名等は予告なく変更、統合、廃止する事があります。
その場合、リリースノートに詳細を掲載するのでご確認ください。
詳細は、[現在の問題点と改善案](https://github.com/users/shimodateakira/projects/3/views/3)を参照してください (日本語)。

[[▲](#目次)]
## インストール方法

### Visual Studio 2022 でのインストール
#### **NuGet パッケージマネージャーからインストール**する方法:
   - Visual Studio 2022 のソリューションエクスプローラーで、プロジェクトを右クリックし、「NuGet パッケージの管理」を選択します。
   - 「参照」タブで `AkiraNetwork.VirtualStorageLibrary` を検索し、選択してインストールします。
#### **パッケージマネージャーコンソールからインストール**する方法:
   - Visual Studio 2022 のメニューから「ツール」>「NuGet パッケージマネージャー」>「パッケージマネージャーコンソール」を選択します。
   - パッケージマネージャーコンソールで以下のコマンドを入力し、インストールします。
```powershell
Install-Package AkiraNetwork.VirtualStorageLibrary
```

### .NET CLIを使用したインストール
- コマンドラインで、プロジェクトファイル (`.csproj`) があるディレクトリに移動します。
- Visual Studio 2022が起動していないことを確認してください。
- 以下のコマンドを入力して、`VirtualStorageLibrary` をインストールします。  
この方法でプロジェクトファイルに自動的にパッケージが追加されます。
```bash
dotnet add package AkiraNetwork.VirtualStorageLibrary
```

### インストールの確認
インストールが成功すると、プロジェクトの依存関係に `VirtualStorageLibrary` が追加され、使用できるようになります。  
インストール後、必要に応じて `using` ディレクティブを追加してライブラリを参照してください。

[[▲](#目次)]
## 使用方法

### 簡単なサンプル
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

### usingの設定
```csharp
using AkiraNetwork.VirtualStorageLibrary;
```
`VirtualStorageLibrary`を参照する為の名前空間の指定です。  
ほとんどの機能はこれで十分ですが、使用する機能によってはオプションで以下の名前空間を指定する必要があります。
```csharp
using AkiraNetwork.VirtualStorageLibrary.Utilities;
using AkiraNetwork.VirtualStorageLibrary.WildcardMatchers;
```

### ライブラリの初期化
```csharp
VirtualStorageSettings.Initialize();
```
`VirtualStorageLibrary`を初期化します。アプリーションコードで最初に一度だけ呼び出してください。  
この初期化では、パスで使われる文字(区切り文字、ルート、ドット、ドットドット)、パスやノード名の禁止文字、
マイルドカードマッチャー、ノードリスト表示条件、ノード名生成時のprefix等を初期設定しています。  
この初期化を行わないと、後続の操作が正しく動作しない可能性があります。

### ユーザー定義クラスの定義
```csharp
public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; } = 0;
}
```
`VirtualStorageLibrary`で使用するユーザー定義クラスを定義します。  
`VirtualStorageLibrary`は、ユーザー定義クラスのインスタンスをツリー構造で管理する事のできるジェネリック型のコレクションです。  
その為、ユーザーアプリケーションでは管理したいユーザー定義クラスを定義しなければなりません。  
この簡単なサンプルでは名前と年齢を管理する簡単な`Person`クラスを定義しています。  

### VirtualStorageクラスのインスタンスの作成
```csharp
VirtualStorage<Person> vs = new();
```
`VirtualStorage`クラスのインスタンスを作成します。  
作成直後は、ルートディレクトリのみが存在しています。

### ユーザー定義クラスのインスタンスの作成
```csharp
Person person1 = new() { Name = "John", Age = 20 };
```
ユーザー定義クラスのインスタンスを作成します。

### VirtualItemクラスのインスタンスの作成
```csharp
VirtualItem<Person> item1 = new("item1", person1);
```
`VirtualItem`クラスのインスタンスを作成します。
コンストラクタの第一パラメータにはノード名を指定し、第二パラメータにはユーザー定義クラスのインスタンスを指定します。

### ディレクトリの追加
```csharp
vs.AddDirectory("/home1");
```
ルートディレクトリに"home1"というディレクトリを追加します。
以下のようにサブディレクトリを一度に追加する場合は、第二パラメータ(`createSubdirectories`)をtrueで指定します。
`createSubdirectories`のデフォルト値はfalseです。
```csharp
vs.AddDirectory("/home1/subdir1/subdir2", true);
```

### アイテムの追加
```csharp
vs.AddItem("/home1", item1);
```
`VirtualItem`クラスのインスタンスを"/home1"ディレクトリに追加します。
この時、このアイテムのノード名は、`VirtualItem`クラスのインスタンスを作成した時のノード名になります。
結果として"/home1/item1"という名前のノードが作成されます。
同じディレクトリに同じ名前のノード名が既に存在している場合は例外が発生します。
ただし、以下のように第三パラメータ(`overwrite`)をtrueで指定した場合は上書きする事ができます。
`overwrite`のデフォルト値はfalseです。
```csharp
vs.AddItem("/home1", item1, true);
```

### アイテムの取得
```csharp
Person result = vs.GetItem("/home1/item1").ItemData!;
```
`GetItem()`メソッドはパラメータで指定したパスに対する`VirtualItem`のインスタンスを取得します。
`VirtualItem`の`ItemData`プロパティは、ユーザー定義クラスのインスタンスを公開しています。
その為、resultには"/home1/item1"に格納された`Person`クラスのインスタンスが設定されます。

### Personの表示
```csharp
Console.WriteLine($"Name: {result.Name}, Age: {result.Age}");
```
取得した`Person`を表示します。
結果として、以下のように表示されます。
```
Name: John, Age: 20
```

[[▲](#目次)]
## ドキュメント
このライブラリの詳細な使用方法やリファレンスについては、以下のドキュメントを参照してください。

- [Introduction](https://shimodateakira.github.io/VirtualStorageLibrary/introduction.html)  
  ライブラリの概要と設計思想を説明しています。  
  どのような目的で開発されたのか、その基本的な機能と特徴を紹介します。  
  新しいユーザーがライブラリの全体像を把握するための入門ガイドです。  

- [Getting Started](https://shimodateakira.github.io/VirtualStorageLibrary/getting-started.html)  
  ライブラリを使い始めるためのステップバイステップガイドです。  
  インストール方法から初期設定、簡単なサンプルコードまで、ライブラリを導入するために必要な基本的な手順を説明します。  

- [APIリファレンス](https://shimodateakira.github.io/VirtualStorageLibrary/api/AkiraNetwork.VirtualStorageLibrary)  
  ライブラリに含まれる全てのクラス、メソッド、およびプロパティについての詳細な情報を提供しています。  
  各メンバーの使用方法やパラメータについての説明が含まれており、ライブラリの具体的な使い方を確認するのに役立ちます。  

- チュートリアル (執筆予定)  
  実際のユースケースに基づいた詳細な使用例を提供し、ライブラリの応用的な使い方を学ぶためのガイドです。今後追加予定です。  

[[▲](#目次)]
## 設定とカスタマイズ
このライブラリの初期設定は、`VirtualStorageSettings.Initialize()`メソッドを呼び出すことで自動的に行われます。  
これにより、パス区切り文字やルートディレクトリの名前、禁止文字など、全てのデフォルト設定が適用されます。  
特に手動で設定を行う必要はありませんが、アプリケーションの動作中に設定を変更したい場合は、
`VirtualStorageState.State`プロパティを通じて各種設定プロパティを変更することが可能です。  
詳細については、[APIリファレンス](https://shimodateakira.github.io/VirtualStorageLibrary/)をご参照ください。  

[[▲](#目次)]
## ライセンス
このプロジェクトは、[GNU Lesser General Public License v3.0 or later (LGPL-3.0-or-later)](https://www.gnu.org/licenses/lgpl-3.0.html)のもとでライセンスされています。

© 2024 Akira Shimodate

VirtualStorageLibraryはフリーソフトウェアです。このソフトウェアは、GNU Lesser General Public Licenseのバージョン3、または（オプションとして）その後のバージョンの条件の下で配布されています。VirtualStorageLibraryは有用であることを願って配布されていますが、いかなる保証も提供されていません。商業的な価値の適合性や特定の目的への適合性についての黙示的な保証も含まれていません。詳細については、GNU Lesser General Public Licenseをご覧ください。このソフトウェアと共にGNU Lesser General Public LicenseのコピーがリポジトリのルートにLICENSEというファイル名で保存されています。提供されていない場合は、[こちら](https://www.gnu.org/licenses/lgpl-3.0.html)でご確認ください。

### 将来の商用ライセンス
今後、VirtualStorageLibraryの商用ライセンスを導入する予定があります。商用ライセンスの詳細や導入時期については、[akiranetwork211@gmail.com](mailto:akiranetwork211@gmail.com)までお問い合わせください。

---

## docfx

このドキュメントはDocFXを使用しており、© 2019 Oscar Vasquezによって作成されています。  
このソフトウェアはMITライセンスのもとでライセンスされています。詳細については、[docfx GitHub リポジトリ](https://github.com/dotnet/docfx)をご覧ください。

---

## DocFX Material

このドキュメントはDocFX Materialテーマを使用しており、© 2019 Oscar Vásquezによって作成されています。  
このテーマはMITライセンスのもとでライセンスされています。詳細については、[DocFX Material GitHub リポジトリ](https://github.com/ovasquez/docfx-material)をご覧ください。

[[▲](#目次)]
## 貢献ガイドライン
まずは、とにかくこの`VirtualStorageLibrary`を使ってみてください。  
現在はプレリリースの段階です。実際に使ってみることで多くのフィードバックや改善点が見つかることを期待しています。  

- **フィードバック**: 使用感や機能についてのご意見があればお知らせください。  
- **バグ報告**: 発見したバグがあればご報告ください。  
- **機能の改善、追加の要望**: 必要な機能の改善、追加の要望があればお知らせください。  

これらは当プロジェクトの[Issue](https://github.com/shimodateakira/VirtualStorageLibrary/issues)にて受け付けております。  

- **技術的質問**: 技術的な質問があれば[StackOverflow](https://stackoverflow.com/)に書き込みをしてみてください。  
                  タグは「c#」、「.net」、「tree」、「shared-libraries」、「generic-collections」というタグのいずれかの組み合わせで指定して頂けると見つけやすいです。  

現在、複数人による開発体制を整えてない為、プルリクエストは当分の間、受け付けておりません。  
ご理解のほど、よろしくお願い致します。  

[[▲](#目次)]
## 著者と謝辞

### 著者
このプロジェクトは、Akira Shimodateによって開発されています。
個人的なプロジェクトとしてスタートし、仮想ストレージライブラリのアイデアを実現するために作成されました。
`VirtualStorageLibrary`の設計および実装を担当。

### 謝辞
このプロジェクトは、以下のツールとリソースに大きく依存しています。
これらの貢献者に深く感謝いたします。

- [DocFX](https://github.com/dotnet/docfx):
  プロジェクトのドキュメント生成を支援する強力なツールです。
  
- [DocFX Material](https://github.com/ovasquez/docfx-material):
  DocFX用の美しいMaterialデザインテーマを提供してくれました。
