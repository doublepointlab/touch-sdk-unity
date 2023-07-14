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

        public event Action<String>? OnMessage = null;


        public void OnTouchSdkMessage(string message)
        {
            Debug.Log("receiver got " + message);
            OnMessage?.Invoke(message);
        }

    }

}
