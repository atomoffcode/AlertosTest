using System;
using System.IO;
using System.Text;

namespace HeedNotify.Utils
{
    public static class Logger
    {
        private static readonly object Sync = new();
        public static string LogDirectory { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HeedNotify", "logs");

        private static string CurrentFile =>
            Path.Combine(LogDirectory, $"HeedNotify-{DateTime.Now:yyyyMMdd}.log");

        public static void Initialize()
        {
            Directory.CreateDirectory(LogDirectory);
            Info("Logger initialized.");
        }

        public static void Info(string message) => Write("INFO", message);
        public static void Error(string message) => Write("ERROR", message);

        private static void Write(string level, string message)
        {
            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
                lock (Sync)
                {
                    File.AppendAllText(CurrentFile, line, Encoding.UTF8);
                }
            }
            catch { }
        }
    }
}
