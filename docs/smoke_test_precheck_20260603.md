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
