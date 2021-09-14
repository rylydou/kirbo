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

		public Database()
		{
			ReloadAll();
		}

		public void Save()
		{
			foreach (var playlistPath in Directory.GetFiles(Config.playlistsPath))
			{
				File.Delete(playlistPath);
			}
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
				Directory.GetFiles(musicFolder);
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

		public void UpdateUI(Notebook playlistsNotebook)
		{
			for (int i = playlistsNotebook.NPages - 1; i >= 0; i--)
			{
				playlistsNotebook.RemovePage(i);
			}

			foreach (var playlist in playlists)
			{
				var page = new ListBox();

				page.Add(new Entry(playlist.title));
				page.Add(new Entry(playlist.description));
				var playButton = new Button(new Label("Play"));
				playButton.Clicked += delegate
				{
					MainWindow.current.player.LoadPlaylist(playlist);
					MainWindow.current.player.PlayRandomSongFromPlaylist();
				};
				page.Add(playButton);

				var songsList = new TreeView();
				// page.HeadersVisible = false;

				var listStore = new ListStore(typeof(string), typeof(string), typeof(string));
				songsList.Model = listStore;

				var titleCellView = new CellRendererText();
				var titleColumn = new TreeViewColumn("Title", titleCellView);
				titleColumn.AddAttribute(titleCellView, "text", 0);
				songsList.AppendColumn(titleColumn);

				var artistCellView = new CellRendererText();
				var artistColumn = new TreeViewColumn("Artist", artistCellView);
				artistColumn.AddAttribute(artistCellView, "text", 1);
				songsList.AppendColumn(artistColumn);

				var albumCellView = new CellRendererText();
				var albumColumn = new TreeViewColumn("Album", albumCellView);
				albumColumn.AddAttribute(albumCellView, "text", 2);
				songsList.AppendColumn(albumColumn);

				foreach (var song in playlist.songs)
				{
					listStore.AppendValues(song.title, song.artist, song.album);
				}

				page.Add(songsList);
				playlistsNotebook.AppendPage(page, new Label(playlist.title));
			}
		}
	}
}
