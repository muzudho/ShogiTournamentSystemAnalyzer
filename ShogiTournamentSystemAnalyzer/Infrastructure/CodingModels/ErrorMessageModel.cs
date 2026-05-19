namespace ShogiTournamentSystemAnalyzer.Infrastructure.CodingModels;

/// <summary>
/// ［エラーメッセージ］だ。
/// </summary>
internal readonly record struct ErrorMessageModel(string Value)
{
    internal static ErrorMessageModel Empty { get; private set; } = new(string.Empty);

    internal static ErrorMessageModel FromString(string value)
        => string.IsNullOrWhiteSpace(value) ? Empty : new(value);

    public override string ToString() => Value;
}
