internal static class VariableTop8Rule
{
    internal static int GetPromotedInnovCount(VariableTop8Mode mode, int additionalApexCount)
    {
        return mode == VariableTop8Mode.On ? additionalApexCount : 0;
    }

    internal static string GetLabel(VariableTop8Mode mode)
    {
        return mode == VariableTop8Mode.On
            ? "On（本戦不出場Apex人数ぶん Innov 上位を総合上位8へ引き上げる）"
            : "Off（定員8固定）";
    }
}

