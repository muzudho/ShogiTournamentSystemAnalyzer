/*
 * ［アプリケーション　＞　手動入力後　＞　要求ファイル完了対象］
 */
namespace ShogiTournamentSystemAnalyzer.Application.RequestFileCreate;

/// <summary>
/// 要求ファイル完了対象
/// </summary>
/// <param name="ManualInputLogPath">要求ファイル作成パス</param>
/// <param name="RecordedLines">記録した手動入力行</param>
internal sealed record RequestFileCompletionTarget(
    string ManualInputLogPath,
    IReadOnlyList<string> RecordedLines);
