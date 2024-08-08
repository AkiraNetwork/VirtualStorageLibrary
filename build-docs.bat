@echo off
setlocal

REM プロジェクトのビルドとXMLドキュメントの生成
cd AkiraNetwork\VirtualStorageLibrary
dotnet build VirtualStorageLibrary.csproj /p:DocumentationFile=bin\Debug\net8.0\VirtualStorageLibrary.xml

REM DocFXによるドキュメント生成
cd ..\..\documents
docfx docfx.json

cd ..

REM 終了メッセージ
echo ドキュメント生成が完了しました。
endlocal
