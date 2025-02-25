/// Copyright (c) 2024 Doublepoint Technologies Oy <hello@doublepoint.com>
/// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using UnityEngine;

public class BluetoothDeviceScript : MonoBehaviour
{
    public static Gatt BLE = GattImpl.Instance;
    static Psix.PsixLogger logger = new Psix.PsixLogger("BluetoothDeviceScript");

    public Queue<string> MessagesToProcess = null;

    public List<string> DiscoveredDeviceList;

    public Action InitializedAction;
    public Action DeinitializedAction;
    public Action<string> ErrorAction;
    public Action<string, string> DiscoveredPeripheralAction;
    public Action<string, string, int, byte[]> DiscoveredPeripheralWithAdvertisingInfoAction;
    public Action<string, string> RetrievedConnectedPeripheralAction;
    public Action<string> ConnectedPeripheralAction;
    public Action<string> ConnectedDisconnectPeripheralAction;
    public Action<string> DisconnectedPeripheralAction;
    public Action<string, string> DiscoveredServiceAction;
    public Action<string, string, string> DiscoveredCharacteristicAction;
    public Action<string> DidWriteCharacteristicAction;
    public Dictionary<string, Dictionary<string, Action<string>>> DidUpdateNotificationStateForCharacteristicAction;
    public Dictionary<string, Dictionary<string, Action<string, string>>> DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction;
    public Dictionary<string, Dictionary<string, Action<string, byte[]>>> DidUpdateCharacteristicValueAction;
    public Dictionary<string, Dictionary<string, Action<string, string, byte[]>>> DidUpdateCharacteristicValueWithDeviceAddressAction;
    public Action<string, int> RequestMtuAction;

    // Use this for initialization
    void Start()
    {
        DiscoveredDeviceList = new List<string>();
        DidUpdateNotificationStateForCharacteristicAction = new Dictionary<string, Dictionary<string, Action<string>>>();
        DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction = new Dictionary<string, Dictionary<string, Action<string, string>>>();
        DidUpdateCharacteristicValueAction = new Dictionary<string, Dictionary<string, Action<string, byte[]>>>();
        DidUpdateCharacteristicValueWithDeviceAddressAction = new Dictionary<string, Dictionary<string, Action<string, string, byte[]>>>();

    }

    // Update is called once per frame
    void Update()
    {
#if ENABLE_WINMD_SUPPORT || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (MessagesToProcess != null)
        {
            lock (MessagesToProcess)
            {
                if (MessagesToProcess.Count > 0)
                    logger.Verbose("{0} bluetooth messages to process with dt {1}", MessagesToProcess.Count, Time.deltaTime);
                while (MessagesToProcess.Count > 0)
                {
                    OnBluetoothMessage(MessagesToProcess.Dequeue());
                }
            }
        }
#endif
    }

    const string deviceInitializedString = "Initialized";
    const string deviceDeInitializedString = "DeInitialized";
    const string deviceErrorString = "Error";
    const string deviceDiscoveredPeripheral = "DiscoveredPeripheral";
    const string deviceRetrievedConnectedPeripheral = "RetrievedConnectedPeripheral";
    const string deviceConnectedPeripheral = "ConnectedPeripheral";
    const string deviceDisconnectedPeripheral = "DisconnectedPeripheral";
    const string deviceDiscoveredService = "DiscoveredService";
    const string deviceDiscoveredCharacteristic = "DiscoveredCharacteristic";
    const string deviceDidWriteCharacteristic = "DidWriteCharacteristic";
    const string deviceDidUpdateNotificationStateForCharacteristic = "DidUpdateNotificationStateForCharacteristic";
    const string deviceDidUpdateValueForCharacteristic = "DidUpdateValueForCharacteristic";
    const string deviceLog = "Log";
    const string deviceRequestMtu = "MtuChanged";

    public void OnBluetoothMessage(string message)
    {
        if (message != null)
        {
            char[] delim = new char[] { '~' };
            string[] parts = message.Split(delim);

            if (logger.IsEnabledFor(Psix.LogLevel.Verbose))
            {

                string log = "";
                for (int i = 0; i < parts.Length; ++i)
                    log += string.Format("| {0}", parts[i]);
                logger.Verbose(log);
            }

            Func<string, bool> MessageStartsWith = (str) => message.Length >= str.Length && message.Substring(0, str.Length) == str;

            if (MessageStartsWith(deviceInitializedString))
            {
                if (InitializedAction != null)
                    InitializedAction();
            }
            else if (MessageStartsWith(deviceLog))
            {
                logger.Debug(parts[1]);
            }
            else if (MessageStartsWith(deviceDeInitializedString))
            {
                BLE.DestroyReceiver();

                if (DeinitializedAction != null)
                    DeinitializedAction();
            }
            else if (MessageStartsWith(deviceErrorString))
            {
                string error = "";

                if (parts.Length >= 2)
                    error = parts[1];

                if (ErrorAction != null)
                    ErrorAction(error);
            }
            else if (MessageStartsWith(deviceDiscoveredPeripheral))
            {
                if (parts.Length >= 3)
                {
                    // the first callback will only get called the first time this device is seen
                    // this is because it gets added to the a list in the DiscoveredDeviceList
                    // after that only the second callback will get called and only if there is
                    // advertising data available
                    if (!DiscoveredDeviceList.Contains(parts[1] + "|" + parts[2]))
                    {
                        DiscoveredDeviceList.Add(parts[1] + "|" + parts[2]);

                        if (DiscoveredPeripheralAction != null)
                            DiscoveredPeripheralAction(parts[1], parts[2]);
                    }

                    if (parts.Length >= 5 && DiscoveredPeripheralWithAdvertisingInfoAction != null)
                    {
                        // get the rssi from the 4th value
                        int rssi = 0;
                        if (!int.TryParse(parts[3], out rssi))
                            rssi = 0;

                        // parse the base 64 encoded data that is the 5th value
                        byte[] bytes = System.Convert.FromBase64String(parts[4]);

                        DiscoveredPeripheralWithAdvertisingInfoAction(parts[1], parts[2], rssi, bytes);
                    }
                }
            }
            else if (MessageStartsWith(deviceRetrievedConnectedPeripheral))
            {
                if (parts.Length >= 3)
                {
                    DiscoveredDeviceList.Add(parts[1]);

                    if (RetrievedConnectedPeripheralAction != null)
                        RetrievedConnectedPeripheralAction(parts[1], parts[2]);
                }
            }
            else if (MessageStartsWith(deviceConnectedPeripheral))
            {
                if (parts.Length >= 2 && ConnectedPeripheralAction != null)
                    ConnectedPeripheralAction(parts[1]);
            }
            else if (MessageStartsWith(deviceDisconnectedPeripheral))
            {
                if (parts.Length >= 2)
                {
                    if (ConnectedDisconnectPeripheralAction != null)
                        ConnectedDisconnectPeripheralAction(parts[1]);

                    if (DisconnectedPeripheralAction != null)
                        DisconnectedPeripheralAction(parts[1]);
                }
            }
            else if (MessageStartsWith(deviceDiscoveredService))
            {
                if (parts.Length >= 3 && DiscoveredServiceAction != null)
                    DiscoveredServiceAction(parts[1], parts[2]);
            }
            else if (MessageStartsWith(deviceDiscoveredCharacteristic))
            {
                if (parts.Length >= 4 && DiscoveredCharacteristicAction != null)
                    DiscoveredCharacteristicAction(parts[1], parts[2], parts[3]);
            }
            else if (MessageStartsWith(deviceDidWriteCharacteristic))
            {
                if (parts.Length >= 2 && DidWriteCharacteristicAction != null)
                    DidWriteCharacteristicAction(parts[1]);
            }
            else if (MessageStartsWith(deviceDidUpdateNotificationStateForCharacteristic))
            {
                if (parts.Length >= 3)
                {
                    if (DidUpdateNotificationStateForCharacteristicAction != null && DidUpdateNotificationStateForCharacteristicAction.ContainsKey(parts[1]))
                    {
                        var characteristicAction = DidUpdateNotificationStateForCharacteristicAction[parts[1]];
                        if (characteristicAction != null && characteristicAction.ContainsKey(parts[2]))
                        {
                            var action = characteristicAction[parts[2]];
                            if (action != null)
                                action(parts[2]);
                        }
                    }

                    if (DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction != null && DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey(parts[1]))
                    {
                        var characteristicAction = DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[parts[1]];
                        if (characteristicAction != null && characteristicAction.ContainsKey(parts[2]))
                        {
                            var action = characteristicAction[parts[2]];
                            if (action != null)
                                action(parts[1], parts[2]);
                        }
                    }
                }
            }
            else if (MessageStartsWith(deviceDidUpdateValueForCharacteristic))
            {
                if (parts.Length >= 4)
                    OnBluetoothData(parts[1], parts[2], parts[3]);
            }
            else if (MessageStartsWith(deviceRequestMtu))
            {
                if (parts.Length >= 3)
                {
                    if (RequestMtuAction != null)
                    {
                        int mtu = 0;
                        if (int.TryParse(parts[2], out mtu))
                            RequestMtuAction(parts[1], mtu);
                    }
                }
            }
        }
    }

    public void OnBluetoothData(string base64Data)
    {
        OnBluetoothData("", "", base64Data);
    }

    public void OnBluetoothData(string deviceAddress, string characteristic, string base64Data)
    {
        if (base64Data != null)
        {
            byte[] bytes = System.Convert.FromBase64String(base64Data);
            if (bytes.Length > 0)
            {
                //deviceAddress = deviceAddress.ToUpper ();
                //characteristic = characteristic.ToUpper ();

                if (logger.IsEnabledFor(Psix.LogLevel.Verbose))
                {
                    logger.Verbose("Device: " + deviceAddress + " Characteristic Received: " + characteristic);
                    string byteString = "";
                    foreach (byte b in bytes)
                        byteString += string.Format("{0:X2}", b);
                    logger.Verbose(byteString);
                }

                if (DidUpdateCharacteristicValueAction != null && DidUpdateCharacteristicValueAction.ContainsKey(deviceAddress))
                {
                    var characteristicAction = DidUpdateCharacteristicValueAction[deviceAddress];
#if UNITY_ANDROID
                    characteristic = characteristic.ToLower();
#endif
                    if (characteristicAction != null && characteristicAction.ContainsKey(characteristic))
                    {
                        var action = characteristicAction[characteristic];
                        if (action != null)
                            action(characteristic, bytes);
                    }
                }

                if (DidUpdateCharacteristicValueWithDeviceAddressAction != null && DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey(deviceAddress))
                {
                    var characteristicAction = DidUpdateCharacteristicValueWithDeviceAddressAction[deviceAddress];
#if UNITY_ANDROID
                    characteristic = characteristic.ToLower();
#endif
                    if (characteristicAction != null && characteristicAction.ContainsKey(characteristic))
                    {
                        var action = characteristicAction[characteristic];
                        if (action != null)
                            action(deviceAddress, characteristic, bytes);
                    }
                }
            }
        }
    }

    public void OnApplicationQuit()
    {
        if (Application.isEditor)
        {
            BLE.DeInitialize(() =>
            {
                logger.Debug("Deinitialize complete");
            });
        }
    }
}
