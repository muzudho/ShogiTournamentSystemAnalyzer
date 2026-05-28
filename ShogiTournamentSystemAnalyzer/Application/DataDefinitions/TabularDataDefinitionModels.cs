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

sealed record class TableTypeDefinitionMeta(
    string Name,
    string Type = "TableType",
    string? Comment = null);

sealed record class TableTypeColumnDefinition(
    string Name,
    DataDefinitionValueType Type,
    bool IsRequired = true,
    string? Comment = null);

sealed record class TableTypeDefinition(
    TableTypeDefinitionMeta Meta,
    IReadOnlyList<TableTypeColumnDefinition> Data);

sealed record class TableInstanceMetaDefinition(
    string TableName,
    string TableType,
    string? Comment = null);

sealed record class TableInstanceDefinition(
    TableInstanceMetaDefinition Meta);
