# dotnet test コマンドの実行
dotnet test --collect:"XPlat Code Coverage"

# TestResults ディレクトリのパス
$testResultsDir = "/workspace/VirtualStorageLibrary/VirtualStorageLibrary.Test/TestResults"

# 最新の coverage.cobertura.xml ファイルを検索
$latestCoverageFile = Get-ChildItem -Path $testResultsDir -Recurse -Filter "coverage.cobertura.xml" |
                      Sort-Object LastWriteTime -Descending |
                      Select-Object -First 1

# reportgenerator コマンドの実行
if ($latestCoverageFile -ne $null) {
    $reportGeneratorCmd = "reportgenerator -reports:`"$($latestCoverageFile.FullName)`" -targetdir:`"coveragereport`" -reporttypes:lcov"
    Invoke-Expression $reportGeneratorCmd
} else {
    Write-Host "No coverage file found."
}
