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
    public class WatchProvider : MonoBehaviour
    {
        [SerializeField] public string watchName = "";

        public bool ConnectOnStart = true;

        private bool UseAndroidImplementation = true;

        private IWatch? watch = null;

        private string scanPermission = "android.permission.BLUETOOTH_SCAN";
        private string connectPermission = "android.permission.BLUETOOTH_CONNECT";

        private void Awake()
        {
#if UNITY_ANDROID
            if (UseAndroidImplementation)
                watch = new AndroidWatchImpl(watchName);
            else
                watch = new GattWatchImpl(watchName);
#else
            watch = new GattWatchImpl(watchName);
#endif
            Watch.Instance.RegisterProvider(watch!);
        }

        private bool CheckPermissions()
        {
            return (
            Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)
            && Permission.HasUserAuthorizedPermission(Permission.FineLocation)
            && Permission.HasUserAuthorizedPermission(scanPermission)
            && Permission.HasUserAuthorizedPermission(connectPermission)
            );
        }

        private bool RequestPermissions()
        {
            Permission.RequestUserPermissions(new string[] {
                Permission.CoarseLocation,
                Permission.FineLocation,
                scanPermission,
                connectPermission
            });
        }

        private void Start()
        {
#if UNITY_ANDROID
            if (UseAndroidImplementation && ConnectOnStart)
                watch!.Connect();
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
            if (!UseAndroidImplementation && ConnectOnStart && CheckPermissions())
                watch!.Connect();
        }
#endif

    }

}
