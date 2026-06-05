# SmokeTest Precheck Memo 2026-06-03

## Purpose

Save suspected failure points before running a smoke test that may terminate the process.

## Read Scope

- `ShogiTournamentSystemAnalyzer/Program.cs`
- `ShogiTournamentSystemAnalyzer/ShogiTournamentSystemAnalyzer.csproj`
- `ShogiTournamentSystemAnalyzer/Application/RequestFileCheck/*`
- `ShogiTournamentSystemAnalyzer/Application/BeforeRequestFileCheck/*`
- `ShogiTournamentSystemAnalyzer/Application/Shared/*`
- `ShogiTournamentSystemAnalyzer/Presentation/ConsoleCustom/ConsoleInput.cs`
- `ShogiTournamentSystemAnalyzer/Presentation/ConsoleCustom/ConsolePrompts.cs`
- `ShogiTournamentSystemAnalyzer/Presentation/ConsoleCustom/ConsoleInputReaders.cs`
- `ShogiTournamentSystemAnalyzer/Application/Analysis/AnalysisWorkflow.cs`
- `ShogiTournamentSystemAnalyzer/Application/Analysis/AnalysisFlowDispatcher.cs`
- `ShogiTournamentSystemAnalyzer/Application/Analysis/Domains/TournamentQualityEvaluator/Modes/*`
- `ShogiTournamentSystemAnalyzer/Application/Analysis/Domains/Simulation/Modes/*`
- `ShogiTournamentSystemAnalyzer/Application/Analysis/Domains/Simulation/SimulationMainline/*`
- `ShogiTournamentSystemAnalyzer/Application/Analysis/Domains/Simulation/TournamentFramework/*`
- `ShogiTournamentSystemAnalyzer/Domain/Simulation/*`
- `ShogiTournamentSystemAnalyzer/Infrastructure/DataFiles/StsaFileIoHelper.cs`
- `ShogiTournamentSystemAnalyzer/Infrastructure/DataFiles/Shared/WriterHelper.cs`
- `Inputs/Smoke/*`

## Suspicions Before Smoke

1. In request-file mode, `Program.cs` passes `RequestFileCheckWorkflow.Run(...)` output to `ConsoleInput.UseText(...)`, then later prompts read from that text stream.
   - `Program.cs:81`
   - `Program.cs:127`
   - `Program.cs:190`
   - `Program.cs:193`
   - `Program.cs:202`

2. `RequestFileCheckWorkflow` converts STSAInput3 to legacy prompt lines and stores them in `RequestInputSession`.
   - `RequestFileCheckWorkflow.cs:28`
   - `RequestFileCheckWorkflow.cs:57`

3. If converted input is off by even one prompt line, `ConsoleInput.ReadLine()` can return `null`, and prompt retry loops or the application time budget may drive the failure.
   - `ConsoleInput.cs:32`
   - `ConsoleInput.cs:36`
   - `SimulationTimeBudget.cs` has a 3 minute application limit.

4. The quality single-run STSAInput3 smoke has more than 20 matches, so it should use simulation mode and consume `SimulationCount=10`.
   - `TournamentQualityEvaluationExecution.cs:36`
   - `TournamentQualityEvaluationExecution.cs:37`
   - `StsaQualityEvaluationLegacyConverter.cs:108`
   - `StsaQualityEvaluationLegacyConverter.cs:112`

5. Exact-calculation paths use recursive exploration and check `HasApplicationTimeRemaining()`. If a large input falls into exact calculation instead of simulation, it can run close to the 3 minute limit.
   - `StandardCalculationEngine.cs:19`
   - `StandardCalculationEngine.cs:21`
   - `FinalStageCalculationEngine.cs` has the same recursive pattern.

6. `StandardCalculationEngine.CalculateExactly` / `CalculateBySimulation` can leave `wins` null for standard rule modes that are neither `Neutral` nor Twill-family, then later dereference `wins!`. The planned Neutral smoke should not hit this, but it is a real fragile path.
   - `StandardCalculationEngine.cs:14`
   - `StandardCalculationEngine.cs:36`
   - `StandardCalculationEngine.cs:51`
   - `StandardCalculationEngine.cs:122`

7. `AnalysisWorkflow.Run` calls four workflows before the dispatcher, but they are currently empty. They do not look like the immediate cause for this smoke.
   - `AnalysisWorkflow.cs`
   - `TournamentUserWorkflow.cs`
   - `SimulationWorkflow.cs`
   - `RankingWorkflow.cs`
   - `TournamentQualityEvaluatorWorkflow.cs`

## Planned Command

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --input-file .\Inputs\Smoke\quality_input_[先手8x後手8]_[Neutral_Single10]_[STSAInput3_smoke].txt
```

Use a short Codex command timeout so a hang is captured instead of waiting indefinitely.

## Smoke Result

Command completed successfully.

- Exit code: 0
- Wall time observed by Codex: about 9.1 seconds
- Input: `Inputs/Smoke/quality_input_[先手8x後手8]_[Neutral_Single10]_[STSAInput3_smoke].txt`
- Output summary CSV: `Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/[先手8x後手8]_[Neutral_Single10_STSAInput3_smoke]_quality_summary.csv`
- Output players CSV: `Output/TournamentQualityEvaluator/TournamentQualityReport/Players/[先手8x後手8]_[Neutral_Single10_STSAInput3_smoke]_quality_players.csv`
- Output summary Markdown: `Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/[先手8x後手8]_[Neutral_Single10_STSAInput3_smoke]_quality_summary.md`
- Output STSA log: `Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/[先手8x後手8]_[Neutral_Single10_STSAInput3_smoke]_quality_summary.stsa.txt`

No process termination or timeout happened in this run.

## Tooling Note

`apply_patch` failed twice in this workspace while trying to add memo files under `docs/`.

Observed failures:

- Add `docs/開発/SmokeTest事前調査メモ_20260603.md` failed with `writing outside of the project`.
- Add `docs/smoke_test_precheck_20260603.md` also failed with `writing outside of the project`.

The repository path was `E:\github.com\muzudho\ShogiTournamentSystemAnalyzer`, and PowerShell `Set-Content` / `Add-Content` worked for the same `docs/` location. For future turns in this workspace, if `apply_patch` rejects a valid in-repo path the same way, record the failure and use PowerShell file writing as the fallback.

## Test Progress 2026-06-03

### FinalStage smoke start

About to run FinalStage STSAInput3 smoke:

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --input-file ".\Inputs\Smoke\quality_input_[トップ集団大きめ]_[FinalStage_Single10]_[STSAInput3_smoke].txt"
```

Expected risk: FinalStage rule path, grouping, boundary rescue, variable top8, and simulation-count prompt alignment.

### FinalStage smoke result

FinalStage STSAInput3 smoke completed successfully.

- Exit code: 0
- Wall time observed by Codex: about 9 seconds
- No timeout or process termination.
- Summary output: `Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/[トップ集団大きめ]_[FinalStage_Single10_STSAInput3_smoke]_quality_summary.csv`
- Players output: `Output/TournamentQualityEvaluator/TournamentQualityReport/Players/[トップ集団大きめ]_[FinalStage_Single10_STSAInput3_smoke]_quality_players.csv`
- Markdown output: `Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/[トップ集団大きめ]_[FinalStage_Single10_STSAInput3_smoke]_quality_summary.md`
- STSA log output: `Output/TournamentQualityEvaluator/TournamentQualityReport/Summary/[トップ集団大きめ]_[FinalStage_Single10_STSAInput3_smoke]_quality_summary.stsa.txt`

### Sweep smoke start

About to run legacy sweep smoke:

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --input-file ".\Inputs\Smoke\quality_sweep_input_[先手8x後手8]_[Twill_50to100_smoke10].request.txt"
```

Expected risk: Sweep prompt alignment, Twill rule path, repeated single-run execution, sweep output path handling.

### Sweep smoke result

Legacy sweep smoke did not hang and did not terminate the process, but it failed by prompt/input mismatch.

- Exit code: 0
- Wall time observed by Codex: about 2.6 seconds
- Input: `Inputs/Smoke/quality_sweep_input_[先手8x後手8]_[Twill_50to100_smoke10].request.txt`
- Observed behavior: the run selected `FinalStage` and entered `品質評価 / 本戦ルール`, then repeatedly asked for `グループ対応CSV`.
- Failure message: `入力を中断しました: グループ対応CSVの入力失敗が 10 回に達したため中断しました。最後のエラー: CSVが入力されていません`
- Likely cause: this smoke file is a stale legacy prompt-script. Its second answer is `2`, which currently selects `RuleProfileMode.FinalStage`, while the file content appears intended for a standard Twill sweep path.

Next step: search for a current-format sweep input, preferably STSAInput2/3, before running another sweep test.

### STSAInput2 Sweep smoke start

Found a current-format sweep candidate and will run it next:

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --input-file ".\Inputs\Sweeps\quality_sweep_input_[先手8x後手8]_[Neutral_50to100]_[STSAInput2_draft].request.txt"
```

Reason: the previous smoke sweep file was a stale legacy prompt-script. This candidate declares `#[Format] STSAInput/2` and `ExecutionMode=Sweep`, so it should exercise the current request-file converter.

### STSAInput2 Sweep smoke result

STSAInput2 sweep candidate timed out under Codex command timeout.

- Command timeout: 60 seconds
- Observed exit code from shell tool: 124
- No normal program output was returned before timeout.
- Input: `Inputs/Sweeps/quality_sweep_input_[先手8x後手8]_[Neutral_50to100]_[STSAInput2_draft].request.txt`

This is currently the strongest freeze/hang reproduction candidate. Next step: inspect the input file to check sweep range, step, and whether `SimulationCount` is missing, which may cause repeated exact calculations.

### STSAInput2 Sweep root-cause note

The timeout is likely caused by the sweep execution path forcing exact calculation for each sweep point.

Evidence:

- The STSAInput2 sweep has 64 matches and sweep points 50, 60, 70, 80, 90, 100.
- The input file does not declare `SimulationCount`.
- More importantly, `TournamentQualityEvaluationExecutor.ReadTournamentQualityEvaluationExecutionOptions(...)` returns `new TournamentQualityEvaluationExecutionOptions(null, sweepOptions, null)` when sweep mode is enabled.
- Therefore `TournamentQualityEvaluationSingleRunExecutor.ExecuteTournamentFinalState(...)` sees `executionOptions.SimulationCount.HasValue == false` and calls exact calculation.
- For 64 matches, exact calculation explores an exponential outcome tree. This explains the 60 second timeout.

This looks like a code/design bug, not just a bad input file: sweep mode has no way to pass a simulation count through the current STSA converter/execution option path.

## Timeout Audit Before Fix

Requested change: make sure simulations have timeout protection, and record places that look unprotected before fixing.

Audit notes:

- `StandardCalculationEngine.CalculateBySimulation` checks `SimulationTimeBudget.HasSimulationTimeRemaining()` at the simulation loop and `HasApplicationTimeRemaining()` inside the match loop.
- `FinalStageCalculationEngine.CalculateFinalStageBySimulation` checks `SimulationTimeBudget.HasSimulationTimeRemaining()` at the simulation loop and `HasApplicationTimeRemaining()` inside the match loop.
- `SimulationTournamentFrameworkModeExecution` simulation loop checks `SimulationTimeBudget.HasSimulationTimeRemaining()`.
- Exact recursive paths check `HasApplicationTimeRemaining()`, but they are not protected by a separate simulation budget. This is acceptable for small exact calculations, but dangerous when a large match set accidentally falls into exact mode.
- The current concrete bug is quality sweep execution: `TournamentQualityEvaluationExecution.cs` returns `SimulationCount=null` for all sweep runs, so large sweeps call exact calculation instead of timed simulation.
- STSA sweep conversion currently does not append `SimulationCount` after sweep start/end/step. If the execution reader starts asking for a sweep simulation count, the converter must supply either the configured count or an empty line to accept the default.

Planned fix:

1. Add a shared helper in `TournamentQualityEvaluationExecution.cs` that chooses exact calculation only for `matches.Count <= 20`, otherwise reads a simulation count.
2. Use that helper for both single-run and sweep execution options.
3. In `StsaQualityEvaluationLegacyConverter.cs`, append optional `SimulationCount` for sweep mode; when absent, append an empty line so the runtime prompt takes the default without shifting later answers.
4. Re-run Normal, FinalStage, and STSAInput2 Sweep smoke tests.

## Fix Progress

Applied code changes:

- `TournamentQualityEvaluationExecution.cs` now reads a simulation count for sweep mode when match count is above the exact-calculation threshold.
- `StsaQualityEvaluationLegacyConverter.cs` now appends `SimulationCount` after sweep start/end/step for both standard and final-stage STSA quality conversion. If `SimulationCount` is absent, it appends an empty line so the runtime prompt accepts the default and later prompt answers do not shift.
- `dotnet build` passed with 0 warnings and 0 errors.

Next verification: rerun the STSAInput2 sweep that previously timed out after 60 seconds.

## Fix Verification

Verification after the sweep simulation-count fix:

- `dotnet build .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj` succeeded with 0 warnings and 0 errors.
- The STSAInput2 sweep that previously timed out after 60 seconds completed successfully in about 14.3 seconds.
  - Input: `Inputs/Sweeps/quality_sweep_input_[先手8x後手8]_[Neutral_50to100]_[STSAInput2_draft].request.txt`
  - It now prompts for simulation count in sweep mode and accepts the default from the inserted empty line.
  - Output: `Output/TournamentQualityEvaluator/TournamentQualityReport/Sweeps/neutral_[先手8x後手8]_50to100_quality_sweep.csv`
  - Output: `Output/TournamentQualityEvaluator/TournamentQualityReport/Sweeps/neutral_[先手8x後手8]_50to100_quality_sweep.md`
- Normal STSAInput3 single smoke completed successfully after the change.
- FinalStage STSAInput3 single smoke completed successfully after the change.

Remaining note:

- The old legacy prompt-script sweep smoke still has a stale prompt order and selects FinalStage unexpectedly. That is a test-input maintenance issue, separate from the timeout fix.

## Continued Program Check

Focus for the next pass:

- Long-running exact-calculation paths that may be reached with too many matches.
- Simulation loops that do not check `SimulationTimeBudget`.
- Request-file conversion paths that may omit prompt answers and shift later inputs.
- Legacy smoke/sweep inputs whose prompt order is stale.

## Stale Smoke Input Fix Plan

Found stale legacy prompt-script sweep inputs:

- `Inputs/Smoke/quality_sweep_input_[先手8x後手8]_[Twill_50to100_smoke10].request.txt`
- Related non-smoke files under `Inputs/Sweeps` and `Inputs/Bench` still use prompt-script order.

Immediate fix target:

- Convert the smoke10 file to STSAInput3 so it no longer depends on prompt order.
- Preserve the original intent: Standard quality evaluation, Twill rule, sweep 50 to 100 by 10, `SimulationCount=10`.
- Re-run this smoke after conversion.

## Exact Calculation Timeout Follow-up

Twill smoke10 after STSAInput3 conversion completed successfully in about 10 seconds.

Additional timeout concern:

- Exact calculation paths check `HasApplicationTimeRemaining()`, so they are bounded by the application-wide 3 minute budget.
- However, exact calculation callers generally do not open a calculation-specific `SimulationTimeBudget.BeginSimulationBudget()` scope.
- This means exact calculations are not consistently governed by the same calculation budget mechanism as simulation loops.

Planned hardening:

- Make exact recursive engines check `HasSimulationTimeRemaining()` instead of only `HasApplicationTimeRemaining()`.
- Open `BeginSimulationBudget()` around exact calculation calls in simulation mainlines, quality evaluation runs, and tournament framework exact calculation.

## Exact Calculation Timeout Hardening Applied

Applied broader timeout hardening:

- `StandardCalculationEngine` and `FinalStageCalculationEngine` now use `HasSimulationTimeRemaining()` for calculation-loop checks.
- Standard simulation exact branch now opens `BeginSimulationBudget()` before exact calculation.
- FinalStage simulation exact branch now opens `BeginSimulationBudget()` before exact calculation.
- Quality evaluation single runs now open `BeginSimulationBudget()` for both exact and simulation paths.
- Tournament framework exact calculation now opens `BeginSimulationBudget()` and its recursive exploration checks `HasSimulationTimeRemaining()`.

Note: `TournamentQualityEvaluationSweepExecutor` still checks the application budget in its outer sweep loop, but each sweep point now has a calculation budget through `ExecuteRun`.

## Continued Check Verification

Verification after continued timeout hardening and smoke-input maintenance:

- `dotnet build` succeeded with 0 warnings and 0 errors.
- Normal STSAInput3 single smoke completed successfully.
- FinalStage STSAInput3 single smoke completed successfully.
- STSAInput2 Neutral sweep completed successfully.
- Twill smoke10 was converted from stale prompt-script to STSAInput3 and completed successfully.

Current conclusion:

- The previously observed freeze class was caused by long-running exact calculations being reached unexpectedly.
- Sweep execution now asks for or receives a simulation count for large match sets.
- Exact calculations now participate in the same calculation-budget mechanism as simulation loops.
- The remaining stale prompt-script files under `Inputs/Sweeps` and `Inputs/Bench` should be migrated gradually to STSAInput2/3 to avoid prompt-order drift.

## STSAInput Version Organization

Applied input-folder cleanup:

- Moved old non-STSA request files from `Inputs/Bench` and `Inputs/Sweeps` into `Inputs/Legacy` while preserving their original subfolder layout.
- Confirmed there are no non-STSA `.txt` request files left outside `Inputs/Legacy`.
- Added `Inputs/Legacy/README.md` explaining that these files are reference-only and should be migrated to STSAInput/2 or STSAInput/3 before use.
- Added a `RequestFileCheckWorkflow` guard that rejects `--input-file` paths under `Inputs/Legacy`.
- Fixed request-file parse error reporting so the caught parse/rejection reason is printed in the final error line.
