# VivePassthrough

Welcome to this repository !
In here, you'll find an app we used to experiment with avatar heads in mixed reality.

## Using this app

To use this app, first launch the server on a machine, then launch the client on both computers. This app will only work with HTC Vive Pro Eye.
You'll get this setting screen :

![settings](https://user-images.githubusercontent.com/73835238/214270633-e393690d-82a6-4298-87bb-ea11f74209e7.PNG)


1. The part to activate the other's player avatar, toggle the realistic/cartoon one and the male/female. Having desactivated avatar makes the R/C and M/F option useless.
2. Toggle the ethnicity of the avatar, again no effect if avatars desactivated.
3. Size of the other's user head, from chin to middle of forehead, usefull for scaling avatar (no effect otherwise)
4. The secret word of the player for the 20 question game (enter it in the right language)
5. Id of the player (integer)
6. Ip of the server (192.168.X.X)
7. Type of mouth animation for the avatar, either Lip Sync or facial tracker (no effect otherwise)
8. Language of the app (english/french)
9. Check to disable eye callibration at launch (Mainly for debug purposes)
10. Launch app.

Once the app is launched, eye callibration is done with the user (if not disabled). After a short loading time, the user sees the real world through the HMD camera.

Both participants are then ask to look at ArUco marker 4*4 n°10, sitting on a wall nearby. Its used to setup instruction next to the users.
When both participants have looked at the marker, task one starts in 15 seconds.

### Task 1 : 20 questions game

The users are displayed text with the instruction of the game as well as their secret word. One of the player is instructed to start. They are expected to ask up to 20 yes/no
questions to guess the other player secret word. Once its done, the experiment as a button called "Go to Next task" on the computer screen. Once pressed on both computer, the second task begins.

### Task 2 : Urban planning

Building will appear on Aruco markers 4*4 number 1 to 9. Instructions will be updated to show both player different, yet complementary information to place the buildings on a physical maps in front of them. The buildings have name on top of them to reduce confusion. The buildings are : a train station, a church, a skyscrapper, an hospital, a fire station, a town hall, a stadium, a residential home and a school. Here's the map and the buildings :

![Buildings](https://user-images.githubusercontent.com/73835238/214265275-c8f1d630-d7a0-41fd-a244-23a88bb6b865.PNG)
![map(7)_parks_contours](https://user-images.githubusercontent.com/73835238/214267544-5c0ba839-264b-4ae3-a717-5236e1483db7.png)

They're allowed to share information to solve this puzzle.
Once done, they can press a button which appears on ArUco marker 0 with their hands (with hand tracking). When press by a player, it turns green for him. Once green for both players, the app exits and results are saved.

## Saved results

The following data is shown on the computer screen for the experimenter to see and are saved at the end of each task :

- The total time of the task
- The time passed looking at the other user's head 
- The time passed looking at the other user's forehead 
- The time passed looking at the other user's eyes 
- The time passed looking at the other user's mouth
- The number of fixations on the other user's head
- The average fixation time
- The number of blinks
- The average left/right pupil size
