# Adding new avatars to the app

An avatar must have to be usable in the app :
- blendshapes for the mouth : both basic (Mouth open, smiling, sadness, puffing,...) for facial tracker and visemes for lip sync (OH,CH,E,...) see https://developer.oculus.com/documentation/unity/audio-ovrlipsync-viseme-reference/?locale=fr_FR for exact list of viseme and Vive Facial Tracker doc for exact list of movements needed : https://developer-express.vive.com/resources/vive-sense/eye-and-facial-tracking-sdk/documentation/.

- either blendshapes or skeleton for eye movement (Eye Left/Right Looking Up/Right/Down/Left, Blinking...). See https://developer-express.vive.com/resources/vive-sense/eye-and-facial-tracking-sdk/documentation/ for eye documentation. For the eye skeleton direction to work properly, the forward of the eye must be in the gaze direction.

I advise then to make a prefab and add on the head the following scripts :

  - HeadRescaler.cs, to rescale the head to the size of the user's head
  - AvatarEyeControlMulti.cs to sync eye movement between users.
  - AvatarLipMulti.cs to sync facial tracker between users.
  - LipSyncMulti.cs to sync lip sync between users.

There is a prefab containing all avatars, called "Head". It has a script called AvatarInitializer.cs, allowing the toggle the right avatar at start. You would need to modifiy it a bit depending on your needs. Adding avatars head prefabs to this prefabs would also allow you to callibrate head positions based on the other heads already placed, minimizing possible offset. It also contains the objets needed to detect which part of the head the user's looking at.

Type on avatar is set up during the setting scene and saved in GameManager.cs.
