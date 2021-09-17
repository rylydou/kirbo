using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kirbo
{
	public class PlaylistEntryConverter : JsonConverter<PlaylistEntry>
	{
		public override PlaylistEntry? ReadJson(JsonReader reader, Type objectType, PlaylistEntry? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.StartArray) throw new Exception();

			var sections = reader.ReadAsJArray<string>();

			if (sections.Length != 3) throw new Exception();

			if (sections[0] is null) throw new Exception();
			if (sections[1] is null) throw new Exception();
			if (sections[2] is null) throw new Exception();

			return new PlaylistEntry(sections[0]!, sections[1]!, sections[2]!);
		}

		public override void WriteJson(JsonWriter writer, PlaylistEntry? value, JsonSerializer serializer)
		{
			if (value is null) throw new ArgumentNullException(nameof(value));

			writer.WriteStartArray();
			writer.WriteValue(value.title);
			writer.WriteValue(value.artist);
			writer.WriteValue(value.album);
			writer.WriteEndArray();
		}
	}

	[DataContract]
	public class PlaylistEntry
	{
		public Playlist? playlist;

		public readonly string title;
		public readonly string artist;
		public readonly string album;

		public readonly string systemTitle;
		public readonly string systemArtist;
		public readonly string systemAlbum;

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
					if (!MainWindow.current.database.titleToSong.TryGetValue(systemTitle, out var songsWithTitle)) return null;
					// If it the only one then its been found
					if (songsWithTitle.Count == 1) return FoundSong(songsWithTitle[0]);

					// Find the song by title and artist
					// Find all the songs with that name by the artist
					var songsWithTitleByArtist = songsWithTitle.FindAll(s => s.systemArtist == systemArtist);
					if (songsWithTitleByArtist.Count > 0)
					{
						// If it is the only one then its been found
						if (songsWithTitleByArtist.Count == 1) return FoundSong(songsWithTitleByArtist[0]);
						// Find the song by title artist and album
						return FoundSong(songsWithTitleByArtist.Find(s => s.systemAlbum == systemAlbum));
					}

					// Find song by title and album, an album should never have multiple songs with the same name
					return FoundSong(songsWithTitle.Find(s => s.systemAlbum == systemAlbum));
				}
				return _referencedSong;
			}
		}

		public PlaylistEntry(string title, string artist = "", string album = "")
		{
			this.title = title;
			this.artist = artist;
			this.album = album;

			this.systemTitle = title.ToSystem();
			this.systemArtist = artist.ToSystem();
			this.systemAlbum = album.ToSystem();
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

		public override string ToString() => $"{title}, {artist}, {album}";
	}
}
