#if UNITY_ANDROID

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using UnityEngine.Android;


public class AndroidGatt : Gatt
{

    static AndroidJavaObject _android = null;
    public static AndroidJavaObject Android { get { return _android; } }

    private static Psix.PsixLogger logger = new Psix.PsixLogger("AndroidGatt");

    override public void Log(string message)
    {
        logger.Debug(message);
    }

    override public BluetoothDeviceScript Initialize(bool asCentral, bool asPeripheral, Action action, Action<string> errorAction)
    {
        Log("Initialize");
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            Permission.RequestUserPermission(Permission.FineLocation);
        InitGameObject(action, errorAction);
        if (_android == null)
        {
            AndroidJavaClass javaClass = new AndroidJavaClass("com.shatalmic.unityandroidbluetoothlelib.UnityBluetoothLE");
            _android = javaClass.CallStatic<AndroidJavaObject>("getInstance");
        }

        if (_android != null)
            _android.Call("androidBluetoothInitialize", asCentral, asPeripheral);

        return bluetoothDeviceScript;
    }

    override public void DeInitialize(Action action)
    {
        Log("Initialize");
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DeinitializedAction = action;

        // ???
        if (Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.SendMessage("OnBluetoothMessage", "DeInitialized");
        }
        else if (_android != null)
            _android.Call("androidBluetoothDeInitialize");
    }

    override public void BluetoothEnable(bool enable)
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidBluetoothEnable", enable);
        }
    }

    override public void BluetoothScanMode(ScanMode scanMode)
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidBluetoothScanMode", (int)scanMode);
        }
    }

    override public void BluetoothConnectionPriority(ConnectionPriority connectionPriority)
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidBluetoothConnectionPriority", (int)connectionPriority);
        }
    }

    override public void BluetoothAdvertisingMode(AdvertisingMode advertisingMode)
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidBluetoothAdvertisingMode", (int)advertisingMode);
        }
    }

    override public void BluetoothAdvertisingPower(AdvertisingPower advertisingPower)
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidBluetoothAdvertisingPower", (int)advertisingPower);
        }
    }

    override public void PauseMessages(bool isPaused)
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidBluetoothPause", isPaused);
        }
    }

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

                if (_android != null)
                    _android.Call("androidBluetoothScanForBeacons", proximityUUIDsString);
            }
        }
    }

    override public void RequestMtu(string name, int mtu, Action<string, int> action)
    {
        Log($"Request MTU {mtu}");
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.RequestMtuAction = action;
        if (Application.isEditor)
            action(name, 0);

        if (_android != null)
        {
            _android.Call("androidBluetoothRequestMtu", name, mtu);
        }

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

        if (_android != null)
        {
            if (serviceUUIDsString == null)
                serviceUUIDsString = "";

            _android.Call("androidBluetoothScanForPeripheralsWithServices", serviceUUIDsString, rssiOnly, recordType);
        }
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

            string serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;

            foreach (string serviceUUID in serviceUUIDs)
                serviceUUIDsString += serviceUUID + "|";

            // strip the last delimeter
            serviceUUIDsString = serviceUUIDsString.Substring(0, serviceUUIDsString.Length - 1);

            if (_android != null)
                _android.Call("androidBluetoothRetrieveListOfPeripheralsWithServices", serviceUUIDsString);
        }
    }

    override public void StopScan()
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidBluetoothStopScan");
        }
        else
        {
            //BluetoothLEW32.Instance.StopScan();
        }
    }

    override public void StopBeaconScan()
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidBluetoothStopBeaconScan");
        }
    }

    override public void DisconnectAll()
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidBluetoothDisconnectAll");
        }
    }

    override public void ConnectToPeripheral(string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> disconnectAction = null)
    {
        Log($"ConnectToPeripheral {name}");
        if (bluetoothDeviceScript != null)
        {
            bluetoothDeviceScript.ConnectedPeripheralAction = connectAction;
            bluetoothDeviceScript.DiscoveredServiceAction = serviceAction;
            bluetoothDeviceScript.DiscoveredCharacteristicAction = characteristicAction;
            bluetoothDeviceScript.ConnectedDisconnectPeripheralAction = disconnectAction;
        }
        if (_android != null)
            _android.Call("androidBluetoothConnectToPeripheral", name);
    }

    override public void DisconnectPeripheral(string name, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DisconnectedPeripheralAction = action;
        if (_android != null)
            _android.Call("androidBluetoothDisconnectPeripheral", name);
    }

    override public void ReadCharacteristic(string name, string service, string characteristic, Action<string, byte[]> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
            {
                if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();

                bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name][FullUUID(characteristic).ToLower()] = action;
            }

            if (_android != null)
                _android.Call("androidReadCharacteristic", name, service, characteristic);
        }
    }

    override public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DidWriteCharacteristicAction = action;
#if UNITY_ANDROID
        if (_android != null)
            _android.Call("androidWriteCharacteristic", name, service, characteristic, data, length, withResponse);
#else
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                //BluetoothLEW32.Instance.WriteCharacteristic(name, service, characteristic, data, length, withResponse);
                break;
        }
#endif
    }

    override public void SubscribeCharacteristic(string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action)
    {
        Log("SubscribeCharacteristic");
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
            {
                // name = name.ToUpper ();
                // service = service.ToUpper ();
                // characteristic = characteristic.ToUpper ();

                if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>>();
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][FullUUID(characteristic).ToLower()] = notificationAction;

                if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();
                bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name][FullUUID(characteristic).ToLower()] = action;
            }

            if (_android != null)
                _android.Call("androidSubscribeCharacteristic", name, service, characteristic);
        }
    }

    override public void SubscribeCharacteristicWithDeviceAddress(string name, string service, string characteristic, Action<string, string> notificationAction, Action<string, string, byte[]> action)
    {
        if (bluetoothDeviceScript != null)
        {
            if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
            bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID(characteristic).ToLower()] = notificationAction;

            if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>>();
            bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][FullUUID(characteristic).ToLower()] = null;

            if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey(name))
                bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
            bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][FullUUID(characteristic).ToLower()] = action;
        }

        if (_android != null)
            _android.Call("androidSubscribeCharacteristic", name, service, characteristic);
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
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID(characteristic).ToLower()] = null;

                if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>>();
                bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][FullUUID(characteristic).ToLower()] = action;
            }

            if (_android != null)
                _android.Call("androidUnsubscribeCharacteristic", name, service, characteristic);
        }
    }

    override public void PeripheralName(string newName)
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidPeripheralName", newName);
        }
    }

    override public void CreateService(string uuid, bool primary, Action<string> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.ServiceAddedAction = action;

            if (_android != null)
                _android.Call("androidCreateService", uuid, primary);
        }
    }

    override public void CreateCharacteristic(string uuid, CharacteristicProperties properties, CharacteristicPermissions permissions, byte[] data, int length, Action<string, byte[]> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.PeripheralReceivedWriteDataAction = action;

            if (_android != null)
                _android.Call("androidCreateCharacteristic", uuid, (int)properties, (int)permissions, data, length);
        }
    }

    override public void RemoveService(string uuid)
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidRemoveService", uuid);
        }
    }

    override public void RemoveServices()
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidRemoveServices");
        }
    }

    override public void RemoveCharacteristic(string uuid)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.PeripheralReceivedWriteDataAction = null;

            if (_android != null)
                _android.Call("androidRemoveCharacteristic", uuid);
        }
    }

    override public void RemoveCharacteristics()
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidRemoveCharacteristics");
        }
    }

    override public void StartAdvertising(Action action, bool isConnectable = true, bool includeName = true, int manufacturerId = 0, byte[] manufacturerSpecificData = null)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.StartedAdvertisingAction = action;
            if (_android != null)
                _android.Call("androidStartAdvertising", isConnectable, includeName, manufacturerId, manufacturerSpecificData);
        }
    }

    override public void StopAdvertising(Action action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
                bluetoothDeviceScript.StoppedAdvertisingAction = action;

            if (_android != null)
                _android.Call("androidStopAdvertising");
        }
    }

    override public void UpdateCharacteristicValue(string uuid, byte[] data, int length)
    {
        if (!Application.isEditor)
        {
            if (_android != null)
                _android.Call("androidUpdateCharacteristicValue", uuid, data, length);
        }
    }
}

#endif
