import csv
from collections import defaultdict
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd


def main():
    input_file = "processed/PerVisualisation_cleaned.csv"

    perf_fields = {
        "Q12_1": "Direction",
        "Q12_2": "Magnitude",
    }

    scores = {label: defaultdict(list) for label in perf_fields.values()}
    rows_for_table = []

    with open(input_file, newline='', encoding='utf-8') as infile:

        reader = csv.DictReader(infile)
        next(reader)
        next(reader)
        for row in reader:
            visualisation = row["Q2"]

            for field, label in perf_fields.items():
                value = float(row[field])
                scores[label][visualisation].append(value)
                rows_for_table.append({
                    "Visualisation": visualisation,
                    "Measure": label,
                    "Score": value
                })

    create_plot(scores)
    create_table(rows_for_table)


def create_plot(scores):
    fig, axes = plt.subplots(1, 2, figsize=(16, 6), sharey=True)

    for ax, (label, vis_scores) in zip(axes, scores.items()):
        labels = list(vis_scores.keys())
        data = list(vis_scores.values())

        ax.violinplot(data, positions=range(1, len(data) + 1), widths=0.8)

        flierprops = dict(marker='o', markerfacecolor='none',
                           markersize=4, linestyle='none')
        boxprops = dict(facecolor='lightblue', color='black')
        medianprops = dict(color='black')

        ax.boxplot(data, positions=range(1, len(data) + 1), widths=0.3,
                   patch_artist=True, showfliers=True, flierprops=flierprops,
                   boxprops=boxprops, medianprops=medianprops)
        ax.set_xticks(range(1, len(labels) + 1))
        ax.set_xticklabels(labels, fontsize=8)
        ax.set_xlabel("Visualisation", labelpad=10)
        ax.set_title(f"{label} Interpretation Performance", pad=15)
        ax.set_ylim(0, 20)
        ax.set_yticks(np.arange(0, 21, 2))

    axes[0].set_ylabel("Performance Score", labelpad=10)
    fig.suptitle("Sound Interpretation Performance by Visualisation", fontsize=14)
    plt.tight_layout()
    plt.savefig("graphs/Performance_violin_box.pdf", format="pdf")
    plt.close()


def create_table(rows_for_table):
    df = pd.DataFrame(rows_for_table)
    summary = df.groupby(["Measure", "Visualisation"], sort=False)["Score"].agg(
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
    summary.to_csv("tables/Performance_Summary.csv", index=False)


if __name__ == "__main__":
    main()