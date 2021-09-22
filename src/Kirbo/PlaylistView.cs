using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Kirbo
{
	public class PlaylistView : Box
	{
		public readonly Playlist playlist;

		Label tabLable;

#nullable disable
		[UI] EntryBuffer title;
		[UI] TextBuffer description;
		[UI] ListStore songsList;

		[UI] Button playButton;
		[UI] Image coverImage;
		[UI] TextView descriptionView;
		[UI] TreeView songListView;
#nullable enable

		public PlaylistView(Playlist playlist, Label tabLable) : this(playlist, tabLable, new Builder("PlaylistView.glade")) { }
		PlaylistView(Playlist playlist, Label tabLable, Builder builder) : base(builder.GetRawOwnedObject("PlaylistView"))
		{
			this.playlist = playlist;
			this.tabLable = tabLable;

			builder.Autoconnect(this);

			title.Text = playlist.title;
			title.InsertedText += (sender, args) => playlist.title = title.Text;
			title.DeletedText += (sender, args) => playlist.title = title.Text;

			playButton.Clicked += (sender, args) =>
			{
				MainWindow.current.player.LoadPlaylist(playlist);
				MainWindow.current.player.PlayRandomSongFromPlaylist();
			};

			description.Text = playlist.description;
			description.Changed += (sender, args) => playlist.description = description.Text;

			foreach (var song in playlist.songs)
			{
				songsList.AppendValues(song.title, song.artist, song.album);
			}
		}
	}
}
