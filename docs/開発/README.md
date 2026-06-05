# 開発

このフォルダーは、ShogiTournamentSystemAnalyzer の**開発向けメモ**をまとめた場所です。  
利用者向けの使い方や仕様説明は [docs/設計](../設計/README.md)、このフォルダーは実装予定・改修方針・調査メモを扱います。

## ファイル一覧
- [Codex_apply_patch回避メモ_20260605.md](./Codex_apply_patch回避メモ_20260605.md)
  - `apply_patch` がワークスペース外判定で失敗したときの PowerShell 回避策
- [step63-flow-sequentialization-plan.md](./step63-flow-sequentialization-plan.md)
  - 分析フロー直列化、STSAInput/4、代表要求ファイルモダナイズの実装計画
- [SmokeTest事前調査メモ_20260603.md](./SmokeTest事前調査メモ_20260603.md)
  - スモークテスト、フリーズ原因調査、タイムアウト対策、古い入力ファイル整理の記録
- [ConsoleSetIn副作用除去実装計画_20260602.md](./ConsoleSetIn副作用除去実装計画_20260602.md)
  - `Console.SetIn()` / `Console.In` 退避復元を除去した作業メモ
- [Codexフリーズ復旧メモ_20260531.md](./Codexフリーズ復旧メモ_20260531.md)
  - Codex フリーズ後の復旧メモ
- [rename-to-ShogiTournamentSystemAnalyzer.txt](./rename-to-ShogiTournamentSystemAnalyzer.txt)
  - ShogiTournamentSystemAnalyzer への改名計画
- [品質評価モード実装計画.txt](./品質評価モード実装計画.txt)
  - 品質評価モード実装時の計画メモ
- [実装タスク一覧_本戦専用モードと品質評価モード.txt](./実装タスク一覧_本戦専用モードと品質評価モード.txt)
  - 本戦専用モードと品質評価モードの実装タスク一覧
- [本戦専用モード実装計画.txt](./本戦専用モード実装計画.txt)
  - 本戦専用モード実装時の計画メモ

## 関連入口
- [docs フォルダー案内](../README.md)
- [トップ README](../../README.md)
- [docs 運用ルール](../運用/docs運用ルール.md)
