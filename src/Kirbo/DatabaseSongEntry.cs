using System;
using System.Collections.Generic;
using ATL;

namespace Kirbo
{
	public class DatabaseSongEntry
	{
		public string path;

		public string title;
		public string? artist;
		public string? album;

		public DateTime? lastPlayed;
		public ushort timesPlayed;

		public DatabaseSongEntry(string path)
		{
			this.path = path;

			var track = new Track(path);
			this.title = track.Title;
			this.artist = track.Artist;
			this.album = track.Album;

			if (MainWindow.current.database.titleToSong.TryGetValue(title, out var titleList))
			{
				titleList.Add(this);
			}
			else
			{
				MainWindow.current.database.titleToSong.Add(title, new List<DatabaseSongEntry>() { this });
			}

			if (MainWindow.current.database.artistToSong.TryGetValue(title, out var artistList))
			{
				artistList.Add(this);
			}
			else
			{
				MainWindow.current.database.artistToSong.Add(title, new List<DatabaseSongEntry>() { this });
			}

			if (MainWindow.current.database.albumToSong.TryGetValue(title, out var albumList))
			{
				albumList.Add(this);
			}
			else
			{
				MainWindow.current.database.albumToSong.Add(title, new List<DatabaseSongEntry>() { this });
			}
		}

		public override string ToString() => $"{path} {title} {artist} {album}";
	}
}
