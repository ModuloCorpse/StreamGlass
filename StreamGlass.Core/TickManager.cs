using System.Diagnostics;
using System.Windows.Threading;

namespace StreamGlass.Core
{
    public class TickManager
    {
        public delegate void TickFunction(long deltaTime);

        private readonly List<TickFunction> m_TickFunctions = [];
        private readonly DispatcherTimer m_DispatcherTimer = new();
        private readonly Stopwatch m_Watch = new();
        private volatile bool m_IsTicking = false;

        public void RegisterTickFunction(TickFunction fct) => m_TickFunctions.Add(fct);
        public void UnregisterTickFunction(TickFunction fct) => m_TickFunctions.Remove(fct);

        public void Start()
        {
            if (!m_IsTicking)
            {
                m_DispatcherTimer.Tick += Tick;
                m_DispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
                m_Watch.Start();
                m_DispatcherTimer.Start();
                m_IsTicking = true;
            }
        }

        public void Stop()
        {
            if (m_IsTicking)
            {
                m_Watch.Stop();
                m_DispatcherTimer.Stop();
                m_DispatcherTimer.Tick -= Tick;
                m_IsTicking = false;
            }
        }

        private void Tick(object? sender, EventArgs e)
        {
            m_Watch.Stop();
            long deltaTime = m_Watch.ElapsedMilliseconds;
            foreach (TickFunction fct in m_TickFunctions)
                fct(deltaTime);
            m_Watch.Restart();
        }
    }
}
