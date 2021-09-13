using System;
using ManagedBass;

namespace Kirbo
{
	public class MusicPlayer : IDisposable
	{
		public int currentSongHandle = -1;
		public PlaylistEntry? currentSong;

		public MusicPlayer()
		{
			Bass.Init();
		}

		public void PlaySong(PlaylistEntry song)
		{
			// currentSong = song;

			// if (currentSongHandle >= 0)
			// {
			// 	Bass.StreamFree(currentSongHandle);
			// }

			// currentSongHandle = Bass.CreateStream(song);

			// Bass.ChannelPlay(currentSongHandle);
		}

		public void Dispose()
		{
			Bass.Free();
		}
	}
}
