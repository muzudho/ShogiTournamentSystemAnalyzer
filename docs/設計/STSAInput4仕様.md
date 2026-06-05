# STSAInput/4 仕様ドラフト

`STSAInput/4` は、分析フローを直列ステップとして指定するための要求ファイル形式です。

## 目的

`STSAInput/3` までは、`AnalysisFlowMode=Simulation` または `AnalysisFlowMode=QualityEvaluation` の単一選択でした。

`STSAInput/4` では、コンソールの質問順に合わせて、分析フローを `AnalysisFlowSteps` で表します。

```plaintext
#[Format] STSAInput/4

#[Section] Meta
AnalysisFlowSteps=QualityEvaluation
RuleProfileMode=Standard
ExecutionMode=Single
#[EndSection]
```

## AnalysisFlowSteps

`AnalysisFlowSteps` はカンマ区切りです。

- `Simulation`
- `QualityEvaluation`

例:

```plaintext
AnalysisFlowSteps=Simulation
AnalysisFlowSteps=QualityEvaluation
AnalysisFlowSteps=Simulation,QualityEvaluation
```

## 現在の対応範囲

要求ファイルからの自動実行では、現在は単一ステップだけ対応しています。

- `AnalysisFlowSteps=QualityEvaluation` の `Standard` / `FinalStage`
- `AnalysisFlowSteps=Simulation` の `Standard` / `FinalStage` / `TournamentFramework` / `Empty`

`QualityEvaluation + Standard` と `QualityEvaluation + FinalStage` は、条件がそろう場合 `AnalysisRequest` へ直接パースして実行します。20局超の品質評価は `SimulationCount` がある場合だけ直接ルートで実行し、未対応条件は従来の legacy 入力列変換へ fallback します。

`AnalysisFlowSteps=Simulation,QualityEvaluation` は形式として予約していますが、まだ要求ファイルからの自動変換には対応していません。複数ステップを1ファイルで実行するには、ステップ別入力セクションの仕様が必要です。

## 互換性

`STSAInput/2` と `STSAInput/3` は読み取り互換として残します。

- `AnalysisFlowMode=Simulation` は `AnalysisFlowSteps=Simulation` と同等に扱います。
- `AnalysisFlowMode=QualityEvaluation` は `AnalysisFlowSteps=QualityEvaluation` と同等に扱います。

保存される新しい要求ファイルは `STSAInput/4` を基本とします。

## シミュレーションの最小例

```plaintext
#[Format] STSAInput/4

#[Section] Meta
AnalysisFlowSteps=Simulation
RuleProfileMode=Standard
TournamentRuleSetMode=Neutral
FirstPlayerWinRatePercent=51
SimulationCount=10
#[EndSection]

#[Section] PlayersCsv
name,elo
Alice,1500
Bob,1480
#[EndSection]

#[Section] MatchesInput
first,second
Alice,Bob
#[EndSection]

#[Section] Output
SummaryOutputPath=Output\FinalRanking\sample_simulation_summary.csv
#[EndSection]
```

`SimulationCount` は対局数が 21 局以上で近似計算になる場合だけ使います。対局数が 20 局以下の場合は厳密計算になり、要求ファイル変換では `SimulationCount` を対話入力行へ流しません。
## 品質評価の最小例

```plaintext
#[Format] STSAInput/4

#[Section] Meta
AnalysisFlowSteps=QualityEvaluation
RuleProfileMode=Standard
ExecutionMode=Single
TournamentRuleSetMode=Neutral
FirstPlayerWinRatePercent=51
SimulationCount=10
QualityInnovExpectedRankOffsetMode=Off
TournamentQualityEvaluationReportGrouping=Off
#[EndSection]

#[Section] PlayersCsv
name,elo
Alice,1500
Bob,1480
#[EndSection]

#[Section] MatchesInput
1,Alice-Bob
#[EndSection]

#[Section] Output
SummaryOutputPath=Output\TournamentQualityEvaluator\TournamentQualityReport\Summary\sample_quality_summary.csv
#[EndSection]
```

## 関連

- [入力ファイル仕様](./入力ファイル仕様.md)
- [STSAInput/3 仕様](./STSAInput3仕様.md)