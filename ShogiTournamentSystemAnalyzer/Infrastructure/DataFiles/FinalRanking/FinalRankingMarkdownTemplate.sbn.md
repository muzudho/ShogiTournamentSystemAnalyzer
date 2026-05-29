# 最終順位結果レポート

## 概要
- 結果CSV: {{ output_csv_link }}
- 版: {{ edition_label }}
- 計算モード: {{ mode }}
- 同Elo対局時の先手勝率: {{ first_player_win_rate_percent }}%
- 対象選手数: {{ player_count }}
{{ if representative_ranking_markdown_link != null }}
- representative順位表: {{ representative_ranking_markdown_link }}
{{ end }}
{{ if reference_matches_csv_link != null }}
- 参考対局CSV: {{ reference_matches_csv_link }}
{{ end }}
{{ if overview_note != null }}
- 注記: {{ overview_note }}
{{ end }}

{{ if primary_sections_text != null }}
{{ primary_sections_text }}
{{ end }}

{{ if primary_table_rows_text != null }}
{{ primary_table_rows_text }}
{{ end }}

{{ if trailing_sections_text != null }}
{{ trailing_sections_text }}
{{ end }}

{{ if charts_text != null }}
{{ charts_text }}
{{ end }}
