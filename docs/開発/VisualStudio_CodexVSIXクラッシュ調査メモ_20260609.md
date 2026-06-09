# Visual Studio / Codex VSIX クラッシュ調査メモ 2026-06-09

## 結論

Visual Studio が終了した件は、Codex が `devenv` を終了したのではなく、Visual Studio 2026 の Codex VSIX 拡張がチャット Markdown 表示中に未処理例外を出し、Visual Studio 本体ごとクラッシュした可能性が高い。

## イベントログで確認したこと

- 対象プロセス: `devenv.exe`
- Visual Studio バージョン: `18.6.11822.322`
- 直近クラッシュ時刻: `2026-06-09 23:41:31`
- 直後の再起動確認: `2026-06-09 23:41:48`
- Windows Error Reporting イベント: `CLR20r3`
- 例外: `System.ArgumentException`
- Application Error 側の例外コード: `0xe0434352`
- Faulting module: `KERNELBASE.dll`

## 重要なスタックトレース

`.NET Runtime` イベントの先頭付近に以下が出ていた。

```text
System.IO.Path.CheckInvalidPathChars
System.IO.Path.IsPathRooted
CodexVsix.UI.MarkdownRenderer.TryResolveWorkspaceFileReference
CodexVsix.UI.MarkdownRenderer.CreateInlineCode
CodexVsix.UI.MarkdownRenderer.ParseInlines
CodexVsix.UI.MarkdownRenderer.CreateParagraph
CodexVsix.UI.MarkdownRenderer.CreateDocument
CodexVsix.UI.ChatMarkdownViewer.RenderDocument
```

解釈:

- Codex チャットの Markdown 表示処理が、インラインコード文字列をワークスペース内ファイル参照として解決しようとした。
- その文字列に Windows パスとして不正な文字が含まれていたため、`System.ArgumentException` が発生した。
- 例外が VSIX 内で処理されず、`devenv.exe` が落ちた。

## 再発履歴

同種の `.NET Runtime` / `devenv.exe` / `CodexVsix.UI.MarkdownRenderer` クラッシュが複数回確認された。

- `2026-06-08 21:42:03`
- `2026-06-09 00:20:16`
- `2026-06-09 23:41:31`

WER レポートは以下のような場所に保存されていた。

```text
C:\ProgramData\Microsoft\Windows\WER\ReportArchive\AppCrash_devenv.exe_...
```

## 別件のアプリ側クラッシュ

`2026-06-09 23:08:52` に `ShogiTournamentSystemAnalyzer.exe` の未処理例外も出ていたが、これは Visual Studio 終了とは別件。

内容:

```text
品質評価総合点の重み合計 60001 が満点 100000 と一致しません。
```

発生箇所:

```text
ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator.TournamentQualityScoreCalculator.Validate
```

## 当面の運用

- Codex 側の返答では、Visual Studio 拡張がパス解決しそうな複雑なインラインコードやローカルファイルリンクをなるべく避ける。
- 巨大ログ、巨大 tool output、バッククォートを大量に含む Markdown をそのままチャットへ出さない。
- Visual Studio を終了させる `Stop-Process devenv` / `taskkill` 系コマンドは、明示要求がない限り使わない。
- 根本対応は Codex VSIX 側の `MarkdownRenderer` 修正、または拡張更新。
