using CorpseLib.Json;
using CorpseLib.Placeholder;
using CorpseLib.StructuredText;
using StreamGlass.Events;
using StreamGlass.Profile;
using StreamGlass.Stat;
using StreamGlass.StreamChat;
using StreamGlass.Twitch;
using System.Collections.Generic;
using TwitchCorpse;

namespace StreamGlass
{
    public class StreamGlassContext : Context
    {
        private static readonly StatisticManager ms_Statistics = new();
        private static readonly Dictionary<string, Function> ms_Functions = new();

        public static StatisticManager Statistics => ms_Statistics;

        public static void RegisterFunction(string functionName, Function fct) => ms_Functions[functionName] = fct;
        public static void UnregisterFunction(string functionName) => ms_Functions.Remove(functionName);

        public static void UpdateStatistic(string statistic, object value) => ms_Statistics.UpdateStatistic(statistic, value);
        public static T? GetStatistic<T>(string statistic) => ms_Statistics.Get<T>(statistic);
        public static T GetStatistic<T>(string statistic, T defaultValue) => ms_Statistics.GetOr(statistic, defaultValue);

        internal static void Init()
        {
            JHelper.RegisterSerializer(new TwitchUser.JSerializer());
            JHelper.RegisterSerializer(new Text.JSerializer());
            JHelper.RegisterSerializer(new Section.JSerializer());
            JHelper.RegisterSerializer(new UserMessage.JSerializer());
            JHelper.RegisterSerializer(new BanEventArgs.JSerializer());
            JHelper.RegisterSerializer(new DonationEventArgs.JSerializer());
            JHelper.RegisterSerializer(new FollowEventArgs.JSerializer());
            JHelper.RegisterSerializer(new GiftFollowEventArgs.JSerializer());
            JHelper.RegisterSerializer(new MessageAllowedEventArgs.JSerializer());
            JHelper.RegisterSerializer(new RaidEventArgs.JSerializer());
            JHelper.RegisterSerializer(new RewardEventArgs.JSerializer());
            JHelper.RegisterSerializer(new CommandEventArgs.JSerializer());
            JHelper.RegisterSerializer(new CategoryInfo.JSerializer());
            JHelper.RegisterSerializer(new UpdateStreamInfoArgs.JSerializer());

            ms_Statistics.Load();

            ms_Statistics.CreateStatistic("viewer_count");
            ms_Statistics.CreateStatistic("last_bits_donor");
            ms_Statistics.CreateStatistic("last_bits_donation");
            ms_Statistics.CreateStatistic("top_bits_donor");
            ms_Statistics.CreateStatistic("top_bits_donation");
            ms_Statistics.CreateStatistic("last_follow");
            ms_Statistics.CreateStatistic("last_raider");
            ms_Statistics.CreateStatistic("last_gifter");
            ms_Statistics.CreateStatistic("last_nb_gift");
            ms_Statistics.CreateStatistic("top_gifter");
            ms_Statistics.CreateStatistic("top_nb_gift");
            ms_Statistics.CreateStatistic("last_sub");
        }

        internal static void Delete()
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
