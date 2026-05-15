# Copilot Instructions

## プロジェクト ガイドライン
- The user prefers using the term '対局記号表' to refer to the player-symbol mapping list in the ShogiTournamentPairingAnalyzer project.
- The user prefers PascalCase folder names like 'Docs' instead of lowercase 'docs' in this project when naming folders.
- The user prefers extracting toggleable rule logic into separate classes under a Domain/Rules-style folder rather than keeping growing rule logic in Program.cs.
- In ShogiTournamentPairingAnalyzer, use the neutral rule as the evaluation baseline: tournament rules better than the neutral baseline are classified as Good, and rules worse than the neutral baseline are classified as Bad.
- For the 格付けグラフ戦 proposal examples, use the assumption 'the stronger player wins' instead of assuming all ▽ sides lose and ▲ sides win.

## Experimental Improvement Reports
- Experimental improvement reports should be kept separated by good/bad outcome.
- The behavior of the reports should be switchable with an On/Off option to prevent mixing results.