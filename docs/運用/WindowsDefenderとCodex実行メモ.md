# Windows Defender と Codex 実行メモ

このメモは、Codex 作業中の検証コマンドが Windows Defender に不要に怪しまれないようにするための運用ルールです。

## 基本方針

- PowerShell からアプリ DLL を `Assembly.LoadFrom` で読み込み、reflection で private / internal メソッドを呼ぶ検証は使わない。
- PowerShell で長い inline script を組み立てて、ファイル生成や DLL 実行をまとめて行わない。
- 検証用の入口が必要なときは、アプリ本体の通常のコマンドラインオプション、または .NET のテストプロジェクトとして実装する。
- Codex がファイルを作るときは、原則として `apply_patch` で差分を残す。検証用 `.ps1` をその場で生成して実行する運用は避ける。

## 今回の原因

2026-06-06 に Windows Defender が記録した検出は、PowerShell から次のような処理をまとめて実行したことが原因候補です。

- `ShogiTournamentSystemAnalyzer/bin/Debug/net10.0/ShogiTournamentSystemAnalyzer.dll` を読み込む。
- `StsaInput4RequestParser.TryParse` と `StsaInput4RequestWriter.BuildLines` を reflection で呼ぶ。
- `Output/SmokeGenerated` に round-trip 検証用ファイルを書き出す。
- `tools/smoke-request-writer-roundtrip.ps1` を生成する。

この処理はアプリの検証目的でも、Defender から見ると「PowerShell が DLL を読み込み、非公開メソッドを呼び、ファイルを書き出す」挙動になり、警戒されやすい。

## 代替手順

要求ファイル writer の round-trip smoke は、PowerShell reflection ではなくアプリの通常入口から実行する。

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --smoke-request-writer-roundtrip .\Inputs\Smoke\quality_input_[先手8x後手8]_[Neutral_Single10]_[STSAInput4_smoke].request.txt .\Inputs\Smoke\analysis_input_[先手8x後手8]_[Simulation_QualityEvaluation_STSAInput4_smoke].request.txt
```

出力先を変えるときは `--output-dir` を使う。

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --smoke-request-writer-roundtrip --output-dir .\Output\SmokeGenerated .\Inputs\Smoke\quality_input_[先手8x後手8]_[Neutral_Single10]_[STSAInput4_smoke].request.txt
```

## 新しい検証を追加するとき

- 1回限りでも、PowerShell で DLL の中身を reflection 実行する形にしない。
- internal な処理を検証したい場合は、通常のコマンドラインオプションを追加するか、テストプロジェクトから参照する。
- 大量ログを Codex チャットに流さない。`rg` や `Select-Object -First` で対象を絞る。
- Defender の通知が出たら、`Get-MpThreatDetection` で `InitialDetectionTime` と `Resources` を確認し、検知対象がソース、ビルド成果物、コマンドラインのどれかを切り分ける。