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

        [Tooltip("Python path is relevant if you use python touch sdk implementation. Leave empty to not use it.")]
        [SerializeField] private string pythonPath = "";

        public bool ConnectOnStart = true;

        private bool UseAndroidImplementation = true;

        private bool pythonImplActive
        {
            get
            {
                return !String.IsNullOrEmpty(pythonPath) && Application.isEditor;
            }
        }
        private bool androidImplActive
        {
            get
            {
                return !pythonImplActive && UseAndroidImplementation && Application.platform == RuntimePlatform.Android;
            }
        }

        private IWatch? watch = null;

#if UNITY_ANDROID
        private bool connectCalledOnStart = false;
        private string scanPermission = "android.permission.BLUETOOTH_SCAN";
        private string connectPermission = "android.permission.BLUETOOTH_CONNECT";
#endif

        private void Awake()
        {
#if ENABLE_WINMD_SUPPORT
            watch = new UwpWatchImpl(watchName);
#else
            if (pythonImplActive)
                watch = new PythonWatchImpl(pythonPath, watchName);
            else if (androidImplActive)
                watch = new AndroidWatchImpl(watchName, false);
            else
                watch = new GattWatchImpl(watchName);
#endif

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
            if (pythonImplActive)
            {
                if (ConnectOnStart)
                {
                    watch!.Connect();
                }
            }
            else RequestPermissions();
#else
            if (ConnectOnStart)
                watch!.Connect();
#endif
        }

#if UNITY_ANDROID
        private void Update()
        {
            if (!pythonImplActive && (ConnectOnStart && !connectCalledOnStart && CheckPermissions()))
            {
                connectCalledOnStart = true;
                watch!.Connect();
            }
        }
#endif
        private void OnApplicationQuit()
        {
            watch?.Disconnect();
        }

    }

}
