# docs 運用ルール

## 目的

`docs` は、会話や作業で決まったことを人間が後から探せる形で残す場所です。

メモを残すときは、会話ログ全文ではなく、次の情報に絞って要約します。

- 決まった方針
- 未決事項
- 次にやる作業
- 調査結果
- 実行した確認コマンドと結果

## キープする基本構成

`docs` 直下は、次の 2 つを目立つ入口として維持します。

- `README.md`
  - 文書全体の地図
- `続きはここから.md`
  - 作業再開時に最初に見る引き継ぎメモ

この 2 つの隣に、作業途中メモや調査メモを増やさない方針にします。詳細メモは `企画` / `設計` / `開発` / `調査` / `運用` の該当フォルダーへ置きます。

## 文字コードと改行

Windows で `docs` 配下のテキストファイルを書き換えるときは、UTF-8 BOM なし、CRLF 改行を基本にします。Windows 式の CRLF と Linux 式の LF を混在させず、ファイル全体を CRLF に統一します。既存ファイルに明確な規約がある場合は、その規約を維持します。

PowerShell で書き戻すときは、`Set-Content -Encoding UTF8` ではなく、BOM なしを明示した `.NET` API を使います。

```powershell
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($path, $text, $utf8NoBom)
```

BOM の有無は先頭 3 バイトで確認します。

```powershell
$bytes = [System.IO.File]::ReadAllBytes($path)
$hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
```

`$hasBom` が `False` なら UTF-8 BOM なしです。

## PowerShell のパス指定

PowerShell では、ファイルパス中の `[` と `]` がワイルドカード文字として解釈されます。

このリポジトリには `Inputs/Smoke/analysis_input_[先手8x後手8]_[Simulation_QualityEvaluation_STSAInput5_smoke].request.txt` のように角括弧を含むファイル名が多いため、`Get-Content`、`Test-Path`、`Remove-Item`、`Copy-Item` などで実在パスを指定するときは `-Path` ではなく `-LiteralPath` を優先します。

```powershell
Get-Content -Raw -LiteralPath "Inputs/Smoke/analysis_input_[先手8x後手8]_[Simulation_QualityEvaluation_STSAInput5_smoke].request.txt"
Test-Path -LiteralPath "Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/[先手8x後手8]_[Simulation_QualityEvaluation_STSAInput5_smoke]_quality_summary.csv"
```

`-Path` を使うのは、明示的にワイルドカード検索をしたいときだけにします。

## 最初に見る場所

- `docs/README.md`
  - `docs` 全体の入口
- `docs/続きはここから.md`
  - 作業再開時に最初に見る、前回の到達点と次の候補を残すメモ
- 各フォルダーの `README.md`
  - そのフォルダー内の目次

## 置き場

### `docs/企画`

大会ルール案、発想メモ、原文、まだ実装へ落とす前の構想を置きます。

例:

- 新しい大会ルール案
- むずでょ原文
- ルールの一般化案

### `docs/設計`

利用者向け説明、仕様、設計として安定して参照したい文書を置きます。

例:

- 入力ファイル仕様
- STSAInput 仕様
- モード別ガイド
- ライフサイクル設計
- CSV と出力の説明

### `docs/開発`

実装計画、改修メモ、障害調査、スモークテスト記録、復旧メモを置きます。

例:

- 実装ステップ計画
- フリーズ復旧メモ
- タイムアウト調査メモ
- スモークテスト事前調査メモ

### `docs/調査`

まだ結論が固まっていない調査材料を置きます。

結論が固まったら、必要に応じて `docs/設計` または `docs/開発` へ要約を移します。

### `docs/運用`

文書管理、入力ファイル整理、出力ファイル整理など、リポジトリ運用の取り決めを置きます。

## `docs` 直下に置いてよいもの

`docs` 直下は入口だけにします。

- `README.md`
- `続きはここから.md`

続きはここから.md は、ほかのプロジェクトでも使える標準名として扱います。意味は Where We Left Off、つまり作業再開用の引き継ぎメモです。

作業途中メモ、調査メモ、スモークテスト記録は直下へ置かず、該当フォルダーへ入れます。

## 命名

ファイル名は、後から一覧で見たときに種類と日付が分かる形にします。

- 開発メモ: `{対象}実装計画_YYYYMMDD.md`
- 調査メモ: `{対象}調査メモ_YYYYMMDD.md`
- 復旧メモ: `{対象}復旧メモ_YYYYMMDD.md`
- スモークテスト: `SmokeTest{内容}メモ_YYYYMMDD.md`
- 原文: `【むずでょ原文】{題名}.md`
- 企画案: `{題名}案.md`

既存の連番ステップ文書は、現在の名前を維持します。

## 書き方

メモには、できるだけ次の見出しを入れます。

- `目的`
- `対象`
- `結論`
- `作業内容`
- `確認`
- `次にやること`

全部を必須にはしません。短いメモでは、必要な見出しだけで十分です。

## 編集権

### 人間専用ファイル

次のファイルは人間専用ファイルとして扱います。

- `docs/**/【むずでょ原文】*.md`

Codex は人間専用ファイルを読んでよいが、ユーザーが対象ファイルを明示して「この原文ファイルを編集して」と指示した場合を除き、変更してはなりません。

Codex が原文の内容を整理、要約、設計化、転記したい場合は、原文ファイルを直接編集せず、共同編集用の別ファイルを作成または更新します。

### 共同編集ファイル

人間専用ファイル以外の `docs/**/*.md` は、人間と Codex の共同編集対象とします。

Codex が共同編集ファイルを変更するときは、UTF-8 BOM なし、CRLF 改行を維持します。

## 更新ルール

- 入口である `docs/README.md` は、フォルダー構成が変わったときだけ更新します。
- 各フォルダーの `README.md` は、そのフォルダーへ重要な文書を追加したときに更新します。
- 一時メモが長くなったら、結論だけを設計文書や開発計画へ転記します。
- 古いメモはすぐ消さず、不要になった理由が明確なときだけ整理します。

## 判断に迷ったとき

- アイデアなら `docs/企画`
- 仕様として参照したいなら `docs/設計`
- 実装作業の続きなら `docs/開発`
- まだ調べている途中なら `docs/調査`
- 文書やリポジトリの置き方なら `docs/運用`