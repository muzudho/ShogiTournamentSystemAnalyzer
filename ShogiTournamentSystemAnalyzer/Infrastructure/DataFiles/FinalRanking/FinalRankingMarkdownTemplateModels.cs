namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.FinalRanking;

internal sealed record class MarkdownTemplateModel
{
    public string OutputCsvLink { get; init; } = string.Empty;
    public string EditionLabel { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string FirstPlayerWinRatePercent { get; init; } = string.Empty;
    public int PlayerCount { get; init; }
    public string? OverviewNote { get; init; }
    public string? RepresentativeRankingMarkdownLink { get; init; }
    public string? ReferenceMatchesCsvLink { get; init; }
    public IEnumerable<string> AttentionPoints { get; init; } = [];
    public IEnumerable<string> AutoComments { get; init; } = [];
    public string PrimaryTableHeader { get; init; } = string.Empty;
    public string PrimaryTableHeaderSeparator { get; init; } = string.Empty;
    public IEnumerable<string> PrimaryTableRows { get; init; } = [];
    public IEnumerable<MarkdownTemplateSection> TrailingSections { get; init; } = [];
    public IEnumerable<FinalRankingMarkdownChartSpec> Charts { get; init; } = [];
}

internal sealed record class MarkdownTemplateSection(string Title, string TableHeader, IEnumerable<string> Rows);

internal readonly record struct FinalRankingMarkdownChartSpec(
    string Title,
    IEnumerable<string> Categories,
    string YAxisLabel,
    string YAxisRange,
    IEnumerable<string> Values);
