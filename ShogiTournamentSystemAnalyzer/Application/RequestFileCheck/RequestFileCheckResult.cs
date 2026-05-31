/*
 * ［アプリケーション　＞　要求ファイルチェック結果］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;

internal sealed record RequestFileCheckResult(string FullPath, string FilteredInput);