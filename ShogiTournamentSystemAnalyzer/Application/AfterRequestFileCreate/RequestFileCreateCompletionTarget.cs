/*
 * ［アプリケーション　＞　実行　＞　要求ファイル作成後　＞　要求ファイル作成完了対象］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

/// <summary>
/// 要求ファイル作成完了対象
/// </summary>
/// <param name="RequestFileCreatePath">要求ファイル作成パス</param>
/// <param name="RecordingInput">手動入力記録テキストリーダー</param>
internal sealed record RequestFileCreateCompletionTarget(
    string RequestFileCreatePath,
    ManualInputRecordingTextReader RecordingInput);