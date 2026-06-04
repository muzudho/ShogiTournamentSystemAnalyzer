# プログラム4パート構成 第63ステップ実行計画

## 目的

分析フローを「シミュレーションか品質評価の単一選択」から、「シミュレーションをするか」「品質評価をするか」を順番に選ぶ直列フローへ変更する。

あわせて、要求ファイル仕様を単一の `AnalysisFlowMode` から直列ステップを表せる形式へ更新し、既存の代表要求ファイルを新仕様へモダナイズする。

## 方針

- 新仕様は `STSAInput/4` とする。
- `STSAInput/4` の `Meta` では `AnalysisFlowSteps=Simulation,QualityEvaluation` を使う。
- `STSAInput/2` と `STSAInput/3` は互換入力として残し、既存の `AnalysisFlowMode` を読み取れるようにする。
- コンソール入力では、まず「シミュレーションをしますか？」、次に「品質評価をしますか？」を聞く。
- どちらも「いいえ」の場合は中断する。
- ルール選択は当面、既存の `RuleProfileMode` を使う。
- `QualityEvaluation` が含まれる場合、`RuleProfileMode.TournamentFramework` と `RuleProfileMode.Empty` は未対応として弾く。

## 主な変更対象

- `Domain/TournamentQualityEvaluator/TournamentQualityEvaluatorEnums.cs`
  - 直列フロー用の値オブジェクトまたは enum 群を追加する。
- `Domain/Request/RequestBoundary.cs`
  - 単一 `AnalysisFlowMode` ではなく、分析フロー選択を保持できる形へ変更する。
- `Presentation/ConsoleCustom/ConsolePrompts.cs`
  - `ReadAnalysisFlowMode()` を直列質問へ置き換える。
  - はい/いいえ入力の共通処理を追加する。
- `Presentation/ConsoleCustom/ProgramConsoleGuide.cs`
  - 選択済み主線表示を複数ステップ対応にする。
- `Program.cs`
  - 分析フロー選択、ルール選択、ガイド表示の接続を更新する。
- `Application/Analysis/AnalysisFlowDispatcher.cs`
  - 選択された分析ステップを順番に実行する。
- `Application/Analysis/AnalysisWorkflow.cs`
  - 新しい分析フロー選択を Dispatcher へ渡す。
- `Application/RequestFileCheck/StsaInputLegacyConverter.cs`
  - `STSAInput/4` と `AnalysisFlowSteps` を読めるようにする。
  - `STSAInput/2` / `STSAInput/3` は既存仕様のまま受ける。
- `Application/RequestFileCheck/StsaInputValueParser.cs`
  - `AnalysisFlowSteps` のパースを追加する。
- `Application/RequestFileCheck/RequestInputFormatDetector.cs`
  - `STSAInput/4` を判定する。
- `Infrastructure/DataFiles/Request/TournamentRule/RequestInputLogTemplate.sbn.txt`
  - 保存される要求ファイルを `STSAInput/4` へ更新する。
- `docs/設計`
  - `STSAInput/4` 仕様を追加し、README から参照できるようにする。
- `Inputs`
  - 代表的な `STSAInput/3` 要求ファイルを `STSAInput/4` へ更新する。

## 互換性

- 既存の `STSAInput/2` / `STSAInput/3` は読み取り互換を維持する。
- 既存仕様の `AnalysisFlowMode=QualityEvaluation` は、内部では `AnalysisFlowSteps=QualityEvaluation` と同等に扱う。
- 既存仕様の `AnalysisFlowMode=Simulation` は、内部では `AnalysisFlowSteps=Simulation` と同等に扱う。
- 保存される新しい要求ファイルは `STSAInput/4` を基本とする。

## 検証

- `dotnet build` を実行する。
- 代表 smoke:
  - `Inputs/Smoke/quality_input_[先手8x後手8]_[Neutral_Single10]_[STSAInput4_smoke].txt`
  - `Inputs/Smoke/quality_input_[トップ集団大きめ]_[FinalStage_Single10]_[STSAInput4_smoke].txt`
- モダナイズ後は同じファイルまたは後継ファイルで `STSAInput/4` の読み取りを確認する。

## 中断時の再開ポイント

1. この計画書を読む。
2. `docs/続きはここから.md` の最新メモを見る。
3. `git status --short` で変更済みファイルを確認する。
4. `dotnet build` が通るところまで戻す。