# Touch SDK

C# scripts for bridging Unity applications with Doublepoint enabled smart peripherals.

## Installation

[With OpenUPM](https://openupm.com/packages/io.port6.sdk/)

## Example usage

Add the `BluetoothWatchProvider` script (`Runtime/BluetoothWatchProvider.cs`) to an active game object in your Unity project. Then, add the following script:

```csharp

using UnityEngine;

using Psix;

public class WatchExample : MonoBehaviour
{

    public void Start()
    {
        Watch.Instance.OnGesture += OnGesture;

        Watch.Instance.OnAcceleration += OnAcceleration;
        Watch.Instance.OnAngularVelocity += OnAngularVelocity;
        Watch.Instance.OnGravity += OnGravity;
        Watch.Instance.OnOrientation += OnOrientation;
    }
}

```

Copyright (c) 2024 Doublepoint Oy <hello@doublepoint.com>
Licensed under the MIT License. See LICENSE for details.
