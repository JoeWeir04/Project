import pandas as pd
import matplotlib.pyplot as plt
import numpy as np


def create_plot_and_table(columnToGroupBy):
    df = pd.read_csv("processed/VR_log_cleaned.csv")
    grouped = df.groupby("Visualisation", sort=False)[columnToGroupBy]

    labels = []
    data = []

    for name, group in grouped:
        labels.append(name)
        data.append(group.values)

    plt.figure(figsize=(9, 6))
    plt.violinplot(data, positions=range(1, len(data)+1), widths=0.8)

    flierprops = dict(marker='o', markerfacecolor='none',
                      markersize=4, linestyle='none')  # smaller red dots
    boxprops = dict(facecolor='lightblue', color='black')  
    medianprops = dict(color='black')

    plt.boxplot(data, positions=range(1, len(data)+1), widths=0.3,
                patch_artist=True, showfliers=True, flierprops=flierprops,
                boxprops=boxprops, medianprops=medianprops)
    plt.xticks(fontsize=8)
    plt.xticks(range(1, len(labels)+1), labels)
    plt.xlabel("Visualisation",  labelpad=10)
    all_values = np.concatenate(data)
    match(columnToGroupBy):
        case("Distance"):
            plt.ylabel("Distance to Target (m)",  labelpad=10)
            plt.title("Distance to Target Distribution by Visualisation", pad=20)
            plt.yticks(np.arange(0, all_values.max() + 2, 2.0))
        case("Angle Error"):
            plt.ylabel("Angular Error (°)",  labelpad=10)
            plt.title("Angular Error Distribution by Visualisation", pad=20)
            plt.yticks(np.arange(0, all_values.max() + 15, 15.0))
        case("Response Time"):
            plt.ylabel(f"{columnToGroupBy} (s)",  labelpad=10)
            plt.title(f"{columnToGroupBy} Distribution by Visualisation", pad=20)
            plt.yticks(np.arange(0, all_values.max() + 5, 5.0))
    plt.ylim(bottom=0)
    plt.savefig(f"graphs/{columnToGroupBy.replace(' ', '_')}Graph.pdf", format="pdf")
    plt.close()

    summary = df.groupby("Visualisation", sort=False)[columnToGroupBy].agg(
        n="count",
        Mean="mean",
        Median="median",
        SD="std",
        Min="min",
        Max="max"
    ).reset_index()
    summary["Range"] = summary["Max"] - summary["Min"]
    summary = summary.round({
        "Mean": 2,
        "Median": 2,
        "SD": 2,
        "Min": 2,
        "Max": 2,
        "Range": 2
    })
    summary.to_csv(f"tables/{columnToGroupBy.replace(' ', '_')}_Summary.csv", index=False)


if __name__ == "__main__":
    create_plot_and_table("Distance")
    create_plot_and_table("Angle Error")
    create_plot_and_table("Response Time")
