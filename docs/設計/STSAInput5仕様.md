# STSAInput/5 仕様

`STSAInput/5` は、`RuleProfileMode=Standard` / `FinalStage` のような大分類ラベルではなく、`RuleProfileAttributes` セクションでルールプロファイルの属性を指定する要求ファイル形式です。

基本のセクション構造、`AnalysisFlowSteps`、入力 CSV、出力指定は [STSAInput/4 仕様](./STSAInput4仕様.md) と同じです。`STSAInput/5` では `RuleProfileMode` を書かず、単一ステップなら `RuleProfileAttributes`、複数ステップなら `*Step.RuleProfileAttributes` を書きます。

## 目的

`STSAInput/4` では、実行したいルールのまとまりを `RuleProfileMode` で指定します。

```plaintext
RuleProfileMode=Standard
RuleProfileMode=FinalStage
```

`STSAInput/5` では、この互換ラベルを入力ファイルから外し、ルールの性質を属性で表します。

```plaintext
#[Section] RuleProfileAttributes
SimulationShape=ScheduledMatches
UsesFinalStageGrouping=Off
UsesAdditionalApexPlacement=Off
UsesBoundaryRescue=Off
UsesVariableTop8=Off
RankingRuleSetMode=Neutral
HasReferenceMatches=Off
PairingSource=ScheduledMatches
#[EndSection]
```

`STSAInput/5` の parser は、読み取った属性を `RuleProfileAttributes` としてステップ要求の判定へ直接渡します。計算ロジック、入力 CSV、出力 CSV / Markdown の内容は `STSAInput/4` と同じです。

## RuleProfileAttributes

`RuleProfileAttributes` はすべて必須キーです。

| キー | 値 | 意味 |
| --- | --- | --- |
| `SimulationShape` | `ScheduledMatches` / `FinalStageGrouped` / `TournamentFramework` / `Empty` | シミュレーションや品質評価が前提にする大会形状 |
| `UsesFinalStageGrouping` | `Off` / `On` | Apex / Innov の本戦グループを使うか |
| `UsesAdditionalApexPlacement` | `Off` / `On` | 本戦不出場 Apex の追加順位付けを使うか |
| `UsesBoundaryRescue` | `Off` / `On` | 境界救済戦を使うか |
| `UsesVariableTop8` | `Off` / `On` | 可変定員 8 ルールを使うか |
| `RankingRuleSetMode` | `Neutral` / `Twill` / `TwillCommonOpponentWeighted` | 最終順位付けのルールセット |
| `HasReferenceMatches` | `Off` / `On` | 比較用対局表を持つ前提か |
| `PairingSource` | `None` / `ScheduledMatches` / `TournamentFramework` | 対局表の供給元 |

`Off` / `On` は、それぞれ `1` / `2` でも入力できます。`SimulationShape` と `PairingSource` も parser は番号入力を受け付けますが、要求ファイルでは名前で書くほうが読みやすいです。

属性の組み合わせは、parser が次の規則で検証します。

- `SimulationShape=ScheduledMatches` では `PairingSource=ScheduledMatches` を指定します。
- `UsesFinalStageGrouping=Off` の標準形式では、`UsesAdditionalApexPlacement` / `UsesBoundaryRescue` / `UsesVariableTop8` は `Off` にします。
- `SimulationShape=FinalStageGrouped` では `UsesFinalStageGrouping=On` と `PairingSource=ScheduledMatches` を指定します。
- `SimulationShape=TournamentFramework` では `PairingSource=TournamentFramework` を指定し、本戦用属性と `HasReferenceMatches` は `Off` にします。
- `SimulationShape=Empty` では `PairingSource=None` を指定し、本戦用属性と `HasReferenceMatches` は `Off` にします。

## 代表的な属性

標準形式の対局表を使う場合:

```plaintext
#[Section] RuleProfileAttributes
SimulationShape=ScheduledMatches
UsesFinalStageGrouping=Off
UsesAdditionalApexPlacement=Off
UsesBoundaryRescue=Off
UsesVariableTop8=Off
RankingRuleSetMode=Neutral
HasReferenceMatches=Off
PairingSource=ScheduledMatches
#[EndSection]
```

本戦形式の Apex / Innov グループを使う場合:

```plaintext
#[Section] RuleProfileAttributes
SimulationShape=ScheduledMatches
UsesFinalStageGrouping=On
UsesAdditionalApexPlacement=On
UsesBoundaryRescue=On
UsesVariableTop8=On
RankingRuleSetMode=Neutral
HasReferenceMatches=Off
PairingSource=ScheduledMatches
#[EndSection]
```

大会進行フレームワークを使う場合:

```plaintext
#[Section] RuleProfileAttributes
SimulationShape=TournamentFramework
UsesFinalStageGrouping=Off
UsesAdditionalApexPlacement=Off
UsesBoundaryRescue=Off
UsesVariableTop8=Off
RankingRuleSetMode=Neutral
HasReferenceMatches=Off
PairingSource=TournamentFramework
#[EndSection]
```

空ルールの場合:

```plaintext
#[Section] RuleProfileAttributes
SimulationShape=Empty
UsesFinalStageGrouping=Off
UsesAdditionalApexPlacement=Off
UsesBoundaryRescue=Off
UsesVariableTop8=Off
RankingRuleSetMode=Neutral
HasReferenceMatches=Off
PairingSource=None
#[EndSection]
```

## 単一ステップ

単一ステップでは、`Meta` に `AnalysisFlowSteps` とステップ固有キーを書き、`RuleProfileAttributes` セクションを1つ置きます。

```plaintext
#[Format] STSAInput/5

#[Section] Meta
AnalysisFlowSteps=QualityEvaluation
TournamentRuleSetMode=Neutral
ExecutionMode=Single
FirstPlayerWinRatePercent=51
SimulationCount=10
#[EndSection]

#[Section] RuleProfileAttributes
SimulationShape=ScheduledMatches
UsesFinalStageGrouping=Off
UsesAdditionalApexPlacement=Off
UsesBoundaryRescue=Off
UsesVariableTop8=Off
RankingRuleSetMode=Neutral
HasReferenceMatches=Off
PairingSource=ScheduledMatches
#[EndSection]
```

この後に `PlayersCsv`、`MatchesInput`、必要に応じて `ReferenceMatchesInput`、`Output` を続けます。各セクションの詳細は [STSAInput/4 仕様](./STSAInput4仕様.md) を参照してください。

## 複数ステップ

複数ステップでは、`Meta` は実行順だけを書きます。ステップごとの設定は `SimulationStep` / `QualityEvaluationStep`、属性は `SimulationStep.RuleProfileAttributes` / `QualityEvaluationStep.RuleProfileAttributes` に分けます。旧 `Step.Simulation` / `Step.QualityEvaluation` 形式は互換入力として読み取れますが、writer は新形式を出力します。

```plaintext
#[Format] STSAInput/5

#[Section] Meta
AnalysisFlowSteps=Simulation,QualityEvaluation
#[EndSection]

#[Section] SimulationStep
TournamentRuleSetMode=Neutral
FirstPlayerWinRatePercent=51
SimulationCount=10
#[EndSection]

#[Section] SimulationStep.RuleProfileAttributes
SimulationShape=ScheduledMatches
UsesFinalStageGrouping=Off
UsesAdditionalApexPlacement=Off
UsesBoundaryRescue=Off
UsesVariableTop8=Off
RankingRuleSetMode=Neutral
HasReferenceMatches=Off
PairingSource=ScheduledMatches
#[EndSection]

#[Section] QualityEvaluationStep
TournamentRuleSetMode=Neutral
ExecutionMode=Single
FirstPlayerWinRatePercent=51
SimulationCount=10
QualityInnovExpectedRankOffsetMode=Off
TournamentQualityEvaluationReportGrouping=Off
#[EndSection]

#[Section] QualityEvaluationStep.RuleProfileAttributes
SimulationShape=ScheduledMatches
UsesFinalStageGrouping=Off
UsesAdditionalApexPlacement=Off
UsesBoundaryRescue=Off
UsesVariableTop8=Off
RankingRuleSetMode=Neutral
HasReferenceMatches=Off
PairingSource=ScheduledMatches
#[EndSection]
```

入力データは共有セクションを使えます。出力は `SimulationStep.Output` / `QualityEvaluationStep.Output` に分けます。複数ステップ時に共有 `Output` だけを書くと、どのステップの出力先か曖昧になるためエラーになります。

## 現在の制約

- `STSAInput/5` は要求ファイルの直通 parser 専用です。直通 parser に乗らない場合、legacy 入力へ fallback せず明示エラーになります。
- 手入力の要求ファイル保存、writer smoke の既定出力、品質評価の依頼ログ出力は `STSAInput/5` です。既存互換用の `STSAInput/4` writer も明示指定用に維持しています。
- `STSAInput/5` の属性は、互換 `RuleProfileMode` へ正規化せず parser 内部で直接持ち回ります。
- `QualityEvaluation` は `ScheduledMatches` の標準形式または本戦形式に対応します。`TournamentFramework` / `Empty` の品質評価は未対応です。

## 関連

- [入力ファイル仕様](./入力ファイル仕様.md)
- [STSAInput/4 仕様](./STSAInput4仕様.md)
- [大会進行フレームワークガイド](./大会進行フレームワークガイド.md)
