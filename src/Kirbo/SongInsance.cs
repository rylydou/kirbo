using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ManagedBass;

namespace Kirbo
{
	public class SongInsance : IDisposable
	{
		static double _masterVolume;
		public static double masterVolume
		{
			get => _masterVolume;
			set
			{
				if (_masterVolume != value)
				{
					_masterVolume = value;
					_onMasterVolumeChanged.Invoke();
				}
			}
		}
		static Action _onMasterVolumeChanged = () => { };

		public int handle { get; private set; }
		public long length { get; private set; }
		public TimeSpan duration { get; private set; }

		public TimeSpan position { get => BytesToTimeSpan(Bass.ChannelGetPosition(handle)); set => Bass.ChannelSetPosition(handle, TimeSpanToBytes(value)); }

		double _volume;
		public double volume
		{
			get => _volume;
			set
			{
				if (_volume != value)
				{
					_volume = value;
					SetVolume();
					onPropertyChanged.Invoke(this);
				}
			}
		}

		public PlaybackState state => Bass.ChannelIsActive(handle);
		public Action<SongInsance> onStateChanged = s => { };

		public Action<SongInsance> onFinish = s => { };
		public Action<SongInsance> onLoaded = s => { };
		public Action<SongInsance> onError = s => { };
		public Action<SongInsance> onDisposed = s => { };

		public Action<SongInsance> onPropertyChanged = s => { };

		readonly SynchronizationContext? _syncContext;

		public SongInsance()
		{
			_syncContext = SynchronizationContext.Current;

			_onMasterVolumeChanged += () => SetVolume();
		}

		public async Task<bool> LoadAsync(string file)
		{
			Trace.WriteLine("Loading " + file);

			try { if (this.handle != 0) Bass.StreamFree(this.handle); }
			catch { }

			var handle = await Task.Run(() => OnLoad(file));

			if (handle == 0) return false;

			this.handle = handle;

			// Init Events
			Bass.ChannelSetSync(handle, SyncFlags.Free, 0, GetSyncProcedure(() => onDisposed.Invoke(this)));
			Bass.ChannelSetSync(handle, SyncFlags.Stop, 0, GetSyncProcedure(() => onError.Invoke(this)));
			Bass.ChannelSetSync(handle, SyncFlags.End, 0, GetSyncProcedure(() => { onFinish.Invoke(this); onStateChanged.Invoke(this); }));

			length = Bass.ChannelGetLength(handle);
			duration = BytesToTimeSpan(length);

			onLoaded.Invoke(this);

			return true;
		}

		public bool Load(string file)
		{
			Trace.WriteLine("Loading " + file);

			try { if (this.handle != 0) Bass.StreamFree(this.handle); }
			catch { }

			var handle = OnLoad(file);

			if (handle == 0) return false;

			this.handle = handle;

			// Init Events
			Bass.ChannelSetSync(handle, SyncFlags.Free, 0, GetSyncProcedure(() => onDisposed.Invoke(this)));
			Bass.ChannelSetSync(handle, SyncFlags.Stop, 0, GetSyncProcedure(() => onError.Invoke(this)));
			Bass.ChannelSetSync(handle, SyncFlags.End, 0, GetSyncProcedure(() => { onFinish.Invoke(this); onStateChanged.Invoke(this); }));

			length = Bass.ChannelGetLength(handle);
			duration = BytesToTimeSpan(length);

			Bass.ChannelSetAttribute(handle, ChannelAttribute.Volume, _volume);

			onLoaded.Invoke(this);

			return true;
		}

		int OnLoad(string file) => Bass.CreateStream(file);

		public TimeSpan BytesToTimeSpan(long position) => TimeSpan.FromSeconds(Bass.ChannelBytes2Seconds(handle, position));
		public long TimeSpanToBytes(TimeSpan span) => Bass.ChannelSeconds2Bytes(handle, span.TotalSeconds);

		public bool Play()
		{
			try { return Bass.ChannelPlay(handle); }
			finally { onStateChanged.Invoke(this); }
		}

		public bool Pause()
		{
			try { return Bass.ChannelPause(handle); }
			finally { onStateChanged.Invoke(this); }
		}

		public bool Stop()
		{
			try { return Bass.ChannelStop(handle); }
			finally { onStateChanged.Invoke(this); }
		}

		public void Dispose()
		{
			try { if (Bass.StreamFree(handle)) handle = 0; }
			finally { onStateChanged.Invoke(this); }
		}

		SyncProcedure GetSyncProcedure(Action handler)
		{
			return (syncHandle, channel, data, user) =>
			{
				if (handler == null) return;
				if (_syncContext == null) handler.Invoke();
				else _syncContext.Post(S => handler.Invoke(), null);
			};
		}

		void SetVolume()
		{
			Bass.ChannelSetAttribute(handle, ChannelAttribute.Volume, volume * masterVolume);
		}
	}
}
