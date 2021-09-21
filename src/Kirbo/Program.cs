using System;
using Gtk;
using Newtonsoft.Json;

namespace Kirbo
{
	class Program
	{
		static JsonSerializerSettings jsonSerializerSettings;

		static Program()
		{
			jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.Converters.Add(new PlaylistEntryConverter());
			jsonSerializerSettings.Formatting = Formatting.Indented;
		}

		[STAThread]
		public static void Main(string[] args)
		{
#if DEBUG
			Run();
#else
			try
			{
				Run();
			}
			catch (Exception e)
			{
				System.IO.File.WriteAllText(Environment.CurrentDirectory + "/crash.log", e.ToString());
				throw;
			}
#endif
		}

		public static void Run()
		{
			JsonConvert.DefaultSettings += () => jsonSerializerSettings;

			Application.Init();

			var app = new Application("org.Kirbo.Kirbo", GLib.ApplicationFlags.None);
			app.Register(GLib.Cancellable.Current);

			var win = new MainWindow();
			app.AddWindow(win);

			win.Show();
			Application.Run();
		}
	}
}
