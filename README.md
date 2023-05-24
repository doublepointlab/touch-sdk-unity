# Touch SDK Unity

[![openupm](https://img.shields.io/npm/v/io.port6.sdk?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/io.port6.sdk/)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/doublepointlab/touch-sdk-unity)
![GitHub](https://img.shields.io/github/license/doublepointlab/touch-sdk-unity)
[![Discord](https://img.shields.io/discord/869474617729875998)](https://chat.doublepoint.com)

Connect Touch SDK compatible wearable controllers to Unity applications over Bluetooth.

# Installing Touch SDK for Unity

## Requirements

Touch SDK Unity has been tested with the following platform versions:

- Meta Quest devices (recommended) runtime version 46
- Unity Version 2021.3.12f1
- Oculus Integration for Unity Version 46.0

## Installation

> The package name has recently changed from Port 6 SDK to Touch SDK, and our company name has changed from Port 6 to Doublepoint. The old names might still be visible in some places.

1. Set up your Unity project by following either of these instructions:

    - Oculus Developer Documentation: [Configure Unity Settings](https://developer.oculus.com/documentation/unity/unity-conf-settings/)
    - Dilmer’s YouTube video: [Oculus Integration And Interaction SDK Setup](https://www.youtube.com/watch?v=xuGGfl1vJ28)

2. Install the [Touch SDK Unity](https://openupm.com/packages/io.port6.sdk/) package from OpenUPM.

<!--[https://port6.io/sdk/download](https://port6.io/sdk/download)

3. Open the downloaded file
4. Import everything when prompted.

    ![Untitled](Untitled.png)

5. [OpenUPM](https://openupm.com/packages/io.port6.sdk/) Scope Register will be automaticaly added to download the current package

    ![Untitled](Untitled%201.png) -->

3. Go to Package Manager, select **In Project** from the dropdown menu, select **Touch SDK package,** open **Samples** and import **WatchVisualizer Demo**.

    ![Untitled](docs/Untitled%202.png)

7. Open watch visualizer scene in the samples folder.

    ![Untitled](docs/Untitled%203.png)

<!-- 8. If you have several watches or issues connecting to watch you can type in the Device ID in the Watch Name Field. You’ll find the device ID in the Touch SDK watch app settings

    ![Untitled](docs/Untitled%204.png) -->

5. Press **Build and Run** to compile your first app on a connected XR Android device.
10. Open the app on your headset.


    - Do **not** pair the watch manually using Bluetooth settings – this will disrupt the automatic pairing.

    - If you run the project on your Meta Quest for the first time, the app should ask **permission for location access** which is required for Bluetooth functionality to work. If the Oculus app doesn’t seem to be connecting to the watch after the permission has been granted, please restart the app.

    ![Connecting the Controller app to the headset](docs/video.gif)

11. The Unity application will automatically attempt to connect to your watch. If everything goes as expected, you will see a connection request on the Controller app on your watch.
12. After accepting the connection on the watch, you should see moving elements in the watch visualizer scene.
13. Tadaaa you made it!

## How to get watch events to your Unity project

[Touch SDK Unity Reference](docs/Reference.md)

## Play Mode

On windows you can develop in playmode even without a headset.

1. Open your watch app
2. Open a scene where watch manager is available like “WatchVisualizerScene”
3. Select WatchVisualizerPrefab and type in your device-ID under Watch Name (you’ll find it in settings)
4. Press play in Unity and wait. Your watch should connect automatically and turn from blue to green.
<!-- 5. If everything worked it should look like this

[IMG_4599.mp4](docs/IMG_4599.mp4) -->

<!-- > If nothing happens go to [troubleshooting](https://www.notion.so/Getting-the-Watch-App-1467cc771be542c1aafd4f1d395625c0). -->

## Known Issues

- The Bluetooth backend on which the feature is based is unreliable, and may cause editor crashing upon entering Play mode.
- Play Mode for editor platforms other than Windows is not supported.

## Troubleshooting

### Playmode

If after ~60s nothing happens do the following:

- restart the watch app by going to Touch SDK settings, press quit and restart it, wait 10s.
- If still nothing happens try to stop Unity, hit play again and wait another 60s.
- If still nothing happens turn windows bluetooth off and on, hit play again and wait another 60s
- If nothing works you have to build and run your app each time you want to test iterations
- We are working on it to improve this process!


If there is any problem, please reach out to as at hello@doublepoint.com, head to [our Discord](https://chat.doublepoint.com) or send feedback through our [feature request form](https://wtqs2o76hbt.typeform.com/to/M8L2PM6A).


Copyright (c) 2022–2023 Doublepoint <hello@doublepoint.com>
Licensed under the MIT License. See LICENSE for details.