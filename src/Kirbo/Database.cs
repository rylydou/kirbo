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
		}

		public void Save()
		{
			var usedPlaylistsPaths = new List<string>();
			foreach (var playlist in playlists)
			{
				File.WriteAllText(Config.playlistsPath + playlist.systemName + ".json", JsonConvert.SerializeObject(playlist));
			}

			foreach (var playlistPath in Directory.GetFiles(Config.playlistsPath))
			{
				if (!usedPlaylistsPaths.Contains(playlistPath.CleanPath()))
				{
					File.Delete(playlistPath);
				}
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

		public void UpdateUI(Notebook playlistsNotebook)
		{
			for (int i = playlistsNotebook.NPages - 1; i >= 0; i--)
			{
				playlistsNotebook.RemovePage(i);
			}

			// Generate all music
			{
				var allMusicList = new TreeView() { HeadersClickable = true };
				// songsList.HeadersVisible = false;

				var listStore = new ListStore(typeof(string), typeof(string), typeof(string));
				allMusicList.Model = listStore;

				var titleCellView = new CellRendererText();
				var titleColumn = new TreeViewColumn("Title", titleCellView);
				titleColumn.AddAttribute(titleCellView, "text", 0);
				allMusicList.AppendColumn(titleColumn);

				var artistCellView = new CellRendererText();
				var artistColumn = new TreeViewColumn("Artist", artistCellView);
				artistColumn.AddAttribute(artistCellView, "text", 1);
				allMusicList.AppendColumn(artistColumn);

				var albumCellView = new CellRendererText();
				var albumColumn = new TreeViewColumn("Album", albumCellView);
				albumColumn.AddAttribute(albumCellView, "text", 2);
				allMusicList.AppendColumn(albumColumn);

				var pathCellView = new CellRendererText();
				var pathColumn = new TreeViewColumn("Location", albumCellView);
				pathColumn.AddAttribute(pathCellView, "text", 2);
				allMusicList.AppendColumn(pathColumn);

				foreach (var song in MainWindow.current.database.songs)
				{
					listStore.AppendValues(song.title, song.artist, song.album, song.path);
				}

				playlistsNotebook.AppendPage(allMusicList, new Label("All Music"));
			}

			foreach (var playlist in playlists)
			{
				var page = new ListBox();

				var hbox = new HBox(false, 8);
				var titleInput = new Entry(playlist.title);
				titleInput.Changed += (sender, args) => playlist.title = titleInput.Text;
				hbox.Add(titleInput);
				var playButton = new Button(new Label("Play"));
				playButton.Clicked += delegate
				{
					MainWindow.current.player.LoadPlaylist(playlist);
					MainWindow.current.player.PlayRandomSongFromPlaylist();
				};
				hbox.Add(playButton);
				page.Add(hbox);
				var descInput = new Entry(playlist.description);
				descInput.Changed += (sender, args) => playlist.description = descInput.Text;
				page.Add(descInput);

				var songsList = new TreeView() { HeadersClickable = true };
				// songsList.HeadersVisible = false;

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
