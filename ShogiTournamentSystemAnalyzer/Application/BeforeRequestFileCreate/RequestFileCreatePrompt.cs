/*
 * ［アプリケーション　＞　実行　＞　要求ファイル作成前　＞　要求ファイル作成プロンプト］
 */
namespace ShogiTournamentSystemAnalyzer.Application.Input;

using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

internal static class RequestFileCreatePrompt
{
    internal static string? ReadOutputPath()
    {
        Console.WriteLine("今回の入力を保存しておきますか？");
        Console.WriteLine("1. いいえ");
        Console.WriteLine("2. はい\n");

        var attempt = 0;
        while (true)
        {
            attempt++;
            Console.Write("番号を入力してください [1]: ");
            var input = Console.ReadLine()?.Trim();
            if (input is null) throw new OperationCanceledException("要求ファイル作成の選択中に入力ストリームが終了しました。");

            if (string.IsNullOrEmpty(input) || input == "1") return null;

            if (input == "2")
            {
                Console.WriteLine("■［要求ファイル作成］");
                var defaultPath = RequestFileCreate.BuildDefaultPath();
                var outputPath = ConsolePromptReaders.ReadTextWithDefault(
                    $"要求ファイルの出力先パスまたはフォルダーパスを入力してください [{defaultPath}]: ",
                    defaultPath);

                return RequestFileCreate.ResolveOutputPath(outputPath);
            }

            if (attempt >= ConsolePromptReaders.InputRetryLimit) ConsolePromptReaders.ThrowInputRetryLimitExceeded("要求ファイル作成選択", "1 または 2 以外が入力されました");

            Console.WriteLine("1 か 2 を入力してください。\n");
        }
    }
}