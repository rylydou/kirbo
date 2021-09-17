using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gtk;
using ManagedBass;
using Range = Gtk.Range;

namespace Kirbo
{
	public class MusicPlayer : IDisposable
	{
		const int PLAYLIST_SIZE_TO_TRY_USING_SMART_PICK = 10;
		const int MIN_SMART_PICK_POOL_SIZE = 8;
		const int MAX_SMART_PICK_POOL_SIZE = 10;

		public Playlist? playlist;
		public DatabaseSongEntry? currentSong;

		public SongInsance currentSongInstance;

		public Action<Playlist> onPlaylistsChanged = p => { };
		public Action<DatabaseSongEntry> onSongStarted = s => { };

		public MusicPlayer()
		{
			Bass.Init();

			currentSongInstance = new SongInsance();

			currentSongInstance.onFinish += s =>
			{
				Trace.WriteLine("Next song");

				PlayRandomSongFromPlaylist();
			};
		}

		public void LoadPlaylist(Playlist playlist)
		{
			if (playlist == this.playlist) return;

			this.playlist = playlist;

			onPlaylistsChanged.Invoke(playlist);
		}

		public void PlayRandomSongFromPlaylist()
		{
			Trace.WriteLine($"Playing random song from {playlist}");

			if (playlist is null) throw new Exception("USER Select a playlist first");

			var pool = new List<DatabaseSongEntry>();

			if (playlist.songs.Count >= PLAYLIST_SIZE_TO_TRY_USING_SMART_PICK)
			{
				for (int i = 0; i < playlist.songs.Count; i++)
				{
					var song = playlist.songs.PickRandom().referencedSong;
					if (song is null) continue;

					pool.Add(song);

					if (pool.Count >= MAX_SMART_PICK_POOL_SIZE) break;
				}

				// Choose using smart pick
				if (pool.Count >= MIN_SMART_PICK_POOL_SIZE)
				{
					Trace.WriteLine("Choosing song using smart pick");

					PlaySong(SmartPick(pool));
					return;
				}

				pool.Clear();
			}

			// Choose using random pick
			Trace.WriteLine("Choosing song using random pick");

			foreach (var song in playlist.songs)
			{
				var refSong = song.referencedSong;
				if (refSong is null) continue;
				pool.Add(refSong);
			}

			var choosenSong = RandomPick(pool);
			if (choosenSong is not null)
			{
				PlaySong(choosenSong);
				return;
			}

			throw new Exception("Playlist has no working songs");
		}

		DatabaseSongEntry? RandomPick(IList<DatabaseSongEntry> pool)
		{
			if (pool.Count == 1) return pool[0];
			if (pool.Count < 1) return null;

			for (int i = 0; i < pool.Count + 2; i++)
			{
				var song = pool.PickRandom();
				if (song != currentSong) return song;
			}
			Trace.WriteLine("Could not find songs that is not the current song in a resonable time");
			return null;
		}

		DatabaseSongEntry SmartPick(IList<DatabaseSongEntry> pool)
		{
			if (pool.Count < 1) throw new ArgumentException("Pool must not be empty", nameof(pool));

			var bestSong = pool[0];
			if (!bestSong.lastPlayed.HasValue) return bestSong;

			foreach (var song in pool)
			{
				// If the song has never been played then play it
				if (!song.lastPlayed.HasValue) return song;

				// Skip it if it was the song just played
				if (song == currentSong) continue;

				// If the song is older than the best song the choose that one
				if (DateTime.Compare(song.lastPlayed.Value, bestSong.lastPlayed.Value) < 0)
				{
					bestSong = song;
					continue;
				}
			}

			return bestSong;
		}

		public void PlaySong(DatabaseSongEntry song)
		{
			// Setup data
			song.lastPlayed = DateTime.Now;
			song.timesPlayed++;

			currentSong = song;

			currentSongInstance.Load(song.path);

			currentSongInstance.Play();

			onSongStarted.Invoke(song);
		}

		public void Dispose()
		{
			Bass.Stop();
			Bass.Free();
		}
	}
}
