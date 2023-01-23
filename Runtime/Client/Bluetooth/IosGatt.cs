#if UNITY_IOS || UNITY_TVOS

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

// Also works for apple tv.
public class IosGatt: Gatt
{

	[DllImport ("__Internal")]
	public static extern void _iOSBluetoothLELog (string message);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEInitialize (bool asCentral, bool asPeripheral);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDeInitialize ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEPauseMessages (bool isPaused);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEScanForPeripheralsWithServices (string serviceUUIDsString, bool allowDuplicates, bool rssiOnly, bool clearPeripheralList);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERetrieveListOfPeripheralsWithServices (string serviceUUIDsString);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopScan ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEConnectToPeripheral (string name);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectPeripheral (string name);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEReadCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEWriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLESubscribeCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEUnSubscribeCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectAll ();

	[DllImport("__Internal")]
	private static extern void _iOSBluetoothLERequestMtu(string name, int mtu);

    private static Psix.PsixLogger logger = new Psix.PsixLogger("IosGatt");
    override public void Log(string message)
    {
        logger.Debug(message);
    }

    override public BluetoothDeviceScript Initialize(bool asCentral, bool asPeripheral, Action action, Action<string> errorAction)
    {
        InitGameObject(action, errorAction);
		_iOSBluetoothLEInitialize (asCentral, asPeripheral);
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
        else
			_iOSBluetoothLEDeInitialize ();
    }

    override public void BluetoothEnable(bool enable)
    {
        if (!Application.isEditor)
        {
			//_iOSBluetoothLELog (message);
        }
    }

   override public void BluetoothScanMode(ScanMode scanMode) { }

   override public void BluetoothConnectionPriority(ConnectionPriority connectionPriority) { }

   override public void PauseMessages(bool isPaused)
    {
        if (!Application.isEditor)
        {
			_iOSBluetoothLEPauseMessages (isPaused);
        }
    }

    override public void RequestMtu(string name, int mtu, Action<string, int> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.RequestMtuAction = action;
        if (Application.isEditor)
            action(name, 0);

        if (mtu > 180)
            mtu = 180;
	    _iOSBluetoothLERequestMtu (name, mtu);

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
		_iOSBluetoothLEScanForPeripheralsWithServices (serviceUUIDsString, (actionAdvertisingInfo != null), rssiOnly, clearPeripheralList);
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

			_iOSBluetoothLERetrieveListOfPeripheralsWithServices (serviceUUIDsString);
        }
    }

    override public void StopScan()
    {
        if (!Application.isEditor)
        {
			_iOSBluetoothLEStopScan ();
        }
        else
        {
            BluetoothLEW32.Instance.StopScan();
        }
    }

    override public void DisconnectAll()
    {
        if (!Application.isEditor)
        {
			_iOSBluetoothLEDisconnectAll ();
        }
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
			_iOSBluetoothLEConnectToPeripheral (name);
    }

    override public void DisconnectPeripheral(string name, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DisconnectedPeripheralAction = action;
#if UNITY_IOS || UNITY_TVOS
			_iOSBluetoothLEDisconnectPeripheral (name);
#else
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                BluetoothLEW32.Instance.DisconnectPeripheral(name);
                break;
            case RuntimePlatform.OSXEditor:
                //OsxGatt.OSXBluetoothLEDisconnectPeripheral(name);
                break;
        }
#endif
    }

    override public void ReadCharacteristic(string name, string service, string characteristic, Action<string, byte[]> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
            {
                if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
                    bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();

				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
            }

			_iOSBluetoothLEReadCharacteristic (name, service, characteristic);
        }
    }

    override public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action)
    {
        if (bluetoothDeviceScript != null)
            bluetoothDeviceScript.DidWriteCharacteristicAction = action;
		_iOSBluetoothLEWriteCharacteristic (name, service, characteristic, data, length, withResponse);
    }

    override public void SubscribeCharacteristic(string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action)
    {
        if (!Application.isEditor)
        {
            if (bluetoothDeviceScript != null)
            {
                // name = name.ToUpper ();
                // service = service.ToUpper ();
                // characteristic = characteristic.ToUpper ();

				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [characteristic] = notificationAction;

				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] = new Dictionary<string, Action<string, byte[]>> ();
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
            }

			_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
        }
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

		_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
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

				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = null;

				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = action;

            }

			_iOSBluetoothLEUnSubscribeCharacteristic (name, service, characteristic);
        }
    }
}
#endif
