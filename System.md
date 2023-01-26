# Server connection diagram

![Project drawio (2)](https://user-images.githubusercontent.com/73835238/214586314-24ecb755-0b59-46a0-9057-77be2dce4909.png)

Quick explanation of every script :
  - NetworkManager is the script allowing to start the server and start/connect the client as well as assigning player number
  - RegisterResults allow to save measures from the experiment, both locally and on the server, GazeRay allows to gather data from the user's gaze
  - ExperiementStarter and ExperimentEnder allows to start and end tasks on clients
  - SyncHeads is the script allowing to sync avatar heads position and rotation between clients
  - SyncViseme allows to sync visemes between client, with the help of the OVRLipSyncContext and OculusLipSyncInput, base scripts from Oculus to get the data from the mic
  - LipData syncs facial tracker data, getting local data from Sranipal_Lip_Framework, Vive's base script to get facial tracker data
  - SRanipal_Eye_Framework initialize the framework, EyeDataGetter gets local eye data and other user's eye data with SyncGaze script
