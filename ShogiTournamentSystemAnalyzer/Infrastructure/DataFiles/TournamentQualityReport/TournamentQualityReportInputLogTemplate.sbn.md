# 大会ルール入力ログ

## 概要
- 実行日時: {{ executed_at }}
- 解析フロー: {{ analysis_flow_mode }}
- ルールプロファイル: {{ rule_profile_mode }}
- 入力ファイル: {{ input_file_path }}

## ルール条件
- 順位ルール: {{ tournament_rule_set_mode }}
- Apex / Innov の分け方: {{ final_stage_grouping_mode }}
{{~ if additional_apex_placement_mode != null ~}}
- 本戦不出場Apexの扱い: {{ additional_apex_placement_mode }}
{{~ end ~}}
{{~ if boundary_rescue_mode != null ~}}
- 境界救済戦: {{ boundary_rescue_mode }}
{{~ end ~}}
{{~ if variable_top8_mode != null ~}}
- 可変定員8ルール: {{ variable_top8_mode }}
{{~ end ~}}
{{~ if innov_expected_rank_offset_mode != null ~}}
- Innov の期待順位補正: {{ innov_expected_rank_offset_mode }}
{{~ end ~}}

## 入力サイズ
- 選手数: {{ player_count }}
- 対局数: {{ match_count }}
{{~ if reference_match_count != null ~}}
- 参考対局数: {{ reference_match_count }}
{{~ end ~}}
{{~ if additional_apex_player_count != null ~}}
- 本戦不出場Apex数: {{ additional_apex_player_count }}
{{~ end ~}}

## 実行条件
{{~ if first_player_win_rate_percent != null ~}}
- 先手勝率(%): {{ first_player_win_rate_percent }}
{{~ end ~}}
{{~ if simulation_count != null ~}}
- 試行回数: {{ simulation_count }}
{{~ end ~}}
{{~ if sweep_start_percent != null ~}}
- スイープ開始(%): {{ sweep_start_percent }}
{{~ end ~}}
{{~ if sweep_end_percent != null ~}}
- スイープ終了(%): {{ sweep_end_percent }}
{{~ end ~}}
{{~ if sweep_step_percent != null ~}}
- スイープ刻み(%): {{ sweep_step_percent }}
{{~ end ~}}

## 出力先
- 最終順位CSV: {{ final_ranking_csv_path }}
- 最終順位Markdown: {{ final_ranking_markdown_path }}
{{~ if tournament_final_state_csv_path != null ~}}
- 大会最終状態CSV: {{ tournament_final_state_csv_path }}
{{~ end ~}}
{{~ if tournament_quality_report_csv_path != null ~}}
- 大会品質評価CSV: {{ tournament_quality_report_csv_path }}
{{~ end ~}}
{{~ if tournament_quality_report_markdown_path != null ~}}
- 大会品質評価Markdown: {{ tournament_quality_report_markdown_path }}
{{~ end ~}}

{{~ if note != null ~}}
## 注記
- {{ note }}
{{~ end ~}}