using CorpseLib;
using CorpseLib.Json;
using StreamGlass.Core.Profile;

namespace StreamGlass.Core
{
    public static class StreamGlassCanals
    {
        public class ACanalManager(string type)
        {
            private readonly Canal<JNode> m_JCanal = new();
            private readonly string m_Type = type;

            public Canal<JNode> JCanal => m_JCanal;
            public string Type => m_Type;

            protected void Emit(JNode node) => m_JCanal.Emit(node);
        }

        public class TriggerCanalManager(string type, Canal canal) : ACanalManager(type)
        {
            private readonly Canal m_Canal = canal;
            public Canal Canal => m_Canal;
            public TriggerCanalManager(string type) : this(type, new()) { }

            public void Trigger()
            {
                m_Canal.Trigger();
                Emit(new JNull());
            }
        }

        private class CanalManager<T>(string type, Canal<T> canal) : ACanalManager(type)
        {
            private readonly Canal<T> m_Canal = canal;
            public Canal<T> Canal => m_Canal;
            public CanalManager(string type) : this(type, new()) { }

            public void Emit(T? obj)
            {
                m_Canal.Emit(obj);
                Emit(JHelper.Cast(obj));
            }
        }

        private static readonly Dictionary<string, ACanalManager> ms_Managers = [];

        public static ACanalManager[] Managers => [.. ms_Managers.Values];

        public static event Action<ACanalManager>? OnManagerAdded;
        public static event Action<ACanalManager>? OnManagerRemoved;

        public static bool RemoveCanal(string type)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager))
            {
                OnManagerRemoved?.Invoke(canalManager);
                ms_Managers.Remove(type);
                return true;
            }
            return false;
        }

        private static bool AddCanalManager(ACanalManager canalManager)
        {
            if (!ms_Managers.ContainsKey(canalManager.Type))
            {
                ms_Managers[canalManager.Type] = canalManager;
                OnManagerAdded?.Invoke(canalManager);
                return true;
            }
            return false;
        }

        public static void NewCanal(string type) => NewCanal(type, new());
        public static void NewCanal(string type, Canal canal) => AddCanalManager(new TriggerCanalManager(type, canal));
        public static void NewCanal<T>(string type) => NewCanal<T>(type, new());
        public static void NewCanal<T>(string type, Canal<T> canal) => AddCanalManager(new CanalManager<T>(type, canal));

        public static void Register(string type, Action action)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager) &&
                canalManager is TriggerCanalManager triggerCanalManager)
                triggerCanalManager.Canal.Register(action);
        }
        public static void Register(string type, Action<JNode?> action)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager))
                canalManager.JCanal.Register(action);
        }

        public static void Register<T>(string type, Action<T?> action)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager) &&
                canalManager is CanalManager<T> triggerCanalManager)
                triggerCanalManager.Canal.Register(action);
        }

        public static void Unregister(string type, Action action)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager) &&
                canalManager is TriggerCanalManager triggerCanalManager)
                triggerCanalManager.Canal.Unregister(action);
        }

        public static void Unregister(string type, Action<JNode?> action)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager))
                canalManager.JCanal.Unregister(action);
        }

        public static void Unregister<T>(string type, Action<T?> action)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager) &&
                canalManager is CanalManager<T> triggerCanalManager)
                triggerCanalManager.Canal.Unregister(action);
        }

        public static void Trigger(string type)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager) &&
                canalManager is TriggerCanalManager triggerCanalManager)
                triggerCanalManager.Trigger();
        }
        public static void Emit<T>(string type, T? arg)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager) &&
                canalManager is CanalManager<T> triggerCanalManager)
                triggerCanalManager.Emit(arg);
        }

        static StreamGlassCanals()
        {
            NewCanal<UserMessage>("chat_message");
            NewCanal<UpdateStreamInfoArgs>("update_stream_info");
            NewCanal("stream_start");
            NewCanal("stream_stop");
            NewCanal<ProfileCommandEventArgs>("profile_commands");
            NewCanal<string>("profile_changed_menu_item");
            NewCanal("chat_clear");
        }
    }
}