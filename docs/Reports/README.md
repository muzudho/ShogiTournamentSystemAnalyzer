# Reports

このフォルダーは、ShogiTournamentSystemAnalyzer の**実験結果とレポート**をまとめた場所です。  
品質評価モードやスイープ実験の出力 CSV、人が読むための Markdown レポート、実行ログを保管します。

## 何が入るか
- 品質評価のサマリー CSV
- 品質評価の選手別 CSV
- n% スイープ実験の CSV
- 品質評価レポートや改善案メモの Markdown
- 実行ログ（`.stdout.txt` / `.stderr.txt`）

## 使い分け
### Markdown レポートを見る
GitHub 上で結論や考察を読みたいときは、次を開きます。

- `品質評価レポート_*.md`
- `品質評価総括レポート_*.md`
- `改善案*.md`

### CSV を見る
表計算ソフトや再集計に使いたいときは、次を開きます。

- `quality_summary_*.csv`
- `quality_players_*.csv`
- `quality_sweep_*.csv`

## サブフォルダー
### `Good`
ニュートラル基準で「良い」と判定した結果をまとめる場所です。

### `Bad`
ニュートラル基準で「悪い」と判定した結果をまとめる場所です。

## ファイル名の目安
- `quality_summary_*.csv`
  - 品質評価のサマリー数値
- `quality_players_*.csv`
  - 選手別の品質評価結果
- `quality_sweep_*.csv`
  - n% スイープ実験結果
- `quality_summary_*.md`
  - 品質評価サマリーの Markdown レポート
- `quality_sweep_*.md`
  - n% スイープ結果の Markdown レポート
- `品質評価レポート_*.md`
  - 手書きの実験内容、比較、所感
- `改善案*.md`
  - 改善案比較メモ

## 関連入口
- [docs フォルダー案内](../README.md)
- [品質評価の説明書](../Manuals/品質評価.md)
- [Manuals の総合目次](../Manuals/README.md)
- [トップ README](../../README.md)
