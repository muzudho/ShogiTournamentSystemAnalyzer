/*
 * ［アプリケーション　＞　アプリケーション・ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.ManualInput;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCreate;
using ShogiTournamentSystemAnalyzer.Domain.Request;

/// <summary>
/// アプリケーション全体の最上位ワークフロー。
/// </summary>
internal static class ApplicationWorkflow
{
    internal static ApplicationWorkflowResultModel Run(IReadOnlyList<string> args)
    {
        // 現在の実装
        ApplicationStartup.Start();
        RequestWorkflow.Run(args);

        // TODO: ここから下は、将来的な実装

        // ［依頼という境界］
        RequestBoundary requestBoundary = new();

        //          開始
        //          │
        //          ↓
        //          ◆"コマンドライン引数で入力ファイルを指定したか？"
        //          │
        //          ├─────────────────────────┐
        //          │                                                  ・
        //          │ "はい"                                           ・
        if (args.Count > 0)
        {
            //      │                                                  ・
            //      │                                                  ・
            //      ■［要求ファイルチェック］(`RequestFileCheck`)      ・
            var requestModelProducer = RequestFileCheckWorkflow.Run(args);
            //      │                                                  ・
            //      │                                                  ・
            //      ◆"エラーが有ったか？"                              ・
            //      │                                                  ・
            //      │                                                  ・
            //      ├──────────┐                            ・
            //      ・                    │                            ・
            //      ・                    │ "エラー有り"               ・
            if (requestModelProducer.HasError)
            {
                //  ・                    │                            ・
                //  ・                    ↓                            ・
                //  ・                    ●終了                        ・
                return new ApplicationWorkflowResultModel(hasError: true);
            }
            //      │                                                  ・
            //      │                                                  ・
            //      │  "エラー無し"                                    ・
            requestModelProducer.Produce(requestBoundary);
        }
        //          ・                                                  │
        //          ・                                                  │
        //          ・                                                  │ "いいえ"
        else
        {
            //      ・                                                  │
            //      ・                                                  │
            //      ・                                                  ■［手動入力］（`ManualInput`）
            var requestModelProducer = ManualInputWorkflow.Run();
            //      ・                                                  │
            //      ・                                                  │
            //      ・                                                  ◆"エラーが有ったか？"
            //      ・                                                  │
            //      ・                                                  ├──────────┐
            //      ・                                                  ・                    │
            //      ・                                                  ・                    │ "エラー有り"
            if (requestModelProducer.HasError)
            {
                //  ・                                                  ・                    │
                //  ・                                                  ・                    ↓
                //  ・                                                  ・                    ●終了
                return new ApplicationWorkflowResultModel(hasError: true);
            }
            //      ・                                                  │
            //      ・                                                  │
            //      ・  "エラー無し"                                    │
            requestModelProducer.Produce(requestBoundary);
            //      ・                                                  │
            //      ・                                                  ↓
            //      ・                                                  ◆"今回の入力を保存しておきますか？"
            //      ・                                                  │
            //      ・                                                  ├───────────────────────┐
            //      ・                                                  │                                              ・
            //      ・                                                  │ "はい"                                       ・
            if (requestModelProducer.ShallSave)
            {
                //  ・                                                  │                                              ・
                //  ・                                                  │                                              ・
                //  ・                                                  ■［要求ファイル作成］(`RequestFileCreate`)     ・
                RequestFileCreateWorkflow.Run(requestBoundary);
            }
            //      ・                                                  ・                                              │ "いいえ"
            //      ・                                                  ・                                              │
            //      ・                                                  │←──────────────────────┘
            //      ・                                                  │
            //      │←────────────────────────┘
            //      │
        }
        //      │
        //      ↓
        //      ■［分析］(`Analysis`)
        AnalysisWorkflowNewVersion.Run(requestBoundary);
        //      │
        //      ↓
        //      終了
        return new ApplicationWorkflowResultModel(hasError: false);
    }
}
