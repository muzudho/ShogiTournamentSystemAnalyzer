# STSAInput/3 仕様ドラフト

この文書は、標準ルール / 本戦ルールを **1本の専用スクリプト形式** で記述できるようにするための **`STSAInput/3`** 仕様ドラフトです。  
`STSAInput/2` を置き換える最終版ではなく、まずは **品質評価フローの統一入力** を主対象にした足場として整理します。

## 目的

`STSAInput/3` の目的は次の通りです。

- 標準ルール / 本戦ルールを同じ section 構造で書けるようにする
- 共通入力と本戦専用入力を、**必須 / 省略可** で表現できるようにする
- 対話入力の順番に依存しにくい、安定した入力スクリプトにする
- 将来は品質評価だけでなく、対局シミュレーション側にも拡張しやすくする

## 基本方針

`STSAInput/3` では、`STSAInput/2` と同じく `#[Format]` と `#[Section]` を使います。  
ただし、section 名と key 名は **標準ルール / 本戦ルールの共通化** を優先して整理します。

```plaintext
#[Format] STSAInput/3

#[Section] Meta
AnalysisFlowMode=QualityEvaluation
RuleProfileMode=Standard
ExecutionMode=Single
TournamentRuleSetMode=Neutral
FirstPlayerWinRatePercent=51
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
```

## 基本構文

### フォーマット宣言
```plaintext
#[Format] STSAInput/3
```

### セクション開始
```plaintext
#[Section] Meta
```

### セクション終端
```plaintext
#[EndSection]
```

### コメント
- `#` で始まる行はコメントとして扱います
- `#[Format]` / `#[Section]` / `#[EndSection]` は制御タグです

## section 一覧

### 共通 section
- `Meta`
- `PlayersCsv`
- `MatchesInput`
- `ReferenceMatchesInput`（省略可）
- `Output`

### 本戦ルール向け optional section
- `GroupMapCsv`
- `AdditionalApexPlayersCsv`（省略可）

## Meta セクションの推奨キー

### 共通
- `AnalysisFlowMode`
  - 例: `QualityEvaluation`
- `RuleProfileMode`
  - 例: `Standard` / `FinalStage`
- `ExecutionMode`
  - 例: `Single` / `Sweep`
- `FirstPlayerWinRatePercent`
  - `ExecutionMode=Single` のとき使用
- `SimulationCount`
  - 省略可
- `TournamentRuleSetMode`
  - 標準ルールでは必須
  - 本戦ルールでは将来の拡張に備えて optional
- `SweepStartPercent`
  - `ExecutionMode=Sweep` のとき必須
- `SweepEndPercent`
  - `ExecutionMode=Sweep` のとき必須
- `SweepStepPercent`
  - `ExecutionMode=Sweep` のとき必須

### 本戦ルール向け
- `AdditionalApexPlacementMode`
- `BoundaryRescueMode`
- `VariableTop8Mode`
- `QualityInnovExpectedRankOffsetMode`

### 出力分類
- `TournamentQualityEvaluationReportGrouping`
  - `Off` / `On`
- `TournamentQualityEvaluationReportOutcome`
  - `Good` / `Bad`
- `EvaluationMemo`
  - 省略可
- `OutputPath`
  - 単発評価の既定出力先
- `SummaryOutputPath`
  - 単発評価の summary 出力先
- `SweepOutputPath`
  - sweep の出力先

## section ごとの意味

### PlayersCsv
選手 / Player 一覧です。

```plaintext
#[Section] PlayersCsv
name,elo
Alice,1500
Bob,1480
#[EndSection]
```

### MatchesInput
対局入力です。単純な CSV でも、`Round / First-Second / PlayerSymbols` でも構いません。

### ReferenceMatchesInput
参考対局です。品質評価に含めず、説明表示や補助情報として使います。

### GroupMapCsv
本戦ルールで `Apex / Innov` の対応を表します。

```plaintext
#[Section] GroupMapCsv
group,name
Apex,Alice
Innov,Bob
#[EndSection]
```

### AdditionalApexPlayersCsv
本戦不出場 Apex 一覧です。省略可能です。

## 標準ルールの例

```plaintext
#[Format] STSAInput/3

#[Section] Meta
AnalysisFlowMode=QualityEvaluation
RuleProfileMode=Standard
TournamentRuleSetMode=Neutral
ExecutionMode=Single
FirstPlayerWinRatePercent=51
SimulationCount=20000
TournamentQualityEvaluationReportGrouping=On
TournamentQualityEvaluationReportOutcome=Good
EvaluationMemo=Neutral の基準計測
SummaryOutputPath=Output\TournamentQualityEvaluator\TournamentQualityReport\Summary\Good\quality_summary_[neutral].csv
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
SummaryOutputPath=Output\TournamentQualityEvaluator\TournamentQualityReport\Summary\Good\quality_summary_[neutral].csv
#[EndSection]
```

## 本戦ルールの例

```plaintext
#[Format] STSAInput/3

#[Section] Meta
AnalysisFlowMode=QualityEvaluation
RuleProfileMode=FinalStage
ExecutionMode=Sweep
SweepStartPercent=50
SweepEndPercent=55
SweepStepPercent=1
AdditionalApexPlacementMode=Off
BoundaryRescueMode=On
VariableTop8Mode=Off
QualityInnovExpectedRankOffsetMode=On
TournamentQualityEvaluationReportGrouping=On
TournamentQualityEvaluationReportOutcome=Bad
EvaluationMemo=本戦ルール案A
SweepOutputPath=Output\TournamentQualityEvaluator\TournamentQualityReport\Sweeps\Bad\quality_sweep_[final_stage_a].csv
#[EndSection]

#[Section] PlayersCsv
name,elo
Alice,1500
Bob,1480
Carol,1470
#[EndSection]

#[Section] GroupMapCsv
group,name
Apex,Alice
Innov,Bob
Innov,Carol
#[EndSection]

#[Section] AdditionalApexPlayersCsv
name,elo
Zed,1600
#[EndSection]

#[Section] MatchesInput
first,second
Alice,Bob
Bob,Carol
#[EndSection]

#[Section] ReferenceMatchesInput
first,second
Alice,Carol
#[EndSection]

#[Section] Output
SweepOutputPath=Output\TournamentQualityEvaluator\TournamentQualityReport\Sweeps\Bad\quality_sweep_[final_stage_a].csv
#[EndSection]
```

## 必須 / 省略可の整理

| 項目 | Standard | FinalStage |
|---|---|---|
| `PlayersCsv` | 必須 | 必須 |
| `MatchesInput` | 必須 | 必須 |
| `ReferenceMatchesInput` | 省略可 | 省略可 |
| `TournamentRuleSetMode` | 必須 | 省略可 |
| `GroupMapCsv` | 不要 | 必須 |
| `AdditionalApexPlayersCsv` | 不要 | 省略可 |
| `AdditionalApexPlacementMode` | 不要 | 必須 |
| `BoundaryRescueMode` | 不要 | 必須 |
| `VariableTop8Mode` | 不要 | 必須 |
| `QualityInnovExpectedRankOffsetMode` | 不要 | 必須 |

## 現段階の実装方針

まずは品質評価フローで、`STSAInput/3` を受け付ける変換入口を追加します。  
内部では当面、既存の legacy 入力列へ変換して流す想定です。

そのため、実装初期段階では:
- `AnalysisFlowMode=QualityEvaluation` を優先対応
- `RuleProfileMode=Standard` と `RuleProfileMode=FinalStage` を同じ構文で受ける
- 対局シミュレーション側の完全統一は次段階で対応

## 関連資料
- [入力ファイル仕様](./入力ファイル仕様.md)
- [STSAInput/2 仕様](./STSAInput2仕様.md)
- [品質評価](./品質評価.md)

