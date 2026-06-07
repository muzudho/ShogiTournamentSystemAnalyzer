/*
 * ［プレゼンテーション　＞　コンソール改］
 */
namespace ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;

using ShogiTournamentSystemAnalyzer.Application;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Domain.TournamentRuleCore;
using System;
using System.Globalization;

internal static class ConsolePromptReaders
{
    internal static readonly int InputRetryLimit = 10;

    /// <summary>
    /// 無限ループに陥るのを防ぐぜ（＾▽＾）
    /// </summary>
    /// <param name="targetLabel"></param>
    /// <param name="lastErrorMessage"></param>
    /// <exception cref="OperationCanceledException"></exception>
    internal static void ThrowInputRetryLimitExceeded(string targetLabel, string lastErrorMessage)
    {
        throw new OperationCanceledException($"{targetLabel}の入力失敗が {InputRetryLimit} 回に達したため中断しました。最後のエラー: {lastErrorMessage}");
    }

    internal static AnalysisFlowSelection ReadAnalysisFlowSelection()
    {
        var runsSimulation = ReadYesNo("シミュレーションをしますか？", defaultValue: true, targetLabel: "シミュレーション実行選択");
        var runsQualityEvaluation = ReadYesNo("品質評価をしますか？", defaultValue: false, targetLabel: "品質評価実行選択");

        if (!runsSimulation && !runsQualityEvaluation)
        {
            throw new OperationCanceledException("シミュレーションと品質評価のどちらも選ばれませんでした。");
        }

        return AnalysisFlowSelection.FromFlags(runsSimulation, runsQualityEvaluation);
    }

    internal static bool ReadYesNo(string question, bool defaultValue, string targetLabel)
    {
        Console.WriteLine(question);
        Console.WriteLine("1. いいえ");
        Console.WriteLine("2. はい\n");

        var defaultNumber = defaultValue ? "2" : "1";
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired(targetLabel);
            attempt++;
            Console.Write($"番号を入力してください [{defaultNumber}]: ");
            var input = InputFromSomewhere.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine();
                return defaultValue;
            }

            if (input == "1")
            {
                Console.WriteLine();
                return false;
            }

            if (input == "2")
            {
                Console.WriteLine();
                return true;
            }

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded(targetLabel, "1 または 2 以外が入力されました");

            Console.WriteLine("1、2 のいずれかを入力してください。\n");
        }
    }
    /// <summary>
    ///     <pre>
    /// 対象［大会ルール］を属性として選ばせるぜ（＾▽＾）！
    /// これは、外部の設定ファイルで編集できるようにしたいぜ（＾▽＾）！
    ///     </pre>
    /// </summary>
    /// <param name="flowSelection"></param>
    /// <returns></returns>
    internal static RuleProfileAttributes ReadRuleProfileAttributes(AnalysisFlowSelection flowSelection)
    {
        var flowLabel = flowSelection.ToPromptLabel();
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("対象ルールプロファイル選択");
            attempt++;
            Console.WriteLine($"{flowLabel} のルールプロファイル属性を入力してください。");
            Console.WriteLine("1. 属性を一つずつ入力する");
            Console.WriteLine("2. RuleProfileAttributes を直接入力する");
            Console.WriteLine("3. ルールプロファイルファイルから読む\n");

            var inputMode = ReadNumberSelection(
                "ルールプロファイル属性入力方法",
                defaultNumber: "1",
                "1", RuleProfileAttributesInputMode.Prompt,
                "2", RuleProfileAttributesInputMode.Text,
                "3", RuleProfileAttributesInputMode.File);

            var attributes = inputMode switch
            {
                RuleProfileAttributesInputMode.Prompt => ReadRuleProfileAttributesFromPrompts(),
                RuleProfileAttributesInputMode.Text => ReadRuleProfileAttributesFromText(flowLabel),
                RuleProfileAttributesInputMode.File => ReadRuleProfileAttributesFromFile(),
                _ => throw new InvalidOperationException($"未対応のルールプロファイル属性入力方法です: {inputMode}"),
            };

            if (!TryValidateRuleProfileAttributesForFlow(flowSelection, attributes, out var errorMessage))
            {
                if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("対象ルールプロファイル属性", errorMessage);

                Console.WriteLine(errorMessage);
                Console.WriteLine("もう一度入力してください。\n");
                continue;
            }

            Console.WriteLine();
            return attributes;
        }
    }

    static RuleProfileAttributes ReadRuleProfileAttributesFromPrompts()
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            var simulationShape = ReadRuleProfileSimulationShape();
            var defaultUsesFinalStageGrouping = simulationShape == RuleProfileSimulationShape.FinalStageGrouped;
            var usesFinalStageGrouping = ReadOnOffBool(
                "UsesFinalStageGrouping",
                defaultUsesFinalStageGrouping,
                "本戦グループを使うか");
            var usesAdditionalApexPlacement = ReadOnOffBool(
                "UsesAdditionalApexPlacement",
                usesFinalStageGrouping,
                "本戦不出場 Apex の配置を使うか");
            var usesBoundaryRescue = ReadOnOffBool(
                "UsesBoundaryRescue",
                usesFinalStageGrouping,
                "境界救済を使うか");
            var usesVariableTop8 = ReadOnOffBool(
                "UsesVariableTop8",
                usesFinalStageGrouping,
                "可変 Top8 を使うか");
            var rankingRuleSetMode = ReadTournamentRuleSetModeAttribute();
            var hasReferenceMatches = ReadOnOffBool(
                "HasReferenceMatches",
                usesFinalStageGrouping,
                "参考対局を使うか");
            var pairingSource = ReadRuleProfilePairingSource(simulationShape);

            var attributes = new RuleProfileAttributes(
                simulationShape,
                usesFinalStageGrouping,
                usesAdditionalApexPlacement,
                usesBoundaryRescue,
                usesVariableTop8,
                rankingRuleSetMode,
                hasReferenceMatches,
                pairingSource);

            if (attributes.TryValidate(out var errorMessage)) return attributes;

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("対象ルールプロファイル属性", errorMessage);

            Console.WriteLine($"ルールプロファイル属性の組み合わせが不正です: {errorMessage}");
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static RuleProfileAttributes ReadRuleProfileAttributesFromText(string flowLabel)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("ルールプロファイル属性直接入力");
            attempt++;
            Console.WriteLine($"{flowLabel} の RuleProfileAttributes を key=value で貼り付けてください。入力終了は空行です。\n");
            PrintRuleProfileAttributesTextExample();

            var lines = new List<string>();
            while (true)
            {
                var line = InputFromSomewhere.ReadLine();
                if (line is null) throw new OperationCanceledException("ルールプロファイル属性入力中に入力ストリームが終了しました。");
                if (string.IsNullOrWhiteSpace(line)) break;

                lines.Add(line);
            }

            if (RuleProfileAttributesTextParser.TryParse(lines, "(手入力 RuleProfileAttributes)", out var attributes, out var errorMessage)) return attributes;

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("ルールプロファイル属性直接入力", errorMessage);

            Console.WriteLine($"ルールプロファイル属性の読み取りに失敗しました: {errorMessage}");
            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static RuleProfileAttributes ReadRuleProfileAttributesFromFile()
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("ルールプロファイルファイル入力");
            attempt++;
            var path = ConsoleInputReaders.ReadRequiredFilePath("ルールプロファイルファイルのパスを入力してください: ");
            var fullPath = Path.GetFullPath(path);

            try
            {
                var lines = File.ReadAllLines(fullPath);
                if (RuleProfileAttributesTextParser.TryParse(lines, fullPath, out var attributes, out var errorMessage)) return attributes;

                if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("ルールプロファイルファイル入力", errorMessage);
                Console.WriteLine($"ルールプロファイルファイルの読み取りに失敗しました: {errorMessage}");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
            {
                if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("ルールプロファイルファイル入力", ex.Message);
                Console.WriteLine($"ルールプロファイルファイルを読めません: {ex.Message}");
            }

            Console.WriteLine("もう一度入力してください。\n");
        }
    }

    static bool TryValidateRuleProfileAttributesForFlow(
        AnalysisFlowSelection flowSelection,
        RuleProfileAttributes attributes,
        out string errorMessage)
    {
        if (!attributes.TryValidate(out errorMessage))
        {
            errorMessage = $"ルールプロファイル属性の組み合わせが不正です: {errorMessage}";
            return false;
        }

        if (flowSelection.RunsQualityEvaluation
            && attributes.PairingSource != RuleProfilePairingSource.ScheduledMatches)
        {
            errorMessage = "品質評価では PairingSource=ScheduledMatches を指定してください。";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    static void PrintRuleProfileAttributesTextExample()
    {
        Console.WriteLine("SimulationShape=ScheduledMatches");
        Console.WriteLine("UsesFinalStageGrouping=Off");
        Console.WriteLine("UsesAdditionalApexPlacement=Off");
        Console.WriteLine("UsesBoundaryRescue=Off");
        Console.WriteLine("UsesVariableTop8=Off");
        Console.WriteLine("RankingRuleSetMode=Neutral");
        Console.WriteLine("HasReferenceMatches=Off");
        Console.WriteLine("PairingSource=ScheduledMatches\n");
    }

    enum RuleProfileAttributesInputMode
    {
        Prompt,
        Text,
        File,
    }
    static RuleProfileSimulationShape ReadRuleProfileSimulationShape()
    {
        Console.WriteLine("SimulationShape を選んでください。");
        Console.WriteLine("1. ScheduledMatches");
        Console.WriteLine("2. FinalStageGrouped");
        Console.WriteLine("3. TournamentFramework");
        Console.WriteLine("4. Empty\n");

        return ReadNumberSelection(
            "SimulationShape",
            defaultNumber: "1",
            "1", RuleProfileSimulationShape.ScheduledMatches,
            "2", RuleProfileSimulationShape.FinalStageGrouped,
            "3", RuleProfileSimulationShape.TournamentFramework,
            "4", RuleProfileSimulationShape.Empty);
    }

    static RuleProfilePairingSource ReadRuleProfilePairingSource(RuleProfileSimulationShape simulationShape)
    {
        var defaultNumber = simulationShape switch
        {
            RuleProfileSimulationShape.TournamentFramework => "3",
            RuleProfileSimulationShape.Empty => "1",
            _ => "2",
        };

        Console.WriteLine("PairingSource を選んでください。");
        Console.WriteLine("1. None");
        Console.WriteLine("2. ScheduledMatches");
        Console.WriteLine("3. TournamentFramework\n");

        return ReadNumberSelection(
            "PairingSource",
            defaultNumber,
            "1", RuleProfilePairingSource.None,
            "2", RuleProfilePairingSource.ScheduledMatches,
            "3", RuleProfilePairingSource.TournamentFramework);
    }

    static TournamentRuleSetMode ReadTournamentRuleSetModeAttribute()
    {
        Console.WriteLine("RankingRuleSetMode を選んでください。");
        Console.WriteLine("1. Neutral");
        Console.WriteLine("2. Twill");
        Console.WriteLine("3. TwillCommonOpponentWeighted\n");

        return ReadNumberSelection(
            "RankingRuleSetMode",
            defaultNumber: "1",
            "1", TournamentRuleSetMode.Neutral,
            "2", TournamentRuleSetMode.Twill,
            "3", TournamentRuleSetMode.TwillCommonOpponentWeighted);
    }

    static bool ReadOnOffBool(string keyName, bool defaultValue, string question)
    {
        Console.WriteLine($"{keyName}: {question}");
        Console.WriteLine("1. Off");
        Console.WriteLine("2. On\n");

        return ReadNumberSelection(
            keyName,
            defaultValue ? "2" : "1",
            "1", false,
            "2", true);
    }

    static T ReadNumberSelection<T>(
        string targetLabel,
        string defaultNumber,
        string number1,
        T value1,
        string number2,
        T value2)
    {
        return ReadNumberSelection(
            targetLabel,
            defaultNumber,
            new[]
            {
                (number1, value1),
                (number2, value2),
            });
    }

    static T ReadNumberSelection<T>(
        string targetLabel,
        string defaultNumber,
        string number1,
        T value1,
        string number2,
        T value2,
        string number3,
        T value3)
    {
        return ReadNumberSelection(
            targetLabel,
            defaultNumber,
            new[]
            {
                (number1, value1),
                (number2, value2),
                (number3, value3),
            });
    }

    static T ReadNumberSelection<T>(
        string targetLabel,
        string defaultNumber,
        string number1,
        T value1,
        string number2,
        T value2,
        string number3,
        T value3,
        string number4,
        T value4)
    {
        return ReadNumberSelection(
            targetLabel,
            defaultNumber,
            new[]
            {
                (number1, value1),
                (number2, value2),
                (number3, value3),
                (number4, value4),
            });
    }

    static T ReadNumberSelection<T>(
        string targetLabel,
        string defaultNumber,
        IReadOnlyList<(string Number, T Value)> selections)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired(targetLabel);
            attempt++;
            Console.Write($"番号を入力してください [{defaultNumber}]: ");
            var input = InputFromSomewhere.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) input = defaultNumber;

            foreach (var selection in selections)
            {
                if (input == selection.Number)
                {
                    Console.WriteLine();
                    return selection.Value;
                }
            }

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded(targetLabel, "選択肢にない番号が入力されました");

            Console.WriteLine("選択肢の番号を入力してください。\n");
        }
    }

    internal static string ReadTextWithDefault(string prompt, string defaultValue)
    {
        if (!SimulationTimeBudget.HasApplicationTimeRemaining())
        {
            Console.WriteLine(prompt + defaultValue);
            return defaultValue;
        }

        Console.Write(prompt);
        var input = InputFromSomewhere.ReadLine()?.Trim();
        if (input is null) throw new OperationCanceledException("文字列入力中に入力ストリームが終了しました。");

        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }

    internal static int ReadIntWithDefault(string prompt, int defaultValue, int min)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("整数入力");
            Console.Write(prompt);
            var input = InputFromSomewhere.ReadLine()?.Trim();
            if (input is null) throw new OperationCanceledException("整数入力中に入力ストリームが終了しました。");

            if (string.IsNullOrEmpty(input)) return defaultValue;

            attempt++;
            if (int.TryParse(input, out var value) && value >= min) return value;

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("整数入力", $"{min} 以上の整数ではありません");

            Console.WriteLine($"{min} 以上の整数を入力してください。");
        }
    }

    internal static double ReadDoubleWithDefaultInRange(string prompt, double defaultValue, double minInclusive, double maxInclusive)
    {
        var attempt = 0;
        while (true)
        {
            SimulationTimeBudget.ThrowIfApplicationTimeExpired("数値入力");
            Console.Write(prompt);
            var input = InputFromSomewhere.ReadLine()?.Trim();
            if (input is null) throw new OperationCanceledException("数値入力中に入力ストリームが終了しました。");

            if (string.IsNullOrEmpty(input)) return defaultValue;

            attempt++;
            if ((double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var value)
                || double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value))
                && value >= minInclusive
                && value <= maxInclusive) return value;

            if (attempt >= InputRetryLimit) ThrowInputRetryLimitExceeded("数値入力", $"{minInclusive} 以上 {maxInclusive} 以下の数値ではありません");

            Console.WriteLine($"{minInclusive} 以上 {maxInclusive} 以下の数値を入力してください。");
        }
    }
}
