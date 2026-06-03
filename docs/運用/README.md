# 運用

このフォルダーは、文書・入力ファイル・出力ファイルの置き方など、リポジトリ運用の取り決めを置く場所です。

## ファイル一覧

- [docs運用ルール](./docs運用ルール.md)
  - `docs` 配下へメモを残すときの置き場、命名、更新方針
- [skills/docs-handoff/SKILL.md](./skills/docs-handoff/SKILL.md)
  - 他リポジトリでも使いたい Codex 用の docs 引き継ぎルール

## Codex skill のインストール

リポジトリ内の skill 原本を個人用 Codex skills へ入れるときは、次を実行します。

```powershell
.\tools\install-codex-skills.ps1
```

既存の同名 skill に、このリポジトリから入れた管理メタデータが無い場合は上書きしません。明示的に入れ替える場合だけ、バックアップ付きで次を使います。

```powershell
.\tools\install-codex-skills.ps1 -Force
```

## 関連入口

- [docs フォルダー案内](../README.md)
- [トップ README](../../README.md)