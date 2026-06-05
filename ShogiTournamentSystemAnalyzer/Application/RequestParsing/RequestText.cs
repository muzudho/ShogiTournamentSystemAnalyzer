/*
 * ［アプリケーション　＞　要求パース　＞　要求テキスト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal sealed record RequestText(
    string FormatName,
    IReadOnlyList<string> Lines,
    string? SourcePath);