using System;
using System.Collections.Generic;
using System.Diagnostics;
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
					if (File.Exists($"{dataPath}/settings.json"))
					{
						Trace.WriteLine("Loading settings file");
						_current = JsonConvert.DeserializeObject<Config>(File.ReadAllText($"{dataPath}/settings.json"));
					}

					if (_current is null)
					{
						Trace.WriteLine("Creating new settings");
						_current = new Config();
#if DEBUG
						_current.musicFolders.Add(dataPath + "/music");
#else
						_current.musicFolders.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).CleanPath());
						// _current.musicFolders.Add(Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic).CleanPath());
#endif
					}
				}
				return _current;
			}
		}

		static string? _dataPath;
		public static string dataPath
		{
			get
			{
				if (_dataPath is null)
				{
#if DEBUG
					_dataPath = $"{Environment.CurrentDirectory.CleanPath()}/data";
					if (!Directory.Exists(_dataPath)) Directory.CreateDirectory(_dataPath);
#else
					_dataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).CleanPath()}/Kirbo";
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

		public List<string> musicFolders = new List<string>();

		public string downloadFolder = "";

		public void Save()
		{
			File.WriteAllText($"{dataPath}/settings.json", JsonConvert.SerializeObject(_current));
		}
	}
}
