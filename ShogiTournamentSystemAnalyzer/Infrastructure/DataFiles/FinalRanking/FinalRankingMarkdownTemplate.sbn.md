# 最終順位結果レポート

## 概要
- 結果CSV: {{ output_csv_link }}
- 版: {{ edition_label }}
- 計算モード: {{ mode }}
- 同Elo対局時の先手勝率: {{ first_player_win_rate_percent }}%
- 対象選手数: {{ player_count }}
{{~ if representative_ranking_markdown_link != null ~}}
- representative順位表: {{ representative_ranking_markdown_link }}
{{~ end ~}}
{{~ if reference_matches_csv_link != null ~}}
- 参考対局CSV: {{ reference_matches_csv_link }}
{{~ end ~}}
{{~ if overview_note != null ~}}
- 注記: {{ overview_note }}
{{~ end ~}}

## 上位候補一覧
{{ primary_table_header }}
{{ primary_table_header_separator }}
{{~ for row in primary_table_rows ~}}
{{ row }}
{{~ end ~}}

## 注目ポイント
{{~ for line in attention_points ~}}
- {{ line }}
{{~ end ~}}

## 自動コメント
{{~ for line in auto_comments ~}}
- {{ line }}
{{~ end ~}}

{{~ for section in trailing_sections ~}}

{{ section.title }}
{{ section.table_header }}
{{~ for row in section.rows ~}}
{{ row }}
{{~ end ~}}
{{~ end ~}}

{{~ if charts.size > 0 ~}}

## Mermaid 図
{{~ for chart in charts ~}}
{{~ if !for.first ~}}

{{~ end ~}}
```mermaid
xychart-beta
    title "{{ chart.title }}"
    x-axis [{{~ for category in chart.categories ~}}{{~ if !for.first ~}}, {{ end ~}}{{ category }}{{~ end ~}}]
    y-axis "{{ chart.y_axis_label }}" {{ chart.y_axis_range }}
    bar [{{~ for value in chart.values ~}}{{~ if !for.first ~}}, {{ end ~}}{{ value }}{{~ end ~}}]
```
{{~ end ~}}
{{~ end ~}}
