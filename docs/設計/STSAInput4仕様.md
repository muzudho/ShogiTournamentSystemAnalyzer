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

`Simulation + Standard` と `Simulation + FinalStage` は、20局以下、または20局超の近似計算に対応し、`AnalysisRequest` へ直接パースして実行します。`Simulation + TournamentFramework` と `Simulation + Empty` も、要求ファイルを `AnalysisRequest` へ直接パースして実行します。`QualityEvaluation + Standard` と `QualityEvaluation + FinalStage` も、20局以下、または20局超の近似計算に対応し、`AnalysisRequest` へ直接パースして実行します。20局超で `SimulationCount` が未指定の場合は、要求ファイル実行の既定試行回数として `200000` を採用します。

`AnalysisFlowSteps=Simulation,QualityEvaluation` は、ステップ別入力セクションを使う要求ファイルからの自動実行に対応しています。複数ステップを1ファイルで実行するためのステップ別入力セクション仕様は、この文書の「複数ステップの入力セクション」で定義します。

## 複数ステップの入力セクション

複数ステップを1ファイルで実行する場合、`Meta` はフロー全体の宣言だけに使います。

```plaintext
#[Section] Meta
AnalysisFlowSteps=Simulation,QualityEvaluation
#[EndSection]
```

ステップ固有の設定は、ステップ名に `Step` 接尾辞を付けたセクションで表します。旧 `Step.*` 形式は互換入力として読み取れます。

- `SimulationStep`
- `QualityEvaluationStep`

例:

```plaintext
#[Section] SimulationStep
RuleProfileMode=Standard
TournamentRuleSetMode=Neutral
FirstPlayerWinRatePercent=51
SimulationCount=200000
#[EndSection]

#[Section] QualityEvaluationStep
RuleProfileMode=Standard
ExecutionMode=Single
TournamentRuleSetMode=Neutral
FirstPlayerWinRatePercent=51
SimulationCount=200000
QualityInnovExpectedRankOffsetMode=Off
TournamentQualityEvaluationReportGrouping=Off
#[EndSection]
```

`AnalysisFlowSteps` に含まれるステップの `*Step` セクションは必須です。複数ステップ時は、`Meta` に `RuleProfileMode` や `ExecutionMode` などのステップ固有キーを書かず、各 `*Step` セクションに書きます。

入力データは、複数ステップで同じものを使う場合は共有セクションに書けます。

```plaintext
#[Section] PlayersCsv
name,elo
Alice,1500
Bob,1480
#[EndSection]

#[Section] MatchesInput
first,second
Alice,Bob
#[EndSection]
```

ステップごとに入力データを分ける場合は、ステップ名付きセクションを使います。

- `SimulationStep.PlayersCsv`
- `SimulationStep.MatchesInput`
- `QualityEvaluationStep.PlayersCsv`
- `QualityEvaluationStep.MatchesInput`

読み取り時は、ステップ名付きセクションを優先し、なければ共有セクションを使います。

```plaintext
SimulationStep.PlayersCsv -> PlayersCsv
SimulationStep.MatchesInput -> MatchesInput
QualityEvaluationStep.PlayersCsv -> PlayersCsv
QualityEvaluationStep.MatchesInput -> MatchesInput
```

品質評価ステップの入力セクションを丸ごと省略した場合は、直前のシミュレーション request から品質評価入力を組み立てます。この省略は `Simulation,QualityEvaluation` の直列実行用で、標準品質評価は直前が `Standard` シミュレーション、本戦品質評価は直前が `FinalStage` シミュレーションの場合だけ対応します。入力セクションを一部だけ書いた場合は、省略扱いにせず通常の必須セクションとして検証します。

出力は衝突を避けるため、複数ステップ時はステップ名付きセクションを使います。

- `SimulationStep.Output`
- `QualityEvaluationStep.Output`

複数ステップ時に共有の `Output` だけが指定された場合は、どのステップの出力か曖昧になるためエラーとします。

### 複数ステップの最小例

```plaintext
#[Format] STSAInput/4

#[Section] Meta
AnalysisFlowSteps=Simulation,QualityEvaluation
#[EndSection]

#[Section] SimulationStep
RuleProfileMode=Standard
TournamentRuleSetMode=Neutral
FirstPlayerWinRatePercent=51
SimulationCount=200000
#[EndSection]

#[Section] QualityEvaluationStep
RuleProfileMode=Standard
ExecutionMode=Single
TournamentRuleSetMode=Neutral
FirstPlayerWinRatePercent=51
SimulationCount=200000
QualityInnovExpectedRankOffsetMode=Off
TournamentQualityEvaluationReportGrouping=Off
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

#[Section] SimulationStep.Output
SummaryOutputPath=Output\FinalRanking\sample_simulation_summary.csv
#[EndSection]

#[Section] QualityEvaluationStep.Output
SummaryOutputPath=Output\TournamentQualityEvaluator\TournamentQualityReport\Summary\sample_quality_summary.csv
#[EndSection]
```

実行順は `AnalysisFlowSteps` の順序に従います。現在は `Simulation,QualityEvaluation` の2ステップを対応対象とします。

## 互換性

`STSAInput/2` と `STSAInput/3` は読み取り互換として残します。

- `AnalysisFlowMode=Simulation` は `AnalysisFlowSteps=Simulation` と同等に扱います。
- `AnalysisFlowMode=QualityEvaluation` は `AnalysisFlowSteps=QualityEvaluation` と同等に扱います。

`STSAInput/4` は読み取り互換として残します。保存される新しい要求ファイルとリポジトリ内の代表 smoke は `STSAInput/5` を基本とします。

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

`SimulationCount` は対局数が 21 局以上で近似計算になる場合だけ使います。対局数が 20 局以下の場合は厳密計算になり、`AnalysisRequest` 直通ルートでは `SimulationCount` を使いません。シミュレーションと品質評価の20局超では、`SimulationCount` を指定した場合はその値を使い、未指定の場合は要求ファイル実行の既定値 `200000` を使います。
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
