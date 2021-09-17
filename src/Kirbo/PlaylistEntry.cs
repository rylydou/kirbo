using System;
using System.Diagnostics;
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
		public readonly string title;
		public readonly string artist;
		public readonly string album;

		public readonly string systemTitle;
		public readonly string systemArtist;
		public readonly string systemAlbum;

		public DatabaseSongEntry? referencedSong { get; private set; }

		public PlaylistEntry(string title, string artist = "", string album = "")
		{
			this.title = title;
			this.artist = artist;
			this.album = album;

			this.systemTitle = title.ToSystem();
			this.systemArtist = artist.ToSystem();
			this.systemAlbum = album.ToSystem();

			Trace.WriteLine($"Added {this} to playlist");
		}

		public override string ToString() => $"'{title}' by '{artist}' from '{album}'";

		public void FindReferencedSong()
		{
			// Find songs with the same title
			if (!MainWindow.current.database.titleToSong.TryGetValue(systemTitle, out var songsWithTitle))
			{
				Trace.WriteLine($"No songs titled '{systemTitle}'");

				referencedSong = null;
				return;
			}

			// If it the only one then its been found
			if (songsWithTitle.Count == 1)
			{
				Trace.WriteLine($"Found only song titled '{systemTitle}'");

				referencedSong = songsWithTitle[0];
				return;
			}

			// Find the song by title and artist
			// Find all the songs with that name by the artist
			var songsWithTitleByArtist = songsWithTitle.FindAll(s => s.systemArtist == systemArtist);
			if (songsWithTitleByArtist.Count > 0)
			{
				// Find the song by title artist and album
				referencedSong = songsWithTitleByArtist.Find(s => s.systemAlbum == systemAlbum);
				if (referencedSong is not null)
				{
					Trace.WriteLineIf(referencedSong is not null, $"Found song by '{systemArtist}' titled '{systemTitle}'");
					return;
				}
			}

			// Find song by title and album, an album should never have multiple songs with the same name
			referencedSong = songsWithTitle.Find(s => s.systemAlbum == systemAlbum);
			Trace.WriteLineIf(referencedSong is not null, $"Found song by '{systemArtist}' titled '{systemTitle}' in '{systemAlbum}'");
			Trace.WriteLineIf(referencedSong is null, $"Could not find song by '{systemArtist}' titled '{systemTitle}' in '{systemAlbum}'");
		}
	}
}
