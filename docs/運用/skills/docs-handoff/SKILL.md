---
name: docs-handoff
description: Use when a repository should keep shared Codex documentation and handoff conventions, especially Windows text file handling, docs/README.md as the docs map, docs/続きはここから.md as the resume checkpoint, and repo-local skill installation that avoids overwriting skills from other repositories.
---

# Docs Handoff

When editing text files in Windows repositories:

- Write text files with CRLF line endings.
- Write UTF-8 files without BOM.
- Preserve existing encoding and line-ending conventions when a file already has a clear convention.

When organizing `docs`:

- Keep `docs/README.md` as the map for the docs folder.
- Keep `docs/続きはここから.md` as the first file to open when resuming work.
- Treat `docs/続きはここから.md` as the Japanese equivalent of "Where We Left Off".
- Do not place random work notes directly under `docs`.
- Put detailed notes in the appropriate folder:
  - `docs/企画` for ideas, original proposals, and tournament-rule drafts.
  - `docs/設計` for stable specs, user-facing explanations, and design documents.
  - `docs/開発` for implementation plans, investigation notes, smoke tests, and recovery notes.
  - `docs/調査` for unresolved research.
  - `docs/運用` for repository and documentation operating rules.

When writing human-facing operating documents:

- Prefer "what the reader wants to do" over internal terminology as the entry point.
- Keep README files as short maps or signposts. Move detailed procedures into separate focused documents.
- Do not make readers consume broad operating rules just to run a specific tool.
- Use plain labels that make sense before the reader knows project-specific terms such as "Codex skill".
- When a term is unavoidable, explain it through the concrete action it enables.

When creating reusable Codex skills for a repository:

- Keep the repository-local source under a path like `docs/運用/skills/<skill-name>/SKILL.md`.
- Provide an install script under `tools/` instead of writing directly outside the workspace.
- Install into `$env:CODEX_HOME\skills` when `CODEX_HOME` is set; otherwise install into `$HOME\.codex\skills`.
- Avoid overwriting personal skills from other repositories. The installer should write metadata such as `.codex-skill-install.json` and refuse to overwrite an existing destination without matching metadata unless the user explicitly passes a force option.
- If force is used, back up the previous installed skill directory before replacing it.

When leaving a handoff:

- Summarize decisions, completed work, verification, and next steps.
- Prefer links to detailed notes over duplicating long content.
- Keep the checkpoint short enough to read before starting work.