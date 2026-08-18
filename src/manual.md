# User manual 

### /code 
This folder contains python scripts that can be run to estimate the location of spatial sounds using the miniDSP UMA 8 Microphone array. These estimations are then sent over a sockets server.

Put a brief description of your code here. This should at least describe the file structure.

## Build instructions

### To run the prototype
* Update the serverIP variable in the mic socket script to the IP of the laptop connected to the wifi you will be using for the prototype, the script is located at `UnityProject/sound-localisation/Assets/scripts/MicSocket`
* Build the unity project to a quest 3 headset with all scenes included
* Connect the UMA-8 to a laptop, making sure the UMA-8 is running in the RAW advanced mode. 
* Run `pip install requirements.txt` to install dependencies
* Run `python SPRPHATDoa.py` in the code folder to begin networking sound source locations to the headset.

* Ensure that both the headset and laptop are connected to the same wifi ideally a hotspot to ensure a stable connectionl
* Run the project on the quest 3 and hold down both primary buttons to boot into the prototype scene,


### To run the experiment
* Build the unity project to a quest 3 headset
* Open the project on the headset
* press the primary button on the right controller to log metrics and move onto next trial
* press the primary button on the left controller to swap visualisation
* press the menu button on the left controller to start practice for each condition and press it again to begin the condition
* After 7 trials a break will begin, press the left menu button to start the next condition practice and then once again to begin the trial
* Once the experiment has started all log data will be saved to the headset.

### Requirements

* Python 3.9 
* Unity 2022.3.XX LTS
* miniDSP UMA-8 USB mic array - V2.0 
* Packages: listed in `requirements.txt`
* Tested on Windows 10 and Windows 11


### Test steps

Run `SPRPHATDoa.py` in the code folder with the UMA-8 in RAW advanced mode to check that the script is working.

Build and Run the Unity project to a quest 3 headset to check the project can build.


