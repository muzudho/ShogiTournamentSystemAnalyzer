# Legacy Inputs

This folder stores old request files that depend on historical console prompt order.

Files here are intentionally not part of the normal runnable input set. They may no longer match the current prompts and can cause input drift, repeated retries, or long-running calculations if used as-is.

Current policy:

- New runnable inputs should use `#[Format] STSAInput/2` or `#[Format] STSAInput/3`.
- Old prompt-script files should be migrated to STSAInput/3 when they are still useful.
- Files that are too old to trust should stay in this folder as reference material only.
- `RequestFileCheckWorkflow` rejects files under `Inputs/Legacy` when passed with `--input-file`.
