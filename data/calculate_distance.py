import pandas as pd
import numpy as np

SOURCES_FILE = "raw/sourcePositions.csv"
NAV_FILE = "raw/navmeshDistances.csv"
PATHS_FILE = "processed/VR_paths_cleaned.csv"
LOG_FILE = "processed/VR_log_cleaned.csv"
OUTPUT_FILE = "processed/Processed_Distances.csv"


SPAWN_INDEX_OFFSET = 1
AUDIO_INDEX_OFFSET = 1


def load_positions(sources_file):
    """Load sourcePositions.csv into two lookup dicts: name -> (x, z)."""
    positions = pd.read_csv(sources_file)

    spawn_positions = {}
    audio_positions = {}

    for _, row in positions.iterrows():
        name = row["Name"]
        xz = (row["PosX"], row["PosZ"])  
        if row["Type"] == "SpawnPoint":
            spawn_positions[name] = xz
        elif row["Type"] == "AudioSource":
            audio_positions[name] = xz

    return spawn_positions, audio_positions

def load_nav_distances(sources_file):
    nav_distances = pd.read_csv(sources_file)
    distances = {}
    for _, row in nav_distances.iterrows():
        spawn_name = row["SpawnName"]
        audio_name = row["AudioName"]
        distance = row["NavMeshDistance"]
        distances[(spawn_name,audio_name)] = distance

    return distances
    



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
    nav_distances_lookup = load_nav_distances(NAV_FILE)
    straight_line_lookup = compute_straight_line_distances(spawn_positions, audio_positions)
    

    paths_df = pd.read_csv(PATHS_FILE)
    log_df = pd.read_csv(LOG_FILE)

    distance_travelled_df = compute_path_distances(paths_df)

    merged = log_df.merge(distance_travelled_df, on=["PID", "TrialIndex"], how="left")

    merged["SpawnName"] = "SpawnPoint" + (merged["SpawnIndex"] + SPAWN_INDEX_OFFSET).astype(str)
    merged["AudioName"] = "audio" + (merged["AudioIndex"] + AUDIO_INDEX_OFFSET).astype(str)

    merged["StraightLineDistance"] = merged.apply(
        lambda row: straight_line_lookup.get((row["SpawnName"], row["AudioName"]), np.nan),
        axis=1,
    )
    merged["NavDistance"] = merged.apply(
        lambda row: nav_distances_lookup.get((row["SpawnName"], row["AudioName"]), np.nan),
        axis=1,
    )
    

 
    movement_efficiency = merged["StraightLineDistance"] / merged["DistanceTravelled"].replace(0, np.nan)
    merged["MovementEfficiency"] = movement_efficiency.clip(upper=1.0)

    proximity_score = 1 - (merged["Distance"] / merged["StraightLineDistance"])
    merged["ProximityScore"] = proximity_score.clip(lower=0.0)

    merged["Effectiveness"] = merged["MovementEfficiency"] * merged["ProximityScore"]

    merged["Speed"] =  merged["StraightLineDistance"]/merged["Response Time"]
    merged["NavSpeed"] = merged["NavDistance"]/merged["Response Time"]


    missing_paths = merged["DistanceTravelled"].isna().sum()
    if missing_paths > 0:
        print(f"Warning: {missing_paths} trial(s) in VR_log.csv had no matching samples in VR_paths.csv")

    missing_straight_line = merged["StraightLineDistance"].isna().sum()
    if missing_straight_line > 0:
        print(f"Warning: {missing_straight_line} trial(s) had a SpawnIndex/AudioIndex with no matching "
              f"entry in sourcePositions.csv (check the indexing offset)")
    merged = merged.drop(columns=['SpawnName', 'AudioName','MovementEfficiency','ProximityScore'])
    merged.to_csv(OUTPUT_FILE, index=False)
    print(f"Done. Wrote {len(merged)} rows to {OUTPUT_FILE}")


if __name__ == "__main__":
    main()