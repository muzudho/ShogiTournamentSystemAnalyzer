# ShogiTournamentPairingAnalyzer

大会の対局表と Elo レーティングから、順位分布を計算する .NET コンソールアプリです。  
黒番有利なゲームを想定しており、黒番・白番の偏りや、対局表による有利不利を確認できます。

## 主な機能
- プレイヤーごとの Elo レーティング入力
- 黒番有利率 (%) の指定
- 対局CSV入力
- `Round / Black/White / Players` 表からの対局CSV自動生成
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

## 実行方法
Visual Studio から実行するか、プロジェクトディレクトリーで次を実行します。

```powershell
dotnet run --project .\ShogiTournamentPairingAnalyzer\ShogiTournamentPairingAnalyzer.csproj
```

## 入力
アプリ実行後、次の順に入力します。

1. 黒番有利率 (%)
2. プレイヤーCSV
3. 対局CSV または Round/Black-White/対局記号表
4. 必要に応じてシミュレーション試行回数
5. 結果CSVの出力先パスまたはフォルダーパス

### プレイヤーCSV
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
必要なら `対局記号表` セクションで記号と実名を対応付けます。  
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

フォルダーパスを指定した場合は、その中に自動ファイル名で結果CSVを作成します。

## 実効Eloとは
実効Eloは、対戦相手構成と黒白割り当てを含めた期待勝率を、  
色補正なしの通常 Elo 戦に置き換えたときのレーティング相当値です。

- `差分 = 実効Elo - 元Elo`
- 黒番が多く有利なら差分がプラスになりやすい
- 白番が多く不利なら差分がマイナスになりやすい

## 注意
- 未対局プレイヤーは結果から自動除外されます
- コメントは CSV に埋め込まず、`.memo.md` や `.memo.txt` で別管理する想定です
- シミュレーション 1 回でも、同点順位の等分配により 33.33% のような値が出ることがあります

## 関連メモ
- `docs/notes.md`
- `ShogiTournamentPairingAnalyzer/Examples/*.memo.md`
