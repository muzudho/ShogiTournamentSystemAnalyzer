# ShogiTournamentPairingAnalyzer

大会の対局表と Elo レーティングから、順位分布を計算する .NET コンソールアプリです。  
黒番有利なゲームを想定しており、黒番・白番の偏りや、対局表による有利不利を確認できます。

## 主な機能
- 起動時のモード選択
- 順位ルールセット選択
  - `Neutral`（勝ち数ベースのニュートラル順位）
  - `Twill`（ツイル式トーナメント）
- 通常モード（総当たり戦分析）
- 本戦専用モード（Apex / Innov 定先戦分析）
- 品質評価モード（本戦ルールの実力反映性評価）
- `Domain/Modes` へのモード分離
- 選手ごとの Elo レーティング入力
- 黒番有利率 (%) の指定
- 対局CSV入力
- `Round / Black/White / 対局記号表` からの対局CSV自動生成
- 順位分布の計算
  - 少ない対局数では厳密計算
  - 多い対局数ではシミュレーション
- 結果のコンソール表示
- 結果CSV出力

## 前提ルール
- 各対局は黒番・白番を持つ
- 同 Elo 対局時の黒番勝率を 0〜100% で指定する
- 勝率は Elo 差と黒番補正から計算する
- 同点順位は該当順位帯に等分配する

## 順位ルールセット

### Neutral
- 既定の順位ルールです。
- 各シナリオでの勝ち数をもとに順位を決めます。
- 同点順位は該当順位帯に等分配します。

### Twill
- **ツイル式トーナメント** の順位ルールです。
- 各対局結果を `▲ / ▽` の比較グラフとして読み、最後に **重箱表** 相当の指標へたたんで順位を決めます。
- 優先順は概ね次の通りです。
  1. 合計右ノード数
  2. 合計勝ち星
  3. 合計最長経路長
  4. `▽` 側で負けた相手の強さ
  5. 直接対戦

通常モードでは常に `Neutral / Twill` を選べます。  
本戦専用モードと品質評価モードでは、`Apex / Innov` の分け方を `Off` にしたときだけ `Neutral / Twill` を選べます。

## 実行方法
Visual Studio から実行するか、プロジェクトディレクトリーで次を実行します。

```powershell
dotnet run --project .\ShogiTournamentPairingAnalyzer\ShogiTournamentPairingAnalyzer.csproj
```

## 入力
アプリ実行後、まずモードを選びます。

- `1`: 通常モード（総当たり戦分析）
- `2`: 本戦専用モード（Apex / Innov 定先戦分析）
- `3`: 品質評価モード（本戦ルールの実力反映性評価）

その後、モードごとに必要な入力を行います。

### 入力ファイルの置き場

- `ShogiTournamentPairingAnalyzer/Examples`
  - 人が読むためのサンプル CSV とメモ
- `ShogiTournamentPairingAnalyzer/Inputs`
  - `--input-file` で流す実行用入力ファイル
	- `Smoke`: 1 回や少数回のスモークテスト用
  - `Bench`: 単発評価や軽いベンチマーク用
  - `Sweeps`: n% スイープ実験用
- `docs/Reports`
  - 実行結果の CSV や実験レポートの保存先

`--input-file` を使うときは、実行用入力を `ShogiTournamentPairingAnalyzer/Inputs/Smoke`、`Bench`、`Sweeps` のいずれかに置く運用を基本にします。

入力ファイルでは `#` で始まる行をコメントとして無視できます。  
そのため、値だけでは意味が分かりにくい箇所には次のように `#[Prompt]` を添えられます。

```plaintext
#[Prompt] モード番号を入力してください [1]
3
#[Prompt] 同Elo対局時の先手勝率(%)を入力してください [51]
51
```

### 通常モード

次の順に入力します。

1. 順位ルールセット（`Neutral / Twill`）
2. 黒番有利率 (%)
3. 選手一覧CSV
4. 対局CSV または Round/Black-White/対局記号表
5. 必要に応じてシミュレーション試行回数
6. 結果CSVの出力先パスまたはフォルダーパス

### 本戦専用モード

次の順に入力します。

1. 同Elo対局時の先手勝率 (%)
2. 選手一覧CSV
3. Apex / Innov の分け方（On / Off）
4. `Off` のときだけ順位ルールセット（`Neutral / Twill`）
5. `On` のときだけグループ対応CSV
6. `On` のときだけ本戦不出場Apex一覧CSV（省略可）
7. `On` のときだけ本戦不出場Apexの扱い（Off / On）
8. `On` のときだけ境界救済戦の有無（Off / On）
9. `On` かつ品質評価モードのときだけ可変定員8ルールの有無（Off / On）
10. 対局CSV または Round/Black-White/対局記号表
11. 参考対局CSV または Round/Black-White/対局記号表（省略可）
12. 必要に応じてシミュレーション試行回数
13. 結果CSVの出力先パスまたはフォルダーパス

グループ対応CSVの例:

```csv
group,name
Apex,Alice
Apex,Bob
Innov,Carol
Innov,Dave
```

現状の MVP 実装では、次を前提にしています。

- 本戦参加者は 16 名
- Apex / Innov 分け方が `On` のときは `Apex` は 8 名以下
- Apex / Innov 分け方が `On` のときは対局は `Innov` が黒番、`Apex` が白番
- Apex / Innov 分け方が `On` のときは総合順位は、まず本戦出場 `Apex` 内で順位づけし、その後ろに本戦出場 `Innov` を並べる
- Apex / Innov 分け方が `Off` のときは、`Neutral` または `Twill` を選んで全参加者をフラットに順位づけする
- 本戦不出場Apexと境界救済戦は、Apex / Innov 分け方が `On` のときだけ使う

サンプル:

- `ShogiTournamentPairingAnalyzer/Examples/【選手一覧】[黒8x白8].csv`
- `ShogiTournamentPairingAnalyzer/Examples/【グループ対応表】[本戦専用_黒8x白8].csv`
- `ShogiTournamentPairingAnalyzer/Examples/【対局表】[黒8x白8].csv`
- `ShogiTournamentPairingAnalyzer/Examples/【メモ】[本戦専用モード_黒8x白8].md`

### 品質評価モード

入力は本戦専用モードと同じです。  
`Apex / Innov` を `Off` にした場合は、品質評価でも `Neutral / Twill` を切り替えられます。

品質評価では、まず **ニュートラル** を基準にします。  
ニュートラルより良い大会ルールを `Good`、悪くなるルールを `Bad` として扱います。

`Twill` の評価では、`Neutral` を基準にして

- Spearman 相関
- 平均順位ずれ
- 上位8残留
- Elo1位の総合1位確率

がどう変わるかを見る使い方を想定しています。

出力では、順位分布そのものに加えて、次の品質指標を表示・CSV出力します。

- Spearman 相関
- 平均順位ずれ
- Elo上位8名の総合上位8位残留人数（平均）
- Elo1位の総合1位確率
- 最大不利益 / 最大利益

品質評価モードでも、上記の本戦専用モード用サンプルをそのまま使えます。

品質評価モードでは、単発評価のほかに **n% スイープ実験** も選べます。  
これは先手勝率を一定範囲で振りながら、各 n% に対する品質指標の変化をまとめて CSV 出力する機能です。

さらに、実験用に次を切り替えられます。

- Apex / Innov の分け方
  - `Off`: ニュートラル
  - `On`: Apex / Innov を使う

- 本戦不出場Apexの扱い
  - `Off`: Innov より前に順位帯を確保する（現行案）
  - `On`: 総合順位へ挿入しない（改善案A）
- 境界救済戦
  - `Off`: 使わない
  - `On`: Apex最下位相当とInnov最上位相当で救済戦を行う
- 可変定員8ルール
  - `Off`: 定員8固定
  - `On`: 本戦不出場Apex人数ぶんだけ Innov 上位を総合上位8へ引き上げる

参考対局:

- 本戦専用モード / 品質評価モードでは、**参考対局** を別入力で受け付けられます
- 参考対局は表示・CSV保存されます
- **大会記録や品質評価には含めません**

品質評価レポート運用:

- `docs/Reports/Good`
- `docs/Reports/Bad`

に分けて保存できます。  
基準は **ニュートラル** です。  
ニュートラルより良いものを `Good`、悪いものを `Bad` とし、必要なら**評価メモ**を 1 行残して運用します。

### 選手一覧CSV
- 1列目: 名前
- 2列目: Elo レーティング
- ヘッダーは省略可能
- 入力終了は空行

例:

```csv
name,elo
Alice,1500
Bob,1650
Carol,1420
Dave,1800
```

### 対局CSV
- 1列目: 黒番
- 2列目: 白番
- ヘッダーは省略可能
- 入力終了は `END`

例:

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

### Round / Black-White / 対局記号表
横長になる実名の代わりに、`A`, `B`, `C` のような記号を使えます。  
必要なら `対局記号表` セクションで記号と選手名を対応付けます。  
互換性のため、旧ヘッダー `Players` でも読み取れます。

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

### 結果CSV
- コンソール表示の内容
- 各順位の確率
- シミュレーション時は各順位の出現相当回数
- 主なヘッダー名: `calculationMode`, `blackAdvantagePercent`, `participantName`, `originalElo`, `effectiveElo`, `eloDelta`

フォルダーパスを指定した場合は、その中に自動ファイル名で結果CSVを作成します。

### 品質評価CSV

品質評価モードでは、次の 2 種類の CSV を出力します。

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

## 実効Eloとは
実効Eloは、対戦相手構成と黒白割り当てを含めた期待勝率を、  
色補正なしの通常 Elo 戦に置き換えたときのレーティング相当値です。

- `差分 = 実効Elo - 元Elo`
- 黒番が多く有利なら差分がプラスになりやすい
- 白番が多く不利なら差分がマイナスになりやすい

## 注意
- 未対局の選手は結果から自動除外されます
- コメントは CSV に埋め込まず、Examples では `【メモ】...md` のような別ファイルで管理する想定です
- シミュレーション 1 回でも、同点順位の等分配により 33.33% のような値が出ることがあります
- 本戦専用モード / 品質評価モードは MVP を超えて実験運用中であり、改善案の On / Off を含む比較を行えます

## 関連リンク
- [白黒対抗ルール案](https://note.com/muzudho/n/n311536fd1812?app_launch=false)

## 関連メモ
- `docs/notes.md`
- `ShogiTournamentPairingAnalyzer/Examples/【メモ】*.md`
