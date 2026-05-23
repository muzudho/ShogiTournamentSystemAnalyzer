# docs

このフォルダーは、ShogiTournamentSystemAnalyzer の文書置き場です。  
README は最初の入口、`docs` は詳細説明と作業メモの置き場という役割で分けています。

## まずどこを見るか

- 使い方を知りたい
  - [Manuals](./Manuals/README.md)
- 実装予定や改修メモを見たい
  - [実装計画](./実装計画/README.md)
- 手書きのレポートや発表案を見たい
  - [Reports](./Reports/README.md)
- 発想メモや大会ルール案を見たい
  - [むずでょの案](./むずでょの案/)

## 置き場の考え方

- 利用者向けの説明書
  - `docs/Manuals`
- 開発向けの実装計画、設計メモ
  - `docs/実装計画`
- 手書きの比較メモ、発表案、総括レポート
  - `docs/Reports`
- アプリの実行で自動生成される CSV / Markdown
  - `ShogiTournamentSystemAnalyzer/Output/...`
- 実行用入力ファイル、再利用データ
  - `ShogiTournamentSystemAnalyzer/Data/...`

## フォルダー一覧

### [Manuals](./Manuals/README.md)
利用者向け説明書です。

- [説明書の総合目次](./Manuals/README.md)
- [トーナメントルール](./Manuals/トーナメントルール.md)
- [モード別ガイド](./Manuals/モード別ガイド.md)
- [入力ファイル仕様](./Manuals/入力ファイル仕様.md)
- [品質評価](./Manuals/品質評価.md)
- [CSVと出力](./Manuals/CSVと出力.md)
- [プロジェクトの考え方](./Manuals/プロジェクトの考え方.md)
- [実装ファイル案内](./Manuals/実装ファイル案内.md)
  - 実装場所をミニ目次付きで逆引きしたいとき

### [実装計画](./実装計画/README.md)
開発用の実装計画、設計メモ、改修方針を置く場所です。

- [実装計画の目次](./実装計画/README.md)

### [Reports](./Reports/README.md)
手書きの実験レポート、比較メモ、発表案を置く場所です。

- [Reports の案内](./Reports/README.md)
- [ツイル式トーナメント発表案](./Reports/ツイル式トーナメント発表案.md)

### [../ShogiTournamentSystemAnalyzer/Data](../ShogiTournamentSystemAnalyzer/Data/)
アプリで使う実行用入力、選手データ、再利用用ルールデータを置く場所です。

### [../ShogiTournamentSystemAnalyzer/Output](../ShogiTournamentSystemAnalyzer/Output/)
アプリの実行で自動生成される出力を置く場所です。

- `Ranking/FinalRanking`
- `Simulation/TournamentFinalState`
- `TournamentQualityEvaluator/TournamentQualityReport`

### [むずでょの案](./むずでょの案/)
大会ルール案、草案、発想メモを置く場所です。

### [notes.md](./notes.md)
補助メモです。

## 関連入口

- [トップ README](../README.md)
- [Manuals の総合目次](./Manuals/README.md)
- [実装ファイル案内](./Manuals/実装ファイル案内.md)
  - 「最初に読む順」「よくある修正例」「ミニ目次」から辿れます
