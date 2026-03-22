import pandas as pd
import matplotlib.pyplot as plt


def plot_distance_vs_angle():
    df = pd.read_csv("processed/VR_log_cleaned.csv")

    plt.figure(figsize=(8,6))
    for vis in df["Visualisation"].unique():
        subset = df[df["Visualisation"] == vis]
        plt.scatter(subset["Distance"],subset["Angle Error"], label=vis, alpha=0.7)

    plt.xlabel("Distance (Metres)")
    plt.ylabel("Angle Error (Degrees)")
    plt.title("Distance vs Angle Error by Visualisation")
    plt.legend()
    plt.grid(True)
    plt.savefig("graphs/Distance_vs_Angle.svg", format="svg")
    plt.show()


if __name__ == "__main__":
    plot_distance_vs_angle()
