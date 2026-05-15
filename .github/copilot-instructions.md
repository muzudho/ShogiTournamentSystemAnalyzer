# Copilot Instructions

## プロジェクト ガイドライン
- The user prefers using the term '対局記号表' to refer to the player-symbol mapping list in the ShogiTournamentPairingAnalyzer project.
- The user prefers using the term '重箱表' for the aggregated node-ranking table that combines ▲player and ▽player into ・player.
- The user prefers PascalCase folder names like 'Docs' instead of lowercase 'docs' in this project when naming folders.
- The user prefers extracting toggleable rule logic into separate classes under a Domain/Rules-style folder rather than keeping growing rule logic in Program.cs.
- In ShogiTournamentPairingAnalyzer, use the neutral rule as the evaluation baseline: tournament rules better than the neutral baseline are classified as Good, and rules worse than the neutral baseline are classified as Bad.
- For the 格付けグラフ戦 proposal examples, use the assumption 'the stronger player wins' instead of assuming all ▽ sides lose and ▲ sides win.
- In tournament ranking proposals, do not use original Elo in ranking calculation because participants join without computed original Elo.
- For tournament naming in this project, prefer names that can be phrased as '○○式トーナメント' in the style of Swiss-system or ladder-style naming, including 'ツイル式トーナメント' as a preferred naming option and considering the Japanese term '斜文' as part of the concept.

## Tournament Announcement Draft Guidelines
- For the tournament announcement draft, remove making-of/backstory for first-time readers.
- Structure the announcement logically to ensure clarity.
- Make the procedure immediately practical for readers.

## Experimental Improvement Reports
- Experimental improvement reports should be kept separated by good/bad outcome.
- The behavior of the reports should be switchable with an On/Off option to prevent mixing results.