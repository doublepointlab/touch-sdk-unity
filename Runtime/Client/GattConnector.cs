#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Psix
{

    /** Default routine for connecting to a watch.
    * While no accepted device and not timed out:
    *   Scan for short time
    *   If new devices:
    *       Try to connect to each new device
    *       if device accepts:
    *           Stop scanning and return GattConnection with callback
    *   else:
    *       Forget all devices
    */
    public class GattConnector
    {
        private static Psix.PsixLogger logger = new Psix.PsixLogger("GattConnector");

        public int MaxConnections = 1;
        public double ConnectionTimeout = 25 * 1000;

        HashSet<string> _testedAddresses = new HashSet<string>();
        GattScanner? _scanner = null;
        string _nameSubstring = "";

        // Will increase scan timeout if new devices are not found.
        double _scanTimeout = 3000;

        bool _hasTimedOut = false;
        private Timer _timer = new Timer(1);

        private List<Subscription> _subs = new List<Subscription>();
        private Func<byte[], bool> _acceptor = (data => true);

        private Queue<string> _futureConnections = new Queue<string>();
        private HashSet<GattConnection> _connections = new HashSet<GattConnection>();

        Action<GattConnection> _onAccepted;

        public bool Completed { get; private set; } = false;

        public GattConnector(
                Action<GattConnection> onAccepted,
            string nameSubstring, List<Subscription> subscriptions, List<string> advertisedServices, long timeout, Func<byte[], bool>? acceptor = null
         )
        {
            _scanner = new GattScanner(advertisedServices);
            _nameSubstring = nameSubstring;
            _subs = subscriptions;
            _acceptor += acceptor;
            _onAccepted = onAccepted;
            MaxConnections = nameSubstring != "" ? 1 : 4;
            StartConnectionTimeout(timeout);
            GattImpl.Instance.Initialize(true, false,
                () => { Scan(); },
                (error) => { logger.Error("BLE error: " + error); }
            );
        }

        public void StopAndDisconnect()
        {
            lock (_connections)
            {
                foreach (var c in _connections)
                    c.Disconnect();
                _connections.Clear();
            }
            _hasTimedOut = true;
        }

        // Time.time is not thread safe so we use a timer to set a bool...
        private void StartConnectionTimeout(double timeout)
        {
            if (timeout < 0)
                return;
            _hasTimedOut = false;
            {
                _timer = new Timer(timeout);
                _timer.Elapsed += (s, e) =>
                {
                    _hasTimedOut = true;
                };
                _timer.AutoReset = false; // scanning is stopped only once
                _timer.Start();
            }
        }

        void Scan()
        {
            if (_hasTimedOut)
                logger.Info("No bluetooth device was found");
            else
                _scanner?.Scan(OnScanFinish, _nameSubstring, _scanTimeout, MaxConnections);
        }

        void OnAccepted(GattConnection conn)
        {
            logger.Trace("OnAccepted");
            Completed = true;
            lock (_connections)
            {
                if (_connections.Count == 0)
                    return;
                _connections.Remove(conn);
                foreach (var c in _connections)
                    c.Disconnect();
                _connections.Clear();
            }
            _onAccepted(conn);
        }

        void OnDeclined(GattConnection conn)
        {
            lock (_connections)
            {
                _connections.Remove(conn);
                logger.Trace("OnDeclined {0}. {1} remain.", conn.Address, _connections.Count);
                if (logger.IsEnabledFor(LogLevel.Verbose)){
                    foreach (var c in _connections) 
                        logger.Verbose("Remains: {0}", c.Address);
                }
                // We don't have to worry about future connections, because
                // if _connections is empty, nothing will call that method.
                // Furthermore scan should definitely not be running atm.
                if (_connections.Count == 0 && !Completed)
                    Scan();
            }
        }

        /* Connect devices 1 by 1. If a connection fails, we might have exceeded
        the maximum number of connections, so the connections are initiated only
        if connection succeeds. */
        void ConnectNext()
        {
            logger.Trace("ConnectNext");
            lock (_futureConnections)
            {
                if (_futureConnections.Count == 0)
                    return;
                var address = _futureConnections.Dequeue();
                var conn = new GattConnection(address, _subs, _acceptor);
                logger.Trace($"GattConnection({conn.Address})");
                lock (_connections)
                    _connections.Add(conn);
                // R value lambdas are singletons !?!?!?
                conn.Connect(OnAccepted, OnDeclined, ConnectionTimeout, (c) => { ConnectNext(); });
            }
        }

        void TestConnections(List<GattScanner.ScanResult> scanResults)
        {
            lock (_futureConnections)
            {
                foreach (var res in scanResults)
                {
                    _testedAddresses.Add(res.address);
                    _futureConnections.Enqueue(res.address);
                }
            }
            ConnectNext();
        }

        void OnScanFinish()
        {
            logger.Trace("OnScanFinish");
            if (_hasTimedOut)
            {
                logger.Debug("Device scan timed out");
                return;
            }
            var scanResults = _scanner?.PeekResults();
            if (scanResults == null)
                return;
            // Scan everything again if all devices have been tested
            if (scanResults.TrueForAll((res) => _testedAddresses.Contains(res.address)))
            {
                logger.Debug("No new devices found. Scanning again");
                _scanner?.PopResults();
                _scanTimeout = Math.Min(_scanTimeout * 2, 35 * 1000);
                Scan();
                return;
            }
            TestConnections(scanResults);
        }
    }
}