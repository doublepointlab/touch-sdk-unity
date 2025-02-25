/// Copyright (c) 2024 Doublepoint Technologies Oy <hello@doublepoint.com>
/// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


namespace Psix
{

    class TouchSdkMessageReceiver: MonoBehaviour
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
