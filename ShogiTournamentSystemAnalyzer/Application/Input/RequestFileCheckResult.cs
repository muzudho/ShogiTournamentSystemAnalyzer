/*
 * ［アプリケーション　＞　入力　＞　要求ファイルチェック結果］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

internal sealed record RequestFileCheckResult(string FullPath, string FilteredInput);