#nullable enable
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Psix
{
    public enum LogLevel
    {
        Verbose = -1,
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Off = 5
    }

    public class PsixLogger
    {
        UnityEngine.Object? _context;
        string _name;

        public Psix.LogLevel Level;

        private static List<PsixLogger> loggers = new List<PsixLogger>();

        private static Psix.LogLevel _globalLevel = Psix.LogLevel.Info;
        public static Psix.LogLevel GlobalLevel
        {
            get
            {
                return _globalLevel;
            }
            set
            {
                foreach (var logger in loggers)
                {
                    logger.Level = value;
                }
                _globalLevel = value;
            }
        }

        /** Using the log methods reduce the need for stack tracing, but
        * also cause the trace to lead to this source. In addition, stack
        * tracing is slow.
        */
        public static LogOption GlobalLogOption = LogOption.NoStacktrace;

        public PsixLogger(string name, UnityEngine.Object? context = null)
        {
            _context = context;
            _name = String.Format("[{0}] ", name);
            Level = GlobalLevel;
            loggers.Add(this);
        }
        ~PsixLogger()
        {
            loggers.Remove(this);
        }

        public void Verbose(string fmt, params object[] args) { Log(LogLevel.Verbose, fmt, args); }
        public void Trace(string fmt, params object[] args) { Log(LogLevel.Trace, fmt, args); }
        public void Debug(string fmt, params object[] args) { Log(LogLevel.Debug, fmt, args); }
        public void Info(string fmt, params object[] args) { Log(LogLevel.Info, fmt, args); }
        public void Warning(string fmt, params object[] args) { Log(LogLevel.Warning, fmt, args); }
        public void Error(string fmt, params object[] args) { Log(LogLevel.Error, fmt, args); }

        /* Prefer Warning for naming consistency */
        public void Warn(string fmt, params object[] args) { Log(LogLevel.Warning, fmt, args); }

        public bool IsEnabledFor(Psix.LogLevel level){
            // level is message, Level is this logger. If Level is higher, message has too low debug level.
            return (int)level >= (int)Level;
        }

        protected void Log(Psix.LogLevel level, string fmt, params object[] args)
        {
            if (!IsEnabledFor(level))
            return;

#if !UNITY_EDITOR && UNITY_ANDROID
            if (AndroidGatt.Android != null)
                AndroidGatt.Android.Call("androidBluetoothLog", _name + String.Format(fmt, args));
#elif !UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)
	        IosGatt._iOSBluetoothLELog(_name + String.Format(fmt, args));
#else

            const string colorFmt = "<color=#{0:X2}{1:X2}{2:X2}>{3}";
            LogType t;
            switch (level)
            {
                case LogLevel.Warning:
                    fmt = _name + fmt;
                    t = LogType.Warning;
                    break;
                case LogLevel.Error:
                    fmt = _name + fmt;
                    t = LogType.Error;
                    break;
                case LogLevel.Debug:
                    fmt = string.Format(colorFmt, (byte)150, (byte)150, (byte)200, _name) + fmt + "</color>";
                    t = LogType.Log;
                    break;
                case LogLevel.Trace:
                    fmt = string.Format(colorFmt, (byte)130, (byte)130, (byte)100, _name) + fmt + "</color>";
                    t = LogType.Log;
                    break;
                case LogLevel.Verbose:
                    // Keep it simple for speed.
                    fmt = _name + fmt;
                    t = LogType.Log;
                    break;
                default:
                    fmt = _name + fmt;
                    t = LogType.Log;
                    break;
            }
            try
            {
                UnityEngine.Debug.LogFormat(t, GlobalLogOption, _context, fmt, args);
            }
            catch (FormatException e)
            {
                UnityEngine.Debug.LogError(e.ToString());
                UnityEngine.Debug.Log("Did a log message contain curly brackets? Dump:");
                UnityEngine.Debug.Log(fmt);
                foreach (var arg in args)
                    UnityEngine.Debug.Log(arg);
                UnityEngine.Debug.Log("--end of dump--");
            }
#endif
        }
    }

    class LogUtility : MonoBehaviour
    {
        [SerializeField] private Psix.LogLevel _level;
        [SerializeField] private LogOption _logOption = LogOption.NoStacktrace;
        private PsixLogger logger;
        LogUtility() : base()
        {
            logger = new PsixLogger("LogUtility", this);
        }
        void Awake()
        {
            PsixLogger.GlobalLevel = _level;
            PsixLogger.GlobalLogOption = _logOption;
            logger.Info("Logging level set to {0}", _level.ToString());
        }
    }

}
