using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HeedNotify.Models
{
    public class AppConfig
    {
        public bool Enabled { get; set; } = true;
        public List<ScheduleRule> Schedules { get; set; } = new();
        public QuietHours? Quiet { get; set; }
        public int DefaultDurationSeconds { get; set; } = 5;
    }

    public class QuietHours
    {
        public string Start { get; set; } = "22:00";
        public string End { get; set; } = "06:00";
    }

    public class ScheduleRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = "Reminder";
        public string Message { get; set; } = "Stay focused.";
        public List<string>? Times { get; set; }
        public int? RepeatMinutes { get; set; }
        public string[]? DaysOfWeek { get; set; }
        public ActiveBetween? ActiveBetween { get; set; }
        public int? DurationSeconds { get; set; }
        [JsonIgnore] public DateTime? LastFiredUtc { get; set; }
    }

    public class ActiveBetween
    {
        public string Start { get; set; } = "08:00";
        public string End { get; set; } = "18:00";
    }
}
