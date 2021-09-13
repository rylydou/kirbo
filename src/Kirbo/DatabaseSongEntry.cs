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

		public DatabaseSongEntry(string path)
		{
			this.path = path;

			var track = new Track(path);
			this.title = track.Title;
			this.artist = track.Artist;
			this.album = track.Album;

			if (Database.current.titleToSong.TryGetValue(title, out var titleList))
			{
				titleList.Add(this);
			}
			else
			{
				Database.current.titleToSong.Add(title, new List<DatabaseSongEntry>() { this });
			}

			if (Database.current.artistToSong.TryGetValue(title, out var artistList))
			{
				artistList.Add(this);
			}
			else
			{
				Database.current.artistToSong.Add(title, new List<DatabaseSongEntry>() { this });
			}

			if (Database.current.albumToSong.TryGetValue(title, out var albumList))
			{
				albumList.Add(this);
			}
			else
			{
				Database.current.albumToSong.Add(title, new List<DatabaseSongEntry>() { this });
			}
		}
	}
}
