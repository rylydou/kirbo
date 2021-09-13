using System;
using Gtk;

namespace Kirbo
{
	class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
#if DEBUG
			Application.Init();

			var win = new MainWindow();
			win.ShowAll();

			Application.Run();
#else
			try
			{
				Application.Init();

				var win = new MainWindow();
				win.ShowAll();

				Application.Run();
			}
			catch (Exception e)
			{
				System.IO.File.WriteAllText(Environment.CurrentDirectory + "/crash.log", e.ToString());
			}
#endif
		}
	}
}
