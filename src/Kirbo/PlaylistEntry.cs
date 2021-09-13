using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Kirbo
{
	public class PlaylistEntryFormatter : JsonConverter<PlaylistEntry>
	{
		public override PlaylistEntry? ReadJson(JsonReader reader, Type objectType, PlaylistEntry? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var value = reader.ReadAsString();
			if (value is null) throw new NullReferenceException(nameof(value));
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

		public PlaylistEntry(string title, string artist = "", string album = "")
		{
			this.title = title;
			this.artist = artist;
			this.album = album;
		}
	}
}
