namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.RequestFileWrite;
using ShogiTournamentSystemAnalyzer.Application.RequestParsing;
using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using ShogiTournamentSystemAnalyzer.Infrastructure.DataFiles;
using ShogiTournamentSystemAnalyzer.Presentation.ConsoleCustom;
using System.Text;
using static ShogiTournamentSystemAnalyzer.Application.ApplicationTournamentUser;

internal static class ApplicationWorkflow
{
    public static void Run(string[] args)
    {
        // ●開始

        //┌───┴──┐
        //│オープニング│
        //└───┬──┘
        Opening();

        // （開発用の機能）
        // PowerShell reflection smoke の代わりに、通常の .NET 実行入口として使う。
        if (RequestWriterRoundTripSmoke.TryRun(args)) return;
        if (RequestParseFailureSmoke.TryRun(args)) return;

        //┌───┴──┐
        //│大会利用者域│
        //└───┬──┘
        if (!ApplicationTournamentUser.TryRunTournamentUserDomain(args, out var tournamentUserDomainResult)) return;  // エラー終了

        // ［要求ファイル］から読んだ STSAInput/4 または STSAInput/5 直通要求は、要求側の StepRequests リスト構造で実行する。
        if (tournamentUserDomainResult.AnalysisRequest is not null)
        {
            ProgramConsoleGuide.PrintSelectedMainline(tournamentUserDomainResult);
            Console.WriteLine("■［分析］"); // TODO: この出力、消していいかだぜ（＾～＾）？
            AnalysisRequestDispatcher.Execute(tournamentUserDomainResult.AnalysisRequest);
            return;
        }

        var manualExecutionState = tournamentUserDomainResult.IsManualInput
            ? new ManualAnalysisExecutionState(tournamentUserDomainResult.ManualRequestFilePath)
            : null;

        if (!tournamentUserDomainResult.IsManualInput)
        {
            // legacy 入力変換経路は、要求ファイルから復元した疑似コンソール入力を従来 dispatcher へ流す。
            ProgramConsoleGuide.PrintSelectedMainline(tournamentUserDomainResult);
            Console.WriteLine("■［分析］"); // TODO: この出力、消していいかだぜ（＾～＾）？
        }

        //┌───┴─────┐
        //│シミュレーション域│
        //└───┬─────┘
        ExecuteSimulationDomain(tournamentUserDomainResult, manualExecutionState);

        //┌───┴─────┐
        //│最終順位付け域　　│
        //└───┬─────┘
        ExecuteFinalRankingDomain();

        //┌───┴─────┐
        //│大会品質評価域　　│
        //└───┬─────┘
        ExecuteQualityEvaluationDomain(tournamentUserDomainResult, manualExecutionState);

        WriteManualRequestFile(manualExecutionState);

        // ローカル関数

        // ●終了
        return;
    }

    /// <summary>
    /// ［開始］
    /// </summary>
    private static void Opening()
    {
        // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        InputFromSomewhere.UseConsole();

        // プログラムの実行が長引いて、いくら待っても応答が返ってこない、なんてことを防ぐために、タイムアウトを設定するぜ（＾▽＾）！
        SimulationTimeBudget.BeginApplicationBudget();

        // このプログラムの説明を最初に表示するぜ（＾▽＾）！
        ProgramConsoleGuide.PrintProgramIntroduction();
    }

    /// <summary>
    /// ［シミュレーション域］実行
    /// </summary>
    /// <param name="result"></param>
    /// <param name="manualExecutionState"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ExecuteSimulationDomain(
        TournamentUserDomainResult result,
        ManualAnalysisExecutionState? manualExecutionState)
    {
        if (manualExecutionState is not null)
        {
            Console.WriteLine("■［シミュレーション域］");
            var runsSimulation = ConsolePromptReaders.ReadYesNo(
                "シミュレーションをしますか？",
                defaultValue: true,
                targetLabel: "シミュレーション実行選択");
            if (!runsSimulation) return;

            var flowSelection = AnalysisFlowSelection.FromSingle(AnalysisFlowMode.Simulation);
            var manualRuleProfileAttributes = ConsolePromptReaders.ReadRuleProfileAttributes(flowSelection);
            if (!ManualAnalysisRequestReader.TryReadSimulationRequest(manualRuleProfileAttributes, out var stepRequest))
            {
                throw new InvalidOperationException("未対応の手入力シミュレーション要求です。");
            }

            if (!SimulationRequestDispatcher.TryExecute(stepRequest, out var simulationResult))
            {
                throw new InvalidOperationException("未対応のシミュレーション域です。");
            }

            manualExecutionState.Context.SetSimulationResult(stepRequest, simulationResult);
            manualExecutionState.AddStepRequest(stepRequest);
            return;
        }

        var analysisFlowSelection = result.AnalysisFlowSelection ?? throw new InvalidOperationException("分析フローが選択されていません。");
        var ruleProfileAttributes = result.RuleProfileAttributes ?? throw new InvalidOperationException("ルールプロファイル属性が選択されていません。");
        if (!analysisFlowSelection.RunsSimulation) return;
        if (SimulationFlowDispatcher.TryExecute(AnalysisFlowMode.Simulation, ruleProfileAttributes)) return;

        throw new InvalidOperationException("未対応のシミュレーション域です。");
    }

    /// <summary>
    /// ［最終順位付け域］実行
    /// </summary>
    private static void ExecuteFinalRankingDomain()
    {
        // 現時点の手入力フローでは、最終順位付け域の処理はシミュレーション域の中から呼ばれる。
        // アプリケーション直下の順序としてはここに置き、後続分離時の差し込み位置を固定する。
    }

    /// <summary>
    /// ［大会品質評価域］実行
    /// </summary>
    /// <param name="result"></param>
    /// <param name="manualExecutionState"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ExecuteQualityEvaluationDomain(
        TournamentUserDomainResult result,
        ManualAnalysisExecutionState? manualExecutionState)
    {
        if (manualExecutionState is not null)
        {
            Console.WriteLine("■［大会品質評価域］");
            var runsQualityEvaluation = ConsolePromptReaders.ReadYesNo(
                "品質評価をしますか？",
                defaultValue: false,
                targetLabel: "品質評価実行選択");
            if (!runsQualityEvaluation) return;

            var flowSelection = AnalysisFlowSelection.FromSingle(AnalysisFlowMode.QualityEvaluation);
            var manualRuleProfileAttributes = ConsolePromptReaders.ReadRuleProfileAttributes(flowSelection);
            if (!ManualAnalysisRequestReader.TryReadQualityEvaluationRequest(manualRuleProfileAttributes, out var stepRequest))
            {
                throw new InvalidOperationException("未対応の手入力品質評価要求です。");
            }

            if (!QualityEvaluationRequestDispatcher.TryExecute(stepRequest, manualExecutionState.Context))
            {
                throw new InvalidOperationException("未対応の大会品質評価域です。");
            }

            manualExecutionState.AddStepRequest(stepRequest);
            return;
        }

        var analysisFlowSelection = result.AnalysisFlowSelection ?? throw new InvalidOperationException("分析フローが選択されていません。");
        var ruleProfileAttributes = result.RuleProfileAttributes ?? throw new InvalidOperationException("ルールプロファイル属性が選択されていません。");
        if (!analysisFlowSelection.RunsQualityEvaluation) return;
        if (QualityEvaluationFlowDispatcher.TryExecute(AnalysisFlowMode.QualityEvaluation, ruleProfileAttributes)) return;

        throw new InvalidOperationException("未対応の大会品質評価域です。");
    }

    static void WriteManualRequestFile(ManualAnalysisExecutionState? manualExecutionState)
    {
        if (manualExecutionState is null) return;

        if (manualExecutionState.StepRequests.Count == 0)
        {
            Console.WriteLine("実行する分析は選ばれませんでした。\n");
            return;
        }

        if (string.IsNullOrWhiteSpace(manualExecutionState.RequestFilePath)) return;

        var request = new AnalysisRequest(
            new AnalysisFlowSelection(manualExecutionState.StepRequests.Select(GetAnalysisFlowMode).ToArray()),
            manualExecutionState.StepRequests.ToArray());

        Console.WriteLine($"要求ファイルを書き出します: {manualExecutionState.RequestFilePath}\n");
        StsaFileIOHelper.Write(
            label: "要求ファイル",
            outputPath: manualExecutionState.RequestFilePath,
            lines: StsaInputRequestWriter.BuildAttributeLines(request));
    }

    static AnalysisFlowMode GetAnalysisFlowMode(AnalysisStepRequest stepRequest)
    {
        return stepRequest switch
        {
            SimulationStepRequest => AnalysisFlowMode.Simulation,
            QualityEvaluationStepRequest => AnalysisFlowMode.QualityEvaluation,
            DeferredQualityEvaluationStepRequest => AnalysisFlowMode.QualityEvaluation,
            _ => throw new InvalidOperationException($"未対応の分析要求です: {stepRequest.GetType().Name}"),
        };
    }

    sealed class ManualAnalysisExecutionState
    {
        readonly List<AnalysisStepRequest> _stepRequests = new();

        internal ManualAnalysisExecutionState(string? requestFilePath)
        {
            RequestFilePath = requestFilePath;
        }

        internal string? RequestFilePath { get; }

        internal AnalysisExecutionContext Context { get; } = new();

        internal IReadOnlyList<AnalysisStepRequest> StepRequests => _stepRequests;

        internal void AddStepRequest(AnalysisStepRequest stepRequest)
        {
            _stepRequests.Add(stepRequest);
        }
    }
}
