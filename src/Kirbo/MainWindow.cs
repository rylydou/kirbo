using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Kirbo
{
	class MainWindow : Window
	{
		bool ENABLE_CUSTOM_STYLES = false;

		CssProvider resetProvider;
		CssProvider styleProvider;

		public MusicPlayer player;
		public Database database;
		public Random rng;

		Timer? tickTimer;

		// Nullable hacks
#nullable disable
		public static MainWindow current;

		[UI] ListStore musicList;

		[UI] Button status_state;
		[UI] Image status_state_image;
		[UI] Button status_skip;

		[UI] Label status_title;
		[UI] Label status_artist;
		[UI] Label status_position;
		[UI] ProgressBar status_bar;
		[UI] Label status_duration;

		[UI] Adjustment volume;

		[UI] Notebook page_music;
		[UI] Button page_music_add;
		[UI] Button page_music_delete;

		[UI] SearchEntry page_music_all_filter;
#nullable enable

		public MainWindow() : this(new Builder("MainWindow.glade")) { }
		MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
		{
			current = this;

			builder.Autoconnect(this);

			rng = new Random(Guid.NewGuid().GetHashCode());

			player = new MusicPlayer();
			database = new Database();
			database.ReloadAll();

			status_state.Clicked += (sender, args) =>
			{
				player.currentSongInstance.TogglePause();
			};

			player.currentSongInstance.onStateChanged += (s) =>
			{
				switch (s.state)
				{
					case ManagedBass.PlaybackState.Playing:
						status_state_image.SetFromIconName("media-playback-pause-symbolic", IconSize.Button);
						break;
					case ManagedBass.PlaybackState.Stalled:
						status_state_image.SetFromIconName("emblem-synchronizing-symbolic", IconSize.Button);
						break;
					case ManagedBass.PlaybackState.Stopped:
					case ManagedBass.PlaybackState.Paused:
						status_state_image.SetFromIconName("media-playback-start-symbolic", IconSize.Button);
						break;
				}
			};

			status_skip.Clicked += (sender, args) =>
			{
				if (player.playlist is not null)
				{
					player.PlayRandomSongFromPlaylist();
				}
			};

			volume.Value = SongInsance.masterVolume * 100;
			volume.ValueChanged += (sender, args) =>
			{
				SongInsance.masterVolume = volume.Value / 100;
			};

			player.onSongStarted += (s) =>
			{
				status_title.Text = s.title;
				status_artist.Text = s.artist;

				status_duration.Text = player.currentSongInstance.duration.ToString(@"mm\:ss");
			};

			foreach (var song in database.songs)
			{
				musicList.AppendValues(song.title, song.artist, song.album, song.path);
			}

			foreach (var playlist in database.playlists)
			{
				var lable = new Label(playlist.title);
				var playlistView = new PlaylistView(playlist, lable);

				page_music.AppendPage(playlistView, lable);
			}

			page_music.ShowAll();

			resetProvider = new CssProvider();
			styleProvider = new CssProvider();

			if (ENABLE_CUSTOM_STYLES)
			{
				ReloadStyles();

				StyleContext.AddProviderForScreen(Screen, resetProvider, int.MaxValue - 1);
				StyleContext.AddProviderForScreen(Screen, styleProvider, int.MaxValue);
			}

			Tick();
		}

		void Tick()
		{
			if (player.currentSongInstance.state == ManagedBass.PlaybackState.Playing)
			{
				var position = player.currentSongInstance.position;

				status_position.Text = position.ToString(@"mm\:ss");
				status_bar.Fraction = position / player.currentSongInstance.duration;
			}

			Task.Delay(1000).ContinueWith(t => Tick());
		}

		void ReloadStyles()
		{
			SetIconFromFile("images/icons/icon.png");

			try
			{
				styleProvider.LoadFromPath("styles/styles.css");

				try
				{
					resetProvider.LoadFromPath("styles/reset.css");
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				resetProvider.LoadFromData(string.Empty);
			}
		}

		protected override void OnDestroyed()
		{
			player.Dispose();

			Config.current.Save();
			database.Save();

			Application.Quit();

			base.OnDestroyed();
		}

		protected override void OnFocusActivated()
		{
			if (ENABLE_CUSTOM_STYLES) ReloadStyles();

			base.OnFocusActivated();
		}
	}
}
