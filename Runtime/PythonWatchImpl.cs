/// Copyright (c) 2024 Doublepoint Technologies Oy <hello@doublepoint.com>
/// Licensed under the MIT License. See LICENSE for details.

#nullable enable

using System;
using Google.Protobuf;
using System.IO;
using System.Collections.Concurrent;
using UnityEngine;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

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

        private string watchName = "";
        private string pythonPath;
        GameObject? receiverObject;
        StreamWriter? processInput;

        private static PsixLogger logger = new PsixLogger("PythonWatchImpl");

        /** Python path is used for installing a virtual environment, which is then
         * used for the rest
         */
        public PythonWatchImpl()
        {
            logger.Debug("PythonWatchImpl");
            this.pythonPath = "python";
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
            Thread? installThread;

            private ConcurrentQueue<byte[]> lineQueue = new();
            private AutoResetEvent newDataEvent = new AutoResetEvent(false);
            private string data_path = "";

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
                installThread = new Thread(StarterTask);
                data_path = Application.persistentDataPath;
                installThread.Start();
            }

            private void StarterTask()
            {
                // Create process start info
                List<string> python_alternatives = new List<string> { "python3.11", "python3.10", "python3", "python" };
                List<string> supported_versions = new List<string> { "3.11", "3.10", "3.9" };
                bool found_python = false;
                foreach (string nn in python_alternatives)
                {
                    var name = Application.platform == RuntimePlatform.WindowsEditor ? nn + ".exe" : nn;
                    if (IsProgramAvailable(name))
                    {
                        var version = GetPythonVersion(name);
                        if (supported_versions.Any(v => version.StartsWith(v)))
                        {
                            logger.Debug("Using python {0}", name);
                            found_python = true;
                            if (InstallPythonToUnity(name))
                            {
                                StartPythonToUnity();
                                return;
                            }
                        }
                    }
                }
                logger.Error("Installation of touch-sdk-python failed");
                if (!found_python)
                {
                    logger.Info("Supported versions are: {0}", String.Join(",", supported_versions));
                }
                // Remove venv folder if it didnt work; next time we may have better luck
                RecursiveDelete(GetVenvPath());
            }

            private string GetVenvPath()
            {
                return System.IO.Path.Combine(this.data_path, "sdk_venv");
            }

            private bool RunLogging(string cmd, string args)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = cmd;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;

                startInfo.Arguments = args;

                logger.Debug("Running `{0} {1}`", cmd, args);
                var proc = Process.Start(startInfo);
                // Non zero exit some times happens if already installed (windows?).
                while (!proc.StandardOutput.EndOfStream && !proc.HasExited)
                {
                    string line = proc.StandardOutput.ReadLine();
                    logger.Trace("{0}`", line);
                }
                var error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    logger.Debug("Failed to run {0}: {1} [{2}].", cmd, proc.ExitCode, error);
                    return false;
                }
                logger.Debug("Command succeeded");

                return true;
            }

            private static bool IsProgramAvailable(string programName)
            {
                string command = (Application.platform == RuntimePlatform.WindowsEditor) ? "where" : "which";

                ProcessStartInfo processInfo = new ProcessStartInfo {
                    FileName = command,           Arguments = programName, RedirectStandardOutput = true,
                    RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    return !string.IsNullOrWhiteSpace(output); // If output contains a path, the program exists.
                }
            }

            private static string GetPythonVersion(string pythonPath)
            {
                try
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo {
                        FileName = pythonPath,         Arguments = "-c \"import sys; print(sys.version)\"",
                        RedirectStandardOutput = true, RedirectStandardError = true,
                        UseShellExecute = false,       CreateNoWindow = true
                    };

                    using (Process process = new Process { StartInfo = processInfo })
                    {
                        process.Start();
                        string output = process.StandardOutput.ReadToEnd().Trim();
                        string errorOutput = process.StandardError.ReadToEnd().Trim();
                        process.WaitForExit();

                        // logger.Debug($"Python Standard Output: {output}");
                        // logger.Debug($"Python Error Output: {errorOutput}");

                        // Check if the output contains a valid Python version
                        if (!string.IsNullOrEmpty(output) && output.Contains("."))
                        {
                            return output; // This is a valid Python response
                        }

                        return string.Empty; // Likely a stub executable
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug($"Error checking Python version: {ex.Message}");
                    return string.Empty;
                }
            }

            private bool InstallPythonToUnity(string python)
            {
                string venvPath = GetVenvPath();

                string scripts = (Application.platform == RuntimePlatform.WindowsEditor) ? "Scripts" : "bin";
                string venv_python = (Application.platform == RuntimePlatform.WindowsEditor) ? "python.exe" : "python";
                venv_python = System.IO.Path.Combine(venvPath, scripts, venv_python);
                parent!.pythonPath = venv_python;

                if (!File.Exists(venv_python))
                {
                    logger.Info("installing venv at {0}. This may take some time.", venvPath);
                    var args = $"-m venv \"{venvPath}\""; // Hope that version is correct!

                    if (!RunLogging(python, args))
                        return false;
                }
                else
                    logger.Debug("Venv found at {0}", venvPath);

                // Cache nor default tmp dir are accessible from LocalLow
                string tmpDirPath = Path.Combine(this.data_path, "tmp");
                if (!Directory.Exists(tmpDirPath))
                {
                    Directory.CreateDirectory(tmpDirPath);
                }
                // The env variable seems to vary by platform and python version
                Environment.SetEnvironmentVariable("TMPDIR", tmpDirPath, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("TEMP", tmpDirPath, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("TMP", tmpDirPath, EnvironmentVariableTarget.Process);

                var sdk_arg = $"-m pip install touch-sdk>=0.7.0 --no-input --no-cache-dir"; // Hope that
                                                                                            // version is
                                                                                            // correct!
                logger.Debug("Installing touch-sdk python");
                bool result = RunLogging(venv_python, sdk_arg);
                if (result)
                    logger.Debug("Install completed successfully");
                return result;
            }

            public static void RecursiveDelete(DirectoryInfo baseDir)
            {
                if (!baseDir.Exists)
                    return;

                foreach (var dir in baseDir.EnumerateDirectories())
                {
                    RecursiveDelete(dir);
                }
                baseDir.Delete(true);
            }

            public static void RecursiveDelete(string baseDir)
            {
                RecursiveDelete(new DirectoryInfo(baseDir));
            }

            private void StartPythonToUnity()
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = parent!.pythonPath;
                var nameArgument = parent!.watchName == "" ? "" : $"--name-filter {parent!.watchName}";

                startInfo.Arguments = $"-m touch_sdk.stream_watch {nameArgument}";

                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                // Start the external process
                logger.Debug("Starting python sdk");
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

            private void ReadTask()
            {
                while (processOutput?.EndOfStream != true)
                {
                    lineQueue.Enqueue(Convert.FromBase64String(processOutput?.ReadLine()));
                    // PythonWatchImpl.logger.Debug("Line enqued");
                    newDataEvent.Set();
                }
            }

            private void ErrTask()
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
                if (installThread != null)
                {
                    installThread.Abort();
                    installThread.Join();
                    installThread = null;
                }
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
                if (parent != null)
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

        override public void Connect(string name = "")
        {
            this.watchName = name;

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
            var update = new Proto.InputUpdate { HapticEvent = new Proto.HapticEvent {
                Type = Proto.HapticEvent.Types.HapticType.Oneshot, Length = clampedLength, Intensity = clampedAmplitude
            } };

            processInput?.WriteLine(Convert.ToBase64String(update.ToByteArray()));
        }

        override public void CancelVibration()
        {
            var update = new Proto.InputUpdate {
                HapticEvent = new Proto.HapticEvent { Type = Proto.HapticEvent.Types.HapticType.Cancel }
            };

            processInput?.WriteLine(Convert.ToBase64String(update.ToByteArray()));
        }

        override public void RequestGestureDetection(Gesture gesture)
        {
            var update = new Proto.InputUpdate { ModelRequest =
                                                     new Proto.Model { Gestures = { (Proto.GestureType)(gesture) } } };
            processInput?.WriteLine(Convert.ToBase64String(update.ToByteArray()));
        }
    }
}
