# 最終順位結果レポート

## 概要
- 結果CSV: [simulation_input_[先手1x後手1]_[Neutral_Exact]_[STSAInput4_smoke].csv](simulation_input_[先手1x後手1]_[Neutral_Exact]_[STSAInput4_smoke].csv)
- 版: 標準版
- 計算モード: 厳密計算
- 同Elo対局時の先手勝率: 51.00%
- 対象選手数: 2

## 上位候補一覧
| 選手 | 元Elo | 実効Elo | 差分 | 優勝確率 | 平均順位 |
| --- | ---: | ---: | ---: | ---: | ---: |
| Alice | 1500 | 1507 | +7 | 53.87% | 1.461 |
| Bob | 1480 | 1473 | -7 | 46.13% | 1.539 |

## 注目ポイント
- 優勝確率が最も高い選手: **Alice**（53.87%）
- 平均順位が最も良い選手: **Alice**（1.461）
- 実効Elo差分が最も大きくプラスの選手: **Alice**（+7）
- 実効Elo差分が最も大きくマイナスの選手: **Bob**（-7）

## 自動コメント
- 優勝候補の強さ: かなり強いです。
- 先頭の平均順位: かなり前寄りです。
- 実効Eloの押し上げ: 割り当てや対戦構成の影響はかなり小さめです。



## Mermaid 図
```mermaid
xychart-beta
    title "上位候補の優勝確率"
    x-axis [Alice, Bob]
    y-axis "" 
    bar [53.87, 46.13]
```

```mermaid
xychart-beta
    title "上位候補の平均順位"
    x-axis [Alice, Bob]
    y-axis "" 
    bar [1.461, 1.539]
```
