using System;
using System.Globalization;

namespace HeedNotify.Utils
{
    public sealed class Cli
    {
        public sealed class OneOffNote
        {
            public string Title { get; init; } = "Notice";
            public string Message { get; init; } = "";
            public int DurationSeconds { get; init; } = 5;
        }

        public OneOffNote? OneOffNotification { get; init; }
        public bool ExitAfter { get; init; }
        public bool ReloadConfig { get; init; }
        public bool? Enable { get; init; }
        public bool? InstallStartup { get; init; }

        public static Cli Parse(string[] args)
        {
            var cli = new Cli();
            string? title = null, message = null;
            int duration = 5;

            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i];
                switch (a)
                {
                    case "--title":
                        title = i + 1 < args.Length ? args[++i] : title;
                        break;
                    case "--message":
                        message = i + 1 < args.Length ? args[++i] : message;
                        break;
                    case "--duration":
                        if (i + 1 < args.Length && int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var d))
                            duration = Math.Clamp(d, 1, 60);
                        break;
                    case "--notify":
                        if (i + 1 < args.Length)
                        {
                            var parts = args[++i].Split('|', 3, StringSplitOptions.None);
                            title = parts.Length > 0 ? parts[0] : title;
                            message = parts.Length > 1 ? parts[1] : message;
                            if (parts.Length > 2 && int.TryParse(parts[2], out var dd)) duration = dd;
                        }
                        break;
                    case "--exit-after":
                        cli = cli with { ExitAfter = true };
                        break;
                    case "--reload":
                        cli = cli with { ReloadConfig = true };
                        break;
                    case "--enable":
                        cli = cli with { Enable = true };
                        break;
                    case "--disable":
                        cli = cli with { Enable = false };
                        break;
                    case "--install-startup":
                        cli = cli with { InstallStartup = true };
                        break;
                    case "--uninstall-startup":
                        cli = cli with { InstallStartup = false };
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(message))
            {
                cli = cli with
                {
                    OneOffNotification = new OneOffNote
                    {
                        Title = title ?? "Notice",
                        Message = message ?? "",
                        DurationSeconds = duration
                    }
                };
            }

            return cli;
        }

        private Cli with => this;
    }
}
