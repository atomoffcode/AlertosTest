using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using HeedNotify.Utils;

namespace HeedNotify.Services
{
	public sealed class TrayIconService : IDisposable
	{
		private NotifyIcon? notifyIcon;
		private ContextMenuStrip? menu;

		public void Initialize()
		{
			notifyIcon = new NotifyIcon
			{
				Icon = System.Drawing.SystemIcons.Information,
				Visible = true,
				Text = "HeedNotify"
			};

			menu = new ContextMenuStrip();

			var enabledItem = new ToolStripMenuItem("Enabled") { Checked = ConfigService.Current.Enabled, CheckOnClick = true };
			enabledItem.CheckedChanged += (_, __) => ConfigService.SetEnabled(enabledItem.Checked);
			menu.Items.Add(enabledItem);

			var testItem = new ToolStripMenuItem("Test Notification");
			testItem.Click += async (_, __) => await NotificationService.ShowAsync("HeedNotify", "This is a test notification", TimeSpan.FromSeconds(5));
			menu.Items.Add(testItem);

			menu.Items.Add(new ToolStripSeparator());

			var reloadItem = new ToolStripMenuItem("Reload Config");
			reloadItem.Click += (_, __) => ConfigService.Reload();
			menu.Items.Add(reloadItem);

			var openConfigItem = new ToolStripMenuItem("Open Config Folder");
			openConfigItem.Click += (_, __) => OpenFolder(Path.GetDirectoryName(ConfigService.UserConfigPath)!);
			menu.Items.Add(openConfigItem);

			var openLogsItem = new ToolStripMenuItem("Open Logs Folder");
			openLogsItem.Click += (_, __) => OpenFolder(Logger.LogDirectory);
			menu.Items.Add(openLogsItem);

			menu.Items.Add(new ToolStripSeparator());

			var startupItem = new ToolStripMenuItem("Start with Windows");
			startupItem.Click += (_, __) => StartupService.InstallRegistryRun();
			menu.Items.Add(startupItem);

			var noStartupItem = new ToolStripMenuItem("Remove Start with Windows");
			noStartupItem.Click += (_, __) => StartupService.UninstallRegistryRun();
			menu.Items.Add(noStartupItem);

			menu.Items.Add(new ToolStripSeparator());

			var exitItem = new ToolStripMenuItem("Exit");
			exitItem.Click += (_, __) => System.Windows.Application.Current.Shutdown();
			menu.Items.Add(exitItem);

			notifyIcon.ContextMenuStrip = menu;
			notifyIcon.DoubleClick += async (_, __) => await NotificationService.ShowAsync("HeedNotify", "Running in the tray", TimeSpan.FromSeconds(3));
		}

		private static void OpenFolder(string path)
		{
			Directory.CreateDirectory(path);
			Process.Start(new ProcessStartInfo
			{
				FileName = path,
				UseShellExecute = true
			});
		}

		public void Dispose()
		{
			if (notifyIcon != null)
			{
				notifyIcon.Visible = false;
				notifyIcon.Dispose();
				notifyIcon = null;
			}
			menu?.Dispose();
		}
	}
}
