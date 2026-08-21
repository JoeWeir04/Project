#!/bin/bash
set -e  # stop immediately if any script fails

echo "=== Cleaning data & calculating distances ==="
py Clean_log_data.py
py Clean_path_data.py
py Clean_all_visualisations_survey_data.py
py Clean_condition_survey_data.py
py calculate_distance.py

echo "=== Creating scores & plotting graphs ==="
py create_nasa_tlx_plot.py
py create_umux_plot.py
py create_violin_plot.py
py calculate_visual_load.py
py plot_performance_qs.py
py create_distance_vs_angle_plot.py
py plot_participant_distance_switch.py

echo "=== All scripts completed successfully ==="