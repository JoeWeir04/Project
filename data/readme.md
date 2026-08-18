# Data Folder
## /graphs
This folder contains graphs created from scripts in the data parent folder. All R notebooks in the notebooks folder were also used to create graphs in this folder.

## /transcripts

This folder contains all coded transcripts from participants as word documents in the naming convention `PID[number]Interview.docx`. It also contains  an excel sheet of all coded exerpts, the second sheet in this spreadsheet contains all the codes found in order `Int_Codes.xlsx`.

## /raw 
This folder contains data that was collected through logging on the meta quest 3 headset's, these files are `Experiment_logFinalH1.csv`,`Experiment_logFinalH2.csv` and `Experiment_pathsFinalH1.csv`, `Experiment_pathsFinalH2.csv` . This folder also contains data from microphone array benchmarking, These can be found in `\raw\benchmarking` and the files are the csv files named with `angles[number].csv` and the excel file `Benchmark.xlsx`.

This folder also contains the raw exported data from the qualtrics surveys these files are `PerVisualization.csv` and `AllVisualizations.csv`.

This folder also contains the positions for the spawn points and audio sources found in `sourcePositions.csv` as well as the shortest distances between them through the Unity's scene navmesh, found in `navmeshDistances.csv`

## /surveys 
This folder contains pdf copies of the surveys used to gather responses from participants.
## /tables 
This folder cantains tables produced by scripts found in the parent data folder. 
## /processed
This folder contains all processed data produced from the scripts found in thee parent data folder. The scripts used were  `Clean_log_data.py`, `Clean_path_data.py`, `Clean_all_visualisations_survey_data.py`, `Clean_condition_survey_data.py`, and
`calculate_distance.py`

- `AllVisualisations_cleaned.csv` 
    - This contains the data retreived from the survey taken at the end of the experiment
    - Contains rankings of visualisations, preference by distance
- `PerVisualisation_cleaned.csv`
    - This contains the survey responses for each condition/visualisation during the experiment
    - Contains nasa TLX, UMUX, performance questions, Visual load questions and a confidence question
- `Final_VR_log_cleaned.csv`
    - This contains data logged from the headset during the experiment 
    - Headers for columns explained 
        - PID - participant ID for entry
        - Time - timestamp of entry (not the response time)
        - TrialIndex - the trial index number (starting from 0)
        - SpawnIndex - the spawn index number (starting from 0)
        - AudioIndex - the audio index number (starting from 0)
        - AudioAngle - the angle the user was facing from the sound source (from 0 to 360)
        - AngleError - the number of degrees the user was facing from the sound source - using the absolute difference i.e. from 0 to 180
        - Distance - The distance from the sound source (in metres)
        - ResponseTime - The number of seconds the trial took
        - Visualisation - the visualisation that was used during the trial/entry
- `Final_VR_paths_cleaned.csv`
    - This contains the path data that was logged during the experiment
- `Processed_Distances.csv`
    - Contains all the same data as `Final_VR_paths_cleaned.csv` but also contains path and distance metrics that are calculated using `calculate_distance.py`.
        - DistanceTravelled - the number of metres travelled by the user during the trial 
        - StraightLineDistance - The distance between the sound source and audio source (for this trial) as the crow flys
        - NavDistance - The shortest path possible between the sound source and audio source accounting for obstacles by using the Unity scenes NavMesh.
        - Effectiveness - this is a metric which is calculated from the following
            - (NavDistance/DistanceTravelled)*(1-(Distance/NavDistance))
        - NavSpeed - NavDistance/ResponseTime


## /notebooks
This folder contains R notebooks which were used to produce figures and plots from the data in the processed folder.
## /ethics
This folder contains the signed project ethics checklist. It also contains the introduction and debrief scripts for the experiment.


# Instructions for creating all graphs and tables from data 
- Run all the following commands in terminal within the \data folder, graphs will be produced which can be found in `\graphs` as well as tables found in `\tables`

## Virtual Environment cd
- It is important to first activate the virtual environment before running the scripts below 
- This can be done by running the command: `.\venv\Scripts\activate` from the Project directory.
- It is also important to run `pip install requirements.txt` to install dependencies

## Getting data cleaned and distances calculated
- Run `Clean_log_data.py`
- Run `Clean_path_data.py`
- Run `Clean_all_visualisations_survey_data.py`
- Run `Clean_condition_survey_data.py`
- Run `calculate_distance.py`

## Creating scores and plotting graphs
- Run `create_nasa_tlx_plot.py`
- Run `create_umux_plot.py`
- Run `create_violin_plot.py` - this will create violin plots for angular error, distance to target, response time, nav speed, and effectiveness.
- Run `calculate_visual_load.py`
- Run `plot_performance_qs.py`

## Running R scripts to create graphs
- The r notebooks can be found at `\data\notebooks`
- Run all the cells for each notebook in order to create graphs which can be found in `\graphs`once the script is run.

### notebooks to run
- `ConfidenceLikertNotebook.Rmd`
- `DistancePreference.Rmd`
- `VisualLoadNotebook.rmd`
- `RankingsNotebook.Rmd`


