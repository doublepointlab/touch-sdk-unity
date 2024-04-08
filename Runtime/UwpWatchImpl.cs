// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.
#if ENABLE_WINMD_SUPPORT

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.WindowsRuntime;

using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Core;

using UnityEngine;

namespace Psix
{

    using Interaction;
    using CharacDict = Dictionary<string, Action<byte[]>>;
    using Timer = System.Timers.Timer;

    /**
     * Implementation of smartwatch interface for UWP.
     * Provides methods and callbacks related to connecting to Doublepoint
     * Controller smartwatch app.
     * Check also IWatch.
     */
    class UwpWatchImpl : WatchImpl
    {

        private string watchName = "";

        private static PsixLogger logger = new PsixLogger("UwpWatchImpl");

        private static int CONNECTION_TEST_TIMEOUT = 60;

        private BluetoothLEAdvertisementWatcher deviceWatcher;
        private BluetoothLEDevice? device = null;

        private string advertisedServiceUuid = "008e74d0-7bb3-4ac5-8baf-e5e372cced76";

        private HashSet<string> subscriptions = new HashSet<string>();
        private HashSet<string> subscribedServices = new HashSet<string>();
        private HashSet<string> requiredCharacteristics = new HashSet<string>();

        private ConcurrentDictionary<string, float> testedDevices = new ConcurrentDictionary<string, float>(); // Prevent connect disconnect spamming
        private HashSet<BluetoothLEDevice> connectedDevices = new HashSet<BluetoothLEDevice>();

        private GameObject receiverGameObject = new GameObject("TouchSdkGameObject");
        private MessageDispatcher dispatcher;

        private List<GattCharacteristic> availableCharacteristics = new List<GattCharacteristic>();

        public UwpWatchImpl(string name = "") {
            watchName = name;

            deviceWatcher = new BluetoothLEAdvertisementWatcher();
            deviceWatcher.AllowExtendedAdvertisements = true;
            deviceWatcher.ScanningMode = BluetoothLEScanningMode.Active;

            // We advertise interaction service uuid to facilitate filtering of scan results
            deviceWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(Guid.Parse(advertisedServiceUuid));

            subscribedServices.Add("f9d60370-5325-4c64-b874-a68c7c555bad");
            subscriptions.Add("f9d60371-5325-4c64-b874-a68c7c555bad");
            requiredCharacteristics.Add("f9d60371-5325-4c64-b874-a68c7c555bad");

            dispatcher = receiverGameObject.AddComponent<MessageDispatcher>();
            dispatcher.OnMessage += onData;
            dispatcher.OnDisconnect += disconnectAction;

        }

        private void onData(byte[] data) {
            if (!Connected) {
                connectAction();
            }

            OnProtobufData(data);
        }

        override public void Connect()
        {
            deviceWatcher.Received += OnReceived;
            deviceWatcher.Start();
        }

        override public void Disconnect()
        {
            deviceWatcher.Stop();
            device?.Dispose();
        }

        override public void Vibrate(int length, float amplitude)
        {
            Write(GetHapticsMessage(length, amplitude));
        }

        override public void CancelVibration()
        {
            Write(GetHapticsCancellation());
        }

        override public void RequestGestureDetection(Gesture gesture)
        {
            Write(GetGestureDetectionRequest(gesture));
        }

        private void Write(byte[] data) {
            var charac = availableCharacteristics
                 .FirstOrDefault(charac => charac.Uuid.ToString() == "f9d60372-5325-4c64-b874-a68c7c555bad");
            charac?.WriteValueAsync(data.AsBuffer());
        }


        private async void OnReceived(
            BluetoothLEAdvertisementWatcher watcher,
            BluetoothLEAdvertisementReceivedEventArgs args
        ) {

            BluetoothLEDevice dev = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);

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
                ProcessScanResult(dev, manufacturerBytes);
            }

        }

        private void ProcessScanResult(BluetoothLEDevice dev, byte[] advertisedData)
        {
            // Verify that name matches. If it does and device is not enqueued for testing nor been tested within
            // last CONNECTION_TEST_TIMEOUT, enqueue.

            if (DateTimeOffset.Now.ToUnixTimeSeconds() - (testedDevices.TryGetValue(dev.DeviceId, out var value) ? value : -CONNECTION_TEST_TIMEOUT) < CONNECTION_TEST_TIMEOUT)
                return;
            testedDevices[dev.DeviceId] = DateTimeOffset.Now.ToUnixTimeSeconds();
            if ((watchName == ""
                || System.Text.Encoding.UTF8.GetString(advertisedData).ToLower()
                    .Contains(watchName.ToLower())
                || dev.Name.ToLower().Contains(watchName.ToLower()))
                && !connectedDevices.Contains(dev)
            )
            {
                ConnectToPeripheral(dev);
            }
        }

        private readonly object connectionLock = new object();

        private void ConnectToPeripheral(BluetoothLEDevice dev) {

            lock (connectionLock)
            {
                if (device == null)
                {
                    connectedDevices.Add(dev);
                    var t = Task.Run(async () => { await Connect(dev); });
                    t.Wait(CONNECTION_TEST_TIMEOUT * 1000);

                    if (t.IsCompleted) {
                        Debug.Log("completed");
                    } else {
                        dev.Dispose();
                        connectedDevices.Remove(dev);
                        Debug.Log("cancelled");
                    }
                } else {
                    Debug.Log("skipping because already connected");
                }
            }
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Debug.Log($"OnConnectionStatusChanged {args} : {sender.BluetoothAddress} : {device?.BluetoothAddress}");
            if (sender.BluetoothAddress == device?.BluetoothAddress
                && sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected) {
                device?.Dispose();
                device = null;
                dispatcher.Disconnect();
            }
        }


        private async Task Connect(BluetoothLEDevice dev) {

            dev.ConnectionStatusChanged += OnConnectionStatusChanged;
            GattDeviceServicesResult serviceResult = await dev.GetGattServicesAsync();
            if (serviceResult.Status == GattCommunicationStatus.Success)
            {
                var services = serviceResult.Services;

                var subs = services
                    .Where(service => subscribedServices.Contains(service.Uuid.ToString())); // Filter out services that are not subscribed to
                var tasks = subs
                    .Select(async service => await service.GetCharacteristicsAsync()); // Get characteristics
                var results = tasks.Select(result => result.Result) // Wait for results
                .Where(result => result.Status == GattCommunicationStatus.Success); // Filter out failed operations
                var characs = results.Select(result => result.Characteristics)
                .SelectMany(x => x); // Flatten

                // Filter out characteristics which shouldn't be subscribed to
                var subCharacs = characs.Where(characteristic => subscriptions.Contains(characteristic.Uuid.ToString()));

                if (requiredCharacteristics
                    .All(uuid => subCharacs.Select(c => c.Uuid.ToString()).Contains(uuid))
                )
                {
                    Subscribe(dev, subCharacs.ToList(), characs.ToList());
                }
            }
        }

        private void Subscribe(
            BluetoothLEDevice dev,
            List<GattCharacteristic> characteristics,
            List<GattCharacteristic> allCharacteristics
        ) {
            characteristics
            .Select(async characteristic =>
                await characteristic
                .WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify
                )
            )
            .Select(result => result.Result)
            .Zip(characteristics, (status, characteristic) => (status, characteristic))
            .Where(thing => (thing.status == GattCommunicationStatus.Success))
            .Select(thing => thing.characteristic)
            .ToList()
            .ForEach(characteristic => {
                characteristic.ValueChanged += (sender, args) => {
                    var buffer = args.CharacteristicValue;
                    var reader = DataReader.FromBuffer(buffer);
                    var data = new byte[buffer.Length];
                    reader.ReadBytes(data);
                    dispatcher.Dispatch(data);
                };
            });

            availableCharacteristics = allCharacteristics;
            device = dev;
            device?.RequestPreferredConnectionParameters(BluetoothLEPreferredConnectionParameters.ThroughputOptimized);

        }
    }
}

#endif