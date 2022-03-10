// use this to allow the editor to see WINMD code for syntax checking
//#define ENABLE_WINMD_SUPPORT

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using System.Linq;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
#endif

public class BluetoothLEUWP
{
    static private BluetoothLEUWP _instance;
    static public BluetoothLEUWP Instance
    {
        get
        {
            if (_instance == null)
                _instance = new BluetoothLEUWP();

            return _instance;
        }
    }

    private BluetoothDeviceScript _bluetoothDeviceScript;

    public void Log(string message)
    {
        Debug.Log(message);
    }

#if ENABLE_WINMD_SUPPORT
    //private BluetoothLEPreferredConnectionParameters _connectionPriorityParameter = BluetoothLEPreferredConnectionParameters.Balanced;
#endif

    public void ConnectionPriority(BluetoothLEHardwareInterface.ConnectionPriority connectionPriority)
    {
        switch (connectionPriority)
        {
            case BluetoothLEHardwareInterface.ConnectionPriority.Balanced:
#if ENABLE_WINMD_SUPPORT
                //_connectionPriorityParameter = BluetoothLEPreferredConnectionParameters.Balanced;
#endif
                break;

            case BluetoothLEHardwareInterface.ConnectionPriority.High:
#if ENABLE_WINMD_SUPPORT
                //_connectionPriorityParameter = BluetoothLEPreferredConnectionParameters.ThroughPutOptimized;
#endif
                break;

            case BluetoothLEHardwareInterface.ConnectionPriority.LowPower:
#if ENABLE_WINMD_SUPPORT
                //_connectionPriorityParameter = BluetoothLEPreferredConnectionParameters.PowerOptimized;
#endif
                break;
        }
    }

    public void Initialize(BluetoothDeviceScript bluetoothDeviceScript, bool asCentral, bool asPeripheral)
    {
#if ENABLE_WINMD_SUPPORT
        _bluetoothDeviceScript = bluetoothDeviceScript;
        if (_bluetoothDeviceScript != null && _bluetoothDeviceScript.MessagesToProcess == null)
        {
            _bluetoothDeviceScript.MessagesToProcess = new Queue<string>();
            lock(_bluetoothDeviceScript.MessagesToProcess)
            {
                _bluetoothDeviceScript.MessagesToProcess.Enqueue("Initialized");
            }
        }
#endif
    }

#if ENABLE_WINMD_SUPPORT
    private BluetoothLEAdvertisementWatcher _deviceWatcher = null;
#endif

    public void ScanForPeripheralsWithServices(string[] serviceUUIDs, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF)
    {
#if ENABLE_WINMD_SUPPORT
        _deviceWatcher = new BluetoothLEAdvertisementWatcher();
        _deviceWatcher.AllowExtendedAdvertisements = true;
        _deviceWatcher.ScanningMode = BluetoothLEScanningMode.Active;
        if (serviceUUIDs != null && serviceUUIDs.Length > 0)
        {
            foreach (string id in serviceUUIDs)
                _deviceWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(Guid.Parse(id));
        }

        _deviceWatcher.Received += On_Received;
        _deviceWatcher.Start();
#endif
    }

#if ENABLE_WINMD_SUPPORT
        private async void On_Received(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            Log("On_Received");

            var dev = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);

            if (dev != null)
            {
                var manufacturerBytes = new byte[1];
                foreach (var manufacturerData in args.Advertisement.ManufacturerData)
                {
                    var data = new byte[manufacturerData.Data.Length];
                    using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                    {
                        reader.ReadBytes(data);
                        manufacturerBytes = data;
                        break;
                    }
                }

                lock(_bluetoothDeviceScript.MessagesToProcess)
                {
                    _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DiscoveredPeripheral~{dev.DeviceInformation.Id}~{args.Advertisement.LocalName}~{args.RawSignalStrengthInDBm}~{Convert.ToBase64String(manufacturerBytes)}");
                }
            }
        }
#endif

#if ENABLE_WINMD_SUPPORT
    private Dictionary<string, BluetoothLEDevice> _bluetoothDeviceDictionary;

    private BluetoothLEDevice GetAddDevice(string id, BluetoothLEDevice device = null)
    {
        id = id.ToUpper();

        if (_bluetoothDeviceDictionary == null)
            _bluetoothDeviceDictionary = new Dictionary<string, BluetoothLEDevice>();

        if (!_bluetoothDeviceDictionary.ContainsKey(id) && device != null)
        {
            _bluetoothDeviceDictionary.Add(id, device);
            //device.RequestPreferredConnection(_connectionPriorityParameter);
        }

        if (_bluetoothDeviceDictionary.ContainsKey(id))
            return _bluetoothDeviceDictionary[id];

        return null;
    }
#endif

    public void StopScan()
    {
#if ENABLE_WINMD_SUPPORT
        if (_deviceWatcher != null && _deviceWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
        {
            _deviceWatcher.Stop();
            _deviceWatcher = null;
            Log("Scanning Stopped");
        }
        else
            Log("DeviceWatcher null or not started when trying to stop it");
#endif
    }

    public async void ConnectToPeripheral(string id)
    {
#if ENABLE_WINMD_SUPPORT
        // Check permission of this device.
        DeviceAccessStatus access = DeviceAccessInformation.CreateFromId(id).CurrentStatus;
        if (access == DeviceAccessStatus.DeniedBySystem || access == DeviceAccessStatus.DeniedByUser)
        {
            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"Error~Cannot get device permissions: {id}");
            return;
        }

        BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(id);
        bluetoothLeDevice.ConnectionStatusChanged += OnConnectionStatusChanged;

        lock (_bluetoothDeviceScript.MessagesToProcess)
        {
            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"ConnectedPeripheral~{id}");
        }

        GattDeviceServicesResult serviceResult = await bluetoothLeDevice.GetGattServicesAsync();
        if (serviceResult.Status == GattCommunicationStatus.Success)
        {
            GetAddDevice(id, bluetoothLeDevice);

            var services = serviceResult.Services;
            foreach (var service in services)
            {
                lock (_bluetoothDeviceScript.MessagesToProcess)
                {
                    _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DiscoveredService~{id}~{service.Uuid}");
                }

                GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();
                if (characteristicResult.Status == GattCommunicationStatus.Success)
                {
                    var characteristics = characteristicResult.Characteristics;
                    foreach (var characteristic in characteristics)
                    {
                        lock (_bluetoothDeviceScript.MessagesToProcess)
                        {
                            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DiscoveredCharacteristics~{id}~{characteristic.Service.Uuid}~{characteristic.Uuid}");
                        }
                    }
                }
            }
        }
#endif
    }

#if ENABLE_WINMD_SUPPORT
    private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
    {
        if (_bluetoothDeviceScript != null)
        {
            sender.ConnectionStatusChanged -= OnConnectionStatusChanged;

            lock (_bluetoothDeviceScript.MessagesToProcess)
            {
                _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DisconnectedPeripheral~{sender.DeviceId}");
            }
        }
    }
#endif

    public void DisconnectPeripheral(string id)
    {
#if ENABLE_WINMD_SUPPORT
        var bluetoothLeDevice = GetAddDevice(id);
        if (bluetoothLeDevice != null)
        {
            bluetoothLeDevice.Dispose();
            _bluetoothDeviceDictionary.Remove(id.ToUpper());
            lock (_bluetoothDeviceScript.MessagesToProcess)
            {
                _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DisconnectedPeripheral~{id}");
            }
        }
#endif
    }

    public async void RequestMTU(string id)
    {
#if ENABLE_WINMD_SUPPORT
        var bluetoothLeDevice = GetAddDevice(id);
        if (bluetoothLeDevice != null)
        {
            var session = await GattSession.FromDeviceIdAsync(bluetoothLeDevice.BluetoothDeviceId);
            if (session != null)
            {
                lock (_bluetoothDeviceScript.MessagesToProcess)
                {
                    _bluetoothDeviceScript.MessagesToProcess.Enqueue($"MtuChanged~{id}~{session.MaxPduSize}");
                }
            }
        }
#endif
    }

#if ENABLE_WINMD_SUPPORT
    private List<GattCharacteristic> _dontAllowGCCharacteristicList;
#endif

    public async void SubscribeCharacteristicWithDeviceAddress(string id, string serviceUuid, string characteristicUuid)
    {
#if ENABLE_WINMD_SUPPORT
        var bluetoothLeDevice = GetAddDevice(id);
        if (bluetoothLeDevice != null)
        {
            GattDeviceServicesResult serviceResult = await bluetoothLeDevice.GetGattServicesAsync();
            if (serviceResult.Status == GattCommunicationStatus.Success)
            {
                var services = serviceResult.Services;
                foreach (var service in services)
                {
                    if (serviceUuid.ToUpper().Equals(service.Uuid.ToString().ToUpper()))
                    {
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();
                        if (characteristicResult.Status == GattCommunicationStatus.Success)
                        {
                            var characteristics = characteristicResult.Characteristics;
                            foreach (var characteristic in characteristics)
                            {
                                if (characteristicUuid.ToUpper().Equals(characteristic.Uuid.ToString().ToUpper()))
                                {
                                    characteristic.ValueChanged += Characteristic_ValueChanged;

                                    GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                                    if(status == GattCommunicationStatus.Success)
                                    {
                                        lock (_bluetoothDeviceScript.MessagesToProcess)
                                        {
                                            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidUpdateNotificationStateForCharacteristic~{id}~{characteristicUuid.ToUpper()}");

                                            if (_dontAllowGCCharacteristicList == null)
                                                _dontAllowGCCharacteristicList = new List<GattCharacteristic>();
                                            _dontAllowGCCharacteristicList.Add(characteristic);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
#endif
    }

#if ENABLE_WINMD_SUPPORT
    void Characteristic_ValueChanged(GattCharacteristic characteristic, GattValueChangedEventArgs args)
    {
        var base64String = CryptographicBuffer.EncodeToBase64String(args.CharacteristicValue);
        if (base64String != null && base64String.Length > 0)
        {
            // extract just the device id part we need
            string deviceId = characteristic.Service.DeviceId;
            int endIndex = characteristic.Service.DeviceId.IndexOf("#GATT:");
            if (endIndex >= 0)
            {
                // if because maybe some devices are reported differently
                deviceId = deviceId.Substring(0, endIndex);
            }

            lock (_bluetoothDeviceScript.MessagesToProcess)
            {
                _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidUpdateValueForCharacteristic~{deviceId}~{characteristic.Uuid.ToString().ToUpper()}~{base64String}");
            }
        }
    }
#endif

    public async void UnSubscribeCharacteristic(string id, string serviceUuid, string characteristicUuid)
    {
#if ENABLE_WINMD_SUPPORT
        var bluetoothLeDevice = GetAddDevice(id);
        if (bluetoothLeDevice != null)
        {
            GattDeviceServicesResult serviceResult = await bluetoothLeDevice.GetGattServicesAsync();
            if (serviceResult.Status == GattCommunicationStatus.Success)
            {
                var services = serviceResult.Services;
                foreach (var service in services)
                {
                    if (serviceUuid.ToUpper().Equals(service.Uuid.ToString().ToUpper()))
                    {
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();
                        if (characteristicResult.Status == GattCommunicationStatus.Success)
                        {
                            var characteristics = characteristicResult.Characteristics;
                            foreach (var characteristic in characteristics)
                            {
                                if (characteristicUuid.ToUpper().Equals(characteristic.Uuid.ToString().ToUpper()))
                                {
                                    if (_dontAllowGCCharacteristicList != null)
                                    {
                                        int removeIndex = -1;
                                        for (int i = 0; i < _dontAllowGCCharacteristicList.Count(); i++)
                                        {
                                            if (_dontAllowGCCharacteristicList[i].Uuid.ToString().ToUpper() == characteristic.Uuid.ToString().ToUpper())
                                            {
                                                removeIndex = i;
                                                break;
                                            }
                                        }

                                        if (removeIndex > -1)
                                        {
                                            GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                                            if(status == GattCommunicationStatus.Success)
                                            {
                                                lock (_bluetoothDeviceScript.MessagesToProcess)
                                                {
                                                    _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidUpdateNotificationStateForCharacteristic~{id}~{characteristicUuid.ToUpper()}");
                                                }

                                                Log($"***** char count 1 {_dontAllowGCCharacteristicList.Count()}");
                                                _dontAllowGCCharacteristicList.RemoveAt(removeIndex);
                                                Log($"***** char count 2 {_dontAllowGCCharacteristicList.Count()}");
                                            }
                                            else
                                            {
                                                Log($"Unsubscribe status error: {status}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
#endif
    }

    public async void WriteCharacteristic(string id, string serviceUuid, string characteristicUuid, byte[] data, int length, bool withResponse)
    {
#if ENABLE_WINMD_SUPPORT
        var bluetoothLeDevice = GetAddDevice(id);
        if (bluetoothLeDevice != null)
        {
            GattDeviceServicesResult serviceResult = await bluetoothLeDevice.GetGattServicesAsync();
            if (serviceResult.Status == GattCommunicationStatus.Success)
            {
                var services = serviceResult.Services;
                foreach (var service in services)
                {
                    if (serviceUuid.ToUpper().Equals(service.Uuid.ToString().ToUpper()))
                    {
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();
                        if (characteristicResult.Status == GattCommunicationStatus.Success)
                        {
                            var characteristics = characteristicResult.Characteristics;
                            foreach (var characteristic in characteristics)
                            {
                                if (characteristicUuid.ToUpper().Equals(characteristic.Uuid.ToString().ToUpper()))
                                {
                                    var buffer = data.AsBuffer();
                                    var writeType = withResponse ? GattWriteOption.WriteWithResponse : GattWriteOption.WriteWithoutResponse;
                                    await characteristic.WriteValueAsync(buffer, writeType);

                                    lock (_bluetoothDeviceScript.MessagesToProcess)
                                    {
                                        _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidWriteCharacteristic~{characteristic.Uuid.ToString().ToUpper()}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}
