// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

// use this to allow the editor to see WINMD code for syntax checking
//#define ENABLE_WINMD_SUPPORT

#if ENABLE_WINMD_SUPPORT
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //private BluetoothLEPreferredConnectionParameters _connectionPriorityParameter = BluetoothLEPreferredConnectionParameters.Balanced;

    public void ConnectionPriority(Gatt.ConnectionPriority connectionPriority)
    {
        switch (connectionPriority)
        {
            case Gatt.ConnectionPriority.Balanced:
                //_connectionPriorityParameter = BluetoothLEPreferredConnectionParameters.Balanced;
                break;

            case Gatt.ConnectionPriority.High:
                //_connectionPriorityParameter = BluetoothLEPreferredConnectionParameters.ThroughPutOptimized;
                break;

            case Gatt.ConnectionPriority.LowPower:
                //_connectionPriorityParameter = BluetoothLEPreferredConnectionParameters.PowerOptimized;
                break;
        }
    }

    public void Initialize(BluetoothDeviceScript bluetoothDeviceScript, bool asCentral, bool asPeripheral)
    {
        _bluetoothDeviceScript = bluetoothDeviceScript;
        if (_bluetoothDeviceScript != null && _bluetoothDeviceScript.MessagesToProcess == null)
        {
            _bluetoothDeviceScript.MessagesToProcess = new Queue<string>();
            lock(_bluetoothDeviceScript.MessagesToProcess)
            {
                _bluetoothDeviceScript.MessagesToProcess.Enqueue("Initialized");
            }
        }
    }

    private BluetoothLEAdvertisementWatcher _deviceWatcher = null;

    public void ScanForPeripheralsWithServices(string[] serviceUUIDs, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF)
    {
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
    }

        private async void On_Received(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs args)
        {
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

    public void StopScan()
    {
        if (_deviceWatcher != null && _deviceWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started)
        {
            _deviceWatcher.Stop();
            _deviceWatcher = null;
            Log("Scanning Stopped");
        }
        else
            Log("DeviceWatcher null or not started when trying to stop it");
    }

    // Async removed: No awaits
    async public void ConnectToPeripheral(string id)
    {
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
    }

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

    public void DisconnectPeripheral(string id)
    {
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
    }

    // Async removed: no awaits
    async public void RequestMTU(string id)
    {
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
    }

    private List<GattCharacteristic> _dontAllowGCCharacteristicList;

    // Async removed: No awaits
    async public void SubscribeCharacteristicWithDeviceAddress(string id, string serviceUuid, string characteristicUuid)
    {
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
    }

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

    // Async removed: no awaits
    async public void UnSubscribeCharacteristic(string id, string serviceUuid, string characteristicUuid)
    {
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
    }

    // Async removed: no awaits
    async public void WriteCharacteristic(string id, string serviceUuid, string characteristicUuid, byte[] data, int length, bool withResponse)
    {
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
    }
}
#endif