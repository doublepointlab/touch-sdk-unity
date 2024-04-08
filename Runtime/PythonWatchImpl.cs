// Copyright (c) 2022 Port 6 Oy <hello@port6.io>
// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using Google.Protobuf;
using System.IO;
using System.Collections.Concurrent;
using UnityEngine;
using System.Diagnostics;
using System.Threading;

namespace Psix
{

    using Interaction;


    /**
     * Implementation of smartwatch interface using touch-sdk-android.
     * Provides methods and callbacks related to connecting to Doublepoint
     * Controller smartwatch app.
     * Check also IWatch.
     */
    [DefaultExecutionOrder(-50)]
    class PythonWatchImpl : WatchImpl
    {

        private string watchName;
        private string pythonPath;
        GameObject? receiverObject;
        StreamWriter? processInput;

        private static PsixLogger logger = new PsixLogger("PythonWatchImpl");

        public PythonWatchImpl(string pythonPath, string name)
        {
            logger.Debug("PythonWatchImpl");
            this.pythonPath = pythonPath;
            this.watchName = name;
            receiverObject = null;
        }

        private class Receiver : MonoBehaviour
        {
            public PythonWatchImpl? parent;
            Process? externalProcess;
            StreamReader? processOutput;
            StreamReader? processErr;

            Thread? readThread;
            Thread? errThread;

            private ConcurrentQueue<byte[]> lineQueue = new();
            private AutoResetEvent newDataEvent = new AutoResetEvent(false);

            private void OnData(byte[] data)
            {
                if (!parent!.Connected)
                {
                    parent!.connectAction();
                }
                if (data.Length != 0)
                {
                    parent!.OnProtobufData(data);
                }

            }

            private void Start()
            {
                logger.Debug("Start for {0}", parent!.watchName);
                // Create process start info
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = parent!.pythonPath;
                startInfo.Arguments = $"-m touch_sdk.stream_watch --name-filter {parent!.watchName}";
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                // Start the external process
                externalProcess = Process.Start(startInfo);

                // Get the input and output streams of the process
                parent.processInput = externalProcess.StandardInput;
                processOutput = externalProcess.StandardOutput;
                processErr = externalProcess.StandardError;
                if (externalProcess.HasExited)
                {
                    PythonWatchImpl.logger.Warn("Process exited immediately");
                }
                else
                {
                    PythonWatchImpl.logger.Debug("BeginRead");
                    readThread = new Thread(ReadTask);
                    readThread.Start();
                    errThread = new Thread(ErrTask);
                    errThread.Start();
                }
            }

            void ReadTask()
            {
                while (processOutput?.EndOfStream != true)
                {
                    lineQueue.Enqueue(Convert.FromBase64String(processOutput?.ReadLine()));
                    // PythonWatchImpl.logger.Debug("Line enqued");
                    newDataEvent.Set();
                }
            }

            void ErrTask()
            {
                string? val = processErr?.ReadToEnd();
                if (!String.IsNullOrEmpty(val))
                    logger.Warn("From ext process:{0}", val);
            }

            public bool TryGetNextLine(out byte[] line)
            {
                return lineQueue.TryDequeue(out line);
            }
            private void Update()
            {
                if (externalProcess != null && externalProcess.HasExited)
                {
                    logger.Debug("External process exited");
                    parent!.Disconnect();
                    return;
                }

                if (newDataEvent.WaitOne(0))
                {
                    byte[] line;
                    // Process all available lines
                    while (TryGetNextLine(out line))
                    {
                        // Do something with the line
                        OnData(line);
                    }
                }
            }

            private void OnDestroy()
            {
                if (readThread != null)
                {
                    readThread.Abort();
                    readThread.Join();
                    readThread = null;
                }
                if (errThread != null)
                {
                    errThread.Abort();
                    errThread.Join();
                    errThread = null;
                }

                if (parent?.processInput != null)
                {
                    parent?.processInput.Close();
                }

                if (processOutput != null)
                {
                    processOutput.Close();
                }

                if (externalProcess != null)
                {
                    PythonWatchImpl.logger.Debug("Ext close");
                    externalProcess!.CloseMainWindow();
                    externalProcess!.Close();
                }

                externalProcess = null;
                processOutput = null;
                parent!.processInput = null;
            }

            public void Disconnect()
            {
                if (parent!.receiverObject != null)
                {
                    logger.Debug("Disconnect");
                    Destroy(parent!.receiverObject);
                    OnDestroy();
                    parent!.receiverObject = null;
                }
            }

        }

        override public void Connect()
        {
            if (receiverObject == null)
            {
                logger.Debug("Connecting via gameobject");
                GameObject receiverGameObject = new("PythonSdkReceiver");
                Receiver receiver = receiverGameObject.AddComponent<Receiver>();
                receiver.parent = this;
            }
        }

        override public void Disconnect()
        {
            receiverObject?.GetComponent<Receiver>().Disconnect();
        }

        // Following methods copied from GattWatchImpl
        override public void Vibrate(int length, float amplitude)
        {
            int clampedLength = Mathf.Clamp(length, 0, 5000);
            float clampedAmplitude = Mathf.Clamp(amplitude, 0.0f, 1.0f);
            var update = new Proto.InputUpdate
            {
                HapticEvent = new Proto.HapticEvent
                {
                    Type = Proto.HapticEvent.Types.HapticType.Oneshot,
                    Length = clampedLength,
                    Intensity = clampedAmplitude
                }
            };

            processInput?.WriteLine(Convert.ToBase64String(update.ToByteArray()));

        }

        override public void CancelVibration()
        {
            var update = new Proto.InputUpdate
            {
                HapticEvent = new Proto.HapticEvent
                {
                    Type = Proto.HapticEvent.Types.HapticType.Cancel
                }
            };

            processInput?.WriteLine(Convert.ToBase64String(update.ToByteArray()));
        }

        override public void RequestGestureDetection(Gesture gesture)
        {
            var update = new Proto.InputUpdate
            {
                ModelRequest = new Proto.Model
                {
                    Gestures = { (Proto.GestureType)(gesture) }
                }
            };
            processInput?.WriteLine(Convert.ToBase64String(update.ToByteArray()));
        }

    }
}

