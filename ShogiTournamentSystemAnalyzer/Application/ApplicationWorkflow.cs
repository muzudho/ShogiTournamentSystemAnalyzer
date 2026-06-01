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
        ApplicationStartup.Start();
        RequestWorkflow.Run(args);

        RequestModel? requestModel = null;

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
            requestModel = requestModelProducer.RequestModel;
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
            requestModel = requestModelProducer.RequestModel;
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
                RequestFileCreateWorkflow.Run(requestModel);
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
        var analysisResultModel = AnalysisWorkflowNewVersion.Run(requestModel);
        //      │
        //      ↓
        //      終了
        return new ApplicationWorkflowResultModel(hasError: false);
    }
}
