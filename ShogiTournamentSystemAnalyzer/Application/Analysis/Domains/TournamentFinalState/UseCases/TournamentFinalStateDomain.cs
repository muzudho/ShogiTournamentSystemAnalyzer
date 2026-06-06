/*
 * ［アプリケーション　＞　ユースケース　＞　大会最終状態域］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Analysis.Domains.TournamentFinalState.UseCases;

using ShogiTournamentSystemAnalyzer.Application.Analysis.Boundaries;
using ShogiTournamentSystemAnalyzer.Domain.Simulation;
using ShogiTournamentSystemAnalyzer.Domain.TournamentFinalState;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.TournamentFinalState;

internal static class TournamentFinalStateDomain
{
    internal static (string CsvPath, string MarkdownPath) BuildTournamentFrameworkRepresentativeOutputPaths()
    {
        var csvPath = ReportOutputPathBuilder.BuildTournamentFinalStateDefaultOutputPath($"representative_tournament_final_state_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var markdownPath = CsvOutputHelpers.ChangeOutputExtension(csvPath, ".md");
        return (csvPath, markdownPath);
    }

    internal static void WriteTournamentFrameworkRepresentativeOutputs(
        IReadOnlyList<StageEntry> stages,
        IReadOnlyList<PlayerEntry> players,
        TournamentFinalStateData tournamentFinalStateData,
        string csvPath,
        string markdownPath,
        string aggregateResultMarkdownPath,
        string representativeRankingMarkdownPath)
    {
        const string TournamentFinalStateOverviewNote = "この大会最終状態テーブルは代表実行 1 件の対局記録です。順位表の aggregate 結果そのものではありません。";

        WriterHelper.WriteText(
            outputPath: csvPath,
            getLines: () => TournamentFinalStateDataFileWriter.CreateTournamentMatchRecordCsv(
                stages,
                players,
                tournamentFinalStateData.MatchRecords,
                overviewNote: TournamentFinalStateOverviewNote));

        WriterHelper.WriteText(
            outputPath: markdownPath,
            getLines: () => TournamentFinalStateDataFileWriter.CreateTournamentMatchRecordMarkdown(
                markdownPath,
                csvPath,
                stages,
                players,
                tournamentFinalStateData.MatchRecords,
                overviewNote: TournamentFinalStateOverviewNote,
                aggregateResultMarkdownPath: aggregateResultMarkdownPath,
                representativeRankingMarkdownPath: representativeRankingMarkdownPath));
    }
}
