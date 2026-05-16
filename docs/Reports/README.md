# Reports

このフォルダーは、ShogiTournamentPairingAnalyzer の**実験結果とレポート**をまとめた場所です。  
品質評価モードやスイープ実験の出力 CSV、比較レポート、実行ログを保管します。

## 何が入るか
- 品質評価のサマリー CSV
- 品質評価の参加者別 CSV
- n% スイープ実験の CSV
- 品質評価レポートや比較メモ
- 実行ログ（`.stdout.txt` / `.stderr.txt`）

## よくある見方
- 品質評価の結論を読みたい
  - `品質評価レポート_*.txt`
- 数値を表計算ソフトで確認したい
  - `quality_summary_*.csv`
  - `quality_participants_*.csv`
- スイープ実験の変化を見たい
  - `quality_sweep_*.csv`
- 良し悪しで見分けたい
  - `Good/`
  - `Bad/`

## サブフォルダー
### `Good`
ニュートラル基準で「良い」と判定した結果をまとめる場所です。

### `Bad`
ニュートラル基準で「悪い」と判定した結果をまとめる場所です。

## ファイル名の目安
- `quality_summary_*.csv`
  - 品質評価のサマリー
- `quality_participants_*.csv`
  - 参加者別の品質評価結果
- `quality_sweep_*.csv`
  - n% スイープ実験結果
- `品質評価レポート_*.txt`
  - 実験内容の説明や所感
- `改善案*.txt`
  - 改善案比較メモ

## 関連入口
- [docs フォルダー案内](../README.md)
- [品質評価の説明書](../Manuals/品質評価.md)
- [Manuals の総合目次](../Manuals/README.md)
- [トップ README](../../README.md)
