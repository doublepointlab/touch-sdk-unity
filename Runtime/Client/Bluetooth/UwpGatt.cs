//#if ENABLE_WINMD_SUPPORT
// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

// Windows UWP
public class UwpGatt : Gatt
{

    private static Psix.PsixLogger logger = new Psix.PsixLogger("UwpGatt");

    override public void Log(string message)
    {
        logger.Debug(message);
    }

    override public BluetoothDeviceScript Initialize(bool asCentral, bool asPeripheral, Action action, Action<string> errorAction)
    {
        InitGameObject(action, errorAction);

        // TODO: fix
        //GameObject.DontDestroyOnLoad(bluetoothLEReceiver);
        BluetoothLEUWP.Instance.Initialize(bluetoothDeviceScript, asCentral, asPeripheral);
        return bluetoothDeviceScript;
    }

    override public void DeInitialize(Action action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DeinitializedAction = action;

        if (Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.SendMessage("OnBluetoothMessage", "DeInitialized");
        }
    }

    override public void BluetoothEnable(bool enable) { }

    override public void BluetoothScanMode(ScanMode scanMode) { }

    override public void BluetoothConnectionPriority(ConnectionPriority connectionPriority)
    {
        if (!Application.isEditor)
        {
            BluetoothLEUWP.Instance.ConnectionPriority(connectionPriority);
        }
    }

    override public void PauseMessages(bool isPaused) { }

    // scanning for beacons requires that you know the Proximity UUID
    override public void ScanForBeacons(string[] proximityUUIDs, Action<iBeaconData> actionBeaconResponse)
    {
        if (proximityUUIDs != null && proximityUUIDs.Length >= 0)
        {
            if (!Application.isEditor)
            {
                if (bluetoothDeviceScript != null)
                    bluetoothDeviceScript.DiscoveredBeaconAction = actionBeaconResponse;

                string proximityUUIDsString = null;

                if (proximityUUIDs != null && proximityUUIDs.Length > 0)
                {
                    proximityUUIDsString = "";

                    foreach (string proximityUUID in proximityUUIDs)
                        proximityUUIDsString += proximityUUID + "|";

                    proximityUUIDsString = proximityUUIDsString.Substring(0, proximityUUIDsString.Length - 1);
                }

            }
        }
    }

    override public void RequestMtu(string name, int mtu, Action<string, int> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.RequestMtuAction = action;
        if (Application.isEditor)
            action(name, 0);

        BluetoothLEUWP.Instance.RequestMTU(name);
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
        BluetoothLEUWP.Instance.ScanForPeripheralsWithServices(serviceUUIDs, rssiOnly, clearPeripheralList, recordType);
    }

    override public void RetrieveListOfPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
            {
                bluetoothDeviceScript.RetrievedConnectedPeripheralAction = action;

                if (bluetoothDeviceScript.DiscoveredDeviceList != null)
                    bluetoothDeviceScript.DiscoveredDeviceList.Clear();
            }
        }
    }

    override public void StopScan()
    {
        if (!Application.isEditor)
            BluetoothLEUWP.Instance.StopScan();
        else
            BluetoothLEW32.Instance.StopScan();
    }

    override public void StopBeaconScan() { }

    override public void DisconnectAll() { }

    override public void ConnectToPeripheral(string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> disconnectAction = null)
    {
        if (bluetoothDeviceScript != null)
        {
            bluetoothDeviceScript.ConnectedPeripheralAction = connectAction;
            bluetoothDeviceScript.DiscoveredServiceAction = serviceAction;
            bluetoothDeviceScript.DiscoveredCharacteristicAction = characteristicAction;
            bluetoothDeviceScript.ConnectedDisconnectPeripheralAction = disconnectAction;
        }
#if ENABLE_WINMD_SUPPORT
		BluetoothLEUWP.Instance.ConnectToPeripheral(name);
#else
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                BluetoothLEW32.Instance.ConnectToPeripheral(name);
                break;
            case RuntimePlatform.OSXEditor:
                //OSXBluetoothLEConnectToPeripheral(name);
                break;
        }
#endif
    }

    override public void DisconnectPeripheral(string name, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DisconnectedPeripheralAction = action;
        BluetoothLEUWP.Instance.DisconnectPeripheral(name);
    }

    override public void ReadCharacteristic(string name, string service, string characteristic, Action<string, byte[]> action)
    {
        if (bluetoothDeviceScript != null)
        {
            if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();
        }
    }

    override public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DidWriteCharacteristicAction = action;
        BluetoothLEUWP.Instance.WriteCharacteristic(name, service, characteristic, data, length, withResponse);
    }

    override public void SubscribeCharacteristic(string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action) { }

    override public void SubscribeCharacteristicWithDeviceAddress(string name, string service, string characteristic, Action<string, string> notificationAction, Action<string, string, byte[]> action)
    {
        BluetoothLEUWP.Instance.SubscribeCharacteristicWithDeviceAddress(name, service, characteristic);
    }

    override public void UnSubscribeCharacteristic(string name, string service, string characteristic, Action<string> action)
    {
        if (!Application.isEditor)
        {
            BluetoothLEUWP.Instance.UnSubscribeCharacteristic(name, service, characteristic);
        }
    }
}

//#endif
