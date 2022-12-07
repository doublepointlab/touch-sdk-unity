// #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
// #define EXPERIMENTAL_MACOS_EDITOR

// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/*

This build includes an experimental implementation for the macOS editor of Unity
It is experiemental because of the way that the Unity editor hangs on to plugin
instances after leaving play mode. This causes this plugin to not free up its
resources and therefore can cause crashes in the Unity editor on macOS.

Since Unity does not give plugins or apps a chance to do anything when the user
hits the play / stop button in the Editor there isn't a chance for the app to
deinitialize this plugin.

What I have found in my own use of this is that if you put a button on your app
somewhere that you can press before hitting the stop button in the editor and
then in that button handler call this plugin's Deinitialize method it seems to
minimize how often the editor crashes.

WARNING: using the macOS editor can cause the editor to crash an loose your work
and settings. Save often. You have been warned, so please don't contact me if
you have lost work becausee of this problem. This is experimental only. Use at
your own risk.

*/

public class OsxGatt : Gatt
{

    public delegate void UnitySendMessageCallbackDelegate(IntPtr objectName, IntPtr commandName, IntPtr commandData);

    [DllImport("BluetoothLEOSX")]
    private static extern void ConnectUnitySendMessageCallback([MarshalAs(UnmanagedType.FunctionPtr)] UnitySendMessageCallbackDelegate callbackMethod);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLELog(string message);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEInitialize([MarshalAs(UnmanagedType.Bool)] bool asCentral, [MarshalAs(UnmanagedType.Bool)] bool asPeripheral);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEDeInitialize();

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEPauseMessages(bool isPaused);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEScanForPeripheralsWithServices(string serviceUUIDsString, bool allowDuplicates, bool rssiOnly, bool clearPeripheralList);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLERetrieveListOfPeripheralsWithServices(string serviceUUIDsString);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEStopScan();

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEConnectToPeripheral(string name);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEDisconnectAll();

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLERequestMtu(string name, int mtu);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEDisconnectPeripheral(string name);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEReadCharacteristic(string name, string service, string characteristic);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEWriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLESubscribeCharacteristic(string name, string service, string characteristic);

    [DllImport("BluetoothLEOSX")]
    private static extern void OSXBluetoothLEUnSubscribeCharacteristic(string name, string service, string characteristic);


    static Psix.PsixLogger logger = new Psix.PsixLogger("BleOsxInterface");
    override public void Log(string message)
    {
        logger.Debug(message);
    }

    override public BluetoothDeviceScript Initialize(bool asCentral, bool asPeripheral, Action action, Action<string> errorAction)
    {
        InitGameObject(action, errorAction);
        ConnectUnitySendMessageCallback((objectName, commandName, commandData) =>
        {
            string name = Marshal.PtrToStringAuto(objectName);
            string command = Marshal.PtrToStringAuto(commandName);
            string data = Marshal.PtrToStringAuto(commandData);

            GameObject foundObject = GameObject.Find(name);
            if (foundObject != null)
                foundObject.SendMessage(command, data);
        });

        OSXBluetoothLEInitialize(asCentral, asPeripheral);
        return bluetoothDeviceScript;
    }

    override public void DeInitialize(Action action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DeinitializedAction = action;

        OSXBluetoothLEDeInitialize();
    }

    override public void BluetoothEnable(bool enable) { }

    override public void BluetoothScanMode(ScanMode scanMode) { }

    override public void BluetoothConnectionPriority(ConnectionPriority connectionPriority) { }

    override public void PauseMessages(bool isPaused)
    {
        OSXBluetoothLEPauseMessages(isPaused);
    }

    override public void RequestMtu(string name, int mtu, Action<string, int> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.RequestMtuAction = action;
        if (Application.isEditor)
            action(name, 0);

        if (mtu > 184)
            mtu = 184;
        OSXBluetoothLERequestMtu(name, mtu);
    }

    override public void ScanForPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> action, Action<string, string, int, byte[]> actionAdvertisingInfo = null, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF)
    {

        if (bluetoothDeviceScript != null)
        {
            bluetoothDeviceScript.DiscoveredPeripheralAction = action;
            bluetoothDeviceScript.DiscoveredPeripheralWithAdvertisingInfoAction = actionAdvertisingInfo;

            if (bluetoothDeviceScript.DiscoveredDeviceList != null)
            {
                bluetoothDeviceScript.DiscoveredDeviceList.Clear();
            }
        }

        string serviceUUIDsString = null;

        if (serviceUUIDs != null && serviceUUIDs.Length > 0)
        {
            serviceUUIDsString = "";

            foreach (string serviceUUID in serviceUUIDs)
            {
                serviceUUIDsString += serviceUUID + "|";
            }

            serviceUUIDsString = serviceUUIDsString.Substring(0, serviceUUIDsString.Length - 1);
        }

        OSXBluetoothLEScanForPeripheralsWithServices(serviceUUIDsString, (actionAdvertisingInfo != null), rssiOnly, clearPeripheralList);
    }

    override public void RetrieveListOfPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> action)
    {
        if (bluetoothDeviceScript != null)
        {
            bluetoothDeviceScript.RetrievedConnectedPeripheralAction = action;

            if (bluetoothDeviceScript.DiscoveredDeviceList != null)
                bluetoothDeviceScript.DiscoveredDeviceList.Clear();
        }

        string serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;

        foreach (string serviceUUID in serviceUUIDs)
            serviceUUIDsString += serviceUUID + "|";

        // strip the last delimeter
        serviceUUIDsString = serviceUUIDsString.Substring(0, serviceUUIDsString.Length - 1);

        OSXBluetoothLERetrieveListOfPeripheralsWithServices(serviceUUIDsString);
    }

    override public void StopScan()
    {
        OSXBluetoothLEStopScan();
    }

    override public void DisconnectAll()
    {
        OSXBluetoothLEDisconnectAll();
    }

    override public void ConnectToPeripheral(string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> disconnectAction = null)
    {
        if (bluetoothDeviceScript != null)
        {
            bluetoothDeviceScript.ConnectedPeripheralAction = connectAction;
            bluetoothDeviceScript.DiscoveredServiceAction = serviceAction;
            bluetoothDeviceScript.DiscoveredCharacteristicAction = characteristicAction;
            bluetoothDeviceScript.ConnectedDisconnectPeripheralAction = disconnectAction;
        }
#if UNITY_STANDALONE_OSX
			OSXBluetoothLEConnectToPeripheral (name);
#else
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                BluetoothLEW32.Instance.ConnectToPeripheral(name);
                break;
            case RuntimePlatform.OSXEditor:
                OSXBluetoothLEConnectToPeripheral(name);
                break;
        }
#endif
    }

    override public void DisconnectPeripheral(string name, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DisconnectedPeripheralAction = action;
#if UNITY_STANDALONE_OSX
			OSXBluetoothLEDisconnectPeripheral (name);
#else
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                BluetoothLEW32.Instance.DisconnectPeripheral(name);
                break;
            case RuntimePlatform.OSXEditor:
                OSXBluetoothLEDisconnectPeripheral(name);
                break;
        }
#endif
    }

    override public void ReadCharacteristic(string name, string service, string characteristic, Action<string, byte[]> action)
    {
        if (bluetoothDeviceScript != null)
        {
            if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();

            bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name][characteristic] = action;
        }

        OSXBluetoothLEReadCharacteristic(name, service, characteristic);
    }

    override public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DidWriteCharacteristicAction = action;
#if UNITY_STANDALONE_OSX
		OSXBluetoothLEWriteCharacteristic(name, service, characteristic, data, length, withResponse);
#else
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                BluetoothLEW32.Instance.WriteCharacteristic(name, service, characteristic, data, length, withResponse);
                break;
            case RuntimePlatform.OSXEditor:
                OSXBluetoothLEWriteCharacteristic(name, service, characteristic, data, length, withResponse);
                break;
        }
#endif
    }

    override public void SubscribeCharacteristic(string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action)
    {
        if (bluetoothDeviceScript != null)
        {
            // name = name.ToUpper ();
            // service = service.ToUpper ();
            // characteristic = characteristic.ToUpper ();

            if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>>();
            bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = notificationAction;

            if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();
            bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name][characteristic] = action;
        }

        OSXBluetoothLESubscribeCharacteristic(name, service, characteristic);
    }

    override public void SubscribeCharacteristicWithDeviceAddress(string name, string service, string characteristic, Action<string, string> notificationAction, Action<string, string, byte[]> action)
    {
        if (bluetoothDeviceScript != null)
        {
            if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
            bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = notificationAction;

            if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>>();
            bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = null;

            if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
            bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][characteristic] = action;
        }

        OSXBluetoothLESubscribeCharacteristic(name, service, characteristic);
    }

    override public void UnSubscribeCharacteristic(string name, string service, string characteristic, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
        {
            // name = name.ToUpper ();
            // service = service.ToUpper ();
            // characteristic = characteristic.ToUpper ();

            if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
            bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = null;

            if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>>();
            bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = action;

        }

        OSXBluetoothLEUnSubscribeCharacteristic(name, service, characteristic);
    }
}

//#endif
