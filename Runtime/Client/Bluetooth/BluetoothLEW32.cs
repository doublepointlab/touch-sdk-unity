// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Psix;

public class BluetoothLEW32
{
    static PsixLogger logger = new PsixLogger("BluetoothLEW32");

    static PsixLogger libLogger = new PsixLogger("BleWinRT");
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
    private string[] _advertisedServices = new string[0];

    private List<string> SubscribedUUIDs = new List<string>();

    private bool ScanEnabled = false;
    private Queue<string> ToConnect = new Queue<string>();

    string lastError;

    static void LibLog(string message)
    {
        message = message.Replace("{", "");
        message = message.Replace("}", "");
        libLogger.Trace(message);
    }

    bool initialized = false;
    public void Initialize(BluetoothDeviceScript bluetoothDeviceScript, bool asCentral, bool asPeripheral)
    {
        if (initialized)
        {
            logger.Debug("BleW32 duplicate initialize");
            return;
        }
        logger.Debug("BleW32 Initialize");
        initialized = true;
        BleApi.RegisterLogCallback(LibLog);
        { // Clear errors
            BleApi.ErrorMessage Errorres = new BleApi.ErrorMessage();
            BleApi.GetError(out Errorres);
        }
        _bluetoothDeviceScript = bluetoothDeviceScript;
        if (_bluetoothDeviceScript != null && _bluetoothDeviceScript.MessagesToProcess == null)
        {
            _bluetoothDeviceScript.MessagesToProcess = new Queue<string>();
            lock (_bluetoothDeviceScript.MessagesToProcess)
            {
                _bluetoothDeviceScript.MessagesToProcess.Enqueue("Initialized");
            }

            _bluetoothDeviceScript.StartCoroutine(PollDevice());
        }
        else
            logger.Error("BleW32 Initialize failed: invalid device script");
    }

    /* Iterate data from BleApi */
    IEnumerator PollDevice()
    {
        bool wasScanning = false;
        while (true)
        {
            BleApi.ScanStatus status;
            // Devices
            if (ScanEnabled)
            {
                // This loop will not exit until scan is finished!
                // However, scan might be then reinitialized.
                BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
                do
                {
                    status = BleApi.PollDevice(ref res, false);
                    if (status == BleApi.ScanStatus.AVAILABLE)
                    {
                        wasScanning = true;
                        lock (_bluetoothDeviceScript.MessagesToProcess)
                            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DiscoveredPeripheral~{res.id}~{res.name}~{0}~{Convert.ToBase64String(res.advData.Take((int)res.advDataLen).ToArray())}");
                    }
                    else if (status == BleApi.ScanStatus.PROCESSING)
                        wasScanning = true;
                    else if (status == BleApi.ScanStatus.FINISHED)
                    {
                        if (wasScanning)
                            logger.Trace(String.Format("Scan finished. Connecting to {0} devices", ToConnect.Count));
                        wasScanning = false;
                        while (ToConnect.Count > 0 && ScanEnabled)
                            yield return _ConnectToPeripheral();
                        /* Restarting should be used if the library version timeouts the scan.
                        * With advertisement listener scanner will handle restarts! */
                        yield return null;
                        // if (ScanEnabled)
                        // {
                        //     yield return new WaitForSeconds(1);
                        //     logger.Trace("Restarting scanning");
                        //     ScanForPeripherals();
                        // }
                    }
                } while (status == BleApi.ScanStatus.AVAILABLE && ScanEnabled);
            }
            else
            {
                while (ToConnect.Count > 0)
                    yield return _ConnectToPeripheral();
            }

            // Data
            if (SubscribedUUIDs.Count > 0)
            {
                BleApi.BLEData res = new BleApi.BLEData();
                while (BleApi.PollData(out res, false))
                {
                    logger.Verbose("Polled data");
                    string characteristic = res.characteristicUuid.Trim('{').Trim('}');
                    // if (!SubscribedUUIDs.Contains(characteristic))
                    // {
                    //     logger.Verbose("Irrelevant update");
                    //     continue;
                    // }
                    string data = Convert.ToBase64String(res.buf, 0, res.size);

                    lock (_bluetoothDeviceScript.MessagesToProcess)
                    {
                        _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidUpdateValueForCharacteristic~{res.deviceId}~{characteristic}~{data}");
                    }
                }
                yield return null;
            }

            // log potential errors
            BleApi.ErrorMessage Errorres = new BleApi.ErrorMessage();
            BleApi.GetError(out Errorres);
            if (lastError != Errorres.msg)
            {
                if (Errorres.msg != "Ok")
                {
                    if (Errorres.msg != "")
                        logger.Warn(Errorres.msg.Replace("{", "").Replace("}", ""));
                    else
                        logger.Trace("Empty error");
                }
                lastError = Errorres.msg;
            }
            yield return null;
        }
    }

    public void ScanForPeripherals()
    {
        logger.Trace("ScanForPeripherals");
        ScanEnabled = true;
        BleApi.StartDeviceScan(_advertisedServices, _advertisedServices.Length);
    }

    public void ScanForPeripherals(string[] serviceUUIDs)
    {
        _advertisedServices = serviceUUIDs;
        ScanForPeripherals();
    }

    public void ScanForPeripheralsWithServices(string[] serviceUUIDs, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF)
    {
        ScanForPeripherals(serviceUUIDs);
    }

    public void StopScan()
    {
        logger.Trace("StopScan");
        ScanEnabled = false;
        BleApi.StopDeviceScan();
    }

    /* This will disconnect all */
    public void HardResetScanning()
    {
        logger.Debug("BleW32: HardResetScanning");
        BleApi.Quit();
        ScanForPeripherals();
    }

    public Queue<string> DiscoveryMessages = new Queue<string>();
    private void SendMessages()
    {
        lock (_bluetoothDeviceScript.MessagesToProcess)
        {
            foreach (var msg in DiscoveryMessages)
                _bluetoothDeviceScript.MessagesToProcess.Enqueue(msg);
        }
    }
    private async Task _Connect(string id, HashSet<string> knownServices, HashSet<string> knownCharacs)
    {
        await Task.Delay(1000);
        BleApi.ScanServices(id);
        await Task.Yield();
        BleApi.Service service = new BleApi.Service();
        while (BleApi.PollService(out service, false) != BleApi.ScanStatus.FINISHED)
        {
            var serviceUuid = service.uuid.Trim('{').Trim('}');
            await Task.Yield();
            if (!knownServices.Contains(serviceUuid))
            {
                lock (DiscoveryMessages)
                    DiscoveryMessages.Enqueue($"DiscoveredService~{id}~{serviceUuid}");
                knownServices.Add(serviceUuid);
                logger.Trace($"Service discovered: {serviceUuid}");
            }
        }
        await Task.Delay(500);
        foreach (var serviceUuid in knownServices)
        {
            if (serviceUuid == "")
                continue;
            if (!GattServices.RelevantServiceUuids.Contains(serviceUuid))
            {
                logger.Trace($"Ignoring service {serviceUuid}");
                continue;
            }
            logger.Trace($"Scanning service {serviceUuid}");
            BleApi.Characteristic characteristic = new BleApi.Characteristic();
            BleApi.ScanCharacteristics(id, "{" + serviceUuid + "}");
            while (BleApi.PollCharacteristic(out characteristic, false) != BleApi.ScanStatus.FINISHED)
            {
                await Task.Yield();
                var characteristicUuid = characteristic.uuid.Trim('{').Trim('}');
                if (!knownCharacs.Contains(serviceUuid + characteristicUuid))
                {
                    lock (DiscoveryMessages)
                        DiscoveryMessages.Enqueue($"DiscoveredCharacteristics~{id}~{serviceUuid}~{characteristicUuid}");
                    knownCharacs.Add(serviceUuid + characteristicUuid);
                    logger.Trace($"Charac discovered: {characteristicUuid} {characteristic.userDescription}");
                }
            }
        }
    }

    bool _connecting = false; // Prevent simultaneous discoveries
    private async Task _ConnectToPeripheral(int tries = 1)
    {
        if (_connecting)
            return;
        _connecting = true;
        await Task.Delay(500); // Wait because why not
        var id = ToConnect.Dequeue();
        logger.Trace("BleW32: _ConnectToPeripheral " + id);
        lock (_bluetoothDeviceScript.MessagesToProcess)
            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"ConnectedPeripheral~{id}");
        HashSet<string> knownServices = new HashSet<string>();
        HashSet<string> knownCharacs = new HashSet<string>();
        for (int i = 0; i < tries; i++)
        {
            await _Connect(id, knownServices, knownCharacs);
            logger.Trace($"BleW32: At iteration {i + 1}/{tries} have found {knownServices.Count} services and {knownCharacs.Count} characteristics for {id}.");
        }
        await Task.Delay(500);
        SendMessages();
        _connecting = false;
    }

    public void ConnectToPeripheral(string id, int tries = 2)
    {
        logger.Debug("BleW32: ConnectToPeripheral " + id);
        ToConnect.Enqueue(id);
    }

    public void DisconnectPeripheral(string id)
    {
        logger.Debug("BleW32: Disconnecting " + id);
        if (ToConnect.Contains(id))
        {
            var newQue = new Queue<string>();
            while (ToConnect.Count > 0)
            {
                var dev = ToConnect.Dequeue();
                if (dev != id)
                    newQue.Enqueue(id);
            }
            ToConnect = newQue;
        }
        else
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
        string comb = id + serviceUuid + characteristicUuid;
        if (!SubscribedUUIDs.Contains(comb))
        {
            BleApi.SubscribeCharacteristic(id, service, characteristic, false);
            SubscribedUUIDs.Add(comb);
        }
        else
            logger.Warning("Subscription to {0} on {1} exists", characteristicUuid, id);
        lock (_bluetoothDeviceScript.MessagesToProcess)
        {
            _bluetoothDeviceScript.MessagesToProcess.Enqueue($"DidUpdateNotificationStateForCharacteristic~{id}~{characteristicUuid}");
        }
    }

    public void UnSubscribeCharacteristic(string id, string serviceUuid, string characteristicUuid)
    {
        logger.Trace("Unsubscribe characteristic {0}", characteristicUuid);
        string comb = id + serviceUuid + characteristicUuid;
        if (!SubscribedUUIDs.Remove(comb))
            logger.Warning("Characteristic was not subscribed");
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

    public void Deinitialize()
    {
        BleApi.Quit();
    }
}
