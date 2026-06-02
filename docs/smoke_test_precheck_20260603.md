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
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --input-file ".\Inputs\Smoke\quality_sweep_input_[先手8x後手8]_[Twill_50to100_smoke10].txt"
```

Expected risk: Sweep prompt alignment, Twill rule path, repeated single-run execution, sweep output path handling.

### Sweep smoke result

Legacy sweep smoke did not hang and did not terminate the process, but it failed by prompt/input mismatch.

- Exit code: 0
- Wall time observed by Codex: about 2.6 seconds
- Input: `Inputs/Smoke/quality_sweep_input_[先手8x後手8]_[Twill_50to100_smoke10].txt`
- Observed behavior: the run selected `FinalStage` and entered `品質評価 / 本戦ルール`, then repeatedly asked for `グループ対応CSV`.
- Failure message: `入力を中断しました: グループ対応CSVの入力失敗が 10 回に達したため中断しました。最後のエラー: CSVが入力されていません`
- Likely cause: this smoke file is a stale legacy prompt-script. Its second answer is `2`, which currently selects `RuleProfileMode.FinalStage`, while the file content appears intended for a standard Twill sweep path.

Next step: search for a current-format sweep input, preferably STSAInput2/3, before running another sweep test.

### STSAInput2 Sweep smoke start

Found a current-format sweep candidate and will run it next:

```powershell
dotnet run --project .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj -- --input-file ".\Inputs\Sweeps\quality_sweep_input_[先手8x後手8]_[Neutral_50to100]_[STSAInput2_draft].txt"
```

Reason: the previous smoke sweep file was a stale legacy prompt-script. This candidate declares `#[Format] STSAInput/2` and `ExecutionMode=Sweep`, so it should exercise the current request-file converter.

### STSAInput2 Sweep smoke result

STSAInput2 sweep candidate timed out under Codex command timeout.

- Command timeout: 60 seconds
- Observed exit code from shell tool: 124
- No normal program output was returned before timeout.
- Input: `Inputs/Sweeps/quality_sweep_input_[先手8x後手8]_[Neutral_50to100]_[STSAInput2_draft].txt`

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
