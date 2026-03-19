import pandas as pd
import matplotlib.pyplot as plt


def create_plot(columnToGroupBy):
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
    plt.xlabel("Visualization",  labelpad=10)
    match(columnToGroupBy):
        case("Distance"):
            plt.ylabel(f"Distance to Target (m)",  labelpad=10)
        case("Angle Error"):
            plt.ylabel(f"Angular Error (°)",  labelpad=10)
        case("Response Time"):
            plt.ylabel(f"{columnToGroupBy} (s)",  labelpad=10)
    plt.title(f"{columnToGroupBy} Distribution by Visualization", pad=20)
    plt.ylim(bottom=0)
    plt.savefig(f"graphs/{columnToGroupBy.replace(' ', '_')}Graph.svg", format="svg")


if __name__ == "__main__":
    create_plot("Distance")
    create_plot("Angle Error")
    create_plot("Response Time")
