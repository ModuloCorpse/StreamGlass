using CorpseLib.Json;
using CorpseLib.Logging;
using CorpseLib.Placeholder;
using CorpseLib.StructuredText;
using StreamGlass.Core.Profile;
using StreamGlass.Core.Stat;

namespace StreamGlass.Core
{
    public class StreamGlassContext : Context
    {
        public static readonly Logger LOGGER = new("[${d}-${M}-${y} ${h}:${m}:${s}.${ms}] ${log}") { new LogInFile("./log/${y}${M}${d}${h}.log") };

        private static readonly StatisticManager ms_Statistics = new();
        private static readonly Dictionary<string, Function> ms_Functions = [];

        public static StatisticManager Statistics => ms_Statistics;

        public static void RegisterFunction(string functionName, Function fct) => ms_Functions[functionName] = fct;
        public static void UnregisterFunction(string functionName) => ms_Functions.Remove(functionName);

        public static void CreateStatistic(string statistic) => ms_Statistics.CreateStatistic(statistic);
        public static void UpdateStatistic(string statistic, object value) => ms_Statistics.UpdateStatistic(statistic, value);
        public static T? GetStatistic<T>(string statistic) => ms_Statistics.Get<T>(statistic);
        public static T GetStatistic<T>(string statistic, T defaultValue) => ms_Statistics.GetOr(statistic, defaultValue);

        public static void Init()
        {
            JHelper.RegisterSerializer(new Text.JSerializer());
            JHelper.RegisterSerializer(new Section.JSerializer());
            JHelper.RegisterSerializer(new UserMessage.JSerializer());
            JHelper.RegisterSerializer(new ProfileCommandEventArgs.JSerializer());
            JHelper.RegisterSerializer(new CategoryInfo.JSerializer());
            JHelper.RegisterSerializer(new UpdateStreamInfoArgs.JSerializer());

            ms_Statistics.Load();
        }

        public static void Delete()
        {
            ms_Statistics.Save();
        }

        public override string? Call(string functionName, string[] args)
        {
            if (ms_Functions.TryGetValue(functionName, out Function? func))
                return func(args);
            return base.Call(functionName, args);
        }

        public override string? GetVariable(string name) => ms_Statistics.Get(name)?.ToString() ?? base.GetVariable(name);
    }
}
