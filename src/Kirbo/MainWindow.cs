using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Gtk;

namespace Kirbo
{
	class MainWindow : Window
	{
		// Nullable hacks
#nullable disable
		public static MainWindow current;
#nullable enable

		bool ENABLE_CUSTOM_STYLES = false;

		public Notebook playlistsNotebook;

		CssProvider resetProvider;
		CssProvider styleProvider;

		public MusicPlayer player;
		public Database database;

		// Timer tickTimer;

		Label songProgressText;
		Label songDurationText;
		ProgressBar songProgressbar;

		public Random rng;

		public MainWindow() : base("Kirbo")
		{
			current = this;

			SetPosition(WindowPosition.Center);
			SetDefaultSize(1280, 720);

			rng = new Random(Guid.NewGuid().GetHashCode());

			player = new MusicPlayer();
			database = new Database();
			database.ReloadAll();

			// Main Layout
			var mainLayout = new VBox();

			// Tabs
			{
				var tabs = new Notebook();
				tabs.TabPos = PositionType.Left;

				// Music Page
				{
					var musicPage = new HPaned() { WideHandle = true };

					// Playlists
					{
						var playlistsPanel = new Stack();

						playlistsNotebook = new Notebook();
						playlistsNotebook.TabPos = PositionType.Left;

						database.UpdateUI(playlistsNotebook);

						playlistsPanel.Add(playlistsNotebook);

						musicPage.Add(playlistsPanel);
					}

					tabs.AppendPage(musicPage, new Label("🎵"));
				}

				// Settings Page
				{
					var settingsPage = new Label("Settings Page");

					tabs.AppendPage(settingsPage, new Label("🔧"));
				}

				mainLayout.Add(tabs);
			}

			Add(mainLayout);

			// Titlebar
			var titlebar = new HeaderBar();
			titlebar.ShowCloseButton = true;
			titlebar.Title = "Kirbo";

			{
				var vbox = new VBox(false, 0);
				{
					var hBox = new HBox(false, 4);

					var playButtonText = new Label("⏸");
					player.currentSongInstance.onStateChanged += s =>
					{
						switch (s.state)
						{
							case ManagedBass.PlaybackState.Stopped:
							case ManagedBass.PlaybackState.Paused:
								playButtonText.Text = "▶";
								break;
							case ManagedBass.PlaybackState.Playing:
								playButtonText.Text = "⏸";
								break;
							case ManagedBass.PlaybackState.Stalled:
								playButtonText.Text = "…";
								break;
						}
					};
					var playButton = new Button(playButtonText);
					playButton.Clicked += (sender, args) => player.currentSongInstance.TogglePause();
					hBox.PackStart(playButton, false, false, 0);
					var songInfoText = new Label("No song playing") { WidthRequest = 260 };
					player.onSongStarted += s =>
					{
						var sb = new StringBuilder(s.title);

						if (!string.IsNullOrEmpty(s.album))
						{
							sb.Append(" - ");
							sb.Append(s.album);
						}

						if (!string.IsNullOrEmpty(s.artist))
						{
							sb.Append(" - ");
							sb.Append(s.artist);
						}

						songInfoText.Text = sb.ToString();
					};
					hBox.PackStart(songInfoText, true, true, 0);

					var skipButton = new Button("⏭");
					skipButton.Clicked += (sender, args) => player.PlayRandomSongFromPlaylist();
					hBox.PackEnd(skipButton, false, false, 0);

					vbox.PackStart(hBox, true, true, 0);
				}

				{
					var hbox = new HBox();

					songProgressText = new Label("--:--");
					hbox.PackStart(songProgressText, false, false, 0);

					songProgressbar = new ProgressBar();
					hbox.PackStart(songProgressbar, true, true, 0);

					songDurationText = new Label("--:--");
					hbox.PackEnd(songDurationText, false, false, 0);

					vbox.PackEnd(hbox, false, false, 0);
				}

				titlebar.Add(vbox);

				var volumeSlider = new HScale(0, 100, 10) { Value = SongInsance.masterVolume * 100 };
				volumeSlider.ChangeValue += (sender, args) => SongInsance.masterVolume = volumeSlider.Value / 100;

				titlebar.Add(volumeSlider);
			}

			Titlebar = titlebar;

			resetProvider = new CssProvider();
			styleProvider = new CssProvider();

			if (ENABLE_CUSTOM_STYLES)
			{
				ReloadStyles();

				StyleContext.AddProviderForScreen(Screen, resetProvider, int.MaxValue - 1);
				StyleContext.AddProviderForScreen(Screen, styleProvider, int.MaxValue);
			}

			// tickTimer = new Timer(t => Tick(), null, 1000, 100);
		}

		protected override void OnDestroyed()
		{
			player.Dispose();

			Config.current.Save();
			MainWindow.current.database.Save();

			Application.Quit();

			base.OnDestroyed();
		}

		protected override void OnFocusActivated()
		{
			if (ENABLE_CUSTOM_STYLES)
				ReloadStyles();

			base.OnFocusActivated();
		}

		void Tick()
		{
			var position = player.currentSongInstance.position;

			songProgressText.Text = position.ToString(@"mm\:ss");
			songDurationText.Text = player.currentSongInstance.duration.ToString(@"mm\:ss");
			songProgressbar.Fraction = position / player.currentSongInstance.duration;
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
