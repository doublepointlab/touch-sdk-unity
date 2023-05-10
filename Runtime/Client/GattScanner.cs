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

        /* Scan results may have varying addresses for a single device
        * and varying names and adStrings for a single device. It is painful.
        */
        public class ScanResult
        {
            public string address;
            public string name;
            public string adString; // Name can also be in advertisement package
            public string deducedName;

            protected string DeduceName()
            {
                return adString != "" ? adString : name != "" && name != "No Name" ? name : "";
            }

            public ScanResult(string _address, string _name, string _adString)
            {
                address = _address;
                name = _name;
                adString = _adString;
                deducedName = DeduceName();
            }

            public void Update(ScanResult other)
            {
                if (other.name != name)
                {
                    if (name == "" || name == "No Name")
                        name = other.name;
                }
                if (other.adString != adString)
                {
                    if (adString == "")
                        adString = other.adString;
                }
                deducedName = DeduceName();
            }

            public bool HasSameName(ScanResult other)
            {
                return deducedName != "" ? other.deducedName == deducedName : false;
            }
        }

        /* Due to the annoying nature of scan results we handle the set logic here. */ 
        public class ScanResults
        {
            protected List<ScanResult> results = new List<ScanResult>();
            public void Add(ScanResult scanResult)
            {
                foreach (var res in results)
                {
                    if (res.address == scanResult.address)
                    {
                        res.Update(scanResult);
                        return;
                    }
                    else if (res.HasSameName(scanResult))
                    {
                        res.address = scanResult.address;
                        return;
                    }
                }
                results.Add(scanResult);
            }
            public List<ScanResult> Pop()
            {
                var res = results;
                results = new List<ScanResult>();
                return res;
            }

            public List<ScanResult> Peek()
            {
                return results.ToList();
            }

            public int Count {
                get => results.Count;            }
        }
        private ScanResults _scanResults = new ScanResults();

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
                var results = _scanResults.Pop();
                logger.Trace("Popping {0} devices", results.Count);
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
                return _scanResults.Peek();
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
            logger.Trace("Scan for device {0}. Timeout: {1}, maxDevices {2}", nameSubstring, timeout, maxDevices);
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
            _scanResults.Pop();
        }


        /* PRIVATE METHODS */

        private void StartScanTimeout(double timeout, Action onTimeout)
        {
            logger.Trace("StartScanTimeout {0}", timeout);
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
                        if (_previouslyFound < _scanResults.Count && _scanResults.Peek().All(it => { return it.name == "" && it.adString == ""; }))
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

        private string FilterAlphaNumeric(string inp)
        {
            char[] arr = inp.Where(c => (char.IsLetterOrDigit(c) ||
                             char.IsWhiteSpace(c) ||
                             c == '-')).ToArray();

            return new string(arr);
        }

        private void ProcessScanResult(string address, string name, byte[] advertisedData, Action onMaxDevices)
        {
            if (!_isAccepting)
                return;
            var nameString = FilterAlphaNumeric(System.Text.Encoding.UTF8.GetString(advertisedData.ToArray())).ToLower();

            logger.Debug("ProcessScanResult: name=\"{0}\" address={1} nameString=({2}) looking for \"{3}\"", name, address.Split("-").Last(), nameString, _nameSubstring);
            lock (_nameSubstring)
            {
                if (_nameSubstring == ""
                    || nameString.Contains(_nameSubstring)
                    || name.ToLower().Contains(_nameSubstring))
                {
                    if (_maxDevices <= 0 || _scanResults.Count - _previouslyFound < _maxDevices)
                    {
                        logger.Trace("_scanResults.Add ({0}) ({1})", address, nameString);
                        lock (_scanResults)
                            _scanResults.Add(new ScanResult(address, name, nameString ));
                    }
                    if (_maxDevices > 0 && _scanResults.Count - _previouslyFound >= _maxDevices)
                    {
                        logger.Debug("Maximum devices reached");
                        onMaxDevices.Invoke();
                    }
                }
                else
                {
                    logger.Trace("Substring not matched");
                }
            }
        }
    }

}