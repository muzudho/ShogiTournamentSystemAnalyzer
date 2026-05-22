/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Domain.TournamentQualityEvaluator;
using System.Globalization;
using System.Text;

/// <summary>
/// ここがプログラムだぜ（＾▽＾）！
/// </summary>
internal static partial class Program
{


    // ========================================
    // 概要
    // ========================================


    /// <summary>
    /// ここからプログラムが始まるぜ（＾▽＾）！
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        // エンコーディングって大事だよな（＾▽＾）！　文字化けを防ぐぜ（＾▽＾）！
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            // このプログラムの説明を最初にするぜ（＾▽＾）！
            Console.WriteLine("このプログラムは、２人用ゲーム大会（例えば将棋）の大会ルールをいくつか選び、コンピューター上で模擬戦し、その結果を比較して、より良いルール作りを目指すツールだぜ（＾▽＾）！\n");

            // 入力方法を選ばせる（＾▽＾）！
            ConfigureInputSource(args);

            // 大きくモードが分かれるぜ（＾▽＾）！
            //
            // 📍 TODO: ［ルール選択］→［パラメーター設定］→［試行］→［品質評価・レポート作成］の４ステップのシーケンスにした方がいいのでは（＾～＾）？
            //
            var flowMode = ReadAnalysisFlowMode();
            var ruleProfileMode = ReadRuleProfileMode(flowMode);

            switch ((flowMode, ruleProfileMode))
            {
                case (AnalysisFlowMode.Simulation, RuleProfileMode.Standard):
                    RunStandardMode();
                    break;

                case (AnalysisFlowMode.Simulation, RuleProfileMode.FinalStage):
                    RunFinalStageMode();
                    break;

                case (AnalysisFlowMode.Simulation, RuleProfileMode.TournamentFramework):
                    RunTournamentFrameworkMode();
                    break;

                case (AnalysisFlowMode.Simulation, RuleProfileMode.Empty):
                    RunEmptyMode();
                    break;

                case (AnalysisFlowMode.QualityEvaluation, RuleProfileMode.Standard):
                    RunTournamentQualityEvaluationMode(RuleProfileMode.Standard);
                    break;

                case (AnalysisFlowMode.QualityEvaluation, RuleProfileMode.FinalStage):
                    RunTournamentQualityEvaluationMode(RuleProfileMode.FinalStage);
                    break;

                default:
                    throw new InvalidOperationException("未対応のモードです。");
            }
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }


    // ========================================
    // 詳細
    // ========================================


    /// <summary>
    /// シミュレーションは最大ｎ分までにするぜ（＾▽＾）！　あまり長くなりすぎると、結果が出る前に心が折れちゃうからな（＾～＾）！
    /// </summary>
    internal static readonly TimeSpan SimulationTimeLimit = TimeSpan.FromMinutes(3);
    internal static DateTime? _simulationDeadlineUtc;

}

