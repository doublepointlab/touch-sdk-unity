using System;
using UnityEngine;


abstract public class Gatt
{

    public static BluetoothDeviceScript bluetoothDeviceScript;

    public static void InitGameObject(Action action, Action<string> errorAction)
    {
        bluetoothDeviceScript = null;

        GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
        if (bluetoothLEReceiver == null)
            bluetoothLEReceiver = new GameObject("BluetoothLEReceiver");

        if (bluetoothLEReceiver != null)
        {
            bluetoothDeviceScript = bluetoothLEReceiver.GetComponent<BluetoothDeviceScript>();
            if (bluetoothDeviceScript == null)
                bluetoothDeviceScript = bluetoothLEReceiver.AddComponent<BluetoothDeviceScript>();

            if (bluetoothDeviceScript != null)
            {
                bluetoothDeviceScript.InitializedAction = action;
                bluetoothDeviceScript.ErrorAction = errorAction;
            }
        }

        GameObject.DontDestroyOnLoad(bluetoothLEReceiver);
    }

    public void DestroyReceiver()
    {
        GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
        if (bluetoothLEReceiver != null)
            GameObject.Destroy(bluetoothLEReceiver);
    }

    public static string FullUUID(string uuid)
    {
        if (uuid.Length == 4)
            return "0000" + uuid + "-0000-1000-8000-00805F9B34FB";
        return uuid;
    }

    public enum CharacteristicProperties
    {
        Broadcast = 0x01,
        Read = 0x02,
        WriteWithoutResponse = 0x04,
        Write = 0x08,
        Notify = 0x10,
        Indicate = 0x20,
        AuthenticatedSignedWrites = 0x40,
        ExtendedProperties = 0x80,
        NotifyEncryptionRequired = 0x100,
        IndicateEncryptionRequired = 0x200,
    };

    public enum CharacteristicPermissions
    {
#if UNITY_ANDROID
        Readable = 0x01,
        Writeable = 0x10,
        ReadEncryptionRequired = 0x02,
        WriteEncryptionRequired = 0x20,
#else
        Readable = 0x01,
        Writeable = 0x02,
        ReadEncryptionRequired = 0x04,
        WriteEncryptionRequired = 0x08,
#endif
    };

    public enum ScanMode
    {
        LowPower = 0,
        Balanced = 1,
        LowLatency = 2
    }

    public enum ConnectionPriority
    {
        LowPower = 0,
        Balanced = 1,
        High = 2,
    }

    public enum iOSProximity
    {
        Unknown = 0,
        Immediate = 1,
        Near = 2,
        Far = 3,
    }

    abstract public void Log(string message);
    abstract public BluetoothDeviceScript Initialize(bool asCentral, bool asPeripheral, Action action, Action<string> errorAction);
    abstract public void DeInitialize(Action action);
    abstract public void BluetoothEnable(bool enable);
    abstract public void BluetoothScanMode(ScanMode scanMode);
    abstract public void BluetoothConnectionPriority(ConnectionPriority connectionPriority);
    abstract public void PauseMessages(bool isPaused);
    abstract public void RequestMtu(string name, int mtu, Action<string, int> action);
    abstract public void ScanForPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> action, Action<string, string, int, byte[]> actionAdvertisingInfo = null, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF);
    abstract public void RetrieveListOfPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> action);
    abstract public void StopScan();
    abstract public void DisconnectAll();
    abstract public void ConnectToPeripheral(string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> disconnectAction = null);
    abstract public void DisconnectPeripheral(string name, Action<string> action);
    abstract public void ReadCharacteristic(string name, string service, string characteristic, Action<string, byte[]> action);
    abstract public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action);
    abstract public void SubscribeCharacteristic(string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action);
    abstract public void SubscribeCharacteristicWithDeviceAddress(string name, string service, string characteristic, Action<string, string> notificationAction, Action<string, string, byte[]> action);
    abstract public void UnSubscribeCharacteristic(string name, string service, string characteristic, Action<string> action);
}

// Thread-safe singleton pattern from
// https://learn.microsoft.com/en-us/previous-versions/msp-n-p/ff650316(v=pandp.10)
public sealed class GattImpl
{
    private static volatile Gatt instance;
    private static object syncRoot = new object();

    private GattImpl() { }

    public static Gatt Instance
    {
        get
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
#if UNITY_EDITOR

#if UNITY_EDITOR_WIN
                        instance = new WinGatt();
#elif EXPERIMENTAL_MACOS_EDITOR && UNITY_EDITOR_OSX
                        instance = new OsxGatt();
#else
#warning Unsupported editor platform
                        instance = new DummyGatt();
#endif

#else
#if UNITY_ANDROID
                        instance = new AndroidGatt();
#elif UNITY_STANDALONE_WIN
                        instance = new WinGatt();
#elif ENABLE_WINMD_SUPPORT
                        instance = new UwpGatt();
#elif UNITY_IOS || UNITY_TVOS
                        instance = new IosGatt();
#elif EXPERIMENTAL_MACOS_EDITOR && UNITY_STANDALONE_OSX
                        instance = new OsxGatt();
#else
#error Unsupported player platform
#endif
#endif
                    }
                }
            }

            return instance;
        }
    }

}

