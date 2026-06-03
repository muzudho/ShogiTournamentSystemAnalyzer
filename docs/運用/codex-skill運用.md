# Codex skill 運用

このドキュメントは、リポジトリ内の Codex skill 原本を、個人用 Codex skills へ入れる手順をまとめます。

## 原本

このリポジトリでは、Codex skill の原本を次に置きます。

```text
docs/運用/skills/
```

現在の主な skill は次です。

- [docs-handoff](./skills/docs-handoff/SKILL.md)

## インストール

通常はリポジトリルートで次を実行します。

```powershell
.\tools\install-codex-skills.ps1
```

引数を省略した場合は、`docs-handoff` をインストールします。

別の skill 名を指定する場合は、次のように実行します。

```powershell
.\tools\install-codex-skills.ps1 -SkillName docs-handoff
```

## インストール先

`CODEX_HOME` が設定されている場合は、次へ入ります。

```text
$env:CODEX_HOME/skills/<skill-name>
```

`CODEX_HOME` が未設定の場合は、次へ入ります。

```text
$HOME/.codex/skills/<skill-name>
```

## 上書き

既存の同名 skill がある場合、このリポジトリから入れたものだと判断できるときだけ自動で上書きします。

判断には、インストール先の `.codex-skill-install.json` にある次の値を使います。

- `skillName`
- `sourceRepository`

このメタデータが無い、または一致しない場合は、通常実行では止まります。

## 強制入れ替え

既存の同名 skill を明示的に入れ替える場合だけ、次を使います。

```powershell
.\tools\install-codex-skills.ps1 -Force
```

`-Force` を使うと、既存の skill フォルダーは削除せず、次のような名前へ退避してから新しいものをコピーします。

```text
<skill-name>.backup_yyyyMMdd_HHmmss
```

## 実行後の確認

実行が成功すると、インストールした skill 名とコピー先が表示されます。

```text
Installed Codex skill: docs-handoff
Destination: ...
```
