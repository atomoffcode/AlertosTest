using System;
using System.Windows;
using HeedNotify.Services;
using HeedNotify.Utils;

namespace HeedNotify
{
    public partial class App : Application
    {
        private TrayIconService? trayIconService;
        private SchedulerService? schedulerService;

        protected override void OnExit(ExitEventArgs e)
        {
            schedulerService?.Dispose();
            trayIconService?.Dispose();
            base.OnExit(e);
        }

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                Logger.Initialize();
                Logger.Info("HeedNotify starting...");

                ConfigService.Initialize();

                trayIconService = new TrayIconService();
                trayIconService.Initialize();

                var cli = Cli.Parse(e.Args);

                if (cli.InstallStartup.HasValue)
                {
                    if (cli.InstallStartup.Value)
                        StartupService.InstallRegistryRun();
                    else
                        StartupService.UninstallRegistryRun();
                }

                if (cli.Enable.HasValue)
                {
                    ConfigService.SetEnabled(cli.Enable.Value);
                }

                if (cli.ReloadConfig)
                {
                    ConfigService.Reload();
                }

                if (cli.OneOffNotification is not null)
                {
                    var n = cli.OneOffNotification;
                    await NotificationService.ShowAsync(n.Title, n.Message, TimeSpan.FromSeconds(n.DurationSeconds));
                    if (cli.ExitAfter) Shutdown();
                }

                if (ConfigService.Current.Enabled)
                {
                    schedulerService = new SchedulerService();
                    schedulerService.Start();
                }

                var mw = new MainWindow();
                mw.Hide();
            }
            catch (Exception ex)
            {
                try { Logger.Error($"Startup error: {ex}"); } catch { }
                MessageBox.Show($"HeedNotify failed to start: {ex.Message}", "HeedNotify",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }
    }
}
