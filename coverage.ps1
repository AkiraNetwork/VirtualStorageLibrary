# TestResults ディレクトリのパス
$testResultsDir = "/workspace/VirtualStorageLibrary/VirtualStorageLibrary.Test/TestResults"

# TestResults 内の全てのGUID名ディレクトリを削除
Get-ChildItem -Path $testResultsDir -Directory |
    Where-Object { $_.Name -match '^[{(]?[0-9A-F]{8}[-]([0-9A-F]{4}[-]){3}[0-9A-F]{12}[)}]?$' } |
    Remove-Item -Recurse -Force

# dotnet test コマンドの実行
dotnet test --collect:"XPlat Code Coverage"

# 最新の coverage.cobertura.xml ファイルを検索
$latestCoverageFile = Get-ChildItem -Path $testResultsDir -Recurse -Filter "coverage.cobertura.xml" |
                      Sort-Object LastWriteTime -Descending |
                      Select-Object -First 1

# coverage.cobertura.xml ファイルの作成日時を表示
if ($latestCoverageFile -ne $null) {
    Write-Host "Found coverage file: $($latestCoverageFile.FullName)"
    Write-Host "Creation time: $($latestCoverageFile.CreationTime)"

    # reportgenerator コマンドの実行
    $reportGeneratorCmd = "reportgenerator -reports:`"$($latestCoverageFile.FullName)`" -targetdir:`"coveragereport`" -reporttypes:lcov"
    Invoke-Expression $reportGeneratorCmd
} else {
    Write-Host "No coverage file found."
}
