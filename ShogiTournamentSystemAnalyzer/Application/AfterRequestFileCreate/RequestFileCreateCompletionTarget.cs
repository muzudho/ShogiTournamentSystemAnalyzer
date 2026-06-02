/*
 * ［アプリケーション　＞　要求ファイル作成後　＞　要求ファイル作成完了対象］
 */
namespace ShogiTournamentSystemAnalyzer.Application.AfterRequestFileCreate;

/// <summary>
/// 要求ファイル作成完了対象
/// </summary>
/// <param name="RequestFileCreatePath">要求ファイル作成パス</param>
/// <param name="RecordedLines">記録した手動入力行</param>
internal sealed record RequestFileCreateCompletionTarget(
    string RequestFileCreatePath,
    IReadOnlyList<string> RecordedLines);
