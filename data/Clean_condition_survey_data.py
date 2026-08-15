import csv


def main():
    input_file = "raw/PerVisualization.csv"
    output_file = "processed/PerVisualisation_cleaned.csv"

    old_text = "Control - Hearing sound itself without any visualisations"
    new_text = "Control"

    with open(input_file, newline='', encoding='utf-8') as infile, \
         open(output_file, "w", newline='', encoding='utf-8') as outfile:

        reader = csv.DictReader(infile)
        fieldnames = reader.fieldnames[17:]
        writer = csv.DictWriter(outfile, fieldnames=fieldnames)
        writer.writeheader()

        for row in reader:
            filtered_row = {
                key: (row[key].replace(old_text, new_text) if row[key] else row[key])
                for key in fieldnames
            }
            writer.writerow(filtered_row)

if __name__ == "__main__":
    main()