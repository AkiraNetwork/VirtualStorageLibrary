## Update getting-staretd.md - Under Construction

#### This Readme is a work in progress. Once it's completed, an English version will be provided.

---# Getting Started

# 主な機能

- ツリー構造 VirtualStorageクラス
 
  VirtualStorageは一般のファイルシステムと同じくツリー構造のコレクションです。
  このツリー構造はノードの階層構造で構成され、トップのノードがルートとなります。
  VirtualStorageLibraryを初期化後、VirtualStorageクラスのインスタンスを作成するとルートのみのツリー構造が作成されます。
  
- ノード
  
  ノードは以下の3種類のノードに分類されます。
  
  - アイテム VirtualItem&lt;T&gt;クラス
    
    - ユーザー定義型 T
      
  - ディレクトリ VirtualDirectoryクラス
    
  - シンボリックリンク VirtualSymbolicLinkクラス
    
- パス
- ノードの操作
- ノードの取得
- ノードの探索
- ノードのクローン
- シンボリックリンクの解決
- ノードリスト
- インデクサー
