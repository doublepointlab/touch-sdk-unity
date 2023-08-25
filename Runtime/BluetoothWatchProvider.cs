// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif


namespace Psix
{

    using Interaction;

    /**
     * MonoBehaviour that instantiates and register the appropriate IWatch implementation.
     */
    [DefaultExecutionOrder(-50)]
    public class BluetoothWatchProvider : MonoBehaviour
    {
        [SerializeField] public string watchName = "";

        public bool ConnectOnStart = true;

        private bool connectCalledOnStart = false;

        public bool DeviceMenu = true;

        private bool UseAndroidImplementation = true;

        private bool androidImplActive {
            get {
                return UseAndroidImplementation && Application.platform == RuntimePlatform.Android;
            }
        }

        private IWatch? watch = null;

#if UNITY_ANDROID
        private string scanPermission = "android.permission.BLUETOOTH_SCAN";
        private string connectPermission = "android.permission.BLUETOOTH_CONNECT";
#endif

        private void Awake()
        {
            if (androidImplActive)
                watch = new AndroidWatchImpl(watchName, DeviceMenu);
            else
                watch = new GattWatchImpl(watchName);

            Watch.Instance.RegisterProvider(watch!);
        }

        private bool CheckPermissions()
        {
#if UNITY_ANDROID
            var jniClass = AndroidJNI.FindClass("android/os/Build$VERSION");
            var fieldID = AndroidJNI.GetStaticFieldID(jniClass, "SDK_INT", "I");
            var sdkLevel = AndroidJNI.GetStaticIntField(jniClass, fieldID);

            var androidTwelvePermissionsOk = (sdkLevel < 31) ||
                (Permission.HasUserAuthorizedPermission(scanPermission)
                && Permission.HasUserAuthorizedPermission(connectPermission)
            );

            return (
                Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)
                && Permission.HasUserAuthorizedPermission(Permission.FineLocation)
                && androidTwelvePermissionsOk
            );
#else
            return true;
#endif
        }

        private void RequestPermissions()
        {
#if UNITY_ANDROID
            Permission.RequestUserPermissions(new string[] {
                Permission.CoarseLocation,
                Permission.FineLocation,
                scanPermission,
                connectPermission
            });
#endif
        }

        private void Start()
        {
#if UNITY_ANDROID
            if (androidImplActive && ConnectOnStart) {
                watch!.Connect();
                connectCalledOnStart = true;
            }
            else if (!UseAndroidImplementation && !CheckPermissions())
            {
                Debug.Log("No permissions.");
                RequestPermissions();
            }
#else
            if (ConnectOnStart)
                watch!.Connect();
#endif
        }

#if UNITY_ANDROID
        private void Update()
        {
            if (!UseAndroidImplementation && ConnectOnStart && !connectCalledOnStart && CheckPermissions()) {
                connectCalledOnStart = true;
                watch!.Connect();
            }
        }
#endif

    }

}
