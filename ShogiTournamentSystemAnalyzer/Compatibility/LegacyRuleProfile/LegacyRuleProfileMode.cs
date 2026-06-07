/*
 * ［互換性　＞　レガシールールプロファイル］
 */
namespace ShogiTournamentSystemAnalyzer.Compatibility.LegacyRuleProfile;

/// <summary>
///     <pre>
/// STSAInput/4 以前の RuleProfileMode 互換ラベル。
/// 
///     - 外部ファイルのキー名 RuleProfileMode は互換仕様として維持する。
///     - 内部実行では RuleProfileAttributes を使う。
///     </pre>
/// </summary>
enum LegacyRuleProfileMode
{
    Standard,
    FinalStage,

    /// <summary>
    /// シミュレーション域で大会進行フレームワークを使うルールプロファイル。
    /// </summary>
    TournamentFramework,

    Empty,
}
