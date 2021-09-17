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
			try
			{
				Run();
			}
			catch (Exception e)
			{
				System.IO.File.WriteAllText(Environment.CurrentDirectory + "/crash.log", e.ToString());
#if DEBUG
				throw;
#endif
			}
		}

		public static void Run()
		{
			JsonConvert.DefaultSettings += () => jsonSerializerSettings;

			Application.Init();

			var win = new MainWindow();
			win.ShowAll();

			Application.Run();
		}
	}
}
