import csv
from collections import defaultdict
import matplotlib.pyplot as plt


def main():
    input_file = "processed/PerVisualisation_cleaned.csv"

    visual_fields = ["Q5_1", "Q5_2", "Q5_3"]
    scores = defaultdict(list)

    with open(input_file, newline='', encoding='utf-8') as infile:
        reader = csv.DictReader(infile)
        next(reader)
        next(reader)
        for row in reader:
            visualisation = row["Q2"]
            visual_load_values = []
            for f in visual_fields:
                visual_load_values.append(int(row[f]))
            visual_load_score = sum(visual_load_values)/len(visual_load_values)
            scores[visualisation].append(visual_load_score)
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
    plt.ylabel("Visual Load",  labelpad=10)
    plt.title("Visual Load by Visualisation", pad=20)
    plt.ylim(bottom=0)
    plt.savefig("graphs/Visual_Load_violin_box.svg", format="svg")


if __name__ == "__main__":
    main()
