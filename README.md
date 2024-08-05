## Update README.md - Under Construction

#### This Readme is a work in progress. Once it's completed, an English version will be provided.
---
![Version](https://img.shields.io/badge/version-0.8.0-pink.svg)
![License](<https://img.shields.io/badge/license-MIT-green.svg>)
![Platform](https://img.shields.io/badge/platform-.NET%208-blue)
[![Documentation](https://img.shields.io/badge/docs-online-purple.svg)](https://shimodateakira.github.io/VirtualStorageLibrary/)
![Maintenance](https://img.shields.io/badge/maintenance-active-blue.svg)

# VirtualStorageLibraryへようこそ！

VirtualStorageLibraryは、完全にオンメモリで動作するツリー構造のコレクションを提供する.NETのライブラリです。

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


