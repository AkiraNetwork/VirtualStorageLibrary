## Update README.md - Under Construction

#### This Readme is a work in progress. Once it's completed, an English version will be provided.
---
![Version](https://img.shields.io/badge/version-0.8.0-pink.svg)
![License](<https://img.shields.io/badge/license-MIT-green.svg>)
![Platform](https://img.shields.io/badge/platform-.NET%208-blue)
[![Documentation](https://img.shields.io/badge/docs-online-purple.svg)](https://shimodateakira.github.io/VirtualStorageLibrary/)
![Maintenance](https://img.shields.io/badge/maintenance-active-blue.svg)

# VirtualStorageLibraryへようこそ！

## プロジェクトの概要と目的
VirtualStorageLibraryは、完全にオンメモリで動作し、ツリー構造コレクションを提供する.NETライブラリです。
このライブラリは、**データの階層的な構造を管理するための基盤**を提供し、 ユーザー定義型<T>を内包するアイテム、ディレクトリ、シンボリックリンクをサポートします。
このライブラリは **ファイルシステムではありません。** 
従来のファイルシステムの概念を参考にしつつ、より柔軟で使いやすいツリー構造を実現するために **ゼロから再設計** しました。
このライブラリは、ユーザーが **パスの指定による** ノードの参照、探索、操作を **直感的** に行えるようにすることを目的としています。

## 主な機能
- **柔軟なツリー構造**:  
  親子関係に基づく階層的なデータ構造を提供し、柔軟なノード管理が可能です。
- **多様なノードのサポート**:  
  ユーザー定義型\<T\>を含むアイテム、ディレクトリ、シンボリックリンクをサポートします。
- **パスによる直感的なノード操作**:  
  パスを指定することでノードの参照、探索、操作が容易に行え、使いやすいAPIを提供します。
- **リンク管理**:  
  リンク辞書を使ったシンボリックリンクの変更を管理し、ターゲットパスの変更を追跡します。
- **循環参照防止**:  
  シンボリックリンクを含んだパスを解決時、ディレクトリを循環参照するような構造を検出した場合、例外をスローします。
- **柔軟なノードリストの取得**:  
  ディレクトリ内のノードのリストを取得する際、指定されたノードタイプでフィルタ、グルーピングし、指定された属性でソートした結果を取得します。

## 使用事例
 - 自然言語処理(NLP)
 - ナレッジベースシステム
 - ゲーム開発
 - 検索エンジン
 - 
