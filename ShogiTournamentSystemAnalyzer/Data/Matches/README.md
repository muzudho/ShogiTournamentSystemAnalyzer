# Matches

このフォルダーは、再利用する対局表や参考対局表の CSV を置く場所です。

## 今ある例
- `【対局表】Case1.csv`
- `【対局表】[本戦専用_トップ集団大きめ].csv`
- `【対局表】[本戦専用_トップ集団小さめ].csv`
- `【対局表】[黒4x白4].csv`
- `【対局表】[黒8x白8].csv`
- `【参考対局表】[黒8x白8_本戦不出場Apexあり].csv`

## 今後の置き方の例
- ケース別
- 年度別
- 実験テーマ別

例:
- `matches_[黒8x白8].csv`
- `matches_[本戦専用_トップ集団小さめ].csv`
- `reference_matches_[黒8x白8_本戦不出場Apexあり].csv`

## 位置づけ
- `Data/Inputs`: 実行用入力ファイル
- `Data/Players`: 再利用する選手データ
- `Data/Matches`: 再利用する対局データ
- `Data/FinalStage`: 本戦専用の補助データ
- `Data/RuleSets`: 保存して再利用する大会ルール
