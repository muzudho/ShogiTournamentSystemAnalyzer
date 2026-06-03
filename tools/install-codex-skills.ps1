# `install-codex-skills.ps1` コマンドは次の引数を受け取るぜ（＾▽＾）
param(
    [string]$SkillName = "docs-handoff",
    [switch]$Force
)

# このスクリプトの実行中にエラーが有ったら、そこで止まってな（＾▽＾）
$ErrorActionPreference = "Stop"

# このディレクトリーから見て、いろんなディレクトリーやファイルのちゃんとしたパスを作ってるぜ（＾～＾）
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$sourceRoot = Join-Path $repoRoot "docs\運用\skills"
$sourceSkill = Join-Path $sourceRoot $SkillName
$sourceSkillMd = Join-Path $sourceSkill "SKILL.md"

# ［スキル」ファイル無いの（＾～＾）！？　アカンやん、投了だぜ（＾～＾）
if (-not (Test-Path -LiteralPath $sourceSkillMd)) {
    throw "Skill source not found: $sourceSkillMd"
}

# コーデックス用の隠しフォルダーのパスだぜ（＾～＾）ここに［スキル］ファイルを置いていくぜ（＾～＾）
$codexHome = $env:CODEX_HOME
if ([string]::IsNullOrWhiteSpace($codexHome)) {
    $codexHome = Join-Path $HOME ".codex"
}

# ディレクトリーとか、ファイルのパスを作ってるぜ（＾～＾）
#
#   - $destinationSkill: `C:\Users\YourName\.codex\skills\docs-handoff` みたいな名前のフォルダー。
#   - $metadataPath: コピー先の［スキル］フォルダー内の `.codex-skill-install.json` ファイル。
#                    どのリポジトリーがそのスキルを置いたのか、あとで判別できるようにする仕込みだぜ（＾▽＾）
#
$destinationRoot = Join-Path $codexHome "skills"
$destinationSkill = Join-Path $destinationRoot $SkillName
$metadataPath = Join-Path $destinationSkill ".codex-skill-install.json"
$repoRootText = [string]$repoRoot
$sourceSkillText = [string](Resolve-Path $sourceSkill)

# ディレクトリーを作成するぜ（＾▽＾）　すでにあっても大丈夫だぜ（＾▽＾）
New-Item -ItemType Directory -Force -Path $destinationRoot | Out-Null

# コピー先の［スキル］フォルダーへアクセスできるかテストしてから、既存のファイルを削除か、退避等をする作業を始めるぜ（＾▽＾）
if (Test-Path -LiteralPath $destinationSkill) {
    # コピー先にファイルが既存なら、デフォルトでは上書き禁止（＾～＾）
    $canReplace = $false

    # ［メタデータ・ファイル］の保存先にアクセスできるかテスト（＾～＾）
    if (Test-Path -LiteralPath $metadataPath) {
        # ［メタデータ・ファイル］を読み取って、このスキルがこのリポジトリからコピーされたものか確認するぜ（＾～＾）
        $metadata = Get-Content -Raw -LiteralPath $metadataPath | ConvertFrom-Json
        if ($metadata.sourceRepository -eq $repoRootText -and $metadata.skillName -eq $SkillName) {
            # このリポジトリーからコピーされた［スキル］ファイルなので、上書きしても大丈夫だぜ（＾▽＾）
            $canReplace = $true
        }
    }

    # 上書きできない場合で、"-Force"（強制上書き）の指定がなければ、エラーを出して中止するぜ（＾～＾）
    if (-not $canReplace -and -not $Force) {
        throw "Refusing to overwrite existing skill without matching install metadata: $destinationSkill`nUse -Force to back it up and replace it explicitly."
    }

    # 上書きする前に、その古いファイルは名前を変えてバックアップしておくぜ（＾～＾）　メタデータで同じリポジトリーのスキルと判別できておらず、強制上書きするケースならだぜ（＾～＾）
    if ($Force -and -not $canReplace) {
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupPath = "$destinationSkill.backup_$timestamp"
        Move-Item -LiteralPath $destinationSkill -Destination $backupPath
        Write-Host "Backed up existing skill to: $backupPath"
    }
    # 上書きできるんなら、古いファイルは消したろ（＾～＾）
    else {
        Remove-Item -LiteralPath $destinationSkill -Recurse -Force
    }
}

# このリポジトリーに置いてある［スキル］フォルダー丸ごとを、他のリポジトリー担当のコーデックスも読めるローカルフォルダーへ、コピーするぜ（＾▽＾）
#
#   - その中には `SKILL.md` や `agents/openai.yaml` とか、いろんなファイルが入ってるぜ（＾～＾）
#
Copy-Item -LiteralPath $sourceSkill -Destination $destinationSkill -Recurse

# 名前と値のリスト形式だぜ（＾▽＾）　あとで JSON 形式へ変換するぜ（＾▽＾）　内容は、スキル名とこのスクリプトを実行したリポジトリのルートパスだぜ（＾▽＾）　つまり、メタデータだぜ（＾▽＾）
$metadataObject = [ordered]@{
    skillName = $SkillName
    sourceRepository = $repoRootText
    sourceSkillPath = $sourceSkillText
    installedAt = (Get-Date).ToString("o")
    installer = "tools/install-codex-skills.ps1"
}

# メタデータを JSON 形式のファイルとして書き出すぜ（＾▽＾）
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$metadataJson = ($metadataObject | ConvertTo-Json -Depth 4)
[System.IO.File]::WriteAllText($metadataPath, $metadataJson + "`r`n", $utf8NoBom)

# 完了したぜ（＾▽＾）　説明を表示するぜ（＾▽＾）
Write-Host "Installed Codex skill: $SkillName"
Write-Host "Destination: $destinationSkill"
