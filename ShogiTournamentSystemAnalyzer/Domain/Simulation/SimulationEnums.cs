/*
 * ［シミュレーション域］
 */
namespace ShogiTournamentSystemAnalyzer.Domain.Simulation;

enum MatchStatus
{
    Scheduled,
    Running,
    Finished,
    Cancelled,
}

enum MatchResultType
{
    None,
    FirstPlayerWin,
    SecondPlayerWin,
    Draw,
    FirstPlayerForfeitWin,
    SecondPlayerForfeitWin,
    Cancelled,
    NoContest,
}
