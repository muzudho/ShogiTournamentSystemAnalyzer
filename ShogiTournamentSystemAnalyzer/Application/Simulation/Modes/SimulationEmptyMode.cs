/*
 * ［アプリケーション　＞　モード］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Simulation.Modes;

using ShogiTournamentSystemAnalyzer.Application.FinalRanking.UseCases;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class SimulationEmptyMode
{
    internal static FinalRankingDomainInput Run()
    {
        Console.WriteLine("対局シミュレーション / 空ルール: ペアリングを一切行わず、大会最終状態 0 件の最小結果を出力します。\n");
        ConsoleSamplePrinter.PrintSimulationEmptyOverview();
        return RunMainlineToEmptyTournamentFinalState(null);
    }

    internal static FinalRankingDomainInput Run(string? outputPathOverride)
    {
        Console.WriteLine("対局シミュレーション / 空ルール: ペアリングを一切行わず、大会最終状態 0 件の最小結果を出力します。\n");
        return RunMainlineToEmptyTournamentFinalState(outputPathOverride);
    }

    static FinalRankingDomainInput RunMainlineToEmptyTournamentFinalState(string? outputPathOverride)
    {
        return ExecuteEmptyMode(outputPathOverride);
    }

    static FinalRankingDomainInput ExecuteEmptyMode(string? outputPathOverride)
    {
        const string mode = "空ルール / ペアリング0回 / 大会最終状態0件";
        const int pairingCount = 0;
        const int tournamentMatchRecordCount = 0;

        Console.WriteLine($"計算方法: {mode}\n");
        Console.WriteLine($"総ペアリング数: {pairingCount}");
        Console.WriteLine($"大会最終状態件数: {tournamentMatchRecordCount}\n");

        return new FinalRankingDomainInput(
            FinalRankingDomainInputKind.EmptySimulation,
            null,
            0,
            null,
            outputPathOverride,
            Array.Empty<Player>(),
            Array.Empty<Match>(),
            WriteReferenceMatchesForMarkdown: false,
            EmptyFinalRankingInput: new EmptyFinalRankingDomainInput(outputPathOverride));
    }
}
