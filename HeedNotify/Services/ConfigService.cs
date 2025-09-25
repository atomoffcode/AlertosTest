using System;
using System.IO;
using System.Text.Json;
using HeedNotify.Models;
using HeedNotify.Utils;

namespace HeedNotify.Services
{
    public static class ConfigService
    {
        private static readonly object Sync = new();
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        public static AppConfig Current { get; private set; } = new();

        public static string GlobalConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "HeedNotify", "config.json");

        public static string UserConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HeedNotify", "config.json");

        public static void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GlobalConfigPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(UserConfigPath)!);
            Load();
            Logger.Info($"Config loaded. Enabled={Current.Enabled}, schedules={Current.Schedules.Count}");
        }

        public static void Reload()
        {
            Load();
            Logger.Info("Config reloaded.");
        }

        public static void SetEnabled(bool enabled)
        {
            lock (Sync)
            {
                Current.Enabled = enabled;
                SaveUserConfig();
            }
            Logger.Info($"Enabled set to {enabled}");
        }

        private static void SaveUserConfig()
        {
            try
            {
                var json = JsonSerializer.Serialize(Current, JsonOptions);
                File.WriteAllText(UserConfigPath, json);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to write user config: {ex}");
            }
        }

        private static void Load()
        {
            lock (Sync)
            {
                var merged = new AppConfig();

                try
                {
                    if (File.Exists(GlobalConfigPath))
                    {
                        var g = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(GlobalConfigPath), JsonOptions);
                        if (g != null) merged = g;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error reading global config: {ex}");
                }

                try
                {
                    if (File.Exists(UserConfigPath))
                    {
                        var u = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(UserConfigPath), JsonOptions);
                        if (u != null)
                        {
                            merged.Enabled = u.Enabled;
                            merged.DefaultDurationSeconds = u.DefaultDurationSeconds != 0 ? u.DefaultDurationSeconds : merged.DefaultDurationSeconds;
                            if (u.Quiet != null) merged.Quiet = u.Quiet;
                            if (u.Schedules != null && u.Schedules.Count > 0) merged.Schedules = u.Schedules;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error reading user config: {ex}");
                }

                Current = merged;
            }
        }
    }
}
