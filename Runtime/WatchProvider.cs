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

        private void Awake()
        {
            if (UseAndroidImplementation)
                watch = new AndroidWatchImpl(watchName);
            else
                watch = new GattWatchImpl(watchName);

            Watch.Instance.RegisterProvider(watch!);

        }

        private void Start()
        {
#if UNITY_ANDROID
            if (UseAndroidImplementation && ConnectOnStart)
                watch!.Connect();
            else if (!UseAndroidImplementation && !Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.Log("No location permission.");
                Permission.RequestUserPermission(Permission.FineLocation);
            }
#else
            if (ConnectOnStart)
                watch!.Connect();
#endif
        }

#if UNITY_ANDROID
        private void Update()
        {
            if (!UseAndroidImplementation && ConnectOnStart
                    && Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                watch!.Connect();
        }
#endif

    }

}
