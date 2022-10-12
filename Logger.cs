namespace StreamGlass
{
    public static class Logger
    {
        private static LogWindow? ms_LogWindow = null;

        public static void InitLogWindow(LogWindow logWindow) => ms_LogWindow = logWindow;

        public static void SetCurrentLogCategory(string category) => ms_LogWindow?.SetCurrentLogCategory(category);
        public static void Log(string category, string log) => ms_LogWindow?.Log(category, log);
    }
}
