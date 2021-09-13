using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using Newtonsoft.Json;

namespace Kirbo
{
	public class Database
	{
		public List<DatabaseSongEntry> songs = new List<DatabaseSongEntry>();
		public List<Playlist> playlists = new List<Playlist>();

		public Database()
		{
			Application.Default.Shutdown += (sender, args) =>
			{
				foreach (var playlist in playlists)
				{
					File.WriteAllText(Config.playlistsPath + playlist.internalName + ".json", JsonConvert.SerializeObject(playlist));
				}
			};
		}

		public void Reload()
		{
			songs.Clear();
			foreach (var musicFolder in Config.current.musicFolders)
			{
				Directory.GetFiles(musicFolder);
			}

			playlists.Clear();
			foreach (var playlistPath in Directory.GetFiles(Config.playlistsPath))
			{
				var playlist = JsonConvert.DeserializeObject<Playlist>(playlistPath);
				if (playlist is null) throw new NullReferenceException();
				playlists.Add(playlist);
			}
		}

		public void UpdateStore(ListStore list)
		{
			list.Clear();
			foreach (var playlist in playlists)
			{
				list.AppendValues(playlist.title);
			}
		}
	}
}
