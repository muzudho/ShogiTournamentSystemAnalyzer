/*
 * ［インフラストラクチャー　＞　データファイル　＞　依頼という境界］
 */
namespace ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles.Request.TournamentRule;

using Scriban;

internal static class RequestInputLogFileWriter
{
    static string RenderTemplate(object model)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "DataFiles", "Request", "TournamentRule", "RequestInputLogTemplate.sbn.txt");
        var templateText = File.ReadAllText(templatePath);
        var template = Template.Parse(templateText);
        if (template.HasErrors)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, template.Messages));
        }

        return template.Render(model, memberRenamer: member => member.Name);
    }

    internal static IEnumerable<string> CreateRequestInputLogLines(object model)
    {
        var rendered = RenderTemplate(model);
        using var reader = new StringReader(rendered);
        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }
}