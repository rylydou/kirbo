using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kirbo
{
	[DataContract]
	public class Playlist
	{
		[DataMember] public string title;
		[DataMember] public string description = string.Empty;
		[DataMember] public List<PlaylistEntry> songs = new List<PlaylistEntry>();

		// TODO
		public string internalName => title;

		public Playlist(string title)
		{
			this.title = title;
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			foreach (var song in songs)
			{
				song.playlist = this;
			}
		}
	}
}
