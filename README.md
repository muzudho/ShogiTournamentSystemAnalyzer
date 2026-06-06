# STSA - ShogiTournamentSystemAnalyzer

将棋大会をはじめとする 2 人用対戦ゲーム大会のルールを、対局シミュレーションと品質評価で比べるための .NET コンソールアプリです。
この README は説明書ではなく、目的の文書へ進むための案内板です。

## あなたの目的は？

- 大会のルールを策定したい
  - まず動かす: [起動コマンド](#まず動かす)
  - 入力順を知る: [モード別ガイド](./docs/設計/モード別ガイド.md)
  - 入力ファイルで実行する: [入力ファイル仕様](./docs/設計/入力ファイル仕様.md)
  - 結果を読む: [CSVと出力](./docs/設計/CSVと出力.md)
  - 品質評価を見る: [品質評価](./docs/設計/品質評価.md)

- これから、このツールの開発を引き継ぎたい
  - 全体像を知る: [プロジェクトの考え方](./docs/設計/プロジェクトの考え方.md)
  - 実装の場所を探す: [実装ファイル案内](./docs/設計/実装ファイル案内.md)
  - 仕様の入口を見る: [設計 README](./docs/設計/README.md)
  - 開発メモを見る: [開発 README](./docs/開発/README.md)
  - docs の置き場を見る: [docs フォルダー案内](./docs/README.md)

- 今、このツールの開発の続きを再開したい
  - 最初に読む: [続きはここから](./docs/続きはここから.md)
  - 最新の実装計画を見る: [第73ステップ実行計画](./docs/開発/プログラム4パート構成_第73ステップ実行計画.md)
  - Codex 用スキルをコピーする: [Codex skill のコピー手順](./docs/運用/codex-skill運用.md#インストール)
  - `apply_patch` 失敗時の回避を見る: [Codex apply_patch 回避メモ](./docs/開発/Codex_apply_patch回避メモ_20260605.md)

## 3クリック以内の地図

| 目的 | ここから読む | 次に進む先 |
| --- | --- | --- |
| 手入力で動かしたい | [起動コマンド](#まず動かす) | [モード別ガイド](./docs/設計/モード別ガイド.md) |
| 要求ファイルで実行したい | [入力ファイル仕様](./docs/設計/入力ファイル仕様.md) | [STSAInput/4 仕様](./docs/設計/STSAInput4仕様.md) |
| 出力 CSV / Markdown を読みたい | [CSVと出力](./docs/設計/CSVと出力.md) | [Output/Reports](./Output/Reports/README.md) |
| 品質評価を理解したい | [品質評価](./docs/設計/品質評価.md) | [トーナメントルール](./docs/設計/トーナメントルール.md) |
| 大会進行フレームワークを使いたい | [大会進行フレームワークガイド](./docs/設計/大会進行フレームワークガイド.md) | `Data` / `Inputs` の見本 |
| 実装箇所を探したい | [実装ファイル案内](./docs/設計/実装ファイル案内.md) | [開発 README](./docs/開発/README.md) |
| 作業再開したい | [続きはここから](./docs/続きはここから.md) | [開発 README](./docs/開発/README.md) |
| docs を整理したい | [docs フォルダー案内](./docs/README.md) | [docs 運用ルール](./docs/運用/docs運用ルール.md) |
| Codex スキルを入れたい | [Codex skill のコピー手順](./docs/運用/codex-skill運用.md#インストール) | [docs-handoff skill 原本](./docs/運用/skills/docs-handoff/SKILL.md) |

## まず動かす

Visual Studio から実行するか、リポジトリルートで次を実行します。

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj
```

要求ファイルで流すときは次です。

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --request-file ".\Inputs\Smoke\quality_input_[先手8x後手8]_[Neutral_Single10]_[STSAInput4_smoke].request.txt"
```

ビルドだけ確認するときは次です。

```powershell
dotnet build .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj
```

## 主な入口

- [説明書の総合目次](./docs/設計/README.md)
- [開発メモの入口](./docs/開発/README.md)
- [docs フォルダー案内](./docs/README.md)
- [運用ルール](./docs/運用/README.md)
- [Reports の案内](./Output/Reports/README.md)

## 主な置き場

- `ShogiTournamentSystemAnalyzer`
  - アプリ本体
- `Inputs`
  - `--request-file` で流す実行用入力ファイル。要求ファイル名は `*.request.txt`
- `Data`
  - 再利用する選手、対局表、本戦補助、大会ルール
- `Output`
  - アプリが生成する CSV / Markdown / STSA ログ
- `docs/設計`
  - 利用者向け説明、入力仕様、出力仕様、設計説明
- `docs/開発`
  - 実装計画、改修メモ、調査メモ
- `docs/続きはここから.md`
  - 作業再開時の短い引継ぎメモ
- `docs/運用`
  - docs と Codex skill の運用手順

## 関連リンク

- [白黒対抗ルール案](https://note.com/muzudho/n/n311536fd1812?app_launch=false)

## ライセンス

- [MIT License](./LICENSE)
