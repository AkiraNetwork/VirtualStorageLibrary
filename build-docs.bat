@echo off
setlocal

REM �v���W�F�N�g�̃r���h��XML�h�L�������g�̐���
cd AkiraNetwork\VirtualStorageLibrary
dotnet build VirtualStorageLibrary.csproj /p:DocumentationFile=bin\Debug\net8.0\VirtualStorageLibrary.xml

REM DocFX�ɂ��h�L�������g����
cd ..\..\documents
docfx docfx.json

cd ..

REM �I�����b�Z�[�W
echo �h�L�������g�������������܂����B
endlocal
