using System;
using System.Threading;
using System.Collections.Generic;

using UnityEngine;

namespace Psix {
class GattClient {

    private string serverName;
    private string _deviceAddress;

    private State _state = State.None;
    private bool _connected = false;
    private bool _rssiOnly = false;

    private int _rssi = 0;
    private long _timeout = 0;
    private long _lastTime = 0;

    private Thread connectionThread;

    enum State
    {
        None,
        Scan,
        ScanRSSI,
        ReadRSSI,
        Connect,
        RequestMTU,
        Subscribe,
        Unsubscribe,
        Disconnect,
        Terminate,
    }

    private void SetState(State newState, long timeout)
    {
        _state = newState;
        _timeout = timeout;
    }

    public GattClient(string name)
    {
        serverName = name;
    }

    private Queue<(
        string service,
        string characteristic,
        Action<byte[]> callback)> subscribedCharacteristics
        = new Queue<(string service, string characteristic, Action<byte[]> callback)>();

    public void SubscribeToCharacteristic(
       string serviceUuid,
       string characteristicUuid,
       Action<byte[]> callback)
    {
        subscribedCharacteristics.Enqueue((serviceUuid, characteristicUuid, callback));
    }

    private void Initialize()
    {
        BluetoothLEHardwareInterface.BluetoothConnectionPriority(
            BluetoothLEHardwareInterface.ConnectionPriority.High
        );

        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            SetState(State.Scan, 100);
        }, (error) =>
        {
            Debug.Log("BLE error: " + error);
        }
        );
    }

    public void Connect()
    {
        Initialize();
        _lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        connectionThread = new Thread(new ThreadStart(connectionLoop));
        connectionThread.Start();
    }

    public void Disconnect()
    {
        SetState(State.Disconnect, 4000);
    }

    public void SendBytes(byte[] data, string serviceUuid, string characteristicUuid)
    {
        BluetoothLEHardwareInterface.WriteCharacteristic(
            _deviceAddress, serviceUuid, characteristicUuid,
            data, data.Length, false, (characteristicUUID) =>
        {
            Debug.Log("Write Succeeded");
        });
    }

    private void connectionLoop()
    {
        AndroidJNI.AttachCurrentThread(); // BLE interface on Android uses JNI
        while (_state != State.Terminate)
        {
            if (_timeout > 0)
            {
                _timeout -= DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastTime;
                _lastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                if (_timeout <= 0)
                {
                    _timeout = 0;

                    switch (_state)
                    {
                        case State.None:
                            break;

                        case State.Scan:
                            Debug.Log("Scanning for " + serverName);

                            BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
                                null, (address, name) =>
                            {
                                if (!_rssiOnly)
                                {
                                    if (name.Contains(serverName))
                                    {
                                        Debug.Log("Found " + name);

                                        BluetoothLEHardwareInterface.StopScan();

                                        _deviceAddress = address;
                                        SetState(State.Connect, 500);
                                    }
                                }

                            }, (address, name, rssi, bytes) =>
                            {
                                if (name.Contains(serverName))
                                {
                                    Debug.Log("Found " + name);

                                    if (_rssiOnly)
                                    {
                                        _rssi = rssi;
                                    }
                                    else
                                    {
                                        BluetoothLEHardwareInterface.StopScan();

                                        _deviceAddress = address;
                                        SetState(State.Connect, 500);
                                    }
                                }

                            }, _rssiOnly);

                            if (_rssiOnly)
                                SetState(State.ScanRSSI, 500);
                            break;

                        case State.ScanRSSI:
                            break;

                        case State.ReadRSSI:
                            Debug.Log($"Call Read RSSI");
                            BluetoothLEHardwareInterface.ReadRSSI(_deviceAddress, (address, rssi) =>
                            {
                                Debug.Log($"Read RSSI: {rssi}");
                            });

                            SetState(State.ReadRSSI, 2000);
                            break;

                        case State.Connect:
                            Debug.Log("Connecting...");

                            // TODO: make sure the device has required Bluetooth attributes
                            BluetoothLEHardwareInterface.ConnectToPeripheral(
                                _deviceAddress, (address) =>
                            {
                                Debug.Log("Connected");
                                _connected = true;
                                SetState(State.RequestMTU, 2000);
                            }, null, null, (kek) =>
                            {
                                SetState(State.Disconnect, 4000);
                                Debug.Log("Server disconnected: " + kek);
                            }
                            );
                            break;

                        case State.RequestMTU:
                            Debug.Log("Requesting MTU");

                            BluetoothLEHardwareInterface.RequestMtu(_deviceAddress, 185, (address, newMTU) =>
                            {
                                Debug.Log("MTU set to " + newMTU.ToString());

                                SetState(State.Subscribe, 100);
                            });
                            break;

                        case State.Subscribe:

                            try
                            {
                                var p = subscribedCharacteristics.Dequeue();
                                Debug.Log("Subscribing to " + p.characteristic);
                                BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
                                   _deviceAddress, p.service, p.characteristic,
                                   (notifyAddress, notifyCharacteristic) =>
                                {
                                    Debug.Log("notification action called for " + notifyCharacteristic + " with " + subscribedCharacteristics.Count);
                                    SetState(State.Subscribe, 100);

                                }, (address, characteristicUUID, bytes) =>
                                {
                                    Debug.Log("subscribe action called for " + characteristicUUID);
                                    p.callback(bytes);
                                });

                            } catch (InvalidOperationException) {
                                Debug.Log("No more characteristics to subscribe to");
                                SetState(State.ReadRSSI, 1000);
                            }

                            break;

                        case State.Unsubscribe:
                            foreach (var p in subscribedCharacteristics)
                            {
                                BluetoothLEHardwareInterface.UnSubscribeCharacteristic(
                                    _deviceAddress, p.service, p.characteristic, null);
                            }
                            SetState(State.Disconnect, 4000);
                            break;

                        case State.Disconnect:
                            Debug.Log("Commanded disconnect.");

                            if (_connected)
                            {
                                BluetoothLEHardwareInterface.DisconnectPeripheral(
                                    _deviceAddress, (address) =>
                                {
                                    Debug.Log("Device disconnected");
                                    BluetoothLEHardwareInterface.DeInitialize(() =>
                                    {
                                        _connected = false;
                                        _state = State.None;
                                    });
                                });
                            }
                            else
                            {
                                BluetoothLEHardwareInterface.DeInitialize(() =>
                                {
                                    _state = State.None;
                                });
                            }
                            break;
                    }
                }
            }
        }
    AndroidJNI.DetachCurrentThread();
    }
}
}
