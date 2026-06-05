# ShogiTournamentSystemAnalyzer

将棋大会の大会ルールを入力し、対局シミュレーションと品質評価で比べるための .NET コンソールアプリです。  
予選・本戦を含む大会システム全体を対象に、ルールの部品を試しながら、現在の基準より良い大会ルールを反復的に作っていくことを目的にしています。

この README は、最初の 1 回を通すための入口です。  
重い背景説明や用語の詳細は `docs/Manuals` に分けています。

## まずここだけ見ればよい

### 1. 起動方法
Visual Studio から実行するか、プロジェクト ルートで次を実行します。

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj
```

入力ファイルで流すときは次です。

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --input-file .\Inputs\Bench\quality_input_[先手8x後手8]_[Neutral_Single10]_[STSAInput2_draft].request.txt
```

### 2. 大会ルールの入力方法
このツールは、1 枚の設定画面に全部書く方式ではなく、起動後の選択肢を順に選んで大会ルールを組み立てます。

基本の流れ:

1. 目的を選ぶ
   - `1`: 対局シミュレーション
   - `2`: 品質評価
2. ルール種別を選ぶ
   - `1`: 通常ルール
   - `2`: 本戦ルール
   - `4`: 空ルール
3. そのモードで必要な大会ルールの部品を入力する
4. 選手一覧、対局表、必要に応じて参考対局や試行回数を入力する
5. 結果をコンソール、CSV、Markdown で確認する

モードごとの詳しい入力順は [モード別ガイド](./docs/Manuals/モード別ガイド.md) を参照してください。

> [!NOTE]
> 入力ミスで止まらなくならないよう、主要な入力の再試行は 10 回で打ち切り、中断メッセージを表示します。

### 3. 出力先
既定の出力先は `Output` 配下に整理されています。

- 順位表、通常結果、本戦結果
  - `Output/Ranking/FinalRanking`
- 大会最終状態、代表実行の対局記録
  - `Output/Simulation/TournamentFinalState`
- 品質評価レポート
  - `Output/TournamentQualityEvaluator/TournamentQualityReport/Summary`
  - `Output/TournamentQualityEvaluator/TournamentQualityReport/Players`
  - `Output/TournamentQualityEvaluator/TournamentQualityReport/Sweeps`

出力内容の読み方は [CSVと出力](./docs/Manuals/CSVと出力.md) を参照してください。

## 6大境界のフォーマット仕様入口

- [TournamentRule（大会ルールデータ）](./docs/Manuals/CSVと出力.md#boundary-tournament-rule)
- [PlayerList（プレイヤー一覧データ）](./docs/Manuals/CSVと出力.md#boundary-player-list)
- [RankingSettings（順位付けの設定データ）](./docs/Manuals/CSVと出力.md#boundary-ranking-settings)
- [TournamentFinalState（大会最終状態データ）](./docs/Manuals/CSVと出力.md#boundary-tournament-final-state)
- [FinalRanking（最終順位データ）](./docs/Manuals/CSVと出力.md#boundary-final-ranking)
- [TournamentQualityReport（大会品質レポート）](./docs/Manuals/CSVと出力.md#boundary-tournament-quality-report)

### 直列の主線

```text
TournamentRule ┐
PlayerList     ├─> TournamentFinalState -> FinalRanking -> TournamentQualityReport
RankingSettings┘
```

- `TournamentRule`
  - 大会ルールデータ
- `PlayerList`
  - プレイヤー一覧データ
- `RankingSettings`
  - 順位付けの設定データ
- `TournamentFinalState`
  - 大会最終状態データ
- `FinalRanking`
  - 最終順位データ
- `TournamentQualityReport`
  - 大会品質レポート

### 位置づけ

- `TournamentRule` / `PlayerList` / `RankingSettings`
  - 主線へ流し込む前提入力
- `TournamentFinalState`
  - 大会で何が起きたかを表す境界
- `FinalRanking`
  - 大会最終状態を順位へ変換する境界
- `TournamentQualityReport`
  - 最終順位を評価してレポート化する境界

### 設計方針

- 境界は分けたままにする
- 依存方向はできるだけ一方向にそろえる
- 主線は次の 1 本にまとめる

```text
TournamentFinalState -> FinalRanking -> TournamentQualityReport
```

### 依存の見方

- `TournamentFinalState`
  - `TournamentRule`
  - `PlayerList`
  - `RankingSettings`
  - を使って生成される
- `FinalRanking`
  - `TournamentFinalState`
  - 必要に応じて `RankingSettings`
  - を使う
- `TournamentQualityReport`
  - 主に `FinalRanking`
  - 必要に応じて `TournamentRule` / `PlayerList`
  - を参照する

## このツールでできること

- 大会ルールの案を、対局シミュレーションで比べる
- 品質評価で、現在の基準より良いか悪いかを見る
- 黒番 / 白番の偏りや、対局表の有利不利を確認する
- 予選・本戦を含む大会制度全体を試行錯誤する
- 条件を変えながら実験を繰り返し、より良い大会ルールへ寄せる

## 主なモード

- 対局シミュレーション / 通常ルール
  - 総当たり戦などの順位分布を確認する
- 対局シミュレーション / 本戦ルール
  - Apex / Innov や本戦補助ルールを含む大会制度を確認する
- 対局シミュレーション / 空ルール
  - ペアリング 0 回、大会最終状態 0 件の最小動作を確認する
- 品質評価 / 通常ルール
  - 通常ルールの実力反映性を評価する
- 品質評価 / 本戦ルール
  - 本戦ルールの実力反映性を評価する

## 最初に知りたい入力の考え方

### 通常ルール
最初に順位ルールセットを選びます。

- `Neutral`
  - 勝ち数ベースのニュートラル順位
- `Twill`
  - ツイル式トーナメント

### 本戦ルール
本戦ルールでは、次のような部品を順に入力します。

- グループ対応 CSV
- 本戦不出場 Apex 一覧 CSV（省略可）
- 本戦不出場 Apex の扱い
- 境界救済戦の有無
- 可変定員 8 ルールの有無

### 空ルール
空ルールでは、対局を 1 件も組みません。

- ペアリング回数は `0`
- 大会最終状態件数は `0`
- 主に出力先だけを確認する最小ケースです

## 入力ファイルと見本の置き場

- `Inputs`
  - `--input-file` で流す実行用入力ファイル。要求ファイル名は `*.request.txt`
  - `Smoke`: 1 回や少数回の確認用
  - `Bench`: 単発評価や軽いベンチマーク用
  - `Sweeps`: n% スイープ実験用
- `ShogiTournamentSystemAnalyzer/Examples`
  - 人が読むための見本、対局記号表、メモ
- `Data/Players`
  - 再利用する選手データ
- `Data/Matches`
  - 再利用する対局表、参考対局表
- `Data/FinalStage`
  - 本戦専用モードの補助 CSV
- `Data/RuleSets`
  - 保存して再利用する大会ルール

## まずどの文書を見るか

- 入力順を見たい
  - [モード別ガイド](./docs/Manuals/モード別ガイド.md)
- 大会ルールの意味を知りたい
  - [トーナメントルール](./docs/Manuals/トーナメントルール.md)
- 入力ファイルで実行したい
  - [入力ファイル仕様](./docs/Manuals/入力ファイル仕様.md)
- 出力の読み方を知りたい
  - [CSVと出力](./docs/Manuals/CSVと出力.md)
- 品質評価の見方を知りたい
  - [品質評価](./docs/Manuals/品質評価.md)
- 大会進行フレームワークを知りたい
  - [大会進行フレームワークガイド](./docs/Manuals/大会進行フレームワークガイド.md)
- 実装の場所を探したい
  - [実装ファイル案内](./docs/Manuals/実装ファイル案内.md)

## 進め方のイメージ

1. 大会ルールの部品を考える
2. その部品を大会ルールへ取り入れる
3. 対局シミュレーションや品質評価で比べる
4. 現在の基準より良ければ次の基準にする
5. さらに改良する

## 補足ドキュメント

- [説明書の総合目次](./docs/Manuals/README.md)
- [プロジェクトの考え方](./docs/Manuals/プロジェクトの考え方.md)
- [docs フォルダー案内](./docs/README.md)
- [Reports の案内](./Output/Reports/README.md)

## 関連リンク

- [白黒対抗ルール案](https://note.com/muzudho/n/n311536fd1812?app_launch=false)

## ライセンス

- [MIT License](./LICENSE)

