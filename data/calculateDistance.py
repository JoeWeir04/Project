import pandas as pd
import numpy as np

# ---------------------------------------------------------------------------
# CONFIG
# ---------------------------------------------------------------------------
SOURCES_FILE = "raw/sourcePositions.csv"
PATHS_FILE = "processed/VR_paths_cleaned.csv"
LOG_FILE = "processed/VR_log_cleaned.csv"
OUTPUT_FILE = "processed/Processed_Distances.csv"

# SpawnIndex / AudioIndex in VR_log.csv are 0-indexed, while sourcePositions.csv
# names them SpawnPoint1, SpawnPoint2... and audio1, audio2... (1-indexed).
# So SpawnIndex 0 -> "SpawnPoint1", AudioIndex 0 -> "audio1", etc.
SPAWN_INDEX_OFFSET = 1
AUDIO_INDEX_OFFSET = 1


def load_positions(sources_file):
    """Load sourcePositions.csv into two lookup dicts: name -> (x, z)."""
    positions = pd.read_csv(sources_file)

    spawn_positions = {}
    audio_positions = {}

    for _, row in positions.iterrows():
        name = row["Name"]
        xz = (row["PosX"], row["PosZ"])  # 2D distance: X,Z only, ignoring height (Y)
        if row["Type"] == "SpawnPoint":
            spawn_positions[name] = xz
        elif row["Type"] == "AudioSource":
            audio_positions[name] = xz

    return spawn_positions, audio_positions


def compute_straight_line_distances(spawn_positions, audio_positions):
    """
    Compute straight-line (2D, X/Z) distance between every SpawnPoint and every
    AudioSource, once, up front. Returns a dict keyed by (spawn_name, audio_name).
    """
    straight_line = {}
    for spawn_name, (sx, sz) in spawn_positions.items():
        for audio_name, (ax, az) in audio_positions.items():
            dist = np.sqrt((ax - sx) ** 2 + (az - sz) ** 2)
            straight_line[(spawn_name, audio_name)] = dist
    return straight_line


def compute_path_distances(paths_df):
    """
    Compute the actual distance travelled (sum of consecutive sample-to-sample
    distances, 2D X/Z) for every PID + TrialIndex combination in VR_paths.csv.
    Returns a DataFrame with columns: PID, TrialIndex, DistanceTravelled.
    """
    paths_df = paths_df.sort_values(["PID", "TrialIndex", "Time"])

    results = []
    for (pid, trial), group in paths_df.groupby(["PID", "TrialIndex"]):
        if len(group) < 2:
            # Only one sample in this trial -> no movement to measure
            results.append({"PID": pid, "TrialIndex": trial, "DistanceTravelled": 0.0})
            continue

        x = group["PosX"].to_numpy()
        z = group["PosZ"].to_numpy()

        dx = np.diff(x)
        dz = np.diff(z)
        step_distances = np.sqrt(dx ** 2 + dz ** 2)

        results.append({
            "PID": pid,
            "TrialIndex": trial,
            "DistanceTravelled": step_distances.sum(),
        })

    return pd.DataFrame(results)


def main():
    spawn_positions, audio_positions = load_positions(SOURCES_FILE)
    straight_line_lookup = compute_straight_line_distances(spawn_positions, audio_positions)

    paths_df = pd.read_csv(PATHS_FILE)
    log_df = pd.read_csv(LOG_FILE)

    distance_travelled_df = compute_path_distances(paths_df)

    # Merge walked distance into the trial log
    merged = log_df.merge(distance_travelled_df, on=["PID", "TrialIndex"], how="left")

    # Map SpawnIndex / AudioIndex -> actual names used in sourcePositions.csv
    merged["SpawnName"] = "SpawnPoint" + (merged["SpawnIndex"] + SPAWN_INDEX_OFFSET).astype(str)
    merged["AudioName"] = "audio" + (merged["AudioIndex"] + AUDIO_INDEX_OFFSET).astype(str)

    # Look up the precomputed straight-line distance for each trial's spawn/audio pair
    merged["StraightLineDistance"] = merged.apply(
        lambda row: straight_line_lookup.get((row["SpawnName"], row["AudioName"]), np.nan),
        axis=1,
    )

    # --- Effectiveness scoring -------------------------------------------------
    # Two independent measures, each capped to [0, 1], combined multiplicatively.
    # Capping is essential: without it, someone who barely moves gets an inflated
    # MovementEfficiency (tiny DistanceTravelled -> huge ratio) that can mask a
    # terrible ProximityScore, making "barely tried" look identical to "navigated
    # perfectly." Capping both at 1.0 removes that loophole.

    # MovementEfficiency: how direct their route was, relative to the optimal path.
    #   1.0  = travelled (at most) the straight-line distance -- a direct route
    #   <1.0 = wandered, taking a longer route than necessary
    #   Capped at 1.0 so an unrealistically short DistanceTravelled (e.g. barely
    #   moved) can't produce a score above the maximum.
    movement_efficiency = merged["StraightLineDistance"] / merged["DistanceTravelled"].replace(0, np.nan)
    merged["MovementEfficiency"] = movement_efficiency.clip(upper=1.0)

    # ProximityScore: how close they ended up to the target, relative to how far
    # they started.
    #   1.0  = ended exactly at the target (DistanceFromSource = 0)
    #   0.0  = ended up at least as far away as the starting straight-line distance
    #          (i.e. made no net progress towards the target, or moved away from it)
    #   Capped at 0.0 on the low end so moving further away than the starting
    #   distance doesn't produce a negative score.
    proximity_score = 1 - (merged["Distance"] / merged["StraightLineDistance"])
    merged["ProximityScore"] = proximity_score.clip(lower=0.0)

    # Combined effectiveness: only scores well if movement was efficient AND the
    # target was actually approached. Either factor being poor pulls the score down.
    merged["Effectiveness"] = merged["MovementEfficiency"] * merged["ProximityScore"]

    # Flag any trials where distance travelled couldn't be computed (no path samples found)
    missing_paths = merged["DistanceTravelled"].isna().sum()
    if missing_paths > 0:
        print(f"Warning: {missing_paths} trial(s) in VR_log.csv had no matching samples in VR_paths.csv")

    # Flag any trials where the spawn/audio name didn't match sourcePositions.csv
    missing_straight_line = merged["StraightLineDistance"].isna().sum()
    if missing_straight_line > 0:
        print(f"Warning: {missing_straight_line} trial(s) had a SpawnIndex/AudioIndex with no matching "
              f"entry in sourcePositions.csv (check the indexing offset)")

    merged.to_csv(OUTPUT_FILE, index=False)
    print(f"Done. Wrote {len(merged)} rows to {OUTPUT_FILE}")


if __name__ == "__main__":
    main()