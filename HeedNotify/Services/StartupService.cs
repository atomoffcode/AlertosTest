using System;
using Microsoft.Win32;
using HeedNotify.Utils;

namespace HeedNotify.Services
{
	public static class StartupService
	{
		private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
		private const string AppName = "HeedNotify";

		public static void InstallRegistryRun()
		{
			try
			{
				var exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? AppContext.BaseDirectory;
				using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true) ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, true)!;
				key.SetValue(AppName, $"\"{exe}\"");
				Logger.Info("Startup installed (HKCU Run).");
			}
			catch (Exception ex)
			{
				Logger.Error($"Failed to install startup: {ex}");
			}
		}

		public static void UninstallRegistryRun()
		{
			try
			{
				using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
				key?.DeleteValue(AppName, false);
				Logger.Info("Startup uninstalled (HKCU Run).");
			}
			catch (Exception ex)
			{
				Logger.Error($"Failed to uninstall startup: {ex}");
			}
		}
	}
}
