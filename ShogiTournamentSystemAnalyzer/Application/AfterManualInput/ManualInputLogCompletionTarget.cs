/*
 * ［アプリケーション　＞　手動入力後　＞　手入力ログ完了対象］
 */
namespace ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

/// <summary>
/// 手入力ログ完了対象
/// </summary>
/// <param name="ManualInputLogPath">手入力ログ作成パス</param>
/// <param name="RecordedLines">記録した手動入力行</param>
internal sealed record ManualInputLogCompletionTarget(
    string ManualInputLogPath,
    IReadOnlyList<string> RecordedLines);
