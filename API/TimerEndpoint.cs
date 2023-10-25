﻿using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using OBSCorpse;
using System;
using System.Collections.Generic;

namespace StreamGlass.API
{
    public class TimerEndpoint : AHTTPEndpoint
    {
        private readonly Dictionary<string, FileCountdownTimeAction> m_Timers = new();

        public TimerEndpoint() : base("/timer") { }

        protected override Response OnPostRequest(Request request)
        {
            try
            {
                JFile jfile = new(request.Body);
                long duration = jfile.Get<long>("duration")!;
                string path = jfile.Get<string>("path")!;
                string endMessage = jfile.GetOrDefault("end", string.Empty);
                if (m_Timers.TryGetValue(path, out var oldTimer))
                    oldTimer.Stop();
                FileCountdownTimeAction timer = (string.IsNullOrEmpty(endMessage)) ? new(path, duration) : new(path, endMessage, duration);
                timer.OnFinish += (object? sender, EventArgs e) => m_Timers.Remove(path);
                m_Timers[path] = timer;
                timer.Start();
            }
            catch
            {
                return new(400, "Bad Request", "Request body isn't a well-formed json");
            }
            return base.OnPostRequest(request);
        }
    }
}
