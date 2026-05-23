# 【品質評価レポート】[黒8x白8_本戦不出場Apexあり] 3ケース比較

## 作成日
2026-05-17

## 目的
`[黒8x白8_本戦不出場Apexあり]` ケースについて、
現行案（Off）、現行案＋品質評価補正、改善案A（On）を比較し、
品質評価パラメーター追加後にどう見えるかを確認する。

## 比較対象
1. 現行案（Bad / Off）
- 本戦不出場Apexを Innov より前の順位帯へ挿入する
- 出力先: [Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/Bad/quality_summary_[黒8x白8_本戦不出場Apexあり]_[Off]_modern.csv](../../ShogiTournamentSystemAnalyzer/Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/Bad/quality_summary_[黒8x白8_本戦不出場Apexあり]_[Off]_modern.csv)

2. 現行案 + 品質評価補正（Bad / Off+Offset）
- 大会ルール自体は現行案のまま
- 品質評価だけ、Innov の比較基準順位を本戦不出場Apex人数+1ぶん後ろへずらす
- 出力先: [Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/Bad/quality_summary_[黒8x白8_本戦不出場Apexあり]_[Off_QualityInnovOffsetOn].csv](../../ShogiTournamentSystemAnalyzer/Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/Bad/quality_summary_[黒8x白8_本戦不出場Apexあり]_[Off_QualityInnovOffsetOn].csv)

3. 改善案A（Good / On）
- 本戦不出場Apexを総合順位へ挿入しない
- 出力先: [Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/Good/quality_summary_[黒8x白8_本戦不出場Apexあり]_[On]_modern.csv](../../ShogiTournamentSystemAnalyzer/Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/Good/quality_summary_[黒8x白8_本戦不出場Apexあり]_[On]_modern.csv)

## 比較表
### 現行案（Off）
- Spearman 相関: 0.7720
- 平均順位ずれ: 2.996
- Elo上位8名の総合上位8位残留人数: 8.000
- Elo1位の総合1位確率: 22.65%
- 最大不利益: 飛 (+2.460)
- 最大利益: ひよこ (-9.688)

### 現行案 + 品質評価補正（Off+Offset）
- Spearman 相関: 0.7720
- 平均順位ずれ: 4.316
- Elo上位8名の総合上位8位残留人数: 8.000
- Elo1位の総合1位確率: 23.21%
- 最大不利益: 飛 (+2.434)
- 最大利益: ひよこ (-12.751)

### 改善案A（On）
- Spearman 相関: 1.0000
- 平均順位ずれ: 1.380
- Elo上位8名の総合上位8位残留人数: 8.000
- Elo1位の総合1位確率: 23.18%
- 最大不利益: 飛 (+2.425)
- 最大利益: ひよこ (-2.451)

## 結論
改善案A（On）は、このケースでも**明確に良い案**と言ってよい。

一方で、今回追加した**品質評価用の比較基準順位補正**は、
このケースでは現行案（Off）を救済しなかった。

理由は次の通り。

1. Spearman 相関は改善案Aだけが大幅改善
   - Off: 0.7720
   - Off+Offset: 0.7720
   - On: 1.0000

2. 平均順位ずれは改善案Aだけが大幅改善
   - Off: 2.996
   - Off+Offset: 4.316
   - On: 1.380

3. 最大利益の歪みは品質評価補正ではむしろ悪化し、改善案Aで大きく緩和
   - Off: ひよこ (-9.688)
   - Off+Offset: ひよこ (-12.751)
   - On: ひよこ (-2.451)

4. 上位 8 名の上位帯残留は 3 ケースとも維持
   - いずれも 8.000
   - つまり改善案Aは制度の強みを壊さず、弱点だけをかなり改善できている

## どう見直されたか
今回の品質評価補正は、
**Innov の比較基準順位だけを後ろへずらして、制度上の段差を評価時に織り込めるか**を試したものだった。

しかし実測では、
- Spearman 相関は改善しない
- 平均順位ずれはむしろ悪化する
- 最大利益の歪みも大きくなる

という結果になった。

つまり、
**評価式の側で補正を入れても、現行案 Off の制度的な段差そのものは解消できなかった**
と見てよい。

その一方で、改善案Aでは本戦不出場Apexを総合順位へ挿入しないため、
Innov の順位開始位置が不必要に後ろへ押し下げられない。

その結果、
- Innov 側の制度的不利益が大きく減る
- 順位相関が回復する
- 平均順位ずれも、通常ケースとほぼ同程度へ戻る

特に重要なのは、
**本戦不出場Apexありケースで悪化していた品質が、改善案Aでほぼ通常ケース水準まで戻り、
品質評価補正ではそこまで戻らなかった**
ことである。

## 変わらなかった点
- Elo1位の総合1位確率は 3 ケースとも大差ない
- 最大不利益の参加者は依然として `飛`

つまり改善案Aは、
「最強者選抜の弱さ」を解決する案ではない。

改善案Aが解決するのは、
**本戦不出場Apexによる制度的な段差の増幅**
である。

一方、品質評価補正が解決しようとしたのは
**評価時の比較基準順位の取り方**
だが、少なくともこのケースでは有効な改善にはならなかった。

したがって、改善案Aの役割は明確である。

- 解決できるもの
  - 本戦不出場Apexが Innov を不必要に押し下げる問題
  - 相関悪化
  - 平均順位ずれ悪化

- まだ残るもの
  - Apex 内順位の平滑化
  - Elo1位が総合1位を取り切りにくい問題
  - Apex / Innov 境界そのものの段差

## 総括
改善案Aは、
**「本戦不出場Apexを総合順位へ挿入しない」だけで、
制度品質を大きく改善できる有力案**
である。

今回追加した品質評価パラメーターは、
**評価の見方を変える実験としては意味があるが、現行案 Off を良案と見なせるほどの改善は示さなかった**。

少なくとも今回の比較では、
- 現行案 Off は Bad
- 現行案 Off + 品質評価補正On も Bad
- 改善案A On は Good
と分類してよい。

## 次にやるとよいこと
1. 改善案Aを他ケースにも適用して、効果が一貫するか確認する
2. 品質評価補正は、別ケースで有効になる条件があるかを限定的に確認する
3. その上で、境界救済戦や可変枠のような次の改善案を試す
4. 改善案Aを標準ルール候補として扱うか検討する

以上。
