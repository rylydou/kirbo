using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Kirbo
{
	public class PlaylistEntryConverter : JsonConverter<PlaylistEntry>
	{
		public override PlaylistEntry? ReadJson(JsonReader reader, Type objectType, PlaylistEntry? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var value = reader.Value as string;
			if (value is null) throw new JsonException("Item of type string was expected");
			var sections = value.Split(',');
			return new PlaylistEntry(sections[0], sections[1], sections[2]);
		}

		public override void WriteJson(JsonWriter writer, PlaylistEntry? value, JsonSerializer serializer)
		{
			if (value is null) throw new ArgumentNullException(nameof(value));
			writer.WriteValue(string.Concat(value.title, ',', value.artist, ',', value.album));
		}
	}

	[DataContract]
	public class PlaylistEntry
	{
		public Playlist? playlist;

		public string title;
		public string artist;
		public string album;

		bool _songFound;

		public bool? hasNoReference { get; private set; }
		DatabaseSongEntry? _referencedSong;
		public DatabaseSongEntry? referencedSong
		{
			get
			{
				if (!_songFound)
				{
					// Find songs with the same title
					if (!Database.current.titleToSong.TryGetValue(title, out var songsWithTitle)) return null;
					// If it the only one then its been found
					if (songsWithTitle.Count == 1) return FoundSong(songsWithTitle[0]);

					// Find the song by title and artist
					// Find all the songs with that name by the artist
					var songsWithTitleByArtist = songsWithTitle.FindAll(s => s.artist == artist);
					if (songsWithTitleByArtist.Count > 0)
					{
						// If it is the only one then its been found
						if (songsWithTitleByArtist.Count == 1) return FoundSong(songsWithTitleByArtist[0]);
						// Find the song by title artist and album
						return FoundSong(songsWithTitleByArtist.Find(s => s.album == album));
					}

					// Find song by title and album, an album should never have multiple songs with the same name
					return FoundSong(songsWithTitle.Find(s => s.album == album));
				}
				return _referencedSong;
			}
		}

		public PlaylistEntry(string title, string artist = "", string album = "")
		{
			this.title = title;
			this.artist = artist;
			this.album = album;
		}

		DatabaseSongEntry? FoundSong(DatabaseSongEntry? song)
		{
			_songFound = true;
			if (song is null)
			{
				hasNoReference = true;
				return null;
			}
			_referencedSong = song;
			return _referencedSong;
		}
	}
}
