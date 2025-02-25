/// Copyright (c) 2024 Doublepoint Technologies Oy <hello@doublepoint.com>
/// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf;

using UnityEngine;


namespace Psix
{

    using Interaction;

    /**
     * Implementation of smartwatch interface using touch-sdk-android.
     * Provides methods and callbacks related to connecting to Doublepoint
     * Controller smartwatch app.
     * Check also IWatch.
     */
    [DefaultExecutionOrder(-50)]
    class AndroidWatchImpl : WatchImpl
    {

        private static PsixLogger logger = new PsixLogger("AndroidWatchImpl");

        private AndroidJavaObject androidInterface;

        private bool companionDeviceMode = false;

        public AndroidWatchImpl(bool useCompanionDevice = false) {

#if !UNITY_ANDROID
            logger.Warn("AndroidWatchImpl is only supported on Android.");
#endif

            companionDeviceMode = useCompanionDevice;

            // Create a game object which receives messages from the Touch SDK interface.
            GameObject receiverGameObject = new GameObject("TouchSdkGameObject");
            TouchSdkMessageReceiver receiver = receiverGameObject.AddComponent<TouchSdkMessageReceiver>();

            receiver.OnMessage += onData;
            receiver.OnDisconnect += disconnectAction;

            androidInterface = new AndroidJavaObject("io.port6.android.unitywrapper.AndroidUnityWrapper");
        }

        private void onData(byte[] data) {
            if (!Connected) {
                connectAction();
            }

            OnProtobufData(data);
        }

        override public void Connect(string name = "")
        {
            androidInterface.Call("connect", name, companionDeviceMode);
        }

        override public void Disconnect()
        {
            androidInterface.Call("disconnect");
        }

        override public void Vibrate(int length, float amplitude)
        {
            androidInterface.Call("vibrate", length, amplitude);
        }

        override public void CancelVibration()
        {
            androidInterface.Call("cancelVibration");
        }

        override public void RequestGestureDetection(Gesture gesture)
        {
            androidInterface.Call("requestGestureDetection", (int)gesture);
        }
    }
}
