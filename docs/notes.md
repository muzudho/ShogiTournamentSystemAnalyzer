# ShogiTournamentPairingAnalyzer メモ

## 目的
- 大会参加者の Elo レーティング、対局表、黒番有利率を入力し、総当たり戦または指定対局における順位分布を計算する。
- 大会ルール調整時に、黒白の偏りや対局表の有利不利を確認しやすくする。
- 本戦専用ルールや、そのルールが Elo 順をどれだけ保つかの品質評価にも使う。

## 現在の前提ルール
- 各対局は黒番・白番を持つ。
- 同 Elo 対局時の黒番勝率を 0〜100% で指定できる。
- 勝率計算は Elo 差と黒番補正を用いる。
- 対局数が少ない場合は厳密計算を行う。
- 対局数が多い場合はモンテカルロシミュレーションで近似する。
- 同点順位は、該当順位帯に等分配する。

## 入力形式
### モード
- `1`: 通常モード（総当たり戦分析）
- `2`: 本戦専用モード（Apex / Innov 定先戦分析）
- `3`: 品質評価モード（本戦ルールの実力反映性評価）

### 1. 選手一覧CSV
- 1列目: 選手名
- 2列目: Elo レーティング
- ヘッダーは省略可能

例:

```csv
name,elo
Alice,1500
Bob,1650
Carol,1420
Dave,1800
```

### 2. 対局入力
以下のどちらかを受け付ける。

#### 2-1. 対局CSV
- 1列目: 黒番
- 2列目: 白番

例:

```csv
black,white
Alice,Bob
Carol,Alice
Dave,Alice
Bob,Carol
Bob,Dave
Dave,Carol
```

#### 2-2. Round / Black-White / 対局記号表
- `Round` セクション
- `Black/White` セクション
- 必要なら `対局記号表` セクションで記号を選手名へ対応付け
- 旧ヘッダー `Players` も互換で読み取る
- 対局入力の終了は `END` 行

例:

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

### 本戦専用モード追加入力
- Apex / Innov の分け方を On / Off で切り替える
- `On` のときはグループ対応CSVを別入力で受け付ける
- グループ対応CSVは `group,name` 形式
- `On` のときは `Apex` と `Innov` を使用する
- `On` のときは本戦不出場Apex一覧CSV（省略可）を別入力で受け付ける
- `On` のときは本戦不出場Apexの扱いを Off / On で切り替えられる
- `On` のときは境界救済戦を Off / On で切り替えられる
- `Off` のときはニュートラルな対局表として扱う

例:

```csv
group,name
Apex,Alice
Apex,Bob
Innov,Carol
Innov,Dave
```

現状の MVP 実装では、次を検証する。
- 本戦参加者は 16 名
- Apex / Innov 分け方が `On` のときは `Apex` は 8 名以下
- Apex / Innov 分け方が `On` のときは対局は `Innov` が黒番、`Apex` が白番
- Apex / Innov 分け方が `On` のときは同グループ同士の対局は禁止
- Apex / Innov 分け方が `Off` のときは全参加者を同一の土俵で順位づけする

実験運用では、さらに次を切り替える。
- Apex / Innov の分け方
  - Off: ニュートラル
  - On: Apex / Innov を使う
- 本戦不出場Apexの扱い
  - Off: Innov より前に順位帯を確保する
  - On: 総合順位へ挿入しない
- 境界救済戦
  - Off: なし
  - On: Apex最下位相当とInnov最上位相当で救済戦を行う

サンプルファイル:
- `【選手一覧】[黒8x白8].csv`
- `【グループ対応表】[本戦専用_黒8x白8].csv`
- `【対局表】[黒8x白8].csv`
- `【メモ】[本戦専用モード_黒8x白8].md`

## 出力
### コンソール表示
- 元Elo
- 実効Elo
- 差分
- 黒番回数
- 白番回数
- 黒番勝率
- 白番勝率
- 優勝確率
- 平均順位

※ 各順位の確率列は横幅削減のためコンソールには表示しない。

### 結果CSV
- コンソール表示の内容を含む
- 各順位の確率を含む
- シミュレーション時は各順位の出現相当回数も含む
- 主なヘッダー名は `calculationMode`, `blackAdvantagePercent`, `participantName`, `originalElo`, `effectiveElo`, `eloDelta`
- 出力先はファイルパスまたはフォルダーパスを指定可能
- フォルダーパス指定時は自動ファイル名を付与する

### 品質評価CSV
- サマリーCSV
  - `spearmanCorrelation`
  - `meanAbsoluteRankError`
  - `averageTop8Retention`
  - `eloTop1OverallTop1Probability`
  - 必要なら `evaluationMemo`
- 参加者別CSV
  - `participantName`
  - `group`
  - `originalElo`
  - `eloRank`
  - `expectedOverallPlace`
  - `overallPlaceDeltaFromEloRank`
  - `overallTop1ProbabilityPercent`
  - `overallTop8ProbabilityPercent`

## 品質評価の基準
- 品質評価では、ニュートラルを基準にする。
- ニュートラルより良い大会ルールを `Good`、悪い大会ルールを `Bad` とする。
- `docs/Reports/Good` と `docs/Reports/Bad` の振り分けも、この基準で行う。

## 実効Elo の意味
- 対戦相手構成と黒白割り当てを踏まえた期待勝率から、色補正なしの通常 Elo 戦で同等の期待勝率になるレーティングを逆算したもの。
- `差分 = 実効Elo - 元Elo`
- 黒番が多く有利なら差分がプラス、白番が多く不利なら差分がマイナスになりやすい。

## 現在の仕様上の注意
- 未対局の選手は結果から自動除外する。
- 同点順位は乱数で決めず、等分配で扱う。
- シミュレーション 1 回でも、同点時には 33.33% などの分数確率が出ることがある。
- コメントは CSV に埋め込まず、別ファイルで管理する。
- 実行用入力ファイル `.txt` は `ShogiTournamentPairingAnalyzer/Inputs` に置き、結果レポートは `docs/Reports` に置く。
- 改善案比較では `docs/Reports/Good` と `docs/Reports/Bad` を使い分ける。
- Good / Bad は厳密な二元論ではなく、必要に応じて評価メモを 1 行残して運用する。
- `Program.cs` の肥大化対策として、モード実装は `Domain/Modes` に分離した。

## メモ運用方針
- データ本体は `.csv`
- 補足説明は `【メモ】...md` の別ファイル
- 会話ログ全文ではなく、決まった仕様・方針・未決事項を要約して残す

例:
- `【対局表】[黒8x白8].csv`
- `【メモ】[対局表_黒8x白8].md`

## 今後の候補
- タイブレーク規則の実装
- CSV や結果ファイルの自動読込
- 対局CSVのファイル保存補助
- 大会全体の公平性指標の追加
- 実効Elo差分の集計表示
