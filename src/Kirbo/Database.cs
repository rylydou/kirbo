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

		public Dictionary<string, List<DatabaseSongEntry>> titleToSong = new Dictionary<string, List<DatabaseSongEntry>>();
		public Dictionary<string, List<DatabaseSongEntry>> artistToSong = new Dictionary<string, List<DatabaseSongEntry>>();
		public Dictionary<string, List<DatabaseSongEntry>> albumToSong = new Dictionary<string, List<DatabaseSongEntry>>();

		public Database() { }

		public void Save()
		{
			var usedPlaylistsPaths = new List<string>();
			foreach (var playlist in playlists)
			{
				File.WriteAllText(Config.playlistsPath + playlist.systemName + ".json", JsonConvert.SerializeObject(playlist));
			}
		}

		public void ReloadAll()
		{
			ReloadMusicDatabase();
			ReloadPlaylistsDatabase();
		}

		public void ReloadMusicDatabase()
		{
			songs.Clear();
			foreach (var musicFolder in Config.current.musicFolders)
			{
				foreach (var song in Directory.GetFiles(musicFolder, "*", new EnumerationOptions() { RecurseSubdirectories = true }))
				{
					songs.Add(new DatabaseSongEntry(song));
				}
			}
		}

		public void ReloadPlaylistsDatabase()
		{
			playlists.Clear();
			foreach (var playlistPath in Directory.GetFiles(Config.playlistsPath))
			{
				var playlist = JsonConvert.DeserializeObject<Playlist>(File.ReadAllText(playlistPath));
				if (playlist is null) throw new NullReferenceException();
				playlists.Add(playlist);
			}
		}
	}
}
