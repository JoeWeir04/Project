import csv


def main():
    input_file = "raw/AllVisualisations.csv"
    output_file = "processed/AllVisualisations_cleaned.csv"

    with open(input_file, newline='', encoding='utf-8') as infile, \
         open(output_file, "w", newline='', encoding='utf-8') as outfile:

        reader = csv.DictReader(infile)
        fieldnames = reader.fieldnames[17:]
        writer = csv.DictWriter(outfile, fieldnames=fieldnames)

        writer.writeheader()

        for row in reader:
            filtered_row = {key: row[key] for key in fieldnames}
            writer.writerow(filtered_row)


if __name__ == "__main__":
    main()
