using CorpseLib;
using CorpseLib.DataNotation;
using StreamGlass.Core.Profile;

namespace StreamGlass.Core
{
    public static class StreamGlassCanals
    {
        public class ACanalManager(string type)
        {
            private readonly Canal<DataNode> m_JCanal = new();
            private readonly string m_Type = type;

            public Canal<DataNode> JCanal => m_JCanal;
            public string Type => m_Type;

            protected void Emit(DataNode node) => m_JCanal.Emit(node);
        }

        public class TriggerCanalManager(string type, Canal canal) : ACanalManager(type)
        {
            private readonly Canal m_Canal = canal;
            public Canal Canal => m_Canal;
            public TriggerCanalManager(string type) : this(type, new()) { }

            public void Trigger()
            {
                m_Canal.Trigger();
                Emit(new DataValue());
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
                Emit(DataHelper.Cast(obj));
            }
        }

        public static readonly string SEND_MESSAGE = "send_message";
        public static readonly string CHAT_MESSAGE = "chat_message";
        public static readonly string UPDATE_STREAM_INFO = "update_stream_info";
        public static readonly string PROFILE_COMMANDS = "profile_commands";
        public static readonly string PROFILE_CHANGED_MENU_ITEM = "profile_changed_menu_item";
        public static readonly string PROFILE_RESET = "profile_reset";
        public static readonly string PROFILE_LOCK_ALL = "profile_lock_all";
        public static readonly string PROFILE_UNLOCK_ALL = "profile_unlock_all";

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
        public static void Register(string type, Action<DataNode?> action)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager))
                canalManager.JCanal.Register(action);
            else
                StreamGlassContext.LOGGER.Log(string.Format("Cannot find valid canal {0} to register to", type));
        }

        public static void Register<T>(string type, Action<T?> action)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager) &&
                canalManager is CanalManager<T> triggerCanalManager)
                triggerCanalManager.Canal.Register(action);
            else
                StreamGlassContext.LOGGER.Log(string.Format("Cannot find valid canal {0} to register to", type));
        }

        public static void Unregister(string type, Action action)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager) &&
                canalManager is TriggerCanalManager triggerCanalManager)
                triggerCanalManager.Canal.Unregister(action);
        }

        public static void Unregister(string type, Action<DataNode?> action)
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
            else
                StreamGlassContext.LOGGER.Log(string.Format("Cannot find valid canal {0} to trigger", type));
        }
        public static void Emit<T>(string type, T? arg)
        {
            if (ms_Managers.TryGetValue(type, out ACanalManager? canalManager) &&
                canalManager is CanalManager<T> triggerCanalManager)
                triggerCanalManager.Emit(arg);
            else
                StreamGlassContext.LOGGER.Log(string.Format("Cannot find valid canal {0} to emit to", type));
        }

        static StreamGlassCanals()
        {
            NewCanal<string>(SEND_MESSAGE);
            NewCanal<UserMessage>(CHAT_MESSAGE);
            NewCanal<UpdateStreamInfoArgs>(UPDATE_STREAM_INFO);
            NewCanal<ProfileCommandEventArgs>(PROFILE_COMMANDS);
            NewCanal<string>(PROFILE_CHANGED_MENU_ITEM);
            NewCanal(PROFILE_RESET);
            NewCanal(PROFILE_LOCK_ALL);
            NewCanal(PROFILE_UNLOCK_ALL);
        }
    }
}