using System.Management;

namespace StreamGlass.Core
{
    public class StreamGlassProcessListener
    {
        public abstract class Handler
        {
            internal void ProcessAlreadyStarted() => OnProcessAlreadyStarted();
            internal void ProcessStart() => OnProcessStart();
            internal void NewProcessStart() => OnNewProcessStart();
            internal void ProcessAlreadyStopped() => OnProcessAlreadyStopped();
            internal void ProcessStop() => OnProcessStop();
            internal void NewProcessStop() => OnNewProcessStop();

            protected virtual void OnProcessAlreadyStarted() => OnProcessStart();
            protected abstract void OnProcessStart();
            protected abstract void OnNewProcessStart();

            protected virtual void OnProcessAlreadyStopped() => OnProcessStop();
            protected abstract void OnProcessStop();
            protected abstract void OnNewProcessStop();

        }

        private class ProcessListener : IDisposable
        {
            private readonly ManagementEventWatcher m_Watcher;
            private readonly List<Handler> m_Handlers = [];
            private readonly string m_ProcessName;
            private int m_Instance = 0;
            private bool m_HasRun = false;

            public bool HaveListener => m_Handlers.Count > 0;

            public ProcessListener(string processName)
            {
                m_ProcessName = processName;
                string queryString = $"SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = '{m_ProcessName}'";
                string scope = @"\\.\root\CIMV2";
                m_Watcher = new(scope, queryString);
                m_Watcher.EventArrived += new EventArrivedEventHandler(OnEventArrived);
                m_Watcher.Start();

                string isRunningQuery = $"SELECT * FROM Win32_Process Where Name='{m_ProcessName}'";
                ManagementObjectSearcher searcher = new(scope, isRunningQuery);
                ManagementObjectCollection queryCollection = searcher.Get();
                m_Instance = queryCollection.Count;
            }

            public void AddProcessHandler(Handler handler)
            {
                m_Handlers.Add(handler);
                if (m_Instance > 0)
                    handler.ProcessAlreadyStarted();
                else if (m_Instance == 0 && m_HasRun)
                    handler.ProcessAlreadyStopped();
            }

            public void RemoveProcessHandler(Handler handler) => m_Handlers.Remove(handler);

            private void OnEventArrived(object sender, EventArrivedEventArgs e)
            {
                try
                {
                    string eventName = e.NewEvent.ClassPath.ClassName;
                    if (eventName == "__InstanceCreationEvent")
                    {
                        m_HasRun = true;
                        ++m_Instance;
                        foreach (Handler handler in m_Handlers)
                        {
                            handler.NewProcessStart();
                            if (m_Instance == 1)
                                handler.ProcessStart();
                        }
                    }
                    else if (eventName == "__InstanceDeletionEvent")
                    {
                        --m_Instance;
                        foreach (Handler handler in m_Handlers)
                        {
                            handler.NewProcessStop();
                            if (m_Instance == 0)
                                handler.ProcessStop();
                        }
                    }
                } catch { }
            }

            public void Dispose()
            {
                m_Watcher.Stop();
                m_Watcher.Dispose();
            }
        }

        private static readonly Dictionary<string, ProcessListener> ms_ProcessListeners = [];

        public static void RegisterProcessHandler(string processName, Handler handler)
        {
            if (ms_ProcessListeners.TryGetValue(processName, out ProcessListener? listener))
                listener.AddProcessHandler(handler);
            else
            {
                ProcessListener processListener = new(processName);
                processListener.AddProcessHandler(handler);
                ms_ProcessListeners[processName] = processListener;
            }
        }

        public static void UnregisterProcessHandler(string processName, Handler handler)
        {
            if (ms_ProcessListeners.TryGetValue(processName, out ProcessListener? listener))
            {
                listener.RemoveProcessHandler(handler);
                if (!listener.HaveListener)
                {
                    listener.Dispose();
                    ms_ProcessListeners.Remove(processName);
                }
            }
        }
    }
}
