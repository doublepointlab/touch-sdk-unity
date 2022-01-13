#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

using UnityEngine;

using BLE = BluetoothLEHardwareInterface;
using Timer = System.Timers.Timer;

namespace Psix
{

    class GattClient
    {

        public GattClient(string name = "")
        {
            serverName = name;
        }

        public double ScanTimeout
        {
            get { return ScanTimer.Interval; }
            set { ScanTimer.Interval = value; }
        }

        public void SubscribeToCharacteristic(
           string serviceUuid,
           string characteristicUuid,
           Action<byte[]> callback)
        {
            requiredServices.Add(serviceUuid);
            subscriptions.Enqueue((serviceUuid, characteristicUuid, callback));
        }


        public void Connect(Action? onConnected = null, Action? onDisconnected = null, Action? onTimeout = null)
        {
            connectAction = onConnected;
            disconnectAction = onDisconnected;
            timeoutAction = onTimeout;
            bluetoothThread = new Thread(new ThreadStart(Initiate));
            bluetoothThread?.Start();
        }

        public void Disconnect()
        {
            if (serverAddress != "")
            {
                BLE.DisconnectPeripheral(serverAddress, null);
            }
            Cleanup();
        }

        public void SendBytes(byte[] data, string serviceUuid, string characteristicUuid)
        {
            BLE.WriteCharacteristic(
                serverAddress, serviceUuid, characteristicUuid,
                data, data.Length, false, (characteristicUUID) =>
            {
                Debug.Log("Write Succeeded");
            });
        }

        private void Initiate()
        {
            AndroidJNI.AttachCurrentThread(); // Some BLE calls require AndroidJNI

            BLE.BluetoothConnectionPriority(BLE.ConnectionPriority.High);

            BLE.BluetoothScanMode(BLE.ScanMode.LowLatency);

            BLE.Initialize(true, false,
                () => { StartScanning(); },
                (error) => { Debug.Log("BLE error: " + error); }
            );
        }

        private void Cleanup()
        {
            AndroidJNI.AttachCurrentThread();
            BLE.StopScan();
            lock (matchLock)
            {
            if (serverAddress == "")
            {
                foreach (string addr in connectedDevices)
                {
                    BLE.DisconnectPeripheral(addr, null);
                }
                BLE.DeInitialize(() => { Connect(); });
                timeoutAction?.Invoke();
            }
            }

            requiredServices.Clear();
            subscriptions.Clear();
            deviceToDiscoveredServices.Clear();
        }

        private void StartScanning()
        {

            ScanTimer.Elapsed += (s, e) => { Cleanup(); };
            ScanTimer.AutoReset = false; // scanning is stopped only once
            ScanTimer.Start();

            BLE.ScanForPeripheralsWithServices(
                null,
                (address, name) => { ProcessScanResult(address, name); }
            );
        }

        private void ProcessScanResult(string address, string name)
        {
            if (name != "No Name" && name.Contains(serverName) && serverAddress == "")
            {
                try
                {
                    deviceToDiscoveredServices.Add(address, new HashSet<string>());
                } catch (ArgumentException) {}

                lock (connectionLock) {

                    connectedDevices.Add(address);

                    Debug.Log($"connecting to {name} ({address})");
                    BLE.ConnectToPeripheral(
                        address, (addr) => {Debug.Log($"connected to {addr}"); },
                        (addr, service) =>
                        {
                            Debug.Log($"discover service {service} ({addr})");
                            deviceToDiscoveredServices[addr].Add(service);
                            if (requiredServices.All((service) => {
                                return deviceToDiscoveredServices[addr].Contains(service); }))
                            {
                                ProcessDeviceMatch(addr);
                            }
                        }, (addr, service, characteristic) => {
                            Debug.Log($"discover characteristic {characteristic} ({addr})");
                        }, (addr) =>
                        {
                            Debug.Log($"disconnect {addr}");
                            if (addr == serverAddress) disconnectAction?.Invoke();
                            connectedDevices.Remove(addr);
                        }
                    );
                }
            }
        }

        private void ProcessDeviceMatch(string address)
        {
            lock (matchLock)
            {
                if (serverAddress == "")
                {
                    Debug.Log($"MATCH: {address}");
                    serverAddress = address;
                    foreach (string addr in connectedDevices)
                    {
                        if (addr != serverAddress)
                        {
                            BLE.DisconnectPeripheral(addr, null);
                        }
                    }
                    BLE.StopScan();
                    Subscribe();
                }
            }
        }

        private void Subscribe()
        {
            (string service, string characteristic, Action<byte[]> callback) sub;
            try
            {
                sub = subscriptions.Dequeue();
            } catch (InvalidOperationException)
            {
                connectAction?.Invoke();
                return;
            }
            BLE.SubscribeCharacteristicWithDeviceAddress(
                serverAddress, sub.service, sub.characteristic, (addr, characteristic) =>
                {
                    Subscribe();
                }, (addr, characteristic, bytes) =>
                {
                    sub.callback(bytes);
                }
            );

        }

        private string serverName = "";
        private string serverAddress = "";

        private Thread? bluetoothThread = null;

        private Action? connectAction = null;
        private Action? disconnectAction = null;
        private Action? timeoutAction = null;

        private readonly object connectionLock = new object();
        private readonly object matchLock = new object();

        private Timer ScanTimer = new Timer(10000); // Handles scan timeout

        private Queue<(string service,
            string characteristic,
            Action<byte[]> callback)> subscriptions
            = new Queue<(string service, string characteristic, Action<byte[]> callback)>();

        private HashSet<string> connectedDevices = new HashSet<string>();
        private HashSet<string> requiredServices = new HashSet<string>();

        private Dictionary<string, HashSet<string>> deviceToDiscoveredServices
            = new Dictionary<string, HashSet<string>>();

    }
}
