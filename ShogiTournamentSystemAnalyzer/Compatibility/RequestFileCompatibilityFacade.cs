/*
 * ［互換性　＞　要求ファイル互換ファサード］
 */
namespace ShogiTournamentSystemAnalyzer.Compatibility;

using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Compatibility.LegacyRequestFile;

internal static class RequestFileCompatibilityFacade
{
    internal static string ConvertToLegacyInputText(RequestText checkedRequestText)
    {
        var sourcePath = checkedRequestText.SourcePath ?? "(要求テキスト)";
        return checkedRequestText.FormatName switch
        {
            "STSAInput/5" => throw new OperationCanceledException("STSAInput/5 は直通 parser 専用です。RuleProfileAttributes と実行ステップの組み合わせを確認してください。"),
            "STSAInput/4" => StsaInputLegacyConverter.ConvertStsaInput4ToLegacyInput(checkedRequestText.Lines, sourcePath),
            "STSAInput/3" => StsaInputLegacyConverter.ConvertStsaInput3ToLegacyInput(checkedRequestText.Lines, sourcePath),
            "STSAInput/2" => StsaInputLegacyConverter.ConvertStsaInput2ToLegacyInput(checkedRequestText.Lines, sourcePath),
            _ => LegacyInputFileFilter.ConvertToFilteredInput(checkedRequestText.Lines),
        };
    }
}