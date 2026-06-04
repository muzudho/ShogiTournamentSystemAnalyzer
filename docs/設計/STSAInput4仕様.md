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

- `AnalysisFlowSteps=QualityEvaluation`
- `AnalysisFlowSteps=Simulation` のうち、既存 converter が対応している `TournamentFramework` / `Empty`

`AnalysisFlowSteps=Simulation,QualityEvaluation` は形式として予約していますが、まだ要求ファイルからの自動変換には対応していません。理由は、既存実装が要求ファイルを直接モデル化せず、対話入力の回答列へ変換しているためです。複数ステップを1ファイルで実行するには、ステップ別入力セクションの仕様が必要です。

## 互換性

`STSAInput/2` と `STSAInput/3` は読み取り互換として残します。

- `AnalysisFlowMode=Simulation` は `AnalysisFlowSteps=Simulation` と同等に扱います。
- `AnalysisFlowMode=QualityEvaluation` は `AnalysisFlowSteps=QualityEvaluation` と同等に扱います。

保存される新しい要求ファイルは `STSAInput/4` を基本とします。

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