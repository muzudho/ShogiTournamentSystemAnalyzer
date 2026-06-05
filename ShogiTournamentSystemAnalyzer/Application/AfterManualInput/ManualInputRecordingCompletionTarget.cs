/*
 * ［アプリケーション　＞　手動入力後　＞　手動入力録音ログ完了対象］
 */
namespace ShogiTournamentSystemAnalyzer.Application.AfterManualInput;

/// <summary>
/// 手動入力録音ログ完了対象
/// </summary>
/// <param name="RecordingLogPath">録音ログ作成パス</param>
/// <param name="RecordedLines">記録した手動入力行</param>
internal sealed record ManualInputRecordingCompletionTarget(
    string RecordingLogPath,
    IReadOnlyList<string> RecordedLines);
