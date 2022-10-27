using System.Collections.Generic;

namespace StreamGlass
{
    public static class CanalManager
    {
        public delegate void CanalMessageListener<T>(int canalID, T args);
        public delegate void CanalTriggerListener(int canalID);
        private static readonly Dictionary<int, object> m_Canals = new();

        public static bool NewCanal<T>(dynamic canalID)
        {
            int id = (int)canalID;
            if (m_Canals.ContainsKey(id))
                return false;
            ConcurrentList<CanalMessageListener<T>> newCanal = new();
            m_Canals[id] = newCanal;
            return true;
        }

        public static bool NewCanal(dynamic canalID)
        {
            int id = (int)canalID;
            if (m_Canals.ContainsKey(id))
                return false;
            ConcurrentList<CanalTriggerListener> newCanal = new();
            m_Canals[id] = newCanal;
            return true;
        }

        public static bool Register<T>(dynamic canalID, CanalMessageListener<T> listener)
        {
            if (m_Canals.TryGetValue((int)canalID, out var canalVar) && canalVar is ConcurrentList<CanalMessageListener<T>> canal)
            {
                canal.Add(listener);
                return true;
            }
            return false;
        }

        public static bool Register(dynamic canalID, CanalTriggerListener trigger)
        {
            if (m_Canals.TryGetValue((int)canalID, out var canalVar) && canalVar is ConcurrentList<CanalTriggerListener> canal)
            {
                canal.Add(trigger);
                return true;
            }
            return false;
        }

        public static bool Unregister<T>(dynamic canalID, CanalMessageListener<T> listener)
        {
            if (m_Canals.TryGetValue((int)canalID, out var canalVar) && canalVar is ConcurrentList<CanalMessageListener<T>> canal)
            {
                canal.Remove(listener);
                return true;
            }
            return false;
        }

        public static bool Unregister(dynamic canalID, CanalTriggerListener trigger)
        {
            if (m_Canals.TryGetValue((int)canalID, out var canalVar) && canalVar is ConcurrentList<CanalTriggerListener> canal)
            {
                canal.Remove(trigger);
                return true;
            }
            return false;
        }

        public static bool Emit<T>(dynamic canalID, T args)
        {
            int id = (int)canalID;
            if (m_Canals.TryGetValue(id, out var canalVar) && canalVar is ConcurrentList<CanalMessageListener<T>> canal)
            {
                canal.AsyncIterate((messageListener) => messageListener(id, args));
                return true;
            }
            return false;
        }

        public static bool Emit(dynamic canalID)
        {
            int id = (int)canalID;
            if (m_Canals.TryGetValue(id, out var canalVar) && canalVar is ConcurrentList<CanalTriggerListener> canal)
            {
                canal.AsyncIterate((messageTrigger) => messageTrigger(id));
                return true;
            }
            return false;
        }
    }
}
