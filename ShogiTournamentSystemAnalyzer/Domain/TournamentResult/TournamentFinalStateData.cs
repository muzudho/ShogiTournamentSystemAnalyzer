using ShogiTournamentSystemAnalyzer.Domain.Simulation;

/// <summary>
/// ［６大境界］のうち、［大会最終状態］境界データだ。
/// </summary>
/// <param name="MatchRecords"></param>
/// <param name="CurrentTime"></param>
/// <param name="TickCount"></param>
/// <param name="CompletedNaturally"></param>
sealed record class TournamentFinalStateData(
    IReadOnlyList<TournamentMatchRecord> MatchRecords,
    int CurrentTime,
    int TickCount,
    bool CompletedNaturally);
