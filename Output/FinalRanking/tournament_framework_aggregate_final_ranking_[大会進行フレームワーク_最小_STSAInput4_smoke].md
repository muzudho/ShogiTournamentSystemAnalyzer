# 最終順位結果レポート

## 概要
- 結果CSV: [tournament_framework_aggregate_final_ranking_[大会進行フレームワーク_最小_STSAInput4_smoke].csv](tournament_framework_aggregate_final_ranking_[大会進行フレームワーク_最小_STSAInput4_smoke].csv)
- 版: 標準版
- 計算モード: 厳密計算 / 大会進行フレームワーク / FixedMatch / Neutral
- 同Elo対局時の先手勝率: 51.00%
- 対象選手数: 4
- representative順位表: [tournament_framework_representative_final_ranking_20260606_192413.md](../Ranking/FinalRanking/tournament_framework_representative_final_ranking_20260606_192413.md)
- 注記: この順位表は複数回試行の aggregate 結果です。下記の大会最終状態テーブルとは 1 対 1 には対応しません。

## 上位候補一覧
| 選手 | 元Elo | 実効Elo | 差分 | 優勝確率 | 平均順位 |
| --- | ---: | ---: | ---: | ---: | ---: |
| Carol | 1520 | 1520 | 0 | 29.57% | 2.361 |
| Alice | 1500 | 1507 | +7 | 26.41% | 2.459 |
| Bob | 1480 | 1480 | 0 | 23.61% | 2.532 |
| Dave | 1470 | 1463 | -7 | 20.40% | 2.648 |

## 注目ポイント
- 優勝確率が最も高い選手: **Carol**（29.57%）
- 平均順位が最も良い選手: **Carol**（2.361）
- 実効Elo差分が最も大きくプラスの選手: **Alice**（+7）
- 実効Elo差分が最も大きくマイナスの選手: **Dave**（-7）

## 自動コメント
- 優勝候補の強さ: そこそこ確保されています。
- 先頭の平均順位: 比較的前寄りです。
- 実効Eloの押し上げ: 割り当てや対戦構成の影響はかなり小さめです。



## Mermaid 図
```mermaid
xychart-beta
    title "上位候補の優勝確率"
    x-axis [Carol, Alice, Bob, Dave]
    y-axis "" 
    bar [29.57, 26.41, 23.61, 20.40]
```

```mermaid
xychart-beta
    title "上位候補の平均順位"
    x-axis [Carol, Alice, Bob, Dave]
    y-axis "" 
    bar [2.361, 2.459, 2.532, 2.648]
```
