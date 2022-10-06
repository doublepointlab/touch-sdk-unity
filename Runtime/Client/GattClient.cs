// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

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
    using CharacQueue = Queue<(string service, string characteristic, Action<byte[]> callback)>;
    class GattClient
    {

        private Action? connectAction = null;
        private Action? disconnectAction = null;
        private Action? timeoutAction = null;

        private readonly object connectionLock = new object();
        private readonly object matchLock = new object();

        private Timer ScanTimer = new Timer(1); // Handles scan timeout

        private CharacQueue subscriptions = new CharacQueue();

        private Queue<string> testableDevices = new Queue<string>();
        private static int MAX_CONNECTIONS = 4;
        private HashSet<string> connectedDevices = new HashSet<string>();
        private Dictionary<string, float> testedDevices = new Dictionary<string, float>(); // Prevent connect disconnect spamming
        private HashSet<string> requiredServices = new HashSet<string>();
        private HashSet<string> requiredCharacteristics = new HashSet<string>();
        private List<string> advertisedServices = new List<string>();

        private Dictionary<string, HashSet<string>> deviceToDiscoveredServices =
            new Dictionary<string, HashSet<string>>();

        private Dictionary<string, HashSet<string>> deviceToDiscoveredCharacteristics =
            new Dictionary<string, HashSet<string>>();

        private bool selected = false; // First watch to actually send data gets selected once connected
        private string serverAddress = "";
        private void Select(string addr)
        {
            lock (matchLock)
            {
                if (serverAddress != "")
                    return;
                serverAddress = addr;
                connectAction?.Invoke();
                CleanupScanning();
            }
        }

        public GattClient(
           string advertisedService = "008e74d0-7bb3-4ac5-8baf-e5e372cced76"
        )
        {
            advertisedServices.Add(advertisedService);
        }

        public double ScanTimeout
        {
            get { return ScanTimer.Interval; }
            set { ScanTimer.Interval = value; }
        }

        /* Add a characteristic to the subscription queue. The first characteristic
         * will be used to detect if the device is sending data, so it should likely be
         * an imu characteristic
         */
        public void SubscribeToCharacteristic(
           string serviceUuid,
           string characteristicUuid,
           Action<byte[]> callback,
           bool required = true)
        {
            if (required)
            {
                requiredCharacteristics.Add(characteristicUuid);
                requiredServices.Add(serviceUuid);
            }
            subscriptions.Enqueue((serviceUuid, characteristicUuid, callback));
        }


        public bool ConnectToName(
            string nameSubstring,
            Action? onConnected = null,
            Action? onDisconnected = null,
            Action? onTimeout = null,
            int timeout = 60000
        )
        {
            connectAction = onConnected;
            disconnectAction = onDisconnected;
            timeoutAction = onTimeout;

            BLE.Log($"Timeout set to {timeout}");
            ScanTimeout = timeout;

            InitiateScan(nameSubstring);

            return true;
        }

        public bool ConnectToAddress(
                string addr,
           Action? onConnected = null,
           Action? onDisconnected = null
        )
        {
            connectAction = onConnected;
            disconnectAction = onDisconnected;

            InitiateConnect(addr);

            return true;
        }

        public void Disconnect()
        {
            if (serverAddress != "")
            {
                BLE.DisconnectPeripheral(serverAddress, null);
            }
            CleanupScanning();
        }

        public void SendBytes(byte[] data, string serviceUuid, string characteristicUuid)
        {
            BLE.WriteCharacteristic(
                serverAddress, serviceUuid, characteristicUuid,
                data, data.Length, false, (characteristicUUID) =>
            {
                BLE.Log("Write Succeeded");
            });
        }

        private void InitiateScan(string nameSubstring)
        {
            BLE.BluetoothConnectionPriority(BLE.ConnectionPriority.High);

            BLE.BluetoothScanMode(BLE.ScanMode.LowLatency);

            BLE.Initialize(true, false,
                () => { StartScanning(nameSubstring); },
                (error) => { BLE.Log("BLE error: " + error); }
            );
        }

        private void InitiateConnect(string addr)
        {
            BLE.BluetoothConnectionPriority(BLE.ConnectionPriority.High);

            BLE.Initialize(true, false,
                () => { ConnectToPeripheral(addr); },
                (error) => { BLE.Log("BLE error: " + error); }
            );
        }

        private void CleanupScanning()
        {
            BLE.Log("CleanupScanning");
#if UNITY_ANDROID
            AndroidJNI.AttachCurrentThread();
#endif
            BLE.StopScan();
            lock (matchLock)
            {
                foreach (string addr in connectedDevices.ToList())
                {
                    if (addr != serverAddress)
                        BLE.DisconnectPeripheral(addr, null);
                }
            }
        }

        private void StartScanning(string nameSubstring)
        {
#if UNITY_ANDROID
            AndroidJNI.AttachCurrentThread();
#endif

            ScanTimer.Elapsed += (s, e) =>
            {
                if (!selected)
                {
                    CleanupScanning();
                    BLE.DeInitialize(null);
                    timeoutAction?.Invoke();
                }
            };
            ScanTimer.AutoReset = false; // scanning is stopped only once
            ScanTimer.Start();

            BLE.ScanForPeripheralsWithServices(
                advertisedServices.ToArray(),
                (address, name) => { ProcessScanResult(nameSubstring, address, name, new byte[]{}); },
                (address, name, rssi, advert) => { ProcessScanResult(nameSubstring, address, name, advert); }
            );
        }

        private volatile bool subscribing = false;
        private static int MTU = 256;
        private Timer ?cancelTimer;
        private void ConnectToPeripheral(string address)
        {
            // If we are not waiting for a subscription, connect.
            BLE.Log($"ConnectToPeripheral {address}");
            lock (connectionLock)
            {
                if (!connectedDevices.Contains(address))
                {
                    try
                    {
                        deviceToDiscoveredServices.Add(address, new HashSet<string>());
                        deviceToDiscoveredCharacteristics.Add(address, new HashSet<string>());
                    }
                    catch (ArgumentException) { }

                    connectedDevices.Add(address);
                    subscribing = true;
                    cancelTimer = new Timer(CONNECTION_TEST_TIMEOUT * 1000 / 2);
                    cancelTimer.Elapsed += (s, e) =>
                    {
#if UNITY_ANDROID
                        AndroidJNI.AttachCurrentThread();
#endif
                        if (subscribing)
                        {
                            subscribing = false;
                            BLE.DisconnectPeripheral(address, null);
                        }
                        cancelTimer = null;
                    };
                    cancelTimer.AutoReset = false;
                    cancelTimer.Start();

                    // Copy subscriptions so multiple devices can be subscribed to
                    var subs = new CharacQueue(subscriptions);
                    var len = subs.Count; // Subscribe should only be called once: this is to verify that.

                    BLE.ConnectToPeripheral(
                        address, (addr) => { BLE.Log($"connected to {addr}"); },
                        (addr, service) =>
                        {
                            BLE.Log($"discover service {service} ({addr})");
                        }, (addr, service, characteristic) =>
                        {
                            BLE.Log($"discover characteristic {characteristic} ({addr})");
                            deviceToDiscoveredServices[addr].Add(service);
                            deviceToDiscoveredCharacteristics[addr].Add(characteristic);
                            if (!requiredServices.Contains(service))
                                return;

                            if (
                                requiredServices.All((service) =>
                            {
                                return deviceToDiscoveredServices[addr].Contains(service);
                            }) && requiredCharacteristics.All((charac) =>
                            {
                                return deviceToDiscoveredCharacteristics[addr].Contains(charac);
                            })
                            )
                            {
                                BLE.Log($"Connected device is a match: {address}. Already selected: {selected}. Currently {connectedDevices.Count} devices are connected");
                                if (!selected)
                                {
                                    BLE.RequestMtu(addr, MTU, (addr, mtu) => {
                                        BLE.Log($"{addr} got MTU {mtu}");
                                        Subscribe(address, subs, len);
                                    });
                                }
                            }
                        }, (addr) =>
                        {
                            BLE.Log($"disconnect {addr}");
                            if (addr == serverAddress) disconnectAction?.Invoke();
                            lock (connectionLock)
                            {
                                connectedDevices.Remove(addr);
                                deviceToDiscoveredServices[addr].Clear();
                            }
                        }
                    );
                }
            }
        }

        private static float CONNECTION_TEST_TIMEOUT = 10f;
        private void ProcessScanResult(string nameSubstring, string address, string name, byte[] advertisedData)
        {
            // Verify that name matches. If it does and device is not enqueued for testing nor been tested within
            // last CONNECTION_TEST_TIMEOUT, enqueue.
            lock (connectionLock)
            {
                if (Time.time - testedDevices.GetValueOrDefault(address, -CONNECTION_TEST_TIMEOUT) < CONNECTION_TEST_TIMEOUT)
                    return;
                testedDevices[address] = Time.time;
                if ((nameSubstring == ""
                    || System.Text.Encoding.UTF8.GetString(advertisedData.ToArray()).ToLower()
                        .Contains(nameSubstring.ToLower())
                    || name.ToLower().Contains(nameSubstring.ToLower()))
                    && !connectedDevices.Contains(address)
                    && !testableDevices.Contains(address))
                {
                    BLE.Log($"Adding {name} ({address}) to be tested");
                    testableDevices.Enqueue(address);
                }
            }
            HandleTestConnections();
        }

        private void HandleTestConnections()
        {
            // If a selection has not been made and we are not currently trying to subscribe to any device, try
            // connecting to oldest testable device. Handle max connections.
            if (selected)
                return;
            if (subscribing)
            {
                BLE.Log("Skip connect: waiting for previous");
                return;
            }
            lock (connectionLock)
            {
                foreach (string device in connectedDevices)
                {
                    if (Time.time - testedDevices[device] >= CONNECTION_TEST_TIMEOUT)
                    {
                        BLE.DisconnectPeripheral(device, null);
                        return;
                    }
                }
                if (testableDevices.Count == 0)
                    return;
                if (connectedDevices.Count < MAX_CONNECTIONS)
                {
                    ConnectToPeripheral(testableDevices.Dequeue());
                    return;
                }
            }
        }

        private void Subscribe(string address, CharacQueue subs, int subsCount)
        {
            // Subscribe to first characteristic: the device will be selected
            // if no other selection has been made and the characteristic notifies data.
            if (subs.Count != subsCount)
            {
                BLE.Log("Subscribe skipped");
                return;
            }
            var sub = subs.Dequeue();
            BLE.Log($"Subscribing {address} to {sub.characteristic}");

            if (deviceToDiscoveredServices[address].Contains(sub.service)
                    && deviceToDiscoveredCharacteristics[address].Contains(sub.characteristic)) {
                BLE.SubscribeCharacteristicWithDeviceAddress(
                    address, sub.service, sub.characteristic, (sth, sth2) =>
                    {
                        cancelTimer?.Close();
                        cancelTimer = null;
                        BLE.Log($"Subscription notify: {sth} {sth2}");
                        // We should only allow more subscriptions after this!
                        subscribing = false;
                    },
                    (addr, characteristic, bytes) =>
                    {
                        if (!selected)
                        {
                            selected = true;
                            BLE.Log($"Selecting {addr} due to receiving {characteristic}");
                            SubscribeRest(address, subs);
                        }
                        sub.callback(bytes);
                    }
                );

            } else {
                Subscribe(address, subs, subsCount - 1);
            }

        }

        private void SubscribeRest(string address, CharacQueue subs)
        {
            // Subscribe all characteristics remaining after Subscribe.
            if (subscribing)
            {
                BLE.Log("Waiting: subscribing active");
                selected = false;
                return;
            }
            (string service, string characteristic, Action<byte[]> callback) sub;
            try
            {
                sub = subs.Dequeue();
                BLE.Log($"Subscribing {address} to {sub.characteristic}");
            }
            catch (InvalidOperationException)
            {
                BLE.Log($"All subscribed");
                Select(address);
                return;
            }
            if (deviceToDiscoveredServices[address].Contains(sub.service)
                    && deviceToDiscoveredCharacteristics[address].Contains(sub.characteristic))
            {
                BLE.SubscribeCharacteristicWithDeviceAddress(
                    address, sub.service, sub.characteristic, (addr, characteristic) =>
                    {
                        BLE.Log($"{sub.characteristic} subscribed");
                        SubscribeRest(address, subs);
                    }, (addr, characteristic, bytes) =>
                    {
                        sub.callback(bytes);
                    }
                );
            } else {
                SubscribeRest(address, subs);
            }
        }
    }
}
