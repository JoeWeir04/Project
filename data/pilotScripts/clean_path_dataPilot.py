import pandas as pd

input_file = "raw/VR_paths.csv"
output_file = "processed/VR_paths_cleaned.csv"

df = pd.read_csv(input_file)

visualisation_map = {
    1: "Arrow",
    2: "Radar",
    3: "Lights",
    4: "Arrow & Radar",
    5: "Arrow & Lights",
    6: "Lights & Radar"
}

df["Visualisation"] = df["Visualisation"].map(visualisation_map)


df.to_csv(output_file, index=False)

print(f"Cleaned file saved to {output_file}")