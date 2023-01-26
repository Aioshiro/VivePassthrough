# OnlineScene scripts diagram

## Server connection diagram

![Project drawio (2)](https://user-images.githubusercontent.com/73835238/214586314-24ecb755-0b59-46a0-9057-77be2dce4909.png)

Quick explanation of every script :
  - NetworkManager is the script allowing to start the server and start/connect the client as well as assigning player number
  - RegisterResults allow to save measures from the experiment, both locally and on the server, GazeRay allows to gather data from the user's gaze
  - ExperiementStarter and ExperimentEnder allows to start and end tasks on clients
  - SyncHeads is the script allowing to sync avatar heads position and rotation between clients
  - SyncViseme allows to sync visemes between client, with the help of the OVRLipSyncContext and OculusLipSyncInput, base scripts from Oculus to get the data from the mic
  - LipData syncs facial tracker data, getting local data from Sranipal_Lip_Framework, Vive's base script to get facial tracker data
  - SRanipal_Eye_Framework initialize the framework, EyeDataGetter gets local eye data and other user's eye data with SyncGaze script

## Local scripts diagram

![projectbis drawio](https://user-images.githubusercontent.com/73835238/214815398-9f4bc316-87d1-436a-8373-5653fd94e7cb.png)

Quick explanation of every script :
  - GameManager is there to save application setting (avatar, ip, animation type...)
  - TwentyQuestionUpdater and InstructionUpdater are there for UI purposes and show text both in French and English
  - AvatarInitializer activates the right avatar depending on the settings( gender, ethnicity, type) and the right mouth animation
  - HeadRescaler rescales the avatar head to correspond to user's head
  - AvatarLipMulti animates the avatar mouth with LipData's facial tracker data
  - LipSyncMulti animates the avatar mouth with SyncViseme lip sync's data
  - AvatarEyeControlMulti animates the avatar eyes with EyeDataGetter's eye data
  - DetectionMarkers detects ArUco markers in the camera feeds and sends detected position and rotations to MarkerManager, with transmit them to the marker with the right ID with the TransformSmoother script, which smoothes transform to ensure limited jittering of the virtual objects
