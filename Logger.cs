using System.Collections.Generic;
using System.IO;

namespace StreamGlass
{
    public static class Logger
    {
        private static LogWindow? ms_LogWindow = null;
        private static readonly Dictionary<string, List<string>> ms_Logs = new();
        private static StreamWriter? ms_LogFile = null;
        private static bool ms_LogInFile = true;
        private static object ms_Lock = new();

        public static void Init()
        {
            if (!File.Exists("./log/StreamGlass.log"))
            {
                Directory.CreateDirectory("./log");
                ms_LogFile = new(File.Create("./log/StreamGlass.log"));
            }
            else
                ms_LogFile = File.AppendText("./log/StreamGlass.log");
        }

        public static void StopLogger()
        {
            ms_LogWindow?.Close();
            ms_LogInFile = false;
            ms_LogFile?.Flush();
            ms_LogFile?.Close();
        }

        public static void SetLogWindow(LogWindow? logWindow) => ms_LogWindow = logWindow;

        public static IReadOnlyCollection<string> GetCategories()
        {
            lock (ms_Lock)
            {
                return ms_Logs.Keys;
            }
        }

        public static IReadOnlyList<string> GetLogs(string category)
        {
            lock (ms_Lock)
            {
                if (ms_Logs.TryGetValue(category, out List<string>? logs))
                    return logs.AsReadOnly();
                return new List<string>().AsReadOnly();
            }
        }

        public static void SetCurrentLogCategory(string category) => ms_LogWindow?.SetCurrentLogCategory(category);

        public static void Log(string category, string log)
        {
            lock (ms_Lock)
            {
                if (ms_Logs.TryGetValue(category, out List<string>? logs))
                    logs.Add(log);
                else
                    ms_Logs.Add(category, new() { log });
                if (ms_LogInFile)
                    ms_LogFile?.WriteLine(string.Format("--LOG [{0}]: {1}", category, log));
            }
            ms_LogWindow?.UpdateLogs();
        }
    }
}
