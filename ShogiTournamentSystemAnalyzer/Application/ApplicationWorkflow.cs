/*
 * ［アプリケーション　＞　アプリケーション・ワークフロー］
 */
namespace ShogiTournamentSystemAnalyzer.Application;

using ShogiTournamentSystemAnalyzer.Application.Analysis;
using ShogiTournamentSystemAnalyzer.Application.ManualInput;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCheck;
using ShogiTournamentSystemAnalyzer.Application.RequestFileCreate;

/// <summary>
/// アプリケーション全体の最上位ワークフロー。
/// </summary>
internal static class ApplicationWorkflow
{
    internal static void Run(IReadOnlyList<string> args)
    {
        ApplicationStartup.Start();
        RequestWorkflow.Run(args);

        //  開始
        //  │
        //  ↓
        //  ◆"コマンドライン引数で入力ファイルを指定したか？"
        //  │
        //  ├─────────────────────────┐
        //  │                                                  │
        //  │ "はい"                                           │
        //  │                                                  │
        //  ■［要求ファイルチェック］(`RequestFileCheck`)      │
        RequestFileCheckWorkflow.Run();
        //  │                                                  │
        //  ◆"エラーが有ったか？"                              │
        //  │                                                  │
        //  ├──────────┐                            │
        //  │                    │                            │
        //  │ "エラー無し"       │ "エラー有り"               │
        //  │                    │                            │
        //  │                    ↓                            │
        //  │                     終了                         │
        //  │                                                  │
        //  │                                                  │ "いいえ"
        //  │                                                  │
        //  │                                                  ■［手動入力］（`ManualInput`）
        ManualInputWorkflow.Run();
        //  │                                                  │
        //  │                                                  ↓
        //  │                                                  ◆s"今回の入力を保存しておきますか？"
        //  │                                                  │
        //  │                                                  ├───────────────────────┐
        //  │                                                  │                                              │
        //  │                                                  │ "はい"                                       │
        //  │                                                  │                                              │
        //  │                                                  ■［要求ファイル作成］(`RequestFileCreate`)     │
        RequestFileCreateWorkflow.Run();
        //  │                                                  │                                              │
        //  │                                                  │                                              │ "いいえ"
        //  │                                                  │                                              │
        //  │                                                  │←──────────────────────┘
        //  │                                                  │
        //  │←────────────────────────┘
        //  │
        //  ↓
        //  ■［分析］(`Analysis`)
        AnalysisWorkflowNewVersion.Run();
        //  │
        //  ↓
        //  終了
    }
}
