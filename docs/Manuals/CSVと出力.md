# CSVと出力

## 選手 / Player 一覧CSV
- 1列目: 名前
- 2列目: Elo レーティング
- ヘッダーは省略可能
- 入力終了は空行

### 例
```csv
name,elo
Alice,1500
Bob,1650
Carol,1420
Dave,1800
```

## 対局CSV
- 1列目: 黒番
- 2列目: 白番
- ヘッダーは省略可能
- 入力終了は `END`

### 例
```csv
black,white
Alice,Bob
Carol,Alice
Dave,Alice
Bob,Carol
Bob,Dave
Dave,Carol
END
```

## Round / Black-White / 対局記号表
横長になる実名の代わりに、`A`, `B`, `C` のような記号を使えます。
必要なら `対局記号表` セクションで記号と選手 / Player 名を対応付けます。
互換性のため、旧ヘッダー `Players` でも読み取れます。

### 例
```text
Round
 , A, B, C, D
A, -, 3, 2, 1
B, 3, -, 1, 2
C, 2, 1, -, 3
D, 1, 2, 3, -

Black/White
 , A, B, C, D
A, -, b, b, b
B, w, -, b, b
C, w, w, -, b
D, w, w, w, -

対局記号表
A, "Alice"
B, "Bob"
C, "Carol"
D, "Dave"

END
```

## コンソール表示
実行結果では主に次を表示します。

- 元Elo
- 実効Elo
- 差分
- 黒番回数
- 白番回数
- 黒番勝率
- 白番勝率
- 優勝確率
- 平均順位

## 結果CSV
通常モードや本戦専用モードでは、結果CSVに次を出力します。

- コンソール表示の内容
- 各順位の確率
- シミュレーション時は各順位の出現相当回数
- 主なヘッダー名:
  - `calculationMode`
  - `blackAdvantagePercent`
	- `playerName`
  - `originalElo`
  - `effectiveElo`
  - `eloDelta`

フォルダーパスを指定した場合は、その中に自動ファイル名で結果CSVを作成します。

## 結果Markdown
通常モードや本戦専用モードでは、結果CSVと同じ場所に Markdown レポートも出力します。

- 通常モードでは、優勝確率と平均順位を見やすい表でまとめます
- 本戦モードでは、グループ情報、グループ1位確率、総合1位確率も含めてまとめます
- 出力先は結果CSVと同名で、拡張子だけ `.md` になります

## 品質評価CSV
品質評価モードでは、次の 2 種類の CSV を出力します。

### サマリーCSV
- `spearmanCorrelation`
- `meanAbsoluteRankError`
- `averageTop8Retention`
- `eloTop1OverallTop1Probability`
- `mostPenalizedPlayerDelta`
- `mostAdvantagedPlayerDelta`
- 必要に応じて `evaluationMemo`

### 選手別CSV
- `playerName`
- `group`
- `originalElo`
- `eloRank`
- `expectedOverallPlace`
- `overallPlaceDeltaFromEloRank`
- `overallTop1ProbabilityPercent`
- `overallTop8ProbabilityPercent`

## 品質評価Markdownレポート
品質評価モードでは、CSV に加えて人が読みやすい Markdown レポートも出力します。

### 単発評価レポート
- サマリーCSVと同じ場所に、同名の `.md` を出力します
- 主に次をまとめます
  - 計算モード
  - 主要指標の表
  - 最大不利益 / 最大利益
  - ずれが大きい選手の一覧

### n% スイープレポート
- スイープCSVと同じ場所に、同名の `.md` を出力します
- 主に次をまとめます
  - 各評価点の一覧表
  - 最良点の要約
  - Mermaid による推移図

## 実効Eloとは
実効Eloは、対戦相手構成と黒白割り当てを含めた期待勝率を、色補正なしの通常 Elo 戦に置き換えたときのレーティング相当値です。

- `差分 = 実効Elo - 元Elo`
- 黒番が多く有利なら差分がプラスになりやすい
- 白番が多く不利なら差分がマイナスになりやすい

## 注意
- 未対局の選手は結果から自動除外されます
- コメントは CSV に埋め込まず、Examples では `【メモ】...md` のような別ファイルで管理する想定です
- シミュレーション 1 回でも、同点順位の等分配により 33.33% のような値が出ることがあります

## 関連資料
- [入力ファイル仕様](./入力ファイル仕様.md)
- [品質評価](./品質評価.md)
