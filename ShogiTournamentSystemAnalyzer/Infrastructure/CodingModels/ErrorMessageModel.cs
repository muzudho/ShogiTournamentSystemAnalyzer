namespace ShogiTournamentSystemAnalyzer.Infrastructure.CodingModels;

/// <summary>
/// ［エラーメッセージ］だ。
/// </summary>
internal record ErrorMessageModel
{
    public ErrorMessageModel(string value)
    {
        Value = value;
    }

    public string Value { get; }
}
