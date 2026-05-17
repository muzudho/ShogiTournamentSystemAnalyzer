# STSAInput/2 仕様ドラフト

この文書は、実行用入力ファイルの次世代方式として検討している **`STSAInput/2`** の仕様ドラフトです。  
現行の `#[Prompt]` / `#[Enter]` 方式をすぐ廃止するものではなく、**仕様変更に強い入力プロトコル**を目指す案として整理しています。

## 目的

`STSAInput/2` の目的は次の通りです。

- 画面に表示するプロンプト文言の変更に引きずられない
- 入力ファイルの意味を、UI 文言ではなく**安定したキー名**で表す
- 項目の追加・削除・並び替えに強くする
- CSV を含む複数行データを、人にも機械にも分かりやすく書けるようにする
- 将来のパーサー拡張に備えて、**フォーマットバージョン**を明示する

## 基本方針

`STSAInput/2` では、入力ファイルの先頭でフォーマットを宣言し、内容を **Section** 単位で記述します。

```plaintext
#[Format] STSAInput/2

#[Section] Meta
AnalysisFlowMode=QualityEvaluation
RuleProfileMode=FinalStage
FirstPlayerWinRatePercent=51
#[EndSection]

#[Section] PlayersCsv
name,elo
Alice,1500
Bob,1480
#[EndSection]
```

## 基本構文

### 1. フォーマット宣言
ファイル先頭付近に、次の行を置きます。

```plaintext
#[Format] STSAInput/2
```

- これにより、このファイルが `STSAInput/2` 仕様で書かれていることを表します
- 将来 `STSAInput/3` のような新方式が出ても共存できます

### 2. セクション開始
セクションは次の形で始めます。

```plaintext
#[Section] Meta
```

- `Meta` はセクション名です
- セクション名は ASCII の英数字と記号を基本とし、実装側で安定して扱える名前を使います

### 3. セクション終端
セクションの終わりは次で表します。

```plaintext
#[EndSection]
```

- `STSAInput/2` では、複数行の CSV や将来の拡張を考え、空行終端ではなく明示終端を使います

## コメント

将来の仕様では、`#` で始まる行をコメントとして扱う想定です。

```plaintext
# このファイルは品質評価 / 本戦ルール用
```

ただし、`#[Format]`、`#[Section]`、`#[EndSection]` のような **制御タグ** はコメントではなく構文として解釈します。

## セクションの考え方

`STSAInput/2` では、主に次の 2 種類のセクションを想定します。

### Meta セクション
1 行 1 項目の `key=value` 形式で、設定値を書きます。

```plaintext
#[Section] Meta
AnalysisFlowMode=QualityEvaluation
RuleProfileMode=FinalStage
FirstPlayerWinRatePercent=51
SimulationCount=20000
#[EndSection]
```

### CSV / 複合入力セクション
CSV 本文や、`Round / Black/White / 対局記号表` のような複合入力本文をそのまま入れるセクションです。

```plaintext
#[Section] PlayersCsv
name,elo
Alice,1500
Bob,1480
#[EndSection]
```

```plaintext
#[Section] MatchesInput
Round
 , A, B
A, -, 1
B, 1, -

Black/White
 , A, B
A, -, b
B, w, -

対局記号表
A, "Alice"
B, "Bob"
#[EndSection]
```

## 推奨セクション名

現時点のドラフトでは、次のようなセクション名を候補とします。

### 基本設定
- `Meta`

### CSV 系
- `PlayersCsv`
- `MatchesInput`
- `ReferenceMatchesInput`
- `GroupMapCsv`
- `AdditionalApexPlayersCsv`
- `Output`

### 将来拡張候補
- `Options`
- `Notes`

> [!NOTE]
> セクション名は現時点ではドラフトです。実装時に最終決定します。

## Meta セクションの推奨キー

現時点では、次のようなキーを候補とします。

### 共通
- `AnalysisFlowMode`
- `RuleProfileMode`
- `FirstPlayerWinRatePercent`
- `ExecutionMode`
- `SimulationCount`
- `OutputPath`

### 通常ルール向け
- `TournamentRuleSetMode`

### 本戦ルール向け
- `AdditionalApexPlacementMode`
- `BoundaryRescueMode`
- `VariableTop8Mode`
- `QualityInnovExpectedRankOffsetMode`

### 品質評価向け
- `BlackAdvantagePercent`
- `SweepStartPercent`
- `SweepEndPercent`
- `SweepStepPercent`

> [!NOTE]
> 数字の列挙値より、`QualityEvaluation` や `FinalStage` のような**名前付き値**を優先する想定です。  
> ただし移行期には、数値表現も互換入力として許容する案があります。

## 値の書き方

### 1 行値
`Meta` セクションでは `key=value` 形式を使います。

```plaintext
FirstPlayerWinRatePercent=51
SimulationCount=20000
```

### 論理値
On / Off 切り替えは、次のような表現を想定します。

```plaintext
BoundaryRescueMode=On
VariableTop8Mode=Off
```

### 列挙値
数値よりも意味が読み取れる名前付き値を推奨します。

```plaintext
AnalysisFlowMode=QualityEvaluation
RuleProfileMode=FinalStage
TournamentRuleSetMode=Twill
```

## 複合入力の書き方

対局入力は、単純な `black,white` CSV だけではなく、`Round / Black/White / 対局記号表` の複合表記もあります。  
そのため `STSAInput/2` では、対局入力系セクションを `MatchesCsv` のような狭い名前ではなく、`MatchesInput` / `ReferenceMatchesInput` のような広い名前で扱う案を推奨します。

```plaintext
#[Section] MatchesInput
black,white
Alice,Bob
Carol,Alice
#[EndSection]
```

または:

```plaintext
#[Section] MatchesInput
Round
 , A, B
A, -, 1
B, 1, -

Black/White
 , A, B
A, -, b
B, w, -

対局記号表
A, "Alice"
B, "Bob"
#[EndSection]
```

このセクション本文は、**そのまま既存の入力パーサーへ渡せる 1 まとまりのテキスト**として保持する想定です。

## 必須と任意の考え方

`STSAInput/2` では、すべてのファイルが同じ項目を持つとは限りません。  
そのため、項目は次の 3 種類に分けて扱う想定です。

### 必須項目
常に必要な項目です。

例:
- `AnalysisFlowMode`
- `RuleProfileMode`

### 条件付き必須項目
モードによって必要になる項目です。

例:
- `TournamentRuleSetMode`
  - 通常ルールのとき必須
- `GroupMapCsv`
  - 本戦ルールのとき必須

### 任意項目
なくても実行できる項目です。

例:
- `ReferenceMatchesInput`
- `AdditionalApexPlayersCsv`
- `Output` セクション

## 順序の扱い

`STSAInput/2` では、現行方式より**順序依存を弱くする**ことを目指します。

- `Meta` セクション内のキー順は原則自由
- セクションの出現順も、将来的には自由にできる設計を目指す
- ただし最初の実装段階では、分かりやすさのため推奨順を設けてもよい

## 未知キー・未知セクションの扱い

将来拡張をしやすくするため、未知の項目については次の扱いを推奨します。

- 未知キー
  - まずは**警告して無視**
- 未知セクション
  - まずは**警告して無視**

これにより、新しい項目が増えても古い実装が壊れにくくなります。

## エラー方針

次のようなケースは入力エラーとして扱う想定です。

- `#[Format]` がない
- `#[Format]` が `STSAInput/2` ではない
- `#[Section]` に対応する `#[EndSection]` がない
- 必須キーが不足している
- 同じ必須キーが重複している
- 必須入力セクションが不足している

## 現行方式との違い

現行の `#[Prompt]` / `#[Enter]` 方式では、**画面上の流れをそのままファイルへ写す**考え方を取っています。

これは分かりやすい反面、次の弱点があります。

- プロンプト文言が変わると古く見えやすい
- 入力順の変更に弱い
- どの値が何を表すかを UI 表示に依存しやすい

`STSAInput/2` では、これを次のように変えます。

- UI 文言ではなく、安定したキー名で意味を表す
- セクションで意味のまとまりを作る
- 将来の仕様変更を `#[Format]` で吸収しやすくする

## 例: 品質評価 / 本戦ルール

```plaintext
#[Format] STSAInput/2

#[Section] Meta
AnalysisFlowMode=QualityEvaluation
RuleProfileMode=FinalStage
FirstPlayerWinRatePercent=51
AdditionalApexPlacementMode=On
BoundaryRescueMode=On
VariableTop8Mode=On
ExecutionMode=Single
SimulationCount=20000
OutputPath=docs/Reports
#[EndSection]

#[Section] PlayersCsv
name,elo
金,2400
銀,2350
#[EndSection]

#[Section] GroupMapCsv
group,name
Apex,金
Apex,銀
#[EndSection]

#[Section] MatchesInput
black,white
金,銀
#[EndSection]

#[Section] ReferenceMatchesInput
black,white
銀,金
#[EndSection]
```

## 例: 品質評価 / 通常ルール

```plaintext
#[Format] STSAInput/2

#[Section] Meta
AnalysisFlowMode=QualityEvaluation
RuleProfileMode=Standard
TournamentRuleSetMode=Twill
FirstPlayerWinRatePercent=51
ExecutionMode=Sweep
SweepStartPercent=50
SweepEndPercent=100
SweepStepPercent=5
OutputPath=docs/Reports
#[EndSection]

#[Section] PlayersCsv
name,elo
Alice,1500
Bob,1480
#[EndSection]

#[Section] MatchesInput
black,white
Alice,Bob
#[EndSection]
```

## 移行方針案

現実的な移行は、次の段階を想定します。

1. 現行の `#[Prompt]` / `#[Enter]` 方式を維持する
2. 新しく `STSAInput/2` を追加する
3. パーサーは当面、旧方式と新方式の両対応にする
4. 新しいサンプル入力ファイルは `STSAInput/2` を優先する
5. 既存ファイルは必要に応じて徐々に移行する

## この文書の位置づけ

この文書は**仕様ドラフト**です。  
正式採用前のため、セクション名・キー名・必須項目は今後の実装方針に応じて調整される可能性があります。

関連資料:
- [入力ファイル仕様](./入力ファイル仕様.md)
- [モード別ガイド](./モード別ガイド.md)
- [README](../../README.md)
