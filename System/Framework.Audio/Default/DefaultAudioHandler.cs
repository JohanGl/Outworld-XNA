using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Framework.Audio
{
	/// <summary>
	/// Playback of sounds and music
	/// </summary>
	public class DefaultAudioHandler : IAudioHandler
	{
		private ContentManager contentManager;

		private bool enabled;
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			
			set
			{
				enabled = value;
				OnEnabledChanged();
			}
		}

		// Change MaxSounds to set the maximum number of simultaneous sounds that can be playing.
		private const int MaxSounds = 32;

		private Dictionary<string, Song> songs = new Dictionary<string, Song>();
		private Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
		
		private Song currentSong;
		private SoundEffectInstance[] playingSounds = new SoundEffectInstance[MaxSounds];

		// 3d sound
		private Dictionary<int, SoundEffect3d> playingSounds3d = new Dictionary<int, SoundEffect3d>();
		private AudioListener listener = new AudioListener();

		private bool isMusicPaused;

		private bool isFading;
		private MusicFadeEffect fadeEffect;

		/// <summary>
		/// Gets the name of the currently playing song, or null if no song is playing.
		/// </summary>
		public string CurrentSong { get; private set; }

		/// <summary>
		/// Gets or sets the volume to play songs. 1.0f is max volume.
		/// </summary>
		public float MusicVolume
		{
			get { return MediaPlayer.Volume; }
			set { MediaPlayer.Volume = value; }
		}

		/// <summary>
		/// Gets or sets the master volume for all sounds. 1.0f is max volume.
		/// </summary>
		public float SoundVolume
		{
			get { return SoundEffect.MasterVolume; }
			set { SoundEffect.MasterVolume = value; }
		}

		/// <summary>
		/// Gets whether a song is playing or paused (i.e. not stopped).
		/// </summary>
		public bool IsSongActive { get { return currentSong != null && MediaPlayer.State != MediaState.Stopped; } }

		/// <summary>
		/// Gets whether the current song is paused.
		/// </summary>
		public bool IsSongPaused { get { return currentSong != null && isMusicPaused; } }

		/// <summary>
		/// Initializes the audio handler
		/// </summary>
		/// <param name="contentManager">The game content manager</param>
		/// <param name="contentFolder">Root folder to load audio content from</param>
		public DefaultAudioHandler(ContentManager contentManager, string contentFolder = null)
		{
			enabled = true;

			if (String.IsNullOrEmpty(contentFolder))
			{
				this.contentManager = new ContentManager(contentManager.ServiceProvider, contentManager.RootDirectory);
			}
			else
			{
				this.contentManager = new ContentManager(contentManager.ServiceProvider, contentFolder);
			}

			// Set the scale for 3D audio so it matches the scale of our game world.
			// DistanceScale controls how much sounds change volume as you move further away.
			// DopplerScale controls how much sounds change pitch as you move past them.
			SoundEffect.DistanceScale = 10f;
			SoundEffect.DopplerScale = 0.1f;
		}

		/// <summary>
		/// Loads a Song into the AudioManager.
		/// </summary>
		/// <param name="songName">Name of the song to load</param>
		public void LoadSong(string songName)
		{
			LoadSong(songName, songName);
		}

		/// <summary>
		/// Loads a Song into the AudioManager.
		/// </summary>
		/// <param name="songName">Name of the song to load</param>
		/// <param name="songPath">Path to the song asset file</param>
		public void LoadSong(string songName, string songPath)
		{
			if (songs.ContainsKey(songName))
			{
				throw new InvalidOperationException(string.Format("Song '{0}' has already been loaded", songName));
			}

			songs.Add(songName, contentManager.Load<Song>(songPath));
		}

		/// <summary>
		/// Loads a SoundEffect into the AudioManager.
		/// </summary>
		/// <param name="soundName">Name of the sound to load</param>
		public void LoadSound(string soundName)
		{
			LoadSound(soundName, soundName);
		}

		/// <summary>
		/// Loads a SoundEffect into the AudioManager.
		/// </summary>
		/// <param name="soundName">Name of the sound to load</param>
		/// <param name="soundPath">Path to the song asset file</param>
		public void LoadSound(string soundName, string soundPath)
		{
			if (sounds.ContainsKey(soundName))
			{
				throw new InvalidOperationException(string.Format("Sound '{0}' has already been loaded", soundName));
			}

			sounds.Add(soundName, contentManager.Load<SoundEffect>(soundPath));
		}

		/// <summary>
		/// Unloads all loaded songs and sounds.
		/// </summary>
		public void UnloadContent()
		{
			contentManager.Unload();
		}

		/// <summary>
		/// Starts playing the song with the given name. If it is already playing, this method
		/// does nothing. If another song is currently playing, it is stopped first.
		/// </summary>
		/// <param name="songName">Name of the song to play</param>
		public void PlaySong(string songName)
		{
			PlaySong(songName, false);
		}

		/// <summary>
		/// Starts playing the song with the given name. If it is already playing, this method
		/// does nothing. If another song is currently playing, it is stopped first.
		/// </summary>
		/// <param name="songName">Name of the song to play</param>
		/// <param name="loop">True if song should loop, false otherwise</param>
		public void PlaySong(string songName, bool loop)
		{
			if (CurrentSong != songName)
			{
				if (currentSong != null)
				{
					MediaPlayer.Stop();
				}

				if (!songs.TryGetValue(songName, out currentSong))
				{
					throw new ArgumentException(string.Format("Song '{0}' not found", songName));
				}

				CurrentSong = songName;

				isMusicPaused = false;
				MediaPlayer.IsRepeating = loop;
				MediaPlayer.Play(currentSong);

				if (!Enabled)
				{
					MediaPlayer.Pause();
				}
			}
		}

		/// <summary>
		/// Pauses the currently playing song. This is a no-op if the song is already paused,
		/// or if no song is currently playing.
		/// </summary>
		public void PauseSong()
		{
			if (currentSong != null && !isMusicPaused)
			{
				if (Enabled) MediaPlayer.Pause();
				isMusicPaused = true;
			}
		}

		/// <summary>
		/// Resumes the currently paused song. This is a no-op if the song is not paused,
		/// or if no song is currently playing.
		/// </summary>
		public void ResumeSong()
		{
			if (currentSong != null && isMusicPaused)
			{
				if (Enabled) MediaPlayer.Resume();
				isMusicPaused = false;
			}
		}

		/// <summary>
		/// Stops the currently playing song. This is a no-op if no song is currently playing.
		/// </summary>
		public void StopSong()
		{
			if (currentSong != null && MediaPlayer.State != MediaState.Stopped)
			{
				MediaPlayer.Stop();
				isMusicPaused = false;
			}
		}

		/// <summary>
		/// Smoothly transition between two volumes.
		/// </summary>
		/// <param name="targetVolume">Target volume, 0.0f to 1.0f</param>
		/// <param name="duration">Length of volume transition</param>
		public void FadeSong(float targetVolume, TimeSpan duration)
		{
			if (duration <= TimeSpan.Zero)
			{
				throw new ArgumentException("Duration must be a positive value");
			}

			fadeEffect = new MusicFadeEffect(MediaPlayer.Volume, targetVolume, duration);
			isFading = true;
		}

		/// <summary>
		/// Stop the current fade.
		/// </summary>
		/// <param name="option">Options for setting the music volume</param>
		public void CancelFade(FadeCancelOptions option)
		{
			if (isFading)
			{
				switch (option)
				{
					case FadeCancelOptions.Source: MediaPlayer.Volume = fadeEffect.SourceVolume; break;
					case FadeCancelOptions.Target: MediaPlayer.Volume = fadeEffect.TargetVolume; break;
				}

				isFading = false;
			}
		}

		/// <summary>
		/// Plays the sound of the given name.
		/// </summary>
		/// <param name="soundName">Name of the sound</param>
		public void PlaySound(string soundName)
		{
			PlaySound(soundName, 1.0f, 0.0f, 0.0f);
		}

		/// <summary>
		/// Plays the sound of the given name at the given volume.
		/// </summary>
		/// <param name="soundName">Name of the sound</param>
		/// <param name="volume">Volume, 0.0f to 1.0f</param>
		public void PlaySound(string soundName, float volume)
		{
			PlaySound(soundName, volume, 0.0f, 0.0f);
		}

		/// <summary>
		/// Plays the sound of the given name with the given parameters.
		/// </summary>
		/// <param name="soundName">Name of the sound</param>
		/// <param name="volume">Volume, 0.0f to 1.0f</param>
		/// <param name="pitch">Pitch, -1.0f (down one octave) to 1.0f (up one octave)</param>
		/// <param name="pan">Pan, -1.0f (full left) to 1.0f (full right)</param>
		public void PlaySound(string soundName, float volume, float pitch, float pan)
		{
			SoundEffect sound;

			if (!sounds.TryGetValue(soundName, out sound))
			{
				throw new ArgumentException(string.Format("Sound '{0}' not found", soundName));
			}

			int index = GetAvailableSoundIndex();

			if (index != -1)
			{
				playingSounds[index] = sound.CreateInstance();
				playingSounds[index].Volume = volume;
				playingSounds[index].Pitch = pitch;
				playingSounds[index].Pan = pan;
				playingSounds[index].Play();

				if (!Enabled)
				{
					playingSounds[index].Pause();
				}
			}
		}

		public void PlaySound3d(string key, string soundName, float volume, Vector3 position)
		{
			SoundEffect sound;

			if (!sounds.TryGetValue(soundName, out sound))
			{
				throw new ArgumentException(string.Format("Sound '{0}' not found", soundName));
			}

			int index = GetAvailableSoundIndex();

			if (index != -1)
			{
				if (!playingSounds3d.ContainsKey(index))
				{
					playingSounds3d.Add(index, new SoundEffect3d());
				}

				playingSounds3d[index].Key = key;
				playingSounds3d[index].Emitter.Position = position;
				playingSounds3d[index].Emitter.Forward = Vector3.Forward;
				playingSounds3d[index].Emitter.Up = Vector3.Up;
				playingSounds3d[index].Emitter.Velocity = new Vector3(0, 0, 0);

				playingSounds[index] = sound.CreateInstance();
				playingSounds[index].Volume = volume;
				playingSounds[index].Apply3D(listener, playingSounds3d[index].Emitter);
				playingSounds[index].IsLooped = true;
				playingSounds[index].Play();

				if (!Enabled)
				{
					playingSounds[index].Pause();
				}
			}
		}

		public void UpdateSound3d(string key, Vector3 position)
		{
			foreach (var soundEffect3D in playingSounds3d.Values)
			{
				if (soundEffect3D.Key == key)
				{
					soundEffect3D.Emitter.Position = position;
					playingSounds[soundEffect3D.Index].Apply3D(listener, soundEffect3D.Emitter);
					break;
				}
			}
		}

		public void UpdateListener(Vector3 position)
		{
			listener.Position = position;
			listener.Up = Vector3.Up;
			listener.Forward = Vector3.Forward;
		}

		/// <summary>
		/// Stops all currently playing sounds.
		/// </summary>
		public void StopAllSounds()
		{
			for (int i = 0; i < playingSounds.Length; ++i)
			{
				if (playingSounds[i] != null)
				{
					playingSounds[i].Stop();
					playingSounds[i].Dispose();
					playingSounds[i] = null;
				}
			}
		}

		public void Update(GameTime gameTime)
		{
			for (int i = 0; i < playingSounds.Length; ++i)
			{
				if (playingSounds[i] != null && playingSounds[i].State == SoundState.Stopped)
				{
					playingSounds[i].Dispose();
					playingSounds[i] = null;
				}
			}

			if (currentSong != null && MediaPlayer.State == MediaState.Stopped)
			{
				currentSong = null;
				CurrentSong = null;
				isMusicPaused = false;
			}

			if (isFading && !isMusicPaused)
			{
				if (currentSong != null && MediaPlayer.State == MediaState.Playing)
				{
					if (fadeEffect.Update(gameTime.ElapsedGameTime))
					{
						isFading = false;
					}

					MediaPlayer.Volume = fadeEffect.GetVolume();
				}
				else
				{
					isFading = false;
				}
			}
		}

		private void OnEnabledChanged()
		{
			if (Enabled)
			{
				for (int i = 0; i < playingSounds.Length; ++i)
				{
					if (playingSounds[i] != null && playingSounds[i].State == SoundState.Paused)
					{
						playingSounds[i].Resume();
					}
				}

				if (!isMusicPaused)
				{
					MediaPlayer.Resume();
				}
			}
			else
			{
				for (int i = 0; i < playingSounds.Length; ++i)
				{
					if (playingSounds[i] != null && playingSounds[i].State == SoundState.Playing)
					{
						playingSounds[i].Pause();
					}
				}

				MediaPlayer.Pause();
			}
		}

		// Acquires an open sound slot.
		private int GetAvailableSoundIndex()
		{
			for (int i = 0; i < playingSounds.Length; ++i)
			{
				if (playingSounds[i] == null)
				{
					return i;
				}
			}

			return -1;
		}

		#region MusicFadeEffect

		private struct MusicFadeEffect
		{
			public float SourceVolume;
			public float TargetVolume;

			private TimeSpan time;
			private TimeSpan duration;

			public MusicFadeEffect(float sourceVolume, float targetVolume, TimeSpan duration)
			{
				SourceVolume = sourceVolume;
				TargetVolume = targetVolume;
				time = TimeSpan.Zero;
				this.duration = duration;
			}

			public bool Update(TimeSpan time)
			{
				this.time += time;

				if (this.time >= duration)
				{
					this.time = duration;
					return true;
				}

				return false;
			}

			public float GetVolume()
			{
				return MathHelper.Lerp(SourceVolume, TargetVolume, (float)time.Ticks / duration.Ticks);
			}
		}
	
		#endregion
	}

	internal class SoundEffect3d
	{
		public string Key;
		public int Index;
		public AudioEmitter Emitter;

		public SoundEffect3d()
		{
			Emitter = new AudioEmitter();
		}
	}
}