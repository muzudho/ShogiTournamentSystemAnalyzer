# Console.SetIn 副作用除去 実装計画 2026-06-02

## 目的

`Console.SetIn()` のようにプロセス全体の標準入力を差し替える実装をなくす。
要求ファイル入力は `File.ReadAllLines(path)` / `StsaFileIOHelper.ReadAllLines(...)` で明示的に読み取り、後続処理へモデルまたは引数で渡す。

## 禁止事項

- `Console.SetIn(...)` を新規追加しない。
- `Console.In` を一時退避して、後で `Console.SetIn(originalInput)` で戻す設計にしない。
- 要求ファイル入力を、標準入力差し替えによって `Console.ReadLine()` 導線へ流し込まない。

## 調査結果

### 1. 要求ファイルチェックが標準入力を差し替えている

対象: `ShogiTournamentSystemAnalyzer/Application/RequestFileCheck/RequestFileCheckWorkflow.cs`

現在の問題:

- `StsaFileIOHelper.ReadAllLines(...)` で要求ファイルを読んだあと、`filteredInput` を作る。
- その後 `Console.SetIn(new StringReader(filteredInput))` でプロセス全体の標準入力を差し替えている。
- これは「要求ファイルチェック」が「後続の `Console.ReadLine()` に入力を食わせる」副作用を持っている状態。

置き換え方針:

- `RequestFileCheckWorkflow.Run(...)` は `Console.SetIn(...)` を呼ばない。
- `filteredInput` は `string` のまま、または `IReadOnlyList<string>` に戻して、`RequestFileCheckResultVer2` / `RequestInputSession` へ持たせる。
- 後続の分析入力は、要求ファイルモードならその行データから読む。
- 手動入力モードなら従来通りユーザーから `Console.ReadLine()` する。

候補 API:

```csharp
internal sealed class RequestInputSession
{
    internal IReadOnlyList<string>? RequestFileInputLines { get; }
    internal RequestFileCreateCompletionTarget? CompletionTarget { get; }
}
```

または:

```csharp
internal sealed class RequestInputSession
{
    internal TextReader? RequestFileInputReader { get; }
    internal RequestFileCreateCompletionTarget? CompletionTarget { get; }
}
```

ただし `TextReader` を持つ場合でも、`Console.SetIn()` には渡さず、明示的に reader を読む処理へ渡す。

### 2. 手動入力記録開始が標準入力を差し替えている

対象: `ShogiTournamentSystemAnalyzer/Application/AfterManualInput/ManualInputRecordingSessionStarter.cs`

現在の問題:

- `Console.In` を `originalInput` として退避する。
- `ManualInputRecordingTextReader` を作る。
- `Console.SetIn(recordingInput)` でプロセス全体の標準入力を差し替えている。

置き換え方針:

- 手動入力記録は、`Console.In` 差し替えではなく、読み取り入口側に `IInputReader` か `TextReader` を渡して記録する。
- 最小変更なら、コンソール読み取りをラップする小さな入力サービスを作る。
- 記録する場合は、そのサービスが `Console.ReadLine()` の戻り値を記録する。
- 記録しない場合は、そのサービスが `Console.ReadLine()` を返すだけにする。

候補 API:

```csharp
internal interface IConsoleInput
{
    string? ReadLine();
}

internal sealed class ConsoleInput : IConsoleInput
{
    public string? ReadLine() => Console.ReadLine();
}

internal sealed class RecordingConsoleInput : IConsoleInput
{
    readonly IConsoleInput inner;
    readonly List<string> recordedLines = [];

    public string? ReadLine()
    {
        var line = inner.ReadLine();
        if (line is not null) recordedLines.Add(line);
        return line;
    }
}
```

実装時は既存の `ConsoleInputReaders` / `ConsolePromptReaders` / `ConsoleRuleReaders` に一気に注入する範囲が広いので、段階的に行う。

### 3. RequestInputSession.Dispose が標準入力を戻している

対象: `ShogiTournamentSystemAnalyzer/Application/Shared/RequestInputSession.cs`

現在の問題:

- `RequestInputSession` が `TextReader originalInput` を持っている。
- `Dispose()` で `Console.SetIn(originalInput)` を呼んでいる。
- これは `ManualInputRecordingSessionStarter` の差し替えを戻すための後始末であり、差し替え禁止後は不要。

置き換え方針:

- `RequestInputSession` から `IDisposable` と `originalInput` を削除する。
- `CompletionTarget` と要求ファイル由来の読み取り済みデータだけを持つ単純な入力セッションモデルにする。

### 4. Program.cs に入力導線が二重に残っている

対象: `ShogiTournamentSystemAnalyzer/Program.cs`

現在の問題:

- 前半で要求ファイル有無を見て `inputSession` を作っている。
- 直後に `args.Length > 0` で再び分岐し、手動入力時だけ `ManualInputWorkflow.Run()` を実行している。
- 要求ファイルモードでは、`RequestFileCheckWorkflow` が `Console.SetIn(...)` した後、後続の `Console.ReadLine()` が暗黙に要求ファイルを読む前提になっている。

置き換え方針:

- `Program.cs` は、まず `RequestInputSession` を作る。
- その後の分析前入力は、`RequestInputSession` の入力ソースを明示的に使う。
- 要求ファイルモードと手動入力モードを、標準入力差し替えではなく、入力ソースの種類で分岐させる。

## 推奨実装順

1. `RequestInputSession` を、標準入力復元用の disposable ではなく、入力ソースを表す単純なモデルへ変更する。
2. `RequestFileCheckResultVer2` に、読み取り済み要求ファイル入力を持つ `RequestInputSession` を返させる。
3. `RequestFileCheckWorkflow` から `Console.SetIn(new StringReader(filteredInput))` を削除する。
4. 要求ファイル入力を使う後続処理へ、行データまたは専用 reader を明示的に渡す。
5. 手動入力記録用に `IConsoleInput` / `RecordingConsoleInput` のような入力サービスを作る。
6. `ManualInputRecordingSessionStarter` から `Console.SetIn(recordingInput)` を削除する。
7. `RequestInputSession.Dispose()` と `originalInput` を削除する。
8. `rg -n "Console\.SetIn|Console\.In" ShogiTournamentSystemAnalyzer` で禁止対象が消えたことを確認する。
9. `dotnet build .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj` を実行する。

## 段階実装の注意

- 一度に全 `Console.ReadLine()` を置き換えると影響範囲が大きい。
- まずは要求ファイルモードの `Console.SetIn` をなくすため、要求ファイル由来の入力を `RequestInputSession` に持たせる。
- 次に手動入力記録のための `Console.SetIn` をなくす。
- 最後に `ConsoleInputReaders` / `ConsolePromptReaders` / `ConsoleRuleReaders` を、必要な範囲から入力サービス対応に寄せる。

## 確認コマンド

```powershell
rg -n "Console\.SetIn|Console\.In" ShogiTournamentSystemAnalyzer
dotnet build .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj
```

## 現時点の補足

- `Program.cs` の `Console.OutputEncoding = Encoding.UTF8` はプロセス全体設定だが、文字化け対策の起動時設定であり、今回の禁止対象である「入力ソース差し替え」とは別扱いにする。
- ただし、今後「プロセス全体設定を完全禁止」に広げる場合は、別計画で扱う。

## テキストファイル保存方針

- テキストファイルは Windows の改行、つまり CRLF を使う。
- UTF-8 は BOM なしで保存する。

## 2026-06-02 実装開始メモ

- 目的: `Console.SetIn()` / `Console.In` 退避復元をコードから除去する。
- 最初の実装方針: `Console.ReadLine()` 呼び出しを専用の入力口へ寄せ、要求ファイルモードでは読み取り済みテキストをその入力口へ渡す。
- 注意: `Console.SetIn()` は使わない。要求ファイルは `StsaFileIOHelper.ReadAllLines(...)` で読む。
## 2026-06-02 実装途中メモ 1

- `Presentation/ConsoleCustom/ConsoleInput.cs` を追加した。
- 既存の `Console.ReadLine()` 呼び出しは、手動入力の実読み取り口である `ConsoleInput.ReadLine()` へ寄せた。
- `RequestFileCheckWorkflow` の `Console.SetIn(new StringReader(filteredInput))` は削除し、`RequestInputSession` に `filteredInput` を持たせる形へ変更した。
- `ManualInputRecordingSessionStarter` の `Console.In` 退避と `Console.SetIn(recordingInput)` は削除した。
- `RequestInputSession.Dispose()` の `Console.SetIn(originalInput)` は削除した。
- `Program.cs` は、要求ファイルモードでは `ConsoleInput.UseText(inputSession.RequestFileInputText)` を呼び、手動入力記録の書き出しを分析後へ移した。

## 2026-06-02 実装完了メモ

- `rg -n "Console\.SetIn|Console\.In" ShogiTournamentSystemAnalyzer` でコード内の禁止対象が 0 件になった。
- `Console.ReadLine()` は `Presentation/ConsoleCustom/ConsoleInput.cs` の手動入力実読み取り口にだけ残した。
- `dotnet build .\ShogiTournamentSystemAnalyzer\ShogiTournamentSystemAnalyzer.csproj` は警告 0 件、エラー 0 件で成功した。
- 今回の計画メモは CRLF、UTF-8 BOM なしで保存した。
