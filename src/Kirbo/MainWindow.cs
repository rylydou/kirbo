using System;
using Gtk;

namespace Kirbo
{
	class MainWindow : Window
	{
		bool ENABLE_CUSTOM_STYLES = false;

		CssProvider resetProvider;
		CssProvider styleProvider;

		MusicPlayer player;

		public MainWindow() : base("Kirbo")
		{
			SetPosition(WindowPosition.Center);
			SetDefaultSize(1280, 720);

			player = new MusicPlayer();

			// player.PlaySong(new Song(Environment.CurrentDirectory + "/data/music/Baba Is You - Crystal Is Still.wav"));

			// Main Layout
			var mainLayout = new Stack();

			// Tabs
			{
				var tabs = new Notebook();

				// Music Page
				{
					var musicPage = new HPaned() { WideHandle = true };

					// Playlists
					{
						var playlistsPanel = new Stack() { WidthRequest = 360 };


						var listView = new TreeView();
						listView.HeadersVisible = false;

						var listStore = new ListStore(typeof(string));
						listView.Model = listStore;

						var cellView = new CellRendererText();
						var column = new TreeViewColumn("Title", cellView);
						column.AddAttribute(cellView, "text", 0);
						listView.AppendColumn(column);

						playlistsPanel.Add(listView);

						Database.current.UpdatePlaylistsStore(listView);

						musicPage.Add(playlistsPanel);
					}

					// Playlist Content
					{
						var playlistContent = new Stack();

						musicPage.Add(playlistContent);
					}

					tabs.AppendPage(musicPage, new Label("Music"));
				}

				// Settings Page
				{
					var settingsPage = new Label("Settings Page");

					tabs.AppendPage(settingsPage, new Label("Settings"));
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
			Database.current.Save();

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
