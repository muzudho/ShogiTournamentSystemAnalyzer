# Copilot Instructions

## プロジェクト ガイドライン
- 大会制度全体（予選・本戦）を含めた分析を重視し、名称や設計は System 寄りの表現・構成を使用する。
- The user prefers using the term '対局記号表' to refer to the player-symbol mapping list in the ShogiTournamentSystemAnalyzer project.
- The user prefers using the term '重箱表' for the aggregated node-ranking table that combines ▲player and ▽player into ・player.
- Use PascalCase folder names like 'Docs' instead of lowercase 'docs' in this project when naming folders.
- Place a PascalCase 'Inputs' folder adjacent to 'Examples' to hold execution/input files for runs.
- In Inputs, include a short descriptive prompt line in execution/input files as a hash-comment plus PascalCase tag, e.g., #[Prompt] ... , so numeric values are not ambiguous. Mark input termination explicitly with a tag like #[Enter] to avoid dependence on blank lines.
- The user prefers extracting toggleable rule logic into separate classes under a Domain/Rules-style folder rather than keeping growing rule logic in Program.cs.
- In ShogiTournamentSystemAnalyzer, use the neutral rule as the evaluation baseline: tournament rules better than the neutral baseline are classified as Good, and rules worse than the neutral baseline are classified as Bad.
- For the 格付けグラフ戦 proposal examples, use the assumption 'the stronger player wins' instead of assuming all ▽ sides lose and ▲ sides win.
- In tournament ranking proposals, do not use original Elo in ranking calculation because participants join without computed original Elo.
- For tournament naming in this project, prefer names that can be phrased as '○○式トーナメント' in the style of Swiss-system or ladder-style naming, including 'ツイル式トーナメント' as a preferred naming option and considering the Japanese term '斜文' as part of the concept.
- When running expensive tournament simulations in this project, start with a 1-trial smoke test before benchmarking with small trial counts like 10 and 100 to estimate runtime before launching a large sweep.

## Tournament Announcement Draft Guidelines
- For the tournament announcement draft, begin with a worked sample so first-time readers can understand it before the conceptual explanation.
- Remove making-of/backstory for first-time readers.
- Structure the announcement logically to ensure clarity.
- Make the procedure immediately practical for readers.

## Experimental Improvement Reports
- Experimental improvement reports should be kept separated by good/bad outcome.
- The behavior of the reports should be switchable with an On/Off option to prevent mixing results.