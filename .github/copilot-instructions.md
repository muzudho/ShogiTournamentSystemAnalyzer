# Copilot Instructions

## 一般ガイドライン
- 反復作業の抑制を必須としない。必要なら通常どおり反復作業を継続する。
- この開発環境では `run_build` と `rg` コマンドが使えない場合があるため、コンパイル確認には `dotnet build` を優先し、ワークスペース調査は組み込みの検索ツールを優先する。旧命名の探索はワークスペース検索ツールで行う。
- ドキュメントのみの変更では、原則としてビルド確認を省略する。ビルド確認が必要な場合は上記の手順に従う。
- 出力は文字化けさせず、通常の日本語テキストで安定して返すこと。
- 命名変更やコード整理の作業後はユーザー確認を待たずにビルドまで実施する。
- 他で再利用されず親メソッドの文脈に強く依存する補助メソッドはローカル関数にする方針を希望する。
- メソッド内のローカル関数は必要に応じて使い、親メソッドの概要を先に読みやすくするため、呼び出し本体を先に置いてローカル関数定義はメソッド末尾へ寄せる構成を好む。
- このプロジェクトでは SimulationContext の共通プロパティは共通親へ寄せて抽象化を進める方針で、完全共通化が難しい部分は当面サブクラス側に残し、共通化できる処理は親クラスへ寄せ、共通化できない部分だけをサブクラスに残して差分が見えやすい構造を好む。
- このプロジェクトでは、標準版と本戦版が混在する writer は抽象クラス・標準版・本戦版の継承関係へ分け、共通部分だけを親へ置く構造を好む。

## プロジェクト ガイドライン
- README は利用者の導線を優先し、最初の 1 回を通すための入口として構成する。特に冒頭では『何のツールか』『起動方法』『大会ルールの入力方法』を先に案内し、思想・背景・用語の重い説明はサブドキュメントへ分離する。
- モード体系を完全移行する: 上位は『対局シミュレーション / 品質評価』とし、下位をルール種別として増やせる構成にする。
- プロジェクトの目的を明確にする: 大会ルールの部品を考案し、それを大会ルールへ取り入れてシミュレーション品質を現在の最高品質と比較しながら、より良い大会ルールを反復的に作っていくこと。
- 大会運営について考えられるように、とっつきやすいツールとして表現・設計する。
- 大会制度全体（予選・本戦）を含めた分析を重視し、名称や設計は System 寄りの表現・構成を使用する。
- 基本用語は『選手 / Player』を使用して、人間・コンピューター両方の対局主体を指す。コンピューター将棋大会の事情は補足注釈として記載し、必要に応じて（特に説明書では）'参加者' を人間を指す用語として扱う旨を明示する。対局ソフトの表現としては引き続き 'エントリ'、'ソフト'、'対局プログラム' などを併記して区別する。
- このプロジェクトでは旧語を使わず、players に統一する。
- The user prefers using the term '対局記号表' to refer to the player-symbol mapping list in the ShogiTournamentSystemAnalyzer project.
- The user prefers using the term '重箱表' for the aggregated node-ranking table that combines ▲player and ▽player into ・player.
- フォルダー名はパスカルケースに揃える。
- Use PascalCase folder names like 'Docs' instead of lowercase 'docs' in this project when naming folders. Consolidate data-related folders under a single PascalCase 'Data' folder and subdivide within it; avoid creating many separate top-level data folders (ユーザーはデータ系フォルダーを増やしすぎず、`Data` 配下にまとめてその下を細分化する構成を好む)。
- Place a PascalCase 'Inputs' folder adjacent to 'Examples' to hold execution/input files for runs.
- In Inputs, include a short descriptive prompt line in execution/input files as a hash-comment plus PascalCase tag, e.g., #[Prompt] ... , so numeric values are not ambiguous. Mark input termination explicitly with a tag like #[Enter] to avoid dependence on blank lines.
- Extract toggleable rule logic into separate classes under a Domain/Rules-style folder rather than keeping growing rule logic in Program.cs. In Program.cs, prioritize separating input procedures into two responsibilities: rule construction (use builders/composers for rules) and parameter setting (load/parse inputs and configure parameters), so Program.cs functions as orchestration/wiring only. This project prioritizes not aggregating everything into the Program class, but rather establishing dedicated class names for each responsibility to enhance clarity.
- In ShogiTournamentSystemAnalyzer, use the neutral rule as the evaluation baseline: tournament rules better than the neutral baseline are classified as Good, and rules worse than the neutral baseline are classified as Bad.
- For the 格付けグラフ戦 proposal examples, use the assumption 'the stronger player wins' instead of assuming all ▽ sides lose and ▲ sides win.
- In tournament ranking proposals, do not use original Elo in ranking calculation because players join without computed original Elo.
- For tournament naming in this project, prefer names that can be phrased as '○○式トーナメント' in the style of Swiss-system or ladder-style naming, including 'ツイル式トーナメント' as a preferred naming option and considering the Japanese term '斜文' as part of the concept.
- Use longer, explicit type names prefixed with the main data boundary names (大会ルールデータ・プレイヤー一覧データ・順位付けの設定データ・大会結果データ・最終順位データ・大会品質レポート) rather than short generic names in this project.
- このプロジェクトでは 6大境界の名称として TournamentFinalState を使う。
- このプロジェクトでは Domain フォルダー配下を『5大域 / 6大境界』のフォルダーだけで構成する方針を採る。
- ユーザーは Infrastructure/DataFiles 配下を『境界名ごとのデータファイル実装 + Shared』で構成する方針を希望し、この方針を今後も覚えておくことを求めている。
- ユーザーは Infrastructure/DataFiles 配下の大きなファイル内メソッドも、役割に応じて『4大域』『6大境界』または Shared の専用クラスへ分割する構成を好む。
- ユーザーは ISimulationScenario などの新規抽象も ShogiTournamentSystemAnalyzer 直下ではなく、4大域・6大境界に沿った適切な専用 namespace と専用クラスへ整理することを強く希望している。

## アーキテクチャ
- 各データ要素の境界を明確に分離する: 大会ルールデータ・プレイヤー一覧データ・順位付けの設定データ・大会結果データ・最終順位データ・大会品質レポート を個別の責務として扱う。
- 上記の各データ境界をインターフェースとして定義し、具体実装（永続化・読み取り・検証）はインターフェースの実装として Data 配下などに分離する。
- データ境界の分離により、テスト・差し替え・並列開発を容易にし、Program.cs やドメインロジックから永続化の詳細を切り離す。
- Data フォルダー構成と連携させ、Data 配下でインターフェース定義と具体実装を整理する（既存の Data フォルダ方針と整合）。
- このプロジェクトでは設計の骨格として『5大域』と『6大境界』を使う。6大境界の基準名は TournamentRule, PlayerList, RankingSettings, TournamentFinalState, FinalRanking, TournamentQualityReport とする。

### 実行・ベンチマーク
- Start with a 1-trial smoke test before benchmarking with small trial counts (e.g., 10 and 100) to estimate runtime before launching a large sweep.

## Tournament Announcement Draft Guidelines
- For the tournament announcement draft, begin with a worked sample so first-time readers can understand it before the conceptual explanation.
- Remove making-of/backstory for first-time readers.
- Structure the announcement logically to ensure clarity.
- Make the procedure immediately practical for readers.

## Experimental Improvement Reports
- Experimental improvement reports should be kept separated by good/bad outcome.
- The behavior of the reports should be switchable with an On/Off option to prevent mixing results.