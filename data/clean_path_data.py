import pandas as pd

input_file1 = "raw/Experiment_pathsFinalH1.csv"
input_file2 = "raw/Experiment_pathsFinalH2.csv"
output_file = "processed/Final_VR_paths_cleaned.csv"

df1 = pd.read_csv(input_file1, header=None)
df1.columns = ["PID","Time","TrialIndex","SpawnIndex","AudioIndex","Visualisation","PosX","PosY","PosZ","RotY"]
df2 = pd.read_csv(input_file2, header=None)
df2.columns = ["PID","Time","TrialIndex","SpawnIndex","AudioIndex","Visualisation","PosX","PosY","PosZ","RotY"]

df = pd.concat([df1,df2], ignore_index=True)

pids_to_remove = {"1", "44", "2", "3" ,"4", "5", "7", "12", "45", "46","47","53"}  # remove pids not refering to participants

visualisation_map = {
    1: "Arrow",
    2: "Radar",
    3: "Lights",
    4: "Arrow & Radar",
    5: "Arrow & Lights",
    6: "Lights & Radar",
    7: "Control"
}

df["Visualisation"] = df["Visualisation"].map(visualisation_map)

df["PID"] = df["PID"].astype(str)

# Re-map PID 14 -> 13 (trial +7) and PID 50 -> 49 (trial +21), same as the log-cleaning script
mask_14 = df["PID"] == "14"
df.loc[mask_14, "TrialIndex"] = df.loc[mask_14, "TrialIndex"].astype(int) + 7
df.loc[mask_14, "PID"] = "13"

mask_50 = df["PID"] == "50"
df.loc[mask_50, "TrialIndex"] = df.loc[mask_50, "TrialIndex"].astype(int) + 21
df.loc[mask_50, "PID"] = "49"

df = df[~df["PID"].isin(pids_to_remove)]
print("Total participants: ", df['PID'].nunique())
df.to_csv(output_file, index=False)

print(f"Cleaned file saved to {output_file}")