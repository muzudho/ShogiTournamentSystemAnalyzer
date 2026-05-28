/*
 * ［アプリケーション　＞　データ定義］
 */
namespace ShogiTournamentSystemAnalyzer.Application.DataDefinitions;

internal enum DataDefinitionValueType
{
    Int,
    Double,
    String,
}

sealed record class TabularDataDefinitionMeta(
    string TableName,
    string? RuleType = null,
    string? BoundaryName = null,
    string? SchemaName = null,
    string? Comment = null);

sealed record class TabularDataColumnDefinition(
    string Name,
    DataDefinitionValueType Type,
    bool IsRequired,
    string? Comment = null);

sealed record class TabularDataDefinition(
    TabularDataDefinitionMeta Meta,
    IReadOnlyList<TabularDataColumnDefinition> Columns);