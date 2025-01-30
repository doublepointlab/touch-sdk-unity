/// Copyright (c) 2024 Doublepoint Technologies Oy <hello@doublepoint.com>
/// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Google.Protobuf;

using Timer = System.Timers.Timer;


namespace Psix
{
    public class GattConnection
    {
        public static Gatt BLE = GattImpl.Instance;
        private static PsixLogger logger = new PsixLogger("GattConnection");
        const int MTU = 512;

        private string _address = "";
        public string Address { get { return _address; } }

        private Timer? _connectionTimer = null;

        private Dictionary<string, HashSet<string>> _services =
            new Dictionary<string, HashSet<string>>();
        private List<Subscription> _subscriptions = new List<Subscription>();

        // Decide if device is responsive using the first subscription
        private Func<byte[], bool> _acceptor = (data => true);

        /* Called when device is sending data (accepted) */
        private Action<GattConnection>? OnAccept = null;

        /* Called when device is disconnected */
        public Action<GattConnection>? OnDisconnect = null;

        /* Called when device is connected */
        private Action<GattConnection>? _onConnected = null;

        /* True if device is sending data and accepted */
        public bool Accepted { get; private set; } = false;

        /* True if has required services and characteristics */
        public bool IsMatch { get; private set; } = false;

        /* True if device has been disconnected */
        public bool Disconnected
        {
            get; private set;
        } = false;

        /** Construct a gatt connection.
        * @param address Ble address of the device. Retrieve by scanning.
        * @param subscriptions Characteristics to subscribe to. Data from the first one will be forwarded to acceptor
        * @param acceptor Take data from the first characteristic and determine if the device is acceptable.
        */
        public GattConnection(string address, List<Subscription> subscriptions, Func<byte[], bool>? acceptor = null)
        {
            _address = address;
            _subscriptions = subscriptions;
            if (acceptor != null)
                _acceptor = acceptor;
        }
        /** Connect. You need to initialize bluetooth beforehand!
        * @param onAccepted Device is sending data that is accepted by acceptor.
        * @param onDisconnect Connection has either timed out or disconnect has been called.
        * @param connectionTimeout Disconnect after this seconds unless <= 0 or the device is accepted.
        * @param onConnected Device has been connected to, but has not been accepted
        */
        public void Connect(Action<GattConnection>? onAccepted, Action<GattConnection>? onDisconnect, double connectionTimeout = 0, Action<GattConnection>? onConnected = null)
        {
            OnAccept = onAccepted;
            OnDisconnect = onDisconnect;
            _onConnected = onConnected;
            StartConnectionTimeout(connectionTimeout);
            ConnectToPeripheral();
        }


        /** Gatt write operation
        * @param data Self evident
        * @param serviceUuid UUID of the gatt service to be written to
        * @param characteristicUUID UUID of the gatt characteristic to be written to
        */
        public void SendBytes(byte[] data, string serviceUuid, string characteristicUuid)
        {
            BLE.WriteCharacteristic(
                _address, serviceUuid, characteristicUuid,
                data, data.Length, false, (characteristicUUID) =>
            {
                logger.Trace("Write succ");
            });
        }

        /** Gatt read operation
        * @param serviceUuid UUID of the gatt service to be written to
        * @param characteristicUUID UUID of the gatt characteristic to be written to
        */
        public void RequestBytes(string serviceUuid, string characteristicUuid, Action<byte[]> onData)
        {
            BLE.ReadCharacteristic(_address, serviceUuid, characteristicUuid, (charac, data) => onData(data));
        }

        /* Send information about this device and app to the peripheral */
        private void SendClientInfo() {
            var update = new Proto.InputUpdate {
                ClientInfo = new Proto.ClientInfo {
                    AppName = Application.productName,
                    DeviceName = SystemInfo.deviceName,
                    Os = Application.platform.ToString()
                }
            };

            SendBytes(update.ToByteArray(), GattServices.ProtobufServiceUUID, GattServices.ProtobufInputUUID);
        }

        /* Disconnect device */
        public void Disconnect()
        {
            logger.Debug("Disconnect " + _address);
            if (Disconnected)
                return;
            Disconnected = true;
            BLE.DisconnectPeripheral(_address, (s) => { });
        }

        static Dictionary<string, GattConnection> connections = new Dictionary<string, GattConnection>();
        private void ConnectToPeripheral()
        {
            logger.Trace($"ConnectToPeripheral ({_address})");
            if (connections.ContainsKey(_address))
            {
                logger.Error("Connection to this address exists");
                return;
            }
            connections[_address] = this;
            /* Connect to peripheral ONLY HAS ONE SET OF DELEGATES !!!!!!
            * Thus we need to keep track of all the connections and get reference
            * using the address!
            */
            BLE.ConnectToPeripheral(
                _address, (addr) =>
                {
                    if (!connections.ContainsKey(addr))
                        return;
                    logger.Debug($"connected to {addr}");
                    var self = connections[addr];
                    self._onConnected?.Invoke(self);
                },
                (addr, service) =>
                {
                    if (!connections.ContainsKey(addr))
                        return;
                    logger.Trace($"Discover service {service} ({addr})");
                    var self = connections[addr];
                    self._services[service] = new HashSet<string>();
                },
                (addr, service, characteristic) =>
                {
                    if (!connections.ContainsKey(addr))
                        return;
                    var self = connections[addr];
                    if (!self._services.ContainsKey(service))
                        self._services[service] = new HashSet<string>();

                    if (self._services[service].Contains(characteristic))
                        return;
                    var neededCharacs = self._subscriptions.Select((s) => s.characteristic);
                    if (neededCharacs.Contains(characteristic))
                        logger.Debug($"Discovered relevant characteristic {characteristic} ({addr})");
                    self._services[service].Add(characteristic);

                    var subscriptionsAvailable = self._subscriptions.All((tuple) =>
                    {
                        return self._services.ContainsKey(tuple.service) && self._services[tuple.service].Contains(tuple.characteristic);
                    });
                    if (subscriptionsAvailable && !self.IsMatch)
                    {
                        logger.Debug($"Connected device is a match: {self._address}.");
                        self.IsMatch = true;
                        BLE.RequestMtu(addr, MTU, (addr, mtu) =>
                        {
                            logger.Debug($"{addr} got MTU {mtu}");
                            self.SendClientInfo();
                            self.Subscribe(self._subscriptions.Count);
                        });
                    }
                }, (addr) =>
                {
                    var self = connections[addr];
                    logger.Trace($"Disconnect callback {addr}");
                    self.Disconnected = true;
                    connections.Remove(addr);
                    OnDisconnect?.Invoke(self);
                }
            );
        }

        private void StartConnectionTimeout(double timeout)
        {
            _connectionTimer = new Timer(1);
            if (timeout > 0)
            {
                _connectionTimer.Interval = timeout;
                _connectionTimer.Elapsed += (s, e) =>
                {
#if UNITY_ANDROID
                    AndroidJNI.AttachCurrentThread();
#endif
                    logger.Trace("Connection timeout {0}", _address);
                    Disconnect();
                };
                _connectionTimer.AutoReset = false; // scanning is stopped only once
                logger.Trace("Connection timeout set");
                _connectionTimer.Start();
            }
            else
                logger.Debug("Connection to {0} without timeout", _address);
        }

        private void Subscribe(int subsCount)
        {
            if (subsCount == 0)
            {
                logger.Info(string.Format("Connected to {0} with no subscriptions.", _address));
                Accepted = true;
                OnAccept?.Invoke(this);
            }
            var sub = _subscriptions[_subscriptions.Count - subsCount];
            logger.Trace($"Subscribing {_address} to {sub.characteristic}");

            if (_services.ContainsKey(sub.service)
                    && _services[sub.service].Contains(sub.characteristic))
            {
                BLE.SubscribeCharacteristicWithDeviceAddress(
                    _address, sub.service, sub.characteristic, (sth, sth2) =>
                    {
                        logger.Trace($"Subscription notify: {sth} {sth2}");
                    },
                    (addr, characteristic, bytes) =>
                    {
                        if (!Accepted)
                        {
                            if (_acceptor(bytes))
                            {
                                _connectionTimer?.Close();
                                _connectionTimer = null;
                                logger.Debug($"Accepting {addr} due to receiving {characteristic}");
                                SubscribeRest(subsCount - 1);
                            }
                            else
                                logger.Debug($"Declining {addr} due to _acceptor");
                        }
                        else
                        {
                            sub.callback(bytes);
                        }
                    }
                );

            }
            else
                Subscribe(subsCount - 1);

        }

        private void SubscribeRest(int subsCount)
        {
            // Subscription without acceptance check
            Subscription sub;
            if (subsCount <= 0)
            {
                logger.Debug($"All subscribed");
                Accepted = true;
                OnAccept?.Invoke(this);
            }
            else
            {
                sub = _subscriptions[_subscriptions.Count - subsCount];
                logger.Trace($"Subscribing {_address} to {sub.characteristic}");
                if (_services.ContainsKey(sub.service)
                        && _services[sub.service].Contains(sub.characteristic))
                {
                    BLE.SubscribeCharacteristicWithDeviceAddress(
                        _address, sub.service, sub.characteristic, (addr, characteristic) =>
                        {
                            logger.Trace($"{sub.characteristic} subscribed");
                            SubscribeRest(subsCount - 1);
                        }, (addr, characteristic, bytes) =>
                        {
                            sub.callback(bytes);
                        }
                    );
                }
                else
                    SubscribeRest(subsCount - 1);
            }
        }

        public override int GetHashCode()
        {
            return _address.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _address == ((GattConnection)obj)._address;
        }
    }
}
