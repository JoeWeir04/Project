import csv


def main():
    input_file = "raw/VR_log.csv"
    output_file = "processed/VR_log_cleaned.csv"

    pids_to_remove = {"1", "5", "11", "13"}  # remove pids not refering to participants

    with open(input_file, newline='', encoding='utf-8') as infile, \
         open(output_file, "w", newline='', encoding='utf-8') as outfile:

        reader = csv.DictReader(infile)
        fieldnames = reader.fieldnames
        fieldnames[fieldnames.index("DistanceFromSource")] = "Distance"
        fieldnames[fieldnames.index("absError")] = "Angle Error"
        fieldnames[fieldnames.index("ResponseTime")] = "Response Time"
        writer = csv.DictWriter(outfile, fieldnames=fieldnames)

        writer.writeheader()  # write the header row

        for row in reader:
            match row["Visualisation"]:
                case("1"):
                    row["Visualisation"] = "Arrow"
                case("2"):
                    row["Visualisation"] = "Radar"
                case("3"):
                    row["Visualisation"] = "Lights"
                case("4"):
                    row["Visualisation"] = "Arrow & Radar"
                case("5"):
                    row["Visualisation"] = "Arrow & Lights"
                case("6"):
                    row["Visualisation"] = "Lights & Radar"
            if row["PID"] == "2":
                if row["Visualisation"] == "2":
                    continue
                row["PID"] = "3"
                writer.writerow(row)
            elif row["PID"] == "3" and int(row["TrialIndex"]) <= 9:
                continue
            elif row["PID"] not in pids_to_remove:
                writer.writerow(row)


if __name__ == "__main__":
    main()