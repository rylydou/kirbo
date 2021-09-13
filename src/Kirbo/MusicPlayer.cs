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
			currentSong = song;

			if (currentSongHandle >= 0)
			{
				Bass.StreamFree(currentSongHandle);
			}

			if (song.referencedSong is null) return;

			currentSongHandle = Bass.CreateStream(song.referencedSong.path);

			Bass.ChannelPlay(currentSongHandle);
		}

		public void Dispose()
		{
			Bass.Free();
		}
	}
}
