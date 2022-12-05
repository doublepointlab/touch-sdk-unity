// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

// #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class WinGatt : Gatt
{

    private static Psix.PsixLogger logger = new Psix.PsixLogger("WinGatt");

    override public void Log(string message)
    {
        logger.Debug(message);
    }

    override public BluetoothDeviceScript Initialize(bool asCentral, bool asPeripheral, Action action, Action<string> errorAction)
    {
        InitGameObject(action, errorAction);

        BluetoothLEW32.Instance.Initialize(bluetoothDeviceScript, asCentral, asPeripheral);
        return bluetoothDeviceScript;
    }

    override public void DeInitialize(Action action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DeinitializedAction = action;
        BluetoothLEW32.Instance.Deinitialize();

        if (Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.SendMessage("OnBluetoothMessage", "DeInitialized");
        }
    }

    override public void BluetoothEnable(bool enable) { }

    override public void BluetoothScanMode(ScanMode scanMode) { }

    override public void BluetoothConnectionPriority(ConnectionPriority connectionPriority) { }

    override public void BluetoothAdvertisingMode(AdvertisingMode advertisingMode) { }

    override public void BluetoothAdvertisingPower(AdvertisingPower advertisingPower) { }

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
            action(name, 512);
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
        BluetoothLEW32.Instance.ScanForPeripheralsWithServices(serviceUUIDs, rssiOnly, clearPeripheralList, recordType);
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
        BluetoothLEW32.Instance.ConnectToPeripheral(name);
    }

    override public void DisconnectPeripheral(string name, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DisconnectedPeripheralAction = action;
        BluetoothLEW32.Instance.DisconnectPeripheral(name);
    }

    override public void ReadCharacteristic(string name, string service, string characteristic, Action<string, byte[]> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
            {
                if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();
            }
        }
    }

    override public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DidWriteCharacteristicAction = action;
#if UNITY_STANDALONE_WIN
        BluetoothLEW32.Instance.WriteCharacteristic(name, service, characteristic, data, length, withResponse);
#else
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                BluetoothLEW32.Instance.WriteCharacteristic(name, service, characteristic, data, length, withResponse);
                break;
        }
#endif
    }

    override public void SubscribeCharacteristic(string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action) { }

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

        BluetoothLEW32.Instance.SubscribeCharacteristicWithDeviceAddress(name, service, characteristic);
    }

    override public void UnSubscribeCharacteristic(string name, string service, string characteristic, Action<string> action)
    {
        if (!Application.isEditor)
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
        }
    }

    override public void PeripheralName(string newName) { }

    override public void CreateService(string uuid, bool primary, Action<string> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.ServiceAddedAction = action;

        }
    }

    override public void RemoveService(string uuid) { }

    override public void RemoveServices() { }

    override public void CreateCharacteristic(string uuid, CharacteristicProperties properties, CharacteristicPermissions permissions, byte[] data, int length, Action<string, byte[]> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.PeripheralReceivedWriteDataAction = action;

        }
    }

    override public void RemoveCharacteristic(string uuid)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.PeripheralReceivedWriteDataAction = null;

        }
    }

    override public void RemoveCharacteristics() { }

    override public void StartAdvertising(Action action, bool isConnectable = true, bool includeName = true, int manufacturerId = 0, byte[] manufacturerSpecificData = null)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.StartedAdvertisingAction = action;
        }
    }

    override public void StopAdvertising(Action action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.StoppedAdvertisingAction = action;
        }
    }

    override public void UpdateCharacteristicValue(string uuid, byte[] data, int length) { }

}

//#endif
