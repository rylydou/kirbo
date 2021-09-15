using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ManagedBass;

namespace Kirbo
{
	public class MusicPlayer : IDisposable
	{
		const int MIN_SONG_POOL_COUNT = 2;
		const int MAX_SONG_POOL_COUNT = 50;

		public Playlist? playlist;
		public DatabaseSongEntry? currentSong;

		public int currentSongHandle;

		public MusicPlayer()
		{
			Bass.Init();
		}

		public void LoadPlaylist(Playlist playlist)
		{
			this.playlist = playlist;
		}

		public void PlayRandomSongFromPlaylist()
		{
			Trace.WriteLine($"Playing random song from {playlist}");

			if (playlist is null) throw new Exception("USER Select a playlist first");

			var rng = new Random(Guid.NewGuid().GetHashCode());

			var playlistSongsCount = playlist.songs.Count;

			// Pick a bunch of random songs from the playlist (at least 2)
			var songPool = new List<DatabaseSongEntry>();
			if (playlist.songs.Count < MIN_SONG_POOL_COUNT)
			{
				Trace.WriteLine($"Generating pool directly from playlist");

				songPool = playlist.songs.Select(s => s.referencedSong).Where(s => s is not null).ToList()!;

				// If there are no valid songs then give up
				if (songPool.Count == 0) throw new Exception("USER - Playlist contains no working songs");

				// If there is only one valid song then just play that
				if (songPool.Count == 1)
				{
					Trace.WriteLine($"Playlist only has one working song");

					PlaySong(songPool[0]);
					return;
				}
			}
			else
			{
				Trace.WriteLine($"Generating pool from parts of playlist");

				for (int t = 0; t < Math.Clamp(playlistSongsCount / 3, MIN_SONG_POOL_COUNT, MAX_SONG_POOL_COUNT); t++)
				{
					var playlistSong = playlist.songs[rng.Next(0, playlistSongsCount)];
					var song = playlistSong.referencedSong;

					// If the song file does not exsist then skip it
					if (song is null)
					{
						Trace.WriteLine($"- Skiped '{playlistSong}' because it doesn't reference a song");
						continue;
					}

					songPool.Add(song);
				}
			}

			// If the pool has very few songs the just make the pool the array
			if (songPool.Count < MIN_SONG_POOL_COUNT)
			{
				Trace.WriteLine($"Generating pool directly from playlist becuase it has less than {MIN_SONG_POOL_COUNT} songs");

				songPool.Clear();
				songPool = playlist.songs.Select(s => s.referencedSong).Where(s => s is not null).ToList()!;
			}

			// If there are no valid songs then give up
			if (songPool.Count == 0) throw new Exception("USER - Playlist contains no working songs");

			// If there is only one valid song then just play that
			if (songPool.Count == 1)
			{
				Trace.WriteLine($"Playing only working song from playlist");

				PlaySong(songPool[0]);
				return;
			}

			// Play the oldest song from the pool
			Trace.WriteLine($"Choosing song based on freshness");
			DatabaseSongEntry? bestSong = null;
			foreach (var song in songPool)
			{
				// If the song has never been played then play it
				if (!song.lastPlayed.HasValue)
				{
					Trace.WriteLine($"- Playing song that has never been played");

					PlaySong(song);
					return;
				}

				// If there has been no best song then set it
				if (bestSong is null)
				{
					bestSong = song;
					continue;
				}

				// If the song is older than the best song the choose that one
				if (DateTime.Compare(song.lastPlayed.Value, bestSong.lastPlayed!.Value) < 0)
				{
					bestSong = song;
					continue;
				}
			}

			PlaySong(bestSong!);
		}

		public void PlaySong(DatabaseSongEntry song)
		{
			// Setup data
			song.lastPlayed = DateTime.Now;
			song.timesPlayed++;

			// Free the old song
			Trace.WriteLine($"Unloading #{currentSongHandle}");
			Bass.ChannelStop(currentSongHandle);
			Bass.StreamFree(currentSongHandle);

			currentSong = song;

			// Play the new song
			Trace.WriteLine($"Creating stream for '{song}'");
			currentSongHandle = Bass.CreateStream(song.path);

			Trace.WriteLine($"Playing #{currentSongHandle}");
			Bass.ChannelPlay(currentSongHandle);

			Bass.ChannelSetAttribute(currentSongHandle, ChannelAttribute.Volume, 0.5f);
		}

		public void Dispose()
		{
			Bass.Stop();
			Bass.Free();
		}
	}
}
