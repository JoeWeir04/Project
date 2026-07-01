import csv
from collections import defaultdict
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd


def main():
    input_file = "processed/PerVisualisation_cleaned.csv"

    tlx_fields = ["Q4_1", "Q4_2", "Q4_3", "Q4_4", "Q4_5", "Q4_6"]
    scores = defaultdict(list)
    rows_for_table = []

    with open(input_file, newline='', encoding='utf-8') as infile:

        reader = csv.DictReader(infile)
        next(reader)
        next(reader)
        for row in reader:
            visualisation = row["Q2"]
            tlx_values = []
            for f in tlx_fields:
                if f == "Q4_4":
                    tlx_values.append(20-int(row[f]))
                else:
                    tlx_values.append(int(row[f]))
            tlx_score = sum(tlx_values)/len(tlx_values)
            scores[visualisation].append(tlx_score)
            rows_for_table.append({"Visualisation": visualisation, "TLX Score": tlx_score})

    create_plot(scores)
    create_table(rows_for_table)


def create_plot(scores):
    labels = list(scores.keys())
    data = list(scores.values())

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
    plt.ylabel("NASA TLX Score",  labelpad=10)
    plt.title("NASA TLX Distribution by Visualisation", pad=20)
    plt.ylim(0, 10)
    plt.yticks(np.arange(0, 11, 1))
    plt.ylim(bottom=0)
    plt.savefig("graphs/NASA_TLX_violin_box.pdf", format="pdf")
    plt.close()


def create_table(rows_for_table):
    df = pd.DataFrame(rows_for_table)
    summary = df.groupby("Visualisation", sort=False)["TLX Score"].agg(
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
    summary.to_csv("tables/NasaTLX_Summary.csv", index=False)


if __name__ == "__main__":
    main()
