# Touch SDK Unity

![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/doublepointlab/touch-sdk-unity)
![GitHub](https://img.shields.io/github/license/doublepointlab/touch-sdk-unity)
[![Discord](https://img.shields.io/discord/869474617729875998)](https://chat.doublepoint.com)

C# scripts for bridging Unity applications with Doublepoint enabled smart peripherals.

## Installation

### On disk

Using the package manager window inside unity, select "Add package from disk" and select the `package.json` file in the root of this repository.

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


## Changelog (0.13.1)

- Improve sample scene
- Add Python-based editor mode support
- Fix gesture detection issue

Copyright (c) 2025 Doublepoint Oy <hello@doublepoint.com>
Licensed under the MIT License. See LICENSE for details.
