namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Shared;

internal static class CsvSchemaCommonColumns
{
    internal const string BoundaryNameColumn = "boundaryName";
    internal const string SchemaNameColumn = "schemaName";
    internal const string RowTypeColumn = "rowType";

    internal static IReadOnlyList<string> BuildHeaderColumns(params IEnumerable<string>[] specificColumnGroups)
    {
        var columns = new List<string>
        {
            BoundaryNameColumn,
            SchemaNameColumn,
            RowTypeColumn
        };

        foreach (var group in specificColumnGroups)
        {
            columns.AddRange(group);
        }

        return columns;
    }

    internal static List<string> BuildRowColumns(string boundaryName, string schemaName, string rowType, params string[] specificColumns)
    {
        var columns = new List<string>
        {
            boundaryName,
            schemaName,
            rowType
        };

        columns.AddRange(specificColumns);
        return columns;
    }
}
