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
- 1列目: 先手
- 2列目: 後手
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

## Round / First-Second / 対局記号表
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

First/Second
 , A, B, C, D
A, -, f, f, f
B, s, -, f, f
C, s, s, -, f
D, s, s, s, -

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
- 先手回数
- 後手回数
- 先手勝率
- 後手勝率
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
既定では `Output/Ranking/FinalRanking` に出力します。

## 結果Markdown
通常モードや本戦専用モードでは、結果CSVと同じ場所に Markdown レポートも出力します。

- 通常モードでは、優勝確率と平均順位を見やすい表でまとめます
- 本戦モードでは、グループ情報、グループ1位確率、総合1位確率も含めてまとめます
- 出力先は結果CSVと同名で、拡張子だけ `.md` になります
- 既定では結果CSVと同じく `Output/Ranking/FinalRanking` に出力します

## 大会進行フレームワークの出力
大会進行フレームワークでは、通常の結果表に加えて、代表実行 1 件ぶんの大会最終状態テーブルも出力します。

- aggregate 結果と representative 順位表の既定出力先は `Output/Ranking/FinalRanking`
- representative 大会最終状態の既定出力先は `Output/Simulation/TournamentFinalState`

### aggregate 結果
- `tournament_framework_aggregate_final_ranking_*.csv`
- `tournament_framework_aggregate_final_ranking_*.md`
- これは **複数回試行を集計した順位表** です
- `優勝確率` や `平均順位` は、この aggregate 結果を見ます
- CSV には `note` 列が付き、aggregate 結果であることを明示します

### representative 大会最終状態
- `tournament_framework_representative_final_ranking_*.csv`
- `tournament_framework_representative_final_ranking_*.md`
- これは **代表実行 1 件の順位表** です
- コンソールに出る `代表実行順位` と同じ系統の情報です
- aggregate の順位表とは別物です

- `representative_tournament_final_state_*.csv`
- `representative_tournament_final_state_*.md`
- これは **代表実行 1 件の対局記録** です
- aggregate の順位表と 1 対 1 には対応しません
- どんな対局順・勝敗例になったかを確認したいときに見ます
- CSV には `note` 列が付き、representative であることを明示します

### どう見分けるか
- ファイル名に `aggregate` が入っていれば、集計済みの順位表です
- ファイル名に `representative` が入っていれば、代表実行 1 件の大会最終状態です
- Markdown には概要欄の注記が入ります
- コンソールにも同じ趣旨の注記が出ます

### Markdown の行き来
- aggregate 順位表 Markdown から、representative 順位表 Markdown へ移れます
- representative 順位表 Markdown から、representative 大会最終状態 Markdown へ移れます
- representative 大会最終状態 Markdown から、representative 順位表 Markdown と aggregate 順位表 Markdown へ戻れます
- つまり、集計結果 → 代表順位 → 代表対局記録 → 集計結果、という往復ができます

## 空ルールの出力
空ルールでは、対局を 1 件も組まないため、結果は最小形だけを出力します。

- `empty_rule_final_ranking_*.csv`
- `empty_rule_final_ranking_*.md`
- `empty_tournament_final_state_*.csv`
- `empty_tournament_final_state_*.md`
- 総ペアリング数は `0` です
- 大会最終状態件数は `0` です
- 大会最終状態テーブルも 0 件のまま出力されます
- 既定では結果は `Output/Ranking/FinalRanking`、大会最終状態は `Output/Simulation/TournamentFinalState` に出力します

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
- 既定出力先は `Output/TournamentQualityEvaluator/TournamentQualityReport/Summary`

### 選手別CSV
- `playerName`
- `group`
- `originalElo`
- `eloRank`
- `expectedOverallPlace`
- `overallPlaceDeltaFromEloRank`
- `overallTop1ProbabilityPercent`
- `overallTop8ProbabilityPercent`
- 既定出力先は `Output/TournamentQualityEvaluator/TournamentQualityReport/Players`

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
- 既定出力先は `Output/TournamentQualityEvaluator/TournamentQualityReport/Sweeps`

## 実効Eloとは
実効Eloは、対戦相手構成と先手後手割り当てを含めた期待勝率を、色補正なしの通常 Elo 戦に置き換えたときのレーティング相当値です。

- `差分 = 実効Elo - 元Elo`
- 先手が多く有利なら差分がプラスになりやすい
- 後手が多く不利なら差分がマイナスになりやすい

## 注意
- 未対局の選手は結果から自動除外されます
- コメントは CSV に埋め込まず、Examples では `【メモ】...md` のような別ファイルで管理する想定です
- シミュレーション 1 回でも、同点順位の等分配により 33.33% のような値が出ることがあります

## 関連資料
- [入力ファイル仕様](./入力ファイル仕様.md)
- [品質評価](./品質評価.md)
