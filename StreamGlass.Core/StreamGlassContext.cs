using CorpseLib.Json;
using CorpseLib.Logging;
using CorpseLib.Placeholder;
using CorpseLib.StructuredText;
using StreamGlass.Core.Audio;
using StreamGlass.Core.Profile;
using StreamGlass.Core.Stat;

namespace StreamGlass.Core
{
    public class StreamGlassContext : Context
    {
        public static readonly Logger LOGGER = new("[${d}-${M}-${y} ${h}:${m}:${s}.${ms}] ${log}") { new LogInFile("./log/${y}${M}${d}${h}.log") };

        private static readonly StringSourceManager ms_StringSources = new();
        private static readonly Dictionary<string, Function> ms_Functions = [];
        private static readonly Dictionary<string, string> ms_Variables = [];

        public static StringSourceManager StringSources => ms_StringSources;

        public static void RegisterFunction(string functionName, Function fct) => ms_Functions[functionName] = fct;
        public static void UnregisterFunction(string functionName) => ms_Functions.Remove(functionName);

        public static void RegisterVariable(string variableName, string variable) => ms_Variables[variableName] = variable;
        public static void UnregisterVariable(string variableName) => ms_Variables.Remove(variableName);

        public static void RegisterAggregator(StringSourceManager.AggregatorMaker aggregatorMaker) => ms_StringSources.RegisterAggregator(aggregatorMaker);
        public static void CreateStringSource(string source) => ms_StringSources.CreateStringSource(source);
        public static void UpdateStringSource(string source, string value) => ms_StringSources.UpdateStringSource(source, value);
        public static string GetStringSource(string source, string defaultValue) => ms_StringSources.GetOr(source, defaultValue);

        public static void Init()
        {
            JsonHelper.RegisterSerializer(new Text.JSerializer());
            JsonHelper.RegisterSerializer(new Section.JSerializer());
            JsonHelper.RegisterSerializer(new UserMessage.JSerializer());
            JsonHelper.RegisterSerializer(new ProfileCommandEventArgs.JSerializer());
            JsonHelper.RegisterSerializer(new CategoryInfo.JSerializer());
            JsonHelper.RegisterSerializer(new UpdateStreamInfoArgs.JSerializer());
            JsonHelper.RegisterSerializer(new Sound.JsonSerializer());
        }

        public static void AfterPluginInit()
        {
            ms_StringSources.Load();
        }

        public static void Delete(JsonObject json)
        {
            ms_StringSources.SaveTo(json);
        }

        public override string? Call(string functionName, string[] args, Cache cache)
        {
            if (ms_Functions.TryGetValue(functionName, out Function? func))
                return func(args, cache);
            return base.Call(functionName, args, cache);
        }

        public override string? GetVariable(string name)
        {
            if (ms_Variables.TryGetValue(name, out string? variable))
                return variable;
            return ms_StringSources.Get(name) ?? base.GetVariable(name);
        }
    }
}
