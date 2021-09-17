using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Kirbo
{
	public class PlaylistsView : VBox
	{
		public readonly Playlist playlist;

		public readonly Button playButton;
		public readonly Entry titleEntry;
		public readonly TextView descriptionEntry;
		public readonly ScrolledWindow songListScroll;
		public readonly TreeView songsTreeView;
		public readonly ListStore songsList;

		public PlaylistsView(Playlist playlist)
		{
			this.playlist = playlist;

			playButton = new Button(new Label("Play"));
			playButton.Clicked += (sender, args) =>
			{
				MainWindow.current.player.LoadPlaylist(playlist);
				MainWindow.current.player.PlayRandomSongFromPlaylist();
			};
			PackStart(playButton, false, false, 0);

			titleEntry = new Entry(playlist.title);
			titleEntry.Changed += (sender, args) => playlist.title = titleEntry.Text;
			PackStart(titleEntry, false, false, 0);

			descriptionEntry = new TextView() { Editable = true, WrapMode = WrapMode.Word };
			descriptionEntry.Buffer.Text = playlist.description;
			descriptionEntry.Buffer.Changed += (sender, args) => playlist.description = descriptionEntry.Buffer.Text;
			PackStart(descriptionEntry, false, false, 4);

			songListScroll = new ScrolledWindow();

			songsTreeView = new TreeView() { HeadersVisible = false };

			var titleCellView = new CellRendererText();
			var titleColumn = new TreeViewColumn("Title", titleCellView);
			titleColumn.AddAttribute(titleCellView, "text", 0);
			songsTreeView.AppendColumn(titleColumn);

			var artistCellView = new CellRendererText();
			var artistColumn = new TreeViewColumn("Artist", artistCellView);
			artistColumn.AddAttribute(artistCellView, "text", 1);
			songsTreeView.AppendColumn(artistColumn);

			var albumCellView = new CellRendererText();
			var albumColumn = new TreeViewColumn("Album", albumCellView);
			albumColumn.AddAttribute(albumCellView, "text", 2);
			songsTreeView.AppendColumn(albumColumn);

			songsList = new ListStore(typeof(string), typeof(string), typeof(string));

			songsTreeView.Model = songsList;

			foreach (var song in playlist.songs)
			{
				songsList.AppendValues(song.title, song.artist, song.album);
			}

			songListScroll.Add(songsTreeView);

			PackEnd(songListScroll, true, true, 4);

			// ShowAll();
		}
	}
}
