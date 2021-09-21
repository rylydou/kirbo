using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
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

		Timer tickTimer;

		// Nullable hacks
#nullable disable
		public static MainWindow current;

		[UI] ListStore _musicList;

		[UI] Label _status_info;
		[UI] Label _status_position;
		[UI] Label _status_duration;
		[UI] ProgressBar _status_bar;
		[UI] Adjustment _volume;

		[UI] Notebook _page_music;

		[UI] SearchEntry _page_music_all_filter;
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

			foreach (var song in database.songs)
			{
				_musicList.AppendValues(song.title, song.artist, song.album, song.path);
			}

			foreach (var playlist in database.playlists)
			{
				_page_music.AppendPage(new Label(playlist.title), new PlaylistView(playlist));
			}

			_page_music.ShowAll();

			resetProvider = new CssProvider();
			styleProvider = new CssProvider();

			if (ENABLE_CUSTOM_STYLES)
			{
				ReloadStyles();

				StyleContext.AddProviderForScreen(Screen, resetProvider, int.MaxValue - 1);
				StyleContext.AddProviderForScreen(Screen, styleProvider, int.MaxValue);
			}

			tickTimer = new Timer(t => Tick(), null, 1000, 1000);
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

		void Tick()
		{
			// var position = player.currentSongInstance.position;

			// _status_position.Text = position.ToString(@"mm\:ss");
			// _status_duration.Text = player.currentSongInstance.duration.ToString(@"mm\:ss");
			// _status_bar.Fraction = position / player.currentSongInstance.duration;
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
	}
}
