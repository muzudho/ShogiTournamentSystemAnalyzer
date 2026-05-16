internal static partial class Program
{
    static void RunFinalStageMode()
    {
        Console.WriteLine("本戦専用モード: Apex / Innov 分割またはニュートラルな対局表を分析します。\n");

        PrintFinalStageInputSample();
        if (!TryReadFinalStageModeContext(out var context))
        {
            return;
        }

        var result = ExecuteFinalStageMode(context, out var standardResultRows, out var finalStageResultRows);
        PrintFinalStageModeContext(context);
        WriteFinalStageModeOutputs(context, result, standardResultRows, finalStageResultRows);
    }
}

