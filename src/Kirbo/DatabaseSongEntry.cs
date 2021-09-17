using System;
using System.Collections.Generic;
using System.Diagnostics;
using ATL;

namespace Kirbo
{
	public class DatabaseSongEntry
	{
		public string path;

		public readonly string title;
		public readonly string? artist;
		public readonly string? album;

		public readonly string systemTitle;
		public readonly string systemArtist;
		public readonly string systemAlbum;

		public DateTime? lastPlayed;
		public ushort timesPlayed;

		public DatabaseSongEntry(string path)
		{
			this.path = path.CleanPath();

			var track = new Track(path);

			this.title = track.Title;
			this.artist = track.Artist;
			this.album = track.Album;

			this.systemTitle = title.ToSystem();
			this.systemArtist = artist.ToSystem();
			this.systemAlbum = album.ToSystem();

			if (MainWindow.current.database.titleToSong.TryGetValue(systemTitle, out var titleList))
				titleList.Add(this);
			else
				MainWindow.current.database.titleToSong.Add(systemTitle, new List<DatabaseSongEntry>() { this });

			if (MainWindow.current.database.artistToSong.TryGetValue(systemArtist, out var artistList))
				artistList.Add(this);
			else
				MainWindow.current.database.artistToSong.Add(systemArtist, new List<DatabaseSongEntry>() { this });

			if (MainWindow.current.database.albumToSong.TryGetValue(systemAlbum, out var albumList))
				albumList.Add(this);
			else
				MainWindow.current.database.albumToSong.Add(systemAlbum, new List<DatabaseSongEntry>() { this });

			Trace.WriteLine($"Added song {this} to database");
		}

		public override string ToString() => $"'{title}' by '{artist}' from '{album}' at '{path}'";
	}
}
