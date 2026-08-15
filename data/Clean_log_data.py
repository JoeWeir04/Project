import csv
import pandas as pd

def main():
    input_file1 = "raw/Experiment_logFinalH1.csv"
    input_file2 = "raw/Experiment_logFinalH2.csv"
    output_file = "processed/Final_VR_log_cleaned.csv"

    df1 = pd.read_csv(input_file1)
    df2 = pd.read_csv(input_file2)
    df3 = pd.concat([df1,df2], ignore_index=True)
    df3.to_csv("final.csv",index=False)
    

    pids_to_remove = {"1", "2","44", "3" ,"4", "5", "7", "12", "45", "46","47","53"}  # remove pids not refering to participants
    
    with open("final.csv", newline='', encoding='utf-8') as infile, \
        open(output_file, "w", newline='', encoding='utf-8') as outfile:

        reader = csv.DictReader(infile)
        fieldnames = reader.fieldnames
        print(fieldnames)
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
                case("7"):
                    row["Visualisation"] = "Control"
            if row["PID"] == "14":
                row["PID"] = "13"
                row["TrialIndex"] = int(row["TrialIndex"]) + 7
                writer.writerow(row)
            elif row["PID"] == "50":
                row["PID"] = "49"
                row["TrialIndex"] = int(row["TrialIndex"]) + 21
                writer.writerow(row)
            elif row["PID"] not in pids_to_remove:
                writer.writerow(row)
    final = pd.read_csv(output_file)
    print("Total participants: ", final['PID'].nunique())


if __name__ == "__main__":
    main()