import csv
import random
import matplotlib.pyplot as plt
import pandas as pd


INPUT_FILE = "processed/AllVisualisations_cleaned.csv"

OUTPUT_PLOT = "graphs/Participant_Choice_by_Distance.pdf"
OUTPUT_TABLE = "tables/Participant_Choice_by_Distance.csv"
OUTPUT_SUMMARY = "tables/Participant_Choice_by_Distance_Summary.csv"


def clean_value(value):
    """Clean a CSV value."""
    if value is None:
        return ""

    value = str(value).strip()

    # Remove accidental ** formatting
    value = value.replace("**", "")

    return value.strip()


def main():

    rows = []

    with open(INPUT_FILE, newline="", encoding="utf-8-sig") as infile:

        reader = csv.DictReader(infile)

        # ---------------------------------------------------------
        # IMPORTANT:
        #
        # The CSV contains:
        #
        # Row 1: column names
        # Row 2: question text
        # Row 3: ImportId values
        # Row 4+: participant responses
        #
        # DictReader automatically uses Row 1 as the column names.
        # Therefore we need to skip BOTH Row 2 and Row 3.
        # ---------------------------------------------------------

        next(reader, None)  # Skip question text row
        next(reader, None)  # Skip ImportId row

        for row in reader:

            pid = clean_value(row.get("Q4"))

            near = clean_value(row.get("Q2_1"))
            medium = clean_value(row.get("Q2_2"))
            far = clean_value(row.get("Q2_3"))

            # -----------------------------------------------------
            # Safety check:
            # Never allow an ImportId row to be treated as a
            # participant.
            # -----------------------------------------------------

            if "ImportId" in pid:
                continue

            if "ImportId" in near:
                continue

            if "ImportId" in medium:
                continue

            if "ImportId" in far:
                continue

            # Ignore completely empty rows
            if not pid:
                continue

            # Skip incomplete participant responses
            if not (near and medium and far):
                continue

            rows.append({
                "PID": pid,
                "Near": near,
                "Medium": medium,
                "Far": far
            })

    print(f"Loaded {len(rows)} participants.")

    print("\nParticipants found:")
    for row in rows:
        print(
            f"PID {row['PID']}: "
            f"Near={row['Near']}, "
            f"Medium={row['Medium']}, "
            f"Far={row['Far']}"
        )

    if not rows:
        print("No valid participant responses found.")
        return

    create_plot(rows)
    create_table(rows)


def create_plot(rows):

    # ---------------------------------------------------------
    # Find every unique visualisation choice
    # ---------------------------------------------------------

    all_choices = set()

    for row in rows:
        all_choices.add(row["Near"])
        all_choices.add(row["Medium"])
        all_choices.add(row["Far"])

    # Remove anything that looks like an ImportId
    all_choices = {
        choice
        for choice in all_choices
        if "ImportId" not in choice
    }

    # Sort single visualisations first, then combinations
    sorted_choices = sorted(
        all_choices,
        key=lambda choice: (choice.count(","), choice)
    )

    choice_to_y = {
        choice: i
        for i, choice in enumerate(sorted_choices)
    }

    # ---------------------------------------------------------
    # X positions
    # ---------------------------------------------------------

    distances = ["Near", "Medium", "Far"]

    x_positions = {
        "Near": 0,
        "Medium": 1,
        "Far": 2
    }

    # ---------------------------------------------------------
    # Create plot
    # ---------------------------------------------------------

    plt.figure(
        figsize=(
            10,
            max(6, len(sorted_choices) * 0.5)
        )
    )

    # ---------------------------------------------------------
    # Assign a different colour to every participant
    # ---------------------------------------------------------

    num_participants = len(rows)

    if num_participants <= 20:

        cmap = plt.get_cmap("tab20")

        participant_colours = {
            row["PID"]: cmap(i)
            for i, row in enumerate(rows)
        }

    else:

        cmap = plt.get_cmap("turbo")

        participant_colours = {
            row["PID"]: cmap(
                i / max(1, num_participants - 1)
            )
            for i, row in enumerate(rows)
        }

    # ---------------------------------------------------------
    # Reproducible horizontal jitter
    # ---------------------------------------------------------

    random.seed(42)

    jitter_strength = 0.06

    # ---------------------------------------------------------
    # Plot each participant
    # ---------------------------------------------------------

    for row in rows:

        xs = []
        ys = []

        for distance in distances:

            jitter = random.uniform(
                -jitter_strength,
                jitter_strength
            )

            xs.append(
                x_positions[distance] + jitter
            )

            ys.append(
                choice_to_y[row[distance]]
            )

        plt.plot(
            xs,
            ys,
            marker="o",
            color=participant_colours[row["PID"]],
            alpha=0.7,
            linewidth=1.5,
            markersize=5
        )

    # ---------------------------------------------------------
    # Axis formatting
    # ---------------------------------------------------------

    plt.xticks(
        list(x_positions.values()),
        list(x_positions.keys())
    )

    plt.yticks(
        list(choice_to_y.values()),
        list(choice_to_y.keys()),
        fontsize=8
    )

    plt.xlabel(
        "Distance from Sound Source",
        labelpad=10
    )

    plt.ylabel(
        "Preferred Visualisation(s)",
        labelpad=10
    )

    plt.title(
        "Participant Visualisation Choice by Distance",
        pad=20
    )

    # ---------------------------------------------------------
    # Legend
    # ---------------------------------------------------------

    from matplotlib.lines import Line2D

    legend_elements = []

    for row in rows:

        pid = row["PID"]

        # Extra safety: don't create an ImportId legend entry
        if "ImportId" in pid:
            continue

        legend_elements.append(
            Line2D(
                [0],
                [0],
                color=participant_colours[pid],
                lw=2,
                marker="o",
                markersize=4,
                label=f"PID {pid}"
            )
        )

    plt.legend(
        handles=legend_elements,
        title="Participant",
        bbox_to_anchor=(1.02, 1),
        loc="upper left",
        fontsize=8
    )

    # ---------------------------------------------------------
    # Save
    # ---------------------------------------------------------

    plt.tight_layout()

    plt.savefig(
        OUTPUT_PLOT,
        format="pdf",
        bbox_inches="tight"
    )

    plt.close()

    print(f"\nPlot saved to: {OUTPUT_PLOT}")


def create_table(rows):

    df = pd.DataFrame(rows)

    # Determine whether participant changed visualisation
    df["Switched"] = df.apply(
        lambda row: len({
            row["Near"],
            row["Medium"],
            row["Far"]
        }) > 1,
        axis=1
    )

    df.to_csv(
        OUTPUT_TABLE,
        index=False
    )

    summary = (
        df["Switched"]
        .value_counts()
        .rename_axis("Switched")
        .reset_index(name="Count")
    )

    summary.to_csv(
        OUTPUT_SUMMARY,
        index=False
    )

    print(f"Participant table saved to: {OUTPUT_TABLE}")
    print(f"Summary saved to: {OUTPUT_SUMMARY}")


if __name__ == "__main__":
    main()