# Codex apply_patch 回避メモ 2026-06-05

## 症状

Codex の `apply_patch` が、ワークスペース内のファイルに対しても次のように失敗することがある。

```text
patch rejected: writing outside of the project; rejected by user approval settings
```

今回の例では、カレントディレクトリは `E:\github.com\muzudho\ShogiTournamentSystemAnalyzer`、書き込み対象も同じワークスペース配下だったが、`apply_patch` 側のプロジェクト外判定で弾かれた。

## 回避策

`apply_patch` がこの症状で失敗した場合は、PowerShell から .NET API を使って最小限の置換またはファイル書き込みを行う。

既存ファイルの単純置換例:

```powershell
$path = 'ShogiTournamentSystemAnalyzer/Application/Shared/RequestInputSession.cs'
$text = [System.IO.File]::ReadAllText((Resolve-Path $path), [System.Text.Encoding]::UTF8)
$text = $text.Replace('置換前', '置換後')
$encoding = New-Object System.Text.UTF8Encoding($false)
$text = $text -replace "`r?`n", "`r`n"
[System.IO.File]::WriteAllText((Resolve-Path $path), $text, $encoding)
```

新規または全文更新する場合も、`Set-Content` は BOM が付くことがあるため避け、`System.Text.UTF8Encoding($false)` で UTF-8 BOM なしを明示する。

## 作業後の確認

編集後は次を確認する。

```powershell
$bytes = [System.IO.File]::ReadAllBytes('対象ファイル')
$bytes[0..2] | ForEach-Object { $_.ToString('X2') }
```

先頭が `EF BB BF` なら BOM 付きなので、UTF-8 BOM なしで書き直す。CRLF を揃える場合は、書き込み前に `$text = $text -replace "`r?`n", "`r`n"` を通す。

最後に次を実行する。

```powershell
git diff --check
dotnet build .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj
```

## 注意

この回避策は、`apply_patch` がワークスペース外判定で誤って失敗したときだけ使う。通常は差分が読みやすい `apply_patch` を優先する。