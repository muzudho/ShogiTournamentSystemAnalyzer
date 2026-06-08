/*
 * ［アプリケーション　＞　要求ファイルチェック　＞　結果］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;

internal sealed record RequestFileCheckResult(
    bool IsSuccessful,
    RequestText? RequestText);
