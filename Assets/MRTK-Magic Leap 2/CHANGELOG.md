**Unity MRTK Package 0.53.3 Version 1.0**
===================================

* Magic Leap SDK Version 0.53.3
* Unity Version: 2022.2 (custom)
* MRTK Foundations v2.8.2
* MRTK Examples v2.8.2

**Release Focus**
-----------------

Initial Update to 0.53.3

**Features**
------------

* Voice Intents - Refer to SpeechCommandsDemoMagicLeap scene for usage.
* Control - Refer to ControlMagicLeapDemo scene for usage.
* HandTracking - Refer to HandInterationExamplesMagicleap for usage. Only Partially implemented on platform currently.
* EyeTracking - Refer to EyeTrackingDemoMagicLeap for usage.
* Meshing - Refer to MeshingDemoMagicLeap for usage.
* AllInteractions - Refer to InteractionsDemoMagicLeap for Handtracking, Control, EyeTracking, and Voice usage together.

**0.53.3 Version 1 Updates**
----------------------------

* Updated to MagicLeap SDK 0.53.3
* Added a More Robust smoothing option to HandTracking. This is still experimental and being worked on for improvements to the HandTracking experience.
* Added Magic Leap Haptics functions to the Controller. An example can be found in the Control Example with Menu button press.
* Updated Magic Leap HandTracking Input Data Provider settings for new HandTrackign options.
* Fixed Bug where importing the Magic Leap MRTK package into a project with Magic Leap Unity Examples would overwrite some material assets.
* Fixed issues with Hand Menus.

**Deprecations**
----------------

* SetHandTrackingSettings is removed as it was deprecated last release since there is an Input Provider profile now. Settings can still be modified at Runtime the same way.

**Known Issues**
----------------

* Handtracking Performance issues when interacting with other objects. Continuous Improvements made each Sprint.
* To use the simulator when running MRTK, you must set the **Script Changes While Playing** setting to **Stop Playing and Recompile** or **Recompile and Conintue Playing** in the Unity **Preferences**.
* When playing in Editor errors with ImageTracking on start and Configuring HandTracking on stop may show in the logs, these do not cause issues and will be removed in future releases.

**Important Notes**
-------------------

* Instead of copying a configuration file, clone the DefaultMixedReality version and make adjustments. We have found copying an MRTK configuration file can cause issues such as Input Data Providers not loading or visualizers not attaching properly.
* Controller Visualizer sometimes stops positioning and the logs say: Left_ControllerModel(Clone) is missing a IMixedRealityControllerVisualizer component! This happens sporatically, we have found adding the MixedRealityControllerVisualizer component to the model itself resolves this.
* If your application builds and results in a blank/empty scene, you must adjust your projects quality settings. (Known issue in editors 2021.1 and older) To resolve this, remove all but one of the quality presets in your projects quality settings (**Edit>Player Settings>Quality**)
* Users may need to add the tracked pose driver to the camera themselves.