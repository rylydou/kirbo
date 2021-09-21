using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Kirbo
{
	public class PlaylistView : Bin
	{
		public readonly Playlist playlist;

#nullable disable
		[UI] EntryBuffer _title;
		[UI] TextBuffer _description;

		[UI] Button _playButton;
		// [UI] Image _coverImage;
		[UI] ListStore _songsList;
#nullable enable

		public PlaylistView(Playlist playlist) : this(playlist, new Builder("PlaylistView.glade")) { }
		PlaylistView(Playlist playlist, Builder builder) : base(builder.GetRawOwnedObject("PlaylistView"))
		{
			this.playlist = playlist;

			builder.Autoconnect(this);

			_title.Text = playlist.title;
			_title.InsertedText += (sender, args) => playlist.title = _title.Text;
			_title.DeletedText += (sender, args) => playlist.title = _title.Text;

			_description.Text = playlist.description;
			_description.Changed += (sender, args) => playlist.description = _description.Text;

			_playButton.Clicked += (sender, args) =>
			{
				MainWindow.current.player.LoadPlaylist(playlist);
				MainWindow.current.player.PlayRandomSongFromPlaylist();
			};

			foreach (var song in playlist.songs)
			{
				_songsList.AppendValues(song.title, song.artist, song.album);
			}
		}
	}
}
