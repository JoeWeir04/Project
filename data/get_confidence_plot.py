import csv
from collections import defaultdict
import matplotlib.pyplot as plt


def main():
    input_file = "processed/PerVisualisation_cleaned.csv"

    visual_fields = ["Q6_1"]
    scores = defaultdict(list)

    with open(input_file, newline='', encoding='utf-8') as infile:
        reader = csv.DictReader(infile)
        next(reader)
        next(reader)
        for row in reader:
            visualisation = row["Q2"]
            confidence_values = []
            for f in visual_fields:
                confidence_values.append(int(row[f]))
            confidence_score = sum(confidence_values)/len(confidence_values)
            scores[visualisation].append(confidence_score)
    create_plot(scores)


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
    plt.xlabel("Visualization",  labelpad=10)
    plt.ylabel("Confidence",  labelpad=10)
    plt.title("Confidence by Visualisation", pad=20)
    plt.ylim(bottom=0)
    plt.savefig("graphs/Confidence_violin_box.svg", format="svg")


if __name__ == "__main__":
    main()
