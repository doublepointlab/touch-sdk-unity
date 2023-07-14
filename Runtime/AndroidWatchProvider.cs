// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR_WIN
#warning "kek"
#endif

#if UNITY_ANDROID
using UnityEngine.Android;
#endif


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
    public class AndroidWatchProvider : MonoBehaviour, IWatch
    {
        [SerializeField] public string watchName = "";

        // The bluetooth name of the watch */
        public string ConnectedWatchName
        {
            get;
            private set;
        }

        // Connecting to a gatt server might take minutes at worst on some
        // machines.  Most devices will hopefully connect within 30 seconds.
        // [HideInInspector] public int connectionTimeoutSeconds = 120;

        public bool ConnectOnStart = true;

        private static PsixLogger logger = new PsixLogger("AndroidWatchProvider");

        /**
         * Connect to the watch running Doublepoint Controller app.
         */
        public void Connect()
        {
            // TODO
        }

        /**
         * Disconnect a connected watch.
         */
        public void Disconnect()
        {
            // TODO
        }

        public bool Connected { get; private set; } = false;

        /**
         * Trigger a one-shot haptic feedback effect on the watch.
         *
         * @param length The duration of the effect in milliseconds.
         * @param amplitude The strength of the effect, between 0.0 and 1.0.
         */
        public void Vibrate(int length, float amplitude)
        {
            // TODO
        }

        /**
         * Cancel an ongoing haptic effect that was triggered earlier.
         */
        public void CancelVibration()
        {
            // TODO
        }

        public void RequestGestureDetection(Gesture gesture)
        {
            // TODO
        }

        AndroidJavaObject kotlinObject;

        private void Awake()
        {
            // TODO?
            Watch.Instance.RegisterProvider(this);

            GameObject receiverGameObject = new GameObject("TouchSdkGameObject");
            TouchSdkMessageReceiver receiver = receiverGameObject.AddComponent<TouchSdkMessageReceiver>();
            receiver.OnMessage += OnMessage;

            kotlinObject = new AndroidJavaObject("io.port6.android.unitywrapper.AndroidUnityWrapper");

        }

        private void OnMessage(string message) {
            Debug.Log("GOT MESSAGE: " + message);
        }

        public void ClearSubscriptions() {}


        private void Start()
        {
            if (ConnectOnStart)
                kotlinObject.Call("connect");
            else
                kotlinObject.Call("requestPermissions");
        }

#if UNITY_ANDROID
        private void Update()
        {
        }
#endif

        public AndroidWatchProvider()
        {
            // TODO?
        }

        /* Documented in WatchInterface */
        public event Action<Vector3>? OnAngularVelocity = null;
        public event Action<Vector3>? OnAcceleration = null;
        public event Action<Vector3>? OnGravity = null;
        public event Action<Quaternion>? OnOrientation = null;
        public event Action<Gesture>? OnGesture = null;
        public event Action<TouchEvent>? OnTouch = null;
        public event Action? OnButton = null;
        public event Action<Direction>? OnRotary = null;

        private Hand Handedness = Hand.None;
        public event Action<Hand>? OnHandednessChange = null;

        private HashSet<Gesture> ActiveGestures = new HashSet<Gesture>();
        public event Action<HashSet<Gesture>>? OnDetectedGesturesChange = null;

        public event Action? OnConnect = null;
        public event Action? OnDisconnect = null;

        /* Not part of generic interface */
        public event Action? OnScanTimeout = null;

    }
}
