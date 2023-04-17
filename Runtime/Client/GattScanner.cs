// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Timer = System.Timers.Timer;

namespace Psix
{
    public class GattScanner
    {
        public static Gatt BLE = GattImpl.Instance;
        private static PsixLogger logger = new PsixLogger("GattScanner");
        private Timer? _scanTimer = null;
        private string _nameSubstring = "";
        private int _maxDevices = 0;
        // _scanResults might not be cleared so we have this
        private int _previouslyFound = 0;

        private bool _isAccepting = true;

        private List<string> _advertisedServices = new List<string>();

        public struct ScanResult
        {
            public string name;
            public string address;
            public string adString; // Name can also be in advertisement package
            public override int GetHashCode()
            {
                return address.GetHashCode() + name.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                return address == ((ScanResult)obj).address || (name != "" && ((ScanResult)obj).name == name);
            }
        }
        private HashSet<ScanResult> _scanResults = new HashSet<ScanResult>();

        /* PUBLIC METHODS */


        /** Constructor.
        * @param advertisedServices Filter scanned devices based on what services they advertise
        */
        public GattScanner(List<string> advertisedServices)
        {
            _advertisedServices = advertisedServices;
        }
        public GattScanner() { }


        /** Get scan results and forget them for next scan.
        */
        public List<ScanResult> PopResults()
        {
            lock (_scanResults)
            {
                var results = _scanResults.ToList();
                logger.Trace($"Popping {_scanResults.Count} devices");
                _scanResults.Clear();
                _previouslyFound = 0;
                return results;
            }
        }

        /** Get scan results without removing old ones. New
        * scan cycles will only search for new devices.
        */
        public List<ScanResult> PeekResults()
        {
            lock (_scanResults)
            {
                return _scanResults.ToList();
            }
        }

        /** Start scanning for devices which match the name substring. Ble needs to be initialized!
        * @param onFinish Scan has finished due to timeout or max devices reached.
        * @param nameSubstring: Filter devices based on their name or advertisement package.
        * @param timeout Timeout: stop scanning after n=timeout seconds.
        * @param maxDevices Stop scanning after n=maxDevices new devices have been found
        */
        public void Scan(Action onFinish, string nameSubstring = "", double timeout = 0, int maxDevices = 0)
        {
            logger.Trace($"Scan for device {nameSubstring}. Timeout: {timeout}, maxDevices {maxDevices}");
            _maxDevices = maxDevices;
            _nameSubstring = nameSubstring.ToLower();
            _previouslyFound = _scanResults.Count;

            BLE.BluetoothConnectionPriority(Gatt.ConnectionPriority.High);
            BLE.BluetoothScanMode(Gatt.ScanMode.LowLatency);
            StartScanning(onFinish, timeout);
        }

        /** Stop scanning. Note that  onFinish callback will not be called.
        */
        public void StopScanning()
        {
            logger.Debug("StopScanning");
            _isAccepting = false;
            _scanTimer?.Close();
            _scanTimer = null;
            BLE.StopScan();
        }

        /** Stop scanning, deinitialize bluetooth
        */
        public void Terminate()
        {
            StopScanning();
            BLE.DeInitialize(null);
            _scanResults.Clear();
        }


        /* PRIVATE METHODS */

        private void StartScanTimeout(double timeout, Action onTimeout)
        {
            logger.Trace($"StartScanTimeout {timeout}");
            if (timeout > 0)
            {
                _scanTimer?.Close();
                _scanTimer = new Timer(timeout);
                _scanTimer!.Elapsed += (s, e) =>
                {
                    if (_scanTimer != null)
                    {
#if UNITY_ANDROID
                        AndroidJNI.AttachCurrentThread();
#endif
                        logger.Trace("ScanTimeout");
                        _scanTimer = null;
                        StopScanning();
                        if (_previouslyFound < _scanResults.Count && _scanResults.All(it => { return it.name == "" && it.adString == "";}))
                        {
                            if (_nameSubstring != "")
                                logger.Warning("None of the scanned devices supply a name or advertisement package. Connect by watch name will not work.");
                            else
                                logger.Debug("None of the scanned devices supply a name or advertisement package.");
                        }
                        onTimeout?.Invoke();
                    }
                };
                _scanTimer!.AutoReset = false; // scanning is stopped only once
                _scanTimer!.Start();
            }
        }

        private void StartScanning(Action onFinish, double timeout)
        {
            _isAccepting = true;
            StartScanTimeout(timeout, onFinish);
            BLE.ScanForPeripheralsWithServices(
                _advertisedServices.ToArray(),
                (address, name) =>
                {
                    ProcessScanResult(address, name, () =>
                    {
                        logger.Trace("MaxDevices found");
                        StopScanning(); onFinish();
                    });
                },
                (address, name, rssi, advert) =>
                {
                    ProcessScanResult(address, name, advert, () =>
                    {
                        logger.Trace("MaxDevices found");
                        StopScanning(); onFinish();
                    });
                }
            );
        }

        private void ProcessScanResult(string address, string name, Action onMaxDevices)
        {
            ProcessScanResult(address, name, new byte[] { }, onMaxDevices);
        }

        private void ProcessScanResult(string address, string name, byte[] advertisedData, Action onMaxDevices)
        {
            if (!_isAccepting)
                return;
            var nameString = System.Text.Encoding.UTF8.GetString(advertisedData.ToArray()).ToLower();
            logger.Debug($"ProcessScanResult: name={name} address={address.Split('-').Last()} nameString=({nameString}) looking for ({_nameSubstring})");
            lock (_nameSubstring)
            {
                if (_nameSubstring == ""
                    || nameString.Contains(_nameSubstring)
                    || name.ToLower().Contains(_nameSubstring))
                {
                    if (_maxDevices <= 0 || _scanResults.Count - _previouslyFound < _maxDevices)
                    {
                        logger.Trace($"_scanResults.Add {0})({1}", address, nameString);
                        lock (_scanResults)
                            _scanResults.Add(new ScanResult { name = name, address = address, adString = nameString });
                    }
                    else
                    {
                        logger.Trace($"Not adding {nameString}");
                    }

                    if (_maxDevices > 0 && _scanResults.Count - _previouslyFound >= _maxDevices)
                    {
                        logger.Debug("Maximum devices reached");
                        onMaxDevices.Invoke();
                    }
                }
            }
        }
    }

}