using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using OBSCorpse;
using StreamGlass.Core;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StreamGlass.API.Timer
{
    public class TimerEndpoint : AHTTPEndpoint
    {
        private readonly Dictionary<string, FileCountdownTimeAction> m_Timers = [];
        private readonly HashSet<string> m_FileToClear = [];

        public TimerEndpoint() : base("/timer") { }

        ~TimerEndpoint()
        {
            foreach (var pair in m_Timers)
                pair.Value.Stop();
            foreach (string path in m_FileToClear)
                File.WriteAllText(path, string.Empty);
        }

        protected override Response OnPostRequest(Request request)
        {
            try
            {
                JFile jfile = new(request.Body);
                string path = jfile.Get<string>("path")!;
                if (jfile.TryGet("duration", out long duration))
                {
                    string endMessage = jfile.GetOrDefault("end", string.Empty);
                    if (m_Timers.TryGetValue(path, out var oldTimer))
                        oldTimer.Stop();
                    FileCountdownTimeAction timer = string.IsNullOrEmpty(endMessage) ? new(path, duration) : new(path, endMessage, duration);
                    timer.OnFinish += (sender, e) => m_Timers.Remove(path);
                    timer.OnStop += (sender, e) => m_Timers.Remove(path);
                    m_Timers[path] = timer;
                    m_FileToClear.Add(path);
                    timer.Start();
                    if (jfile.TryGet("ads_duration", out uint adsDuration))
                    {
                        if (jfile.TryGet("ads_delay", out int adsDelay))
                            Task.Delay(adsDelay * 1000).ContinueWith(t => StreamGlassCanals.Emit("start_ads", adsDuration));
                        else
                            StreamGlassCanals.Emit("start_ads", adsDuration);
                    }
                }
                else
                {
                    if (m_Timers.TryGetValue(path, out FileCountdownTimeAction? timeAction))
                        timeAction.Stop();
                    File.WriteAllText(path, string.Empty);
                }
            }
            catch
            {
                return new(400, "Bad Request", "Request body isn't a well-formed json");
            }
            return base.OnPostRequest(request);
        }
    }
}
