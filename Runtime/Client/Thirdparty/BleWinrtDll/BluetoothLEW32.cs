// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using BleApi = BleWinrt;
public class BluetoothLEW32
{
    static private BluetoothLEW32 _instance;
    static public BluetoothLEW32 Instance
    {
        get
        {
            if (_instance == null)
                _instance = new BluetoothLEW32();

            return _instance;
        }
    }

    private BluetoothDeviceScript _bluetoothDeviceScript;

    private List<string> SubscribedUUIDs = new List<string>();

    private bool isScanningDevices = false;

    string lastError;

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

            _bluetoothDeviceScript.StartCoroutine(PollDevice());
        }
    }

    IEnumerator PollDevice()
    {
        while (true)
        {
            BleApi.ScanStatus status;
            // Devices
            if(isScanningDevices)
            {
                BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
                do
                {
                    status = BleApi.PollDevice(ref res, false);
                    if (status == BleApi.ScanStatus.AVAILABLE)
                    {
                        lock(_bluetoothDeviceScript.MessagesToProcess)
                        {
                            // TODO Device name isn't coming through for some reason, probably c++ land.
                            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DiscoveredPeripheral~{res.id}~{""}~{0}~{Convert.ToBase64String(new byte[0])}");
                        }
                    }
                } while (status == BleApi.ScanStatus.AVAILABLE);
            }

            // Data
            if(SubscribedUUIDs.Count > 0)
            {
                BleApi.BLEData res = new BleApi.BLEData();
                while (BleApi.PollData(out res, false))
                {
                    string characteristic = res.characteristicUuid.Trim('{').Trim('}');
                    if(!SubscribedUUIDs.Contains(characteristic))
                    {
                        continue;
                    }
                    string data = Convert.ToBase64String(res.buf, 0, res.size);

                    lock (_bluetoothDeviceScript.MessagesToProcess)
                    {
                        _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidUpdateValueForCharacteristic~{res.deviceId}~{characteristic}~{data}");
                    }
                    // 1ms delay
                    yield return new WaitForSeconds(0.0001f);
                }
            }

            // log potential errors
            BleApi.ErrorMessage Errorres = new BleApi.ErrorMessage();
            BleApi.GetError(out Errorres);
            if (lastError != Errorres.msg)
            {
                if(Errorres.msg != "Ok")
                    BluetoothLEHardwareInterface.Log(Errorres.msg);
                lastError = Errorres.msg;
            }
            yield return null;
        }
    }

    public void ScanForPeripheralsWithServices(string[] serviceUUIDs, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF)
    {
        BleApi.StartDeviceScan(serviceUUIDs);
        isScanningDevices = true;
    }

    public void StopScan()
    {
        BleApi.StopDeviceScan();
        isScanningDevices = false;
    }

    public async void ConnectToPeripheral(string id)
    {
        BleApi.ScanServices(id);
        await Task.Yield();
        lock (_bluetoothDeviceScript.MessagesToProcess)
        {
            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"ConnectedPeripheral~{id}");
        }
        BleApi.Service service = new BleApi.Service();
        while(BleApi.PollService(out service, false) != BleApi.ScanStatus.FINISHED)
        {
            lock (_bluetoothDeviceScript.MessagesToProcess)
            {
                _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DiscoveredService~{id}~{service.uuid.Trim('{').Trim('}')}");
            }
            await Task.Yield();

            BleApi.Characteristic characteristic = new BleApi.Characteristic();
            BleApi.ScanCharacteristics(id, service.uuid);
            while(BleApi.PollCharacteristic(out characteristic, false) != BleApi.ScanStatus.FINISHED)
            {
                lock (_bluetoothDeviceScript.MessagesToProcess)
                {
                    _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DiscoveredCharacteristics~{id}~{service.uuid.Trim('{').Trim('}')}~{characteristic.uuid.Trim('{').Trim('}')}");
                }
                await Task.Yield();
            }
        }

    }

    public void DisconnectPeripheral(string id)
    {
        BleApi.Disconnect(id);

        lock (_bluetoothDeviceScript.MessagesToProcess)
        {
            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DisconnectedPeripheral~{id}");
        }
    }

    public void SubscribeCharacteristicWithDeviceAddress(string id, string serviceUuid, string characteristicUuid)
    {
        string service = "{" + serviceUuid + "}";
        string characteristic = "{" + characteristicUuid + "}";
        if(!SubscribedUUIDs.Contains(characteristicUuid))
        {
            BleApi.SubscribeCharacteristic(id, service, characteristic, false);
            SubscribedUUIDs.Add(characteristicUuid);
        }
        lock (_bluetoothDeviceScript.MessagesToProcess)
        {
            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidUpdateNotificationStateForCharacteristic~{id}~{characteristicUuid}");
        }
    }

    public void UnSubscribeCharacteristic(string id, string serviceUuid, string characteristicUuid)
    {
        SubscribedUUIDs.Remove(characteristicUuid);
        lock (_bluetoothDeviceScript.MessagesToProcess)
        {
            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidUpdateNotificationStateForCharacteristic~{id}~{characteristicUuid}");
        }
    }

    public void WriteCharacteristic(string id, string serviceUuid, string characteristicUuid, byte[] data, int length, bool withResponse)
    {
        string service = "{" + serviceUuid + "}";
        string characteristic = "{" + characteristicUuid + "}";

        BleApi.BLEData dataPacket = new BleApi.BLEData();
        dataPacket.buf = new byte[512];
        dataPacket.size = (short)data.Length;
        dataPacket.deviceId = id;
        dataPacket.serviceUuid = service;
        dataPacket.characteristicUuid = characteristic;
        for (int i = 0; i < length; i++)
            dataPacket.buf[i] = data[i];
        BleApi.SendData(in dataPacket, false);

        lock (_bluetoothDeviceScript.MessagesToProcess)
        {
            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidWriteCharacteristic~{characteristicUuid}");
        }
    }
}
