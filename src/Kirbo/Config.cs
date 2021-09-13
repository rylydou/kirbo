using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using Newtonsoft.Json;

namespace Kirbo
{
	public class Config
	{
		static Config? _current;
		public static Config current
		{
			get
			{
				if (_current is null)
				{
					if (File.Exists($"{dataPath}/settings.yaml"))
					{
						_current = JsonConvert.DeserializeObject<Config>(File.ReadAllText($"{dataPath}/settings.json"));
					}

					if (_current is null)
					{
						_current = new Config();
					}

					Application.Default.Shutdown += (sender, args) =>
					{
						File.WriteAllText($"{dataPath}/settings.json", JsonConvert.SerializeObject(_current));
					};
				}
				return _current;
			}
		}

		public List<string> musicFolders = new List<string>();

		static string? _dataPath;
		public static string dataPath
		{
			get
			{
				if (_dataPath is null)
				{
#if DEBUG
					_dataPath = $"{Environment.CurrentDirectory}/data";
					if (!Directory.Exists(_dataPath)) Directory.CreateDirectory(_dataPath);
#else
					_dataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Kirbo";
					if (!Directory.Exists(_dataPath)) Directory.CreateDirectory(_dataPath);
#endif
				}
				return _dataPath;
			}
		}

		static string? _playlistsPath;
		public static string playlistsPath
		{
			get
			{
				if (_playlistsPath is null)
				{
					_playlistsPath = $"{dataPath}/playlists/";
					if (!Directory.Exists(_playlistsPath)) Directory.CreateDirectory(_playlistsPath);
				}

				return _playlistsPath;
			}
		}
	}
}