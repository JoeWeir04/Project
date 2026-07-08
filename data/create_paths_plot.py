import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.cm as cm
import os

# ---------------------------------------------------------------------------
# CONFIG
# ---------------------------------------------------------------------------
SOURCES_FILE = "raw/sourcePositions.csv"
PATHS_FILE = "processed/VR_paths_cleaned.csv"
LOG_FILE = "processed/VR_log_cleaned.csv"
OUTPUT_DIR = "processed/path_plots"

# Same indexing offset used in process_distances.py (VR_log.csv SpawnIndex/AudioIndex
# are 0-indexed, sourcePositions.csv names are 1-indexed: SpawnPoint1, audio1, ...)
SPAWN_INDEX_OFFSET = 1
AUDIO_INDEX_OFFSET = 1


def load_positions(sources_file):
    """Load sourcePositions.csv. Returns the raw DataFrame plus split spawn/audio frames."""
    positions = pd.read_csv(sources_file)
    spawns = positions[positions["Type"] == "SpawnPoint"]
    audios = positions[positions["Type"] == "AudioSource"]
    return spawns, audios


def plot_paths_for_visualisation(vis_value, trials, log_df, spawns, audios, output_dir):
    """
    trials: list of (pid, trial_index, path_df) tuples, where path_df has
            columns PosX, PosZ sorted by Time, for this one trial.
    """
    fig, ax = plt.subplots(figsize=(8, 8))

    # Distinct colour per trial so overlapping paths can still be told apart
    colours = cm.viridis(np.linspace(0, 0.9, max(len(trials), 1)))

    for (pid, trial_index, path_df), colour in zip(trials, colours):
        ax.plot(
            path_df["PosX"], path_df["PosZ"],
            color=colour, alpha=0.6, linewidth=1.5,
            label=f"PID {pid}, Trial {trial_index}",
        )
        # Mark the start of each path with a small dot
        ax.scatter(
            path_df["PosX"].iloc[0], path_df["PosZ"].iloc[0],
            color=colour, marker="o", s=20, zorder=3,
        )

    # Plot every spawn point and audio source as fixed reference markers
    ax.scatter(
        spawns["PosX"], spawns["PosZ"],
        marker="^", s=120, color="tab:blue", edgecolor="black",
        zorder=4, label="Spawn Points",
    )
    for _, row in spawns.iterrows():
        ax.annotate(row["Name"], (row["PosX"], row["PosZ"]),
                    textcoords="offset points", xytext=(5, 5), fontsize=8, color="tab:blue")

    ax.scatter(
        audios["PosX"], audios["PosZ"],
        marker="*", s=180, color="tab:red", edgecolor="black",
        zorder=4, label="Audio Sources",
    )
    for _, row in audios.iterrows():
        ax.annotate(row["Name"], (row["PosX"], row["PosZ"]),
                    textcoords="offset points", xytext=(5, 5), fontsize=8, color="tab:red")

    ax.set_xlabel("X position")
    ax.set_ylabel("Z position")
    ax.set_title(f"Walked paths — Visualisation {vis_value} ({len(trials)} trials)")
    ax.set_aspect("equal", adjustable="datalim")
    ax.grid(True, alpha=0.3)

    # Trial legend gets long fast with many trials; keep it but place outside the plot.
    # Spawn/Audio markers are the ones worth always seeing clearly in-frame.
    handles, labels = ax.get_legend_handles_labels()
    # Keep only the last two legend entries (Spawn Points, Audio Sources) inline,
    # and put the per-trial list in a separate smaller note if there are many trials.
    ax.legend(handles[-2:], labels[-2:], loc="upper right", fontsize=9)

    os.makedirs(output_dir, exist_ok=True)
    out_path = os.path.join(output_dir, f"visualisation_{vis_value}_paths.png")
    fig.savefig(out_path, dpi=150, bbox_inches="tight")
    plt.close(fig)
    print(f"Saved: {out_path} ({len(trials)} trials)")


def plot_single_task_all_conditions(spawn_name, audio_name, spawns, audios, paths_df, log_df, output_dir):
    fig, ax = plt.subplots(figsize=(8, 8))

    # get all visualisations
    visualisations = sorted(log_df["Visualisation"].unique())

    colors = cm.tab10(np.linspace(0, 1, len(visualisations)))

    for vis, colour in zip(visualisations, colors):

        # get all trials for this condition AND this task
        trials_for_vis = log_df[
            (log_df["Visualisation"] == vis) &
            (log_df["SpawnIndex"] == 3) &   # SpawnPoint4 = index 3 (0-based)
            (log_df["AudioIndex"] == 4)     # audio5 = index 4
        ][["PID", "TrialIndex"]].drop_duplicates()

        for _, row in trials_for_vis.iterrows():
            pid, trial_index = row["PID"], row["TrialIndex"]

            trial_path = paths_df[
                (paths_df["PID"] == pid) &
                (paths_df["TrialIndex"] == trial_index)
            ].sort_values("Time")

            if len(trial_path) < 2:
                continue

            ax.plot(
                trial_path["PosX"], trial_path["PosZ"],
                color=colour,
                alpha=0.4,   # light overlay for participants
                linewidth=1.5
            )

        # dummy line for legend
        ax.plot([], [], color=colour, label=vis)

    # --- draw spawn + audio markers ---
    spawn = spawns[spawns["Name"] == spawn_name]
    audio = audios[audios["Name"] == audio_name]

    ax.scatter(spawn["PosX"], spawn["PosZ"],
               marker="^", s=150, color="black", label=spawn_name)

    ax.scatter(audio["PosX"], audio["PosZ"],
               marker="*", s=200, color="red", label=audio_name)

    ax.set_title(f"Single Task Comparison: {spawn_name} → {audio_name}")
    ax.set_xlabel("X Position")
    ax.set_ylabel("Z Position")
    ax.set_aspect("equal", adjustable="datalim")
    ax.grid(True, alpha=0.3)

    ax.legend()

    os.makedirs(output_dir, exist_ok=True)
    out_path = os.path.join(output_dir, "single_task_all_conditions.png")
    plt.savefig(out_path, dpi=150, bbox_inches="tight")
    plt.close()

    print(f"Saved Tier 1 figure: {out_path}")



def main():
    spawns, audios = load_positions(SOURCES_FILE)
    paths_df = pd.read_csv(PATHS_FILE)
    log_df = pd.read_csv(LOG_FILE)
    print(log_df["Visualisation"].value_counts().sort_index())

    valid_trials = log_df[["PID", "TrialIndex"]].drop_duplicates()

    before = len(paths_df)

    paths_df = paths_df.merge(
        valid_trials,
        on=["PID", "TrialIndex"],
        how="inner"
    )

    after = len(paths_df)

    print(f"Removed {before - after} path samples with no matching log entry")

    paths_df = paths_df.sort_values(["PID", "TrialIndex", "Time"])

    # Group trials by Visualisation value (taken from VR_paths.csv, which already
    # has a Visualisation column per sample / per trial)
    trial_keys = log_df[["PID", "TrialIndex", "Visualisation"]].drop_duplicates()

    visualisations = sorted(trial_keys["Visualisation"].unique())
    print(f"Found visualisations: {visualisations}")

    for vis_value in visualisations:
        keys_for_vis = trial_keys[trial_keys["Visualisation"] == vis_value]

        trials = []
        for _, key_row in keys_for_vis.iterrows():
            pid, trial_index = key_row["PID"], key_row["TrialIndex"]
            trial_path = paths_df[
                (paths_df["PID"] == pid) & (paths_df["TrialIndex"] == trial_index)
            ]
            if len(trial_path) < 2:
                print(f"Skipping PID {pid}, Trial {trial_index} ({vis_value}) - only {len(trial_path)} samples")
                continue
            trials.append((pid, trial_index, trial_path))

        if not trials:
            print(f"Skipping Visualisation {vis_value}: no trials with >=2 samples")
            continue

        plot_paths_for_visualisation(vis_value, trials, log_df, spawns, audios, OUTPUT_DIR)

        paths_df = paths_df.merge(valid_trials, on=["PID", "TrialIndex"], how="inner")
        paths_df = paths_df.sort_values(["PID", "TrialIndex", "Time"])

        # FIXED TASK (Tier 1 figure)
        spawn_name = "SpawnPoint4"
        audio_name = "audio5"

        plot_single_task_all_conditions(
            spawn_name,
            audio_name,
            spawns,
            audios,
            paths_df,
            log_df,
            OUTPUT_DIR
        )


if __name__ == "__main__":
    main()