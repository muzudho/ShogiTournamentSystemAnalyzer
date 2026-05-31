# CSVと出力

## 6大境界ごとのフォーマット仕様入口

<a id="boundary-tournament-rule"></a>
### TournamentRule（大会ルールデータ）
- 保存して再利用する大会ルールは `Data/RuleSets` に置きます
- 入口:
  - [トーナメントルール](./トーナメントルール.md)
  - [入力ファイル仕様](./入力ファイル仕様.md)
  - [Data/RuleSets README](../../Data/RuleSets/README.md)

<a id="boundary-player-list"></a>
### PlayerList（プレイヤー一覧データ）
- 選手 / Player 一覧の基本フォーマットは、この文書の [選手 / Player 一覧CSV](#選手--player-一覧csv) を見てください
- 置き場と実例は [Data/Players README](../../Data/Players/README.md) を見てください

<a id="boundary-ranking-settings"></a>
### RankingSettings（順位付けの設定データ）
- 現状は入力ファイルや対話入力の設定として扱います
- 入口:
  - [入力ファイル仕様](./入力ファイル仕様.md)
  - [STSAInput/2 仕様](./STSAInput2仕様.md)
  - [モード別ガイド](./モード別ガイド.md)

<a id="boundary-tournament-result"></a>
### TournamentResult（大会結果データ）
- 対局結果入力の基本フォーマットは、この文書の [対局CSV](#対局csv) と [Round / First-Second / 対局記号表](#round--first-second--対局記号表) を見てください
- 本戦補助データや再利用データの置き場:
  - [Data/Matches README](../../Data/Matches/README.md)
  - [Data/FinalStage README](../../Data/FinalStage/README.md)

<a id="boundary-final-ranking"></a>
### FinalRanking（最終順位データ）
- 出力フォーマットは、この文書の [結果CSV](#結果csv) と [結果Markdown](#結果markdown) を見てください
- 大会進行フレームワークや空ルールの派生出力は、この文書の [大会進行フレームワークの出力](#大会進行フレームワークの出力) と [空ルールの出力](#空ルールの出力) を見てください

<a id="boundary-tournament-quality-report"></a>
### TournamentQualityReport（大会品質レポート）
- 出力フォーマットは、この文書の [品質評価CSV](#品質評価csv) と [品質評価Markdownレポート](#品質評価markdownレポート) を見てください
- 解釈の補足は [品質評価](./品質評価.md) を見てください

## CSV共通規約

出力 CSV は、境界ごとに 1 行の意味や専用列は違いますが、先頭の外枠だけは共通化しています。

### 共通部列
- `boundaryName`
  - どの境界の CSV かを表します
- `schemaName`
  - その境界の中でどの表形式かを表します
- `rowType`
  - その行が何の種類かを表します

つまり、各 CSV は次の形です。

- 共通部列
  - `boundaryName, schemaName, rowType`
- 専用列部
  - 境界ごとの本体列

### rowType の見方
- `data`
  - 通常のデータ行です
- `metric`
  - 指標名と値を並べる行です
- `meta`
  - メモや補足などの行です

### schemaName 一覧

#### FinalRanking
- `representativeExecutionRank`
  - representative 順位表 CSV
- `standardFinalRanking`
  - 通常モードの順位表 CSV
- `finalStageFinalRanking`
  - 本戦専用モードの順位表 CSV

#### TournamentFinalState
- `tournamentMatchRecord`
  - 大会最終状態の対局記録 CSV

#### TournamentQualityReport
- `summaryMetrics`
  - 品質評価サマリー CSV
- `playerReport`
  - 選手別 CSV
- `sweepReport`
  - n% スイープ CSV

### 考え方
- 1 行が何を表すかは境界ごとに違います
- そのため、専用列は無理に共通化しません
- 代わりに、CSV の先頭だけ共通部列を置いて、読み手が「どの境界のどの表か」をすぐ見分けられるようにしています

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

- 先頭に共通部列 `boundaryName`, `schemaName`, `rowType` が付きます
- `schemaName` は通常モードが `standardFinalRanking`、本戦モードが `finalStageFinalRanking` です
- コンソール表示の内容
- 各順位の確率
- シミュレーション時は各順位の出現相当回数
- 主なヘッダー名:
  - `calculationMode`
	- `firstPlayerWinRatePercent`
  - `playerName`
  - `originalElo`
  - `effectiveElo`
  - `eloDelta`
- 通常モードでは主に `championshipProbabilityPercent`, `averagePlace` を出力します
- 本戦モードでは主に `group`, `groupPlace1ProbabilityPercent`, `groupPlaceAverage`, `overallPlace1ProbabilityPercent`, `overallPlaceAverage` を出力します
- 必要に応じて `note` 列が付きます

フォルダーパスを指定した場合は、その中に自動ファイル名で結果CSVを作成します。
既定では `Output/Ranking/FinalRanking` に出力します。

## 結果Markdown
通常モードや本戦専用モードでは、結果CSVと同じ場所に Markdown レポートも出力します。

- 共通して `概要`、`注目ポイント`、`自動コメント`、`上位候補一覧`、`Mermaid 図` を出力します
- 通常モードでは、優勝確率と平均順位を中心にまとめます
- 本戦モードでは、グループ情報、グループ1位確率、総合1位確率に加えて `Apex 注目候補` と `Innov 注目候補` も含めてまとめます
- 参考対局CSVがある場合は、概要欄にそのリンクも出ます
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
- `schemaName` は `standardFinalRanking` です

### representative 大会最終状態
- `tournament_framework_representative_final_ranking_*.csv`
- `tournament_framework_representative_final_ranking_*.md`
- これは **代表実行 1 件の順位表** です
- コンソールに出る `代表実行順位` と同じ系統の情報です
- aggregate の順位表とは別物です
- `schemaName` は `representativeExecutionRank` です

- `representative_tournament_final_state_*.csv`
- `representative_tournament_final_state_*.md`
- これは **代表実行 1 件の対局記録** です
- aggregate の順位表と 1 対 1 には対応しません
- どんな対局順・勝敗例になったかを確認したいときに見ます
- CSV には `note` 列が付き、representative であることを明示します
- `schemaName` は `tournamentMatchRecord` です

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

- 先頭に共通部列 `boundaryName`, `schemaName`, `rowType` が付きます

### サマリーCSV
- `spearmanCorrelation`
- `meanAbsoluteRankError`
- `averageTop8Retention`
- `eloTop1OverallTop1Probability`
- `mostPenalizedPlayerDelta`
- `mostAdvantagedPlayerDelta`
- 必要に応じて `evaluationMemo`
- 既定出力先は `Output/TournamentQualityEvaluator/TournamentQualityReport/Summary`
- `schemaName` は `summaryMetrics` です
- 指標行は `rowType = metric`、補足行は `rowType = meta` です

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
- `schemaName` は `playerReport`、通常行は `rowType = data` です

### スイープCSV
- `firstPlayerWinRatePercent`
- `spearmanCorrelation`
- `meanAbsoluteRankError`
- `averageTop8Retention`
- `eloTop1OverallTop1ProbabilityPercent`
- `mostPenalizedPlayer`
- `mostPenalizedDelta`
- `mostAdvantagedPlayer`
- `mostAdvantagedDelta`
- 必要に応じて `evaluationMemo`
- 既定出力先は `Output/TournamentQualityEvaluator/TournamentQualityReport/Sweeps`
- `schemaName` は `sweepReport` です
- 通常行は `rowType = data`、補足行は `rowType = meta` です

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

