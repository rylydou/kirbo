using System;
using System.Threading;
using System.Threading.Tasks;
using ManagedBass;

namespace Kirbo
{
	public class SongInsance : IDisposable
	{
		public int handle { get; private set; }
		public long length { get; private set; }
		public TimeSpan duration { get; private set; }

		public TimeSpan position
		{
			get => BytesToTimeSpan(Bass.ChannelGetPosition(handle));
			set => Bass.ChannelSetPosition(handle, TimeSpanToBytes(value));
		}

		public float volume { get => (float)Bass.ChannelGetAttribute(handle, ChannelAttribute.Volume); set => Bass.ChannelSetAttribute(handle, ChannelAttribute.Volume, value)}

		public readonly Action<SongInsance> onFinish = s => { };
		public readonly Action<SongInsance> onLoaded = s => { };
		public readonly Action<SongInsance> onError = s => { };
		public readonly Action<SongInsance> onDisposed = s => { };

		public PlaybackState state => Bass.ChannelIsActive(handle);
		public readonly Action<SongInsance> onStateChanged = s => { };

		readonly SynchronizationContext? _syncContext;

		public SongInsance()
		{
			_syncContext = SynchronizationContext.Current;
		}

		int OnLoad(string file) => Bass.CreateStream(file);

		public async Task<bool> LoadAsync(string FileName)
		{
			try
			{
				if (this.handle != 0) Bass.StreamFree(this.handle);
			}
			catch { }

			if (_dev != -1)
				Bass.CurrentDevice = _dev;

			var currentDev = Bass.CurrentDevice;

			if (currentDev == -1 || !Bass.GetDeviceInfo(Bass.CurrentDevice).IsInitialized)
				Bass.Init(currentDev);

			var handle = await Task.Run(() => OnLoad(FileName));

			if (handle == 0) return false;

			this.handle = handle;

			var tags = TagReader.Read(handle);

			InitProperties();

			onLoaded.Invoke(this);

			return true;
		}

		SyncProcedure GetSyncProcedure(Action Handler)
		{
			return (SyncHandle, Channel, Data, User) =>
			{
				if (Handler == null)
					return;

				if (_syncContext == null)
					Handler();
				else _syncContext.Post(S => Handler(), null);
			};
		}

		public TimeSpan BytesToTimeSpan(long position) => TimeSpan.FromSeconds(Bass.ChannelBytes2Seconds(handle, position));
		public long TimeSpanToBytes(TimeSpan span) => Bass.ChannelSeconds2Bytes(handle, span.TotalSeconds);

		public bool Play()
		{
			try { return Bass.ChannelPlay(handle); }
			finally { onStateChanged(this); }
		}

		public bool Pause()
		{
			try { return Bass.ChannelPause(handle); }
			finally { onStateChanged(this); }
		}

		public bool Stop()
		{
			try
			{
				return Bass.ChannelStop(handle);
			}
			finally
			{
				onStateChanged(this);
			}
		}

		public void Dispose()
		{
			Bass.StreamFree(handle);
		}
	}
}
