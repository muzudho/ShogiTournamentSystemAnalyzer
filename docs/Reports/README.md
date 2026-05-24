# Reports

このフォルダーは、ShogiTournamentSystemAnalyzer の**手書きの実験レポートと発表用文書**をまとめた場所です。  
アプリの実行で自動生成される CSV / Markdown は `ShogiTournamentSystemAnalyzer/Output/...` に出力し、このフォルダーには比較メモ、総括、発表案など人が編集する文書を置きます。

## 何が入るか
- 品質評価レポートや改善案メモの Markdown
- 品質評価総括レポートの Markdown
- 発表案や比較メモの Markdown

## 自動生成物の置き場
アプリの実行で自動生成される出力は、次に整理されています。

- `ShogiTournamentSystemAnalyzer/Output/Ranking/FinalRanking`
- `ShogiTournamentSystemAnalyzer/Output/Simulation/TournamentFinalState`
- `ShogiTournamentSystemAnalyzer/Output/TournamentQualityEvaluator/TournamentQualityReport/Summary`
- `ShogiTournamentSystemAnalyzer/Output/TournamentQualityEvaluator/TournamentQualityReport/Players`
- `ShogiTournamentSystemAnalyzer/Output/TournamentQualityEvaluator/TournamentQualityReport/Sweeps`

## 使い分け
### Markdown レポートを見る
GitHub 上で結論や考察を読みたいときは、次を開きます。

- `品質評価レポート_*.md`
- `品質評価総括レポート_*.md`
- `改善案*.md`

#### まず読みやすい代表例
- [品質評価総括レポート 3ケース比較](./品質評価総括レポート_3ケース比較.md)
- [品質評価レポート 先手8x後手8](./品質評価レポート_[先手8x後手8].md)
- [品質評価レポート 先手8x後手8 本戦不出場Apexあり 3ケース比較](./品質評価レポート_[先手8x後手8_本戦不出場Apexあり_BeforeAfter].md)
- [品質評価レポート トップ集団小さめ](./品質評価レポート_[トップ集団小さめ].md)
- [品質評価レポート Twill+CommonOpp 先手8x後手8 50_60_70比較](./品質評価レポート_[Twill+CommonOpp_先手8x後手8_50_60_70比較].md)
- [ツイル式トーナメント発表案](./ツイル式トーナメント発表案.md)

#### 自動生成レポートの見方
- 自動生成レポート本体は `docs/Reports` ではなく `ShogiTournamentSystemAnalyzer/Output/...` にあります。
- `*_quality_summary.md`
  - 品質評価の主要指標、着目選手、自動コメントを読むとき
- `*_quality_sweep.md`
  - n% スイープの一覧表、Mermaid 図、自動おすすめ帯を見るとき
- 通常モードや本戦モードの結果 `.md`
  - 上位候補、グループ別の見え方、Mermaid 図をざっと確認するとき

### CSV を見る
表計算ソフトや再集計に使いたいときは、次を開きます。

- `*_quality_summary.csv`
- `*_quality_players.csv`
- `*_quality_sweep.csv`

これらの CSV は `ShogiTournamentSystemAnalyzer/Output/TournamentQualityEvaluator/TournamentQualityReport/...` に出力されます。

## 補足
- `Good` / `Bad` の振り分けは、自動生成物側では `Output/TournamentQualityEvaluator/TournamentQualityReport/.../Good` と `.../Bad` に保存されます。
- この `docs/Reports` では、良し悪しの自動振り分けよりも、人がまとめ直した比較文書を優先して置きます。

## このフォルダーで扱うファイル名の目安
- `*_quality_summary.csv`
	- 参照用に文中で言及することがある自動生成ファイル名
- `*_quality_players.csv`
	- 選手別の品質評価結果の自動生成ファイル名
- `*_quality_sweep.csv`
	- n% スイープ実験結果の自動生成ファイル名
- `*_quality_summary.md`
	- 品質評価サマリーの自動生成 Markdown 名
- `*_quality_sweep.md`
	- n% スイープ結果の自動生成 Markdown 名
- `品質評価レポート_*.md`
  - 手書きの実験内容、比較、所感
- `改善案*.md`
  - 改善案比較メモ

## 関連入口
- [docs フォルダー案内](../README.md)
- [品質評価の説明書](../Manuals/品質評価.md)
- [Manuals の総合目次](../Manuals/README.md)
- [トップ README](../../README.md)
