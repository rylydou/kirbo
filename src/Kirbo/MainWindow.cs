using System;
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

		public MainWindow() : base("Kirbo")
		{
			current = this;

			SetPosition(WindowPosition.Center);
			SetDefaultSize(1280, 720);

			player = new MusicPlayer();
			database = new Database();

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
						var playlistsPanel = new Stack() { WidthRequest = 360 };

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
				var frame = new Frame();

				{
					var box = new HBox(false, 4);

					box.Add(new Button("Play"));
					box.Add(new Label("C418 - Minecraft - Beta"));
					box.Add(new Button("Skip"));

					frame.Add(box);
				}

				titlebar.Add(frame);
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
