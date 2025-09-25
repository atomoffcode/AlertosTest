using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using HeedNotify.Models;
using HeedNotify.Utils;

namespace HeedNotify.Services
{
	public sealed class SchedulerService : IDisposable
	{
		private readonly Timer timer;
		private readonly Dictionary<string, DateTime> lastFireMapUtc = new();

		public SchedulerService()
		{
			timer = new Timer(15000) { AutoReset = true };
			timer.Elapsed += (_, __) => Tick();
		}

		public void Start()
		{
			Logger.Info("Scheduler started.");
			timer.Start();
			Tick();
		}

		private static bool IsWithinQuietHours(AppConfig cfg, DateTime localNow)
		{
			if (cfg.Quiet == null) return false;
			if (!TimeOnly.TryParse(cfg.Quiet.Start, out var qs)) return false;
			if (!TimeOnly.TryParse(cfg.Quiet.End, out var qe)) return false;
			var now = TimeOnly.FromDateTime(localNow);
			return qs < qe ? (now >= qs && now < qe) : (now >= qs || now < qe);
		}

		private void Tick()
		{
			try
			{
				var cfg = ConfigService.Current;
				if (!cfg.Enabled) return;
				var localNow = DateTime.Now;
				if (IsWithinQuietHours(cfg, localNow)) return;

				foreach (var rule in cfg.Schedules.ToList())
				{
					try
					{
						if (!IsActiveToday(rule, localNow)) continue;

						if (rule.Times != null && rule.Times.Count > 0)
						{
							foreach (var t in rule.Times)
							{
								if (!TimeOnly.TryParse(t, out var time)) continue;
								var fireLocal = localNow.Date + time.ToTimeSpan();
								if (Math.Abs((localNow - fireLocal).TotalMinutes) <= 0.5)
									FireIfNotRecently(rule, localNow);
							}
						}

						if (rule.RepeatMinutes.HasValue && rule.RepeatMinutes.Value > 0)
						{
							var minutes = rule.RepeatMinutes.Value;
							if (rule.ActiveBetween != null &&
								TimeOnly.TryParse(rule.ActiveBetween.Start, out var start) &&
								TimeOnly.TryParse(rule.ActiveBetween.End, out var end))
							{
								var startLocal = localNow.Date + start.ToTimeSpan();
								var endLocal = localNow.Date + end.ToTimeSpan();
								if (endLocal <= startLocal) endLocal = endLocal.AddDays(1);
								if (localNow >= startLocal && localNow <= endLocal)
								{
									var elapsed = (localNow - startLocal).TotalMinutes;
									if (elapsed >= 0 && Math.Abs(elapsed % minutes) <= 0.5)
										FireIfNotRecently(rule, localNow);
								}
							}
							else
							{
								var midnight = localNow.Date;
								var elapsed = (localNow - midnight).TotalMinutes;
								if (Math.Abs(elapsed % minutes) <= 0.5)
									FireIfNotRecently(rule, localNow);
							}
						}
					}
					catch (Exception exRule)
					{
						Logger.Error($"Rule error ({rule.Id}): {exRule}");
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Scheduler tick error: {ex}");
			}
		}

		private static bool IsActiveToday(ScheduleRule rule, DateTime localNow)
		{
			if (rule.DaysOfWeek == null || rule.DaysOfWeek.Length == 0) return true;
			var token = localNow.DayOfWeek switch
			{
				DayOfWeek.Monday => "Mon",
				DayOfWeek.Tuesday => "Tue",
				DayOfWeek.Wednesday => "Wed",
				DayOfWeek.Thursday => "Thu",
				DayOfWeek.Friday => "Fri",
				DayOfWeek.Saturday => "Sat",
				DayOfWeek.Sunday => "Sun",
				_ => "Mon"
			};
			return rule.DaysOfWeek.Contains(token, StringComparer.OrdinalIgnoreCase);
		}

		private void FireIfNotRecently(ScheduleRule rule, DateTime localNow)
		{
			var id = rule.Id ?? $"{rule.Title}|{rule.Message}";
			var nowUtc = localNow.ToUniversalTime();
			if (lastFireMapUtc.TryGetValue(id, out var last) && (nowUtc - last).TotalSeconds < 40)
				return;
			lastFireMapUtc[id] = nowUtc;
			var durationSeconds = rule.DurationSeconds ?? ConfigService.Current.DefaultDurationSeconds;
			_ = NotificationService.ShowAsync(rule.Title, rule.Message, TimeSpan.FromSeconds(durationSeconds));
			Logger.Info($"Fired rule {id}: \"{rule.Title}\"");
		}

		public void Dispose()
		{
			timer.Stop();
			timer.Dispose();
		}
	}
}
