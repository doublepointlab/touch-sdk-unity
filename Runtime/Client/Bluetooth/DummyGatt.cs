using System;

public class DummyGatt : Gatt
{

    override public void Log(string message) {}
    override public BluetoothDeviceScript Initialize(bool asCentral, bool asPeripheral, Action action, Action<string> errorAction) { return bluetoothDeviceScript; }
    override public void DeInitialize(Action action) {}
    override public void BluetoothEnable(bool enable) {}
    override public void BluetoothScanMode(ScanMode scanMode) {}
    override public void BluetoothConnectionPriority(ConnectionPriority connectionPriority) {}
    override public void BluetoothAdvertisingMode(AdvertisingMode advertisingMode) {}
    override public void BluetoothAdvertisingPower(AdvertisingPower advertisingPower) {}
    override public void PauseMessages(bool isPaused) {}
    override public void ScanForBeacons(string[] proximityUUIDs, Action<iBeaconData> actionBeaconResponse) {}
    override public void RequestMtu(string name, int mtu, Action<string, int> action) {}
    override public void ScanForPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> action, Action<string, string, int, byte[]> actionAdvertisingInfo = null, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF) {}
    override public void RetrieveListOfPeripheralsWithServices(string[] serviceUUIDs, Action<string, string> action) {}
    override public void StopScan() {}
    override public void StopBeaconScan() {}
    override public void DisconnectAll() {}
    override public void ConnectToPeripheral(string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> disconnectAction = null) {}
    override public void DisconnectPeripheral(string name, Action<string> action) {}
    override public void ReadCharacteristic(string name, string service, string characteristic, Action<string, byte[]> action) {}
    override public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action) {}
    override public void SubscribeCharacteristic(string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action) {}
    override public void SubscribeCharacteristicWithDeviceAddress(string name, string service, string characteristic, Action<string, string> notificationAction, Action<string, string, byte[]> action) {}
    override public void UnSubscribeCharacteristic(string name, string service, string characteristic, Action<string> action) {}
    override public void PeripheralName(string newName) {}
    override public void CreateService(string uuid, bool primary, Action<string> action) {}
    override public void CreateCharacteristic(string uuid, CharacteristicProperties properties, CharacteristicPermissions permissions, byte[] data, int length, Action<string, byte[]> action) {}
    override public void RemoveService(string uuid) {}
    override public void RemoveServices() {}
    override public void RemoveCharacteristic(string uuid) {}
    override public void RemoveCharacteristics() {}
    override public void StartAdvertising(Action action, bool isConnectable = true, bool includeName = true, int manufacturerId = 0, byte[] manufacturerSpecificData = null) {}
    override public void StopAdvertising(Action action) {}
    override public void UpdateCharacteristicValue(string uuid, byte[] data, int length) {}

}
