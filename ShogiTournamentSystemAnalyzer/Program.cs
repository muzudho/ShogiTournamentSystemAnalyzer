/*
 * ［プログラム］
 */
namespace ShogiTournamentSystemAnalyzer;

using ShogiTournamentSystemAnalyzer.Application.Execution;
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
            SimulationTimeBudget.BeginApplicationBudget();

            // このプログラムの説明を最初にするぜ（＾▽＾）！
            Console.WriteLine("このプログラムは、２人用ゲーム大会（例えば将棋）の大会ルールをいくつか選び、コンピューター上で模擬戦し、その結果を比較して、より良いルール作りを目指すツールだぜ（＾▽＾）！\n");

            // 入力方法を選ばせる（＾▽＾）！
            InputSourceConfiguration.ConfigureInputSource(args);

            // 大きくモードが分かれるぜ（＾▽＾）！
            //
            // 📍 TODO: ［ルール選択］→［パラメーター設定］→［試行］→［品質評価・レポート作成］の４ステップのシーケンスにした方がいいのでは（＾～＾）？
            //
            var flowMode = ConsolePromptReaders.ReadAnalysisFlowMode();
            var ruleProfileMode = ConsolePromptReaders.ReadRuleProfileMode(flowMode);

            // TODO: ここで本来選ぶべきなのは、［シミュレーション域］、［順位付け域］、［大会品質評価フロー域］の３つの内の１つでは（＾～＾）？
            // TODO: まあ、その他に、［大会ルールという境界］に当たる［大会ルール・ファイル］を作成するウィザードがあってもいいかも（＾～＾）！　それはそれで面白そう（＾～＾）！

            switch ((flowMode, ruleProfileMode))
            {
                // TODO: ［標準ルール］は［大会ルールという境界］に吸収されるはず（＾～＾）！
                case (AnalysisFlowMode.Simulation, RuleProfileMode.Standard):
                    RunStandardMode();
                    break;

                // TODO: ［本戦ステージ］は［大会ルールという境界］に吸収されるはず（＾～＾）！
                case (AnalysisFlowMode.Simulation, RuleProfileMode.FinalStage):
                    RunFinalStageMode();
                    break;

                // TODO: これは［シミュレーション域］に吸収されるはず（＾～＾）！
                case (AnalysisFlowMode.Simulation, RuleProfileMode.TournamentFramework):
                    RunTournamentFrameworkMode();
                    break;

                // TODO: ［空ルール］は［大会ルールという境界］に吸収されるはず（＾～＾）！
                case (AnalysisFlowMode.Simulation, RuleProfileMode.Empty):
                    RunEmptyMode();
                    break;

                // TODO: ［大会品質評価フロー域］（＾～＾）！　［標準ルール］は取り外せるはず（＾～＾）！
                case (AnalysisFlowMode.QualityEvaluation, RuleProfileMode.Standard):
                    RunTournamentQualityEvaluationMode(RuleProfileMode.Standard);
                    break;

                // TODO: ［大会品質評価フロー域］（＾～＾）！　［本戦ステージ］は取り外せるはず（＾～＾）！
                case (AnalysisFlowMode.QualityEvaluation, RuleProfileMode.FinalStage):
                    RunTournamentQualityEvaluationMode(RuleProfileMode.FinalStage);
                    break;

                default:
                    throw new InvalidOperationException("未対応のモードです。");
            }

            // TODO: ［提案者域］は、（ループした話になるが）［大会品質レポートという境界］を受け取って、改善案を考える感じ（＾～＾）！　例えば、［提案者域］で、改善案を考える感じ（＾～＾）！

            // TODO: ここで［大会ルールという境界］を用意されている（＾～＾）！　例えば、［ルール選択域］で、ルールを選んで、ルールの詳細を設定する感じ（＾～＾）！
            //      他にも、［プレイヤー一覧という境界］や、［順位付けの設定という境界］なども用意されている（＾～＾）！
            // TODO: ［シミュレーション域］は、［大会ルールという境界］を受け取って、模擬戦の結果を出力する感じ（＾～＾）！　例えば、［シミュレーション域］で、模擬戦の結果を出力する感じ（＾～＾）！

            // TODO: ここで［大会最終状態という境界］を用意されている（＾～＾）！　例えば、［シミュレーション域］で、模擬戦の結果を出力する感じ（＾～＾）！
            // TODO: ［順位付け域］は、［シミュレーション域］の結果を受け取って、順位付けの方法を選んで、最終順位を出力する感じ（＾～＾）！　例えば、［順位付け域］で、順位付けの方法を選んで、最終順位を出力する感じ（＾～＾）！

            // TODO: ここで［最終順位という境界］を用意されている（＾～＾）！　例えば、［順位付け域］で、順位付けの方法を選んで、最終順位を出力する感じ（＾～＾）！
            // TODO: ここで［大会品質評価フロー域］を行う（＾～＾） 結果として［大会品質レポートという境界］を出力する（＾～＾）！

            // TODO: ここで［大会品質レポートという境界］を用意されている（＾～＾）
            // TODO: ［読者域］は、フォルダーを開いて、レポートを読む。ファイルの場所だけ示してやる（＾～＾）


        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"入力を中断しました: {ex.Message}");
        }
    }
}

