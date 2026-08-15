import csv
from collections import defaultdict
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd


def main():
    input_file = "processed/PerVisualisation_cleaned.csv"

    # Positively worded items: higher raw value = better usability
    # Negatively worded items: higher raw value = worse usability
    positive_fields = ["Q10_1", "Q10_3"]  # "meets requirements", "easy to use"
    negative_fields = ["Q10_2", "Q10_4"]  # "frustrating", "too much time correcting things"

    scores = defaultdict(list)
    rows_for_table = []

    with open(input_file, newline='', encoding='utf-8') as infile:

        reader = csv.DictReader(infile)
        next(reader)
        next(reader)
        for row in reader:
            visualisation = row["Q2"]

            rescaled_values = []
            for f in positive_fields:
                rescaled_values.append(int(row[f]) - 1)   # 1-7 -> 0-6, higher = better
            for f in negative_fields:
                rescaled_values.append(7 - int(row[f]))   # 1-7 -> 0-6, higher = better (inverted)

            # Standard UMUX formula: (sum of rescaled items / max possible) * 100
            umux_score = (sum(rescaled_values) / (len(rescaled_values) * 6)) * 100

            scores[visualisation].append(umux_score)
            rows_for_table.append({"Visualisation": visualisation, "UMUX Score": umux_score})

    create_plot(scores)
    create_table(rows_for_table)


def create_plot(scores):
    labels = list(scores.keys())
    data = list(scores.values())

    plt.figure(figsize=(9, 6))
    plt.violinplot(data, positions=range(1, len(data) + 1), widths=0.8)

    flierprops = dict(marker='o', markerfacecolor='none',
                       markersize=4, linestyle='none')
    boxprops = dict(facecolor='lightblue', color='black')
    medianprops = dict(color='black')

    plt.boxplot(data, positions=range(1, len(data) + 1), widths=0.3,
                patch_artist=True, showfliers=True, flierprops=flierprops,
                boxprops=boxprops, medianprops=medianprops)
    plt.xticks(fontsize=8)
    plt.xticks(range(1, len(labels) + 1), labels)
    plt.xlabel("Visualisation", labelpad=10)
    plt.ylabel("UMUX Score", labelpad=10)
    plt.title("UMUX Score Distribution by Visualisation", pad=20)
    plt.ylim(0, 100)
    plt.yticks(np.arange(0, 101, 10))
    plt.savefig("graphs/UMUX_violin_box.pdf", format="pdf")
    plt.close()


def create_table(rows_for_table):
    df = pd.DataFrame(rows_for_table)
    summary = df.groupby("Visualisation", sort=False)["UMUX Score"].agg(
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
    summary.to_csv("tables/UMUX_Summary.csv", index=False)


if __name__ == "__main__":
    main()