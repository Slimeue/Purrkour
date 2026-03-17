using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public class DebuggerManager : MonoBehaviour
    {
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }


        public static DebuggerManager Instance;


        [FormerlySerializedAs("_showLogs")] public bool showLogs;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Logger Manager Log method<!--Logger-->
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        /// <param name="context"></param>
        public void Log(string message, LogLevel logLevel = LogLevel.Info, Object context = null)
        {
            if (!showLogs) return;

            string prefix = $"[{logLevel}] ";
            switch (logLevel)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(prefix + message, context);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(prefix + message, context);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    Debug.LogError(prefix + message, context);
                    break;
            }
        }

        public void DebugLog(string msg, Object context = null) => Log(msg, LogLevel.Debug, context);
        public void Info(string msg, Object context = null) => Log(msg, LogLevel.Info, context);
        public void Warn(string msg, Object context = null) => Log(msg, LogLevel.Warning, context);
        public void Error(string msg, Object context = null) => Log(msg, LogLevel.Error, context);
    }
}