using System;
using System.Threading.Tasks;
using System.Windows;
using HeedNotify.Windows;

namespace HeedNotify.Services
{
	public static class NotificationService
	{
		public static Task ShowAsync(string title, string message, TimeSpan duration)
		{
			var tcs = new TaskCompletionSource();
			Application.Current.Dispatcher.Invoke(() =>
			{
				var toast = new ToastWindow(title, message, duration)
				{
					Opacity = 0
				};
				toast.Show();
				tcs.SetResult();
			});
			return tcs.Task;
		}
	}
}
