# 【品質評価レポート】[黒8x白8_本戦不出場Apexあり] 追加実験

## 作成日
2026-05-14

## 目的
今日は最後の追加実験として、
`[黒8x白8_本戦不出場Apexあり]` に対して
- 改善案A On
- 境界救済戦 On
の併用を試し、
改善案A 単独との違いを確認した。

## 比較対象
A. 改善案A 単独
- AdditionalApexPlacement: On
- BoundaryRescue: Off
- `ShogiTournamentSystemAnalyzer\Output\TournamentQualityEvaluator\TournamentQualityReport\Summary\Good\[黒8x白8_本戦不出場Apexあり]_[On]_quality_summary.csv`

B. 追加実験（併用）
- AdditionalApexPlacement: On
- BoundaryRescue: On
- `ShogiTournamentSystemAnalyzer\Output\TournamentQualityEvaluator\TournamentQualityReport\Summary\Good\[黒8x白8_本戦不出場Apexあり]_[AdditionalOn_BoundaryOn]_quality_summary.csv`

## 比較表
A. 改善案A 単独
- Spearman 相関: 1.000000
- 平均順位ずれ: 1.379675
- Elo上位8名の総合上位8位残留人数: 8.000000
- Elo1位の総合1位確率: 22.698155%
- 最大不利益: 飛 (+2.446975)
- 最大利益: ひよこ (-2.437150)

B. 追加実験（併用）
- Spearman 相関: 1.000000
- 平均順位ずれ: 1.356554
- Elo上位8名の総合上位8位残留人数: 7.642915
- Elo1位の総合1位確率: 22.690708%
- 最大不利益: 飛 (+2.445878)
- 最大利益: ひよこ (-2.463017)
- 評価メモ: かなり良いが、上位8完全残留は少し落ちる

## 所感
- 平均順位ずれは、併用の方が少し良い
- ただし、上位8名の完全残留はやはり少し落ちる
- Elo1位の総合1位確率はほぼ変わらない

## 結論
この追加実験でも、
**改善案A 単独が最も素直で扱いやすい** という整理は変わらなかった。

併用ケースは、
- 公平性をさらに少し押し上げる余地はある
- ただし制度の切れ味を少し落とす
という、これまでの傾向を再確認する結果だった。

今日の締めとしては、
- 改善案A は強い Good 候補
- 境界救済戦は条件付き Good
- 併用は有望だが目的次第
という整理で十分だと思う。

以上。
