// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


namespace Psix
{

    public class TouchSdkMessageReceiver: MonoBehaviour
    {

        public event Action<byte[]>? OnMessage = null;
        public event Action? OnDisconnect = null;


        public void OnTouchSdkMessage(string message)
        {
            OnMessage?.Invoke(System.Convert.FromBase64String(message));
        }

        public void OnTouchSdkDisconnect() {
            OnDisconnect?.Invoke();
        }

    }

}
