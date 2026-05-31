/*
 * ［インフラストラクチャー　＞　コーディングモデル］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.CodingModels;

/// <summary>
/// ［エラーメッセージ］だ。
/// </summary>
internal readonly record struct ErrorMessageModel(string Value)
{
    /// <summary>
    /// 空文字列だ。
    /// </summary>
    internal static ErrorMessageModel Empty { get; private set; } = new(string.Empty);

    /// <summary>
    /// 文字列からインスタンス生成。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static ErrorMessageModel FromString(string value)
        => string.IsNullOrWhiteSpace(value) ? Empty : new(value);

    /// <summary>
    /// 文字列形式で取得。
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Value;
}
