/*
 * ［大会ルールという境界］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;

/// <summary>
/// ［本戦グループ］
/// </summary>
enum FinalStageGroup
{
    Apex,
    Innov,
}

/// <summary>
/// ［大会ルールセットモード］
/// </summary>
enum TournamentRuleSetMode
{
    Neutral,
    Twill,
    TwillCommonOpponentWeighted,
}

/// <summary>
/// ［追加本戦配置モード］
/// </summary>
enum AdditionalApexPlacementMode
{
    Off,
    On,
}

/// <summary>
/// ［境界救済モード］
/// </summary>
enum BoundaryRescueMode
{
    Off,
    On,
}

/// <summary>
/// ［変数トップ8モード］
/// </summary>
enum VariableTop8Mode
{
    Off,
    On,
}
