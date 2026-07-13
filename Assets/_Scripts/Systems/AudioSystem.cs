using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

public class AudioSystem : PersistentSingleton<AudioSystem>
{
    #region Volume Properties

    // PlayerPrefs keys for volume settings
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string AMBIENCE_VOLUME_KEY = "AmbienceVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    // Volume settings with default values
    [BoxGroup("MIXER"), Range(0, 1)]
    public float MasterVolume = 1;

    [BoxGroup("MIXER"), Range(0, 1)]
    public float MusicVolume = 1;

    [BoxGroup("MIXER"), Range(0, 1)]
    public float AmbienceVolume = 1;

    [BoxGroup("MIXER"), Range(0, 1)]
    public float SFXVolume = 1;

    // FMOD Bus references
    private Bus _masterBus;
    private Bus _musicBus;
    private Bus _ambienceBus;
    private Bus _SFXBus;

    #endregion

    [Header("FMOD Bank Settings")]
    [Tooltip("List of FMOD bank names that should be loaded on startup.")]
    public List<string> banksToLoad = new();

    // FMOD event instances
    private EventInstance musicInstance;
    private EventInstance ambienceInstance;
    private Dictionary<string, EventInstance> sfxInstances = new();

    /// <summary>
    /// Initialize audio system and load saved settings.
    /// </summary>
    private void Start()
    {
        InitializeBuses();
        LoadVolumeSettings();
        LoadBanks();
        CreateLongLivingEvents();
    }

    /// <summary>
    /// Initialize FMOD buses.
    /// </summary>
    private void InitializeBuses()
    {
        _masterBus = RuntimeManager.GetBus("bus:/");
        _musicBus = RuntimeManager.GetBus("bus:/Music");
        _ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        _SFXBus = RuntimeManager.GetBus("bus:/SFX");
    }

    /// <summary>
    /// Load saved volume settings from PlayerPrefs.
    /// </summary>
    private void LoadVolumeSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        AmbienceVolume = PlayerPrefs.GetFloat(AMBIENCE_VOLUME_KEY, 1f);
        SFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    /// <summary>
    /// Save current volume settings to PlayerPrefs.
    /// </summary>
    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, MasterVolume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, MusicVolume);
        PlayerPrefs.SetFloat(AMBIENCE_VOLUME_KEY, AmbienceVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SFXVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Apply volume settings to FMOD buses.
    /// </summary>
    private void Update()
    {
        _masterBus.setVolume(MasterVolume);
        _musicBus.setVolume(MusicVolume);
        _ambienceBus.setVolume(AmbienceVolume);
        _SFXBus.setVolume(SFXVolume);
    }

    /// <summary>
    /// Loads specified banks into memory.
    /// </summary>
    private void LoadBanks()
    {
        foreach (var bankName in banksToLoad)
        {
            RuntimeManager.LoadBank(bankName, true);
        }
    }

    /// <summary>
    /// Create event instances for music and ambience.
    /// </summary>
    private void CreateLongLivingEvents()
    {
        if (FMODEvents.Instance.Music.IsNull)
        {
            Debug.Log("Music event reference is not set in FMODEvents.");
            return;
        }
        else
            musicInstance = RuntimeManager.CreateInstance(FMODEvents.Instance.Music);

        if (FMODEvents.Instance.Ambient.IsNull)
        {
            Debug.Log("Ambient event reference is not set in FMODEvents.");
            return;
        }
        else
            ambienceInstance = RuntimeManager.CreateInstance(FMODEvents.Instance.Ambient);
    }

    /// <summary>
    /// Recreate long-living event instances.
    /// </summary>
    public void RenewLongLivingEvents()
    {
        RenewMusicEvent();
        RenewAmbienceEvent();
    }

    /// <summary>
    /// Recreate the music event instance.
    /// </summary>
    public void RenewMusicEvent()
    {
        if (FMODEvents.Instance.Music.IsNull)
        {
            musicInstance.release();
            Debug.Log("Music event reference is not set in FMODEvents.");
            return;
        }
        else
            musicInstance = RuntimeManager.CreateInstance(FMODEvents.Instance.Music);
    }

    /// <summary>
    /// Recreate the ambience event instance.
    /// </summary>
    public void RenewAmbienceEvent()
    {
        if (FMODEvents.Instance.Ambient.IsNull)
        {
            ambienceInstance.release();
            Debug.Log("Ambient event reference is not set in FMODEvents.");
            return;
        }
        else
            ambienceInstance = RuntimeManager.CreateInstance(FMODEvents.Instance.Ambient);
    }

    #region MUSIC

    /// <summary>
    /// Play a specific music event.
    /// </summary>
    public void PlayMusic(EventReference eventReference)
    {
        // Create and start new music instance
        musicInstance = RuntimeManager.CreateInstance(eventReference);
        PlayMusic();
    }

    /// <summary>
    /// Play the current music event instance.
    /// </summary>
    [Button]
    public void PlayMusic()
    {
        PLAYBACK_STATE playbackState;
        musicInstance.getPlaybackState(out playbackState);

        if (playbackState != PLAYBACK_STATE.PLAYING)
        {
            musicInstance.start();
        }
    }

    /// <summary>
    /// Stop the music with optional fade out.
    /// </summary>
    /// <param name="fadeOutTime">Set fadeOutTime to > 0 for a fade out in seconds.</param>
    [Button]
    public void StopMusic(float fadeOutTime = 0f)
    {
        if (fadeOutTime > 0f)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        else
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    /// <summary>
    /// Pause the music playback.
    /// </summary>
    [Button]
    public void PauseMusic()
    {
        bool isPaused;
        musicInstance.getPaused(out isPaused);

        if (!isPaused)
        {
            musicInstance.setPaused(true);
        }
    }

    /// <summary>
    /// Resume the music playback if paused.
    /// </summary>
    [Button]
    public void ResumeMusic()
    {
        bool isPaused;
        musicInstance.getPaused(out isPaused);

        if (isPaused)
        {
            musicInstance.setPaused(false);
        }
    }

    /// <summary>
    /// Set a parameter on the music event.
    /// </summary>
    public void SetMusicParameter(string paramName, float value)
    {
        musicInstance.setParameterByName(paramName, value);
    }

    /// <summary>
    /// Set a global parameter that affects music.
    /// </summary>
    public void SetMusicGlobalParameter(string paramName, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(paramName, value);
    }

    #endregion

    #region AMBIENCE

    /// <summary>
    /// Play the current ambience event instance.
    /// </summary>
    [Button]
    public void PlayAmbience()
    {
        PLAYBACK_STATE playbackState;
        ambienceInstance.getPlaybackState(out playbackState);

        if (playbackState != PLAYBACK_STATE.PLAYING)
        {
            ambienceInstance.start();
        }
    }

    /// <summary>
    /// Stop the ambience with optional fade out.
    /// </summary>
    /// <param name="fadeOutTime">Set fadeOutTime to > 0 for a fade out in seconds.</param>
    [Button]
    public void StopAmbience(float fadeOutTime = 0f)
    {
        if (fadeOutTime > 0f)
        {
            ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        else
        {
            ambienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    /// <summary>
    /// Pause the ambience playback.
    /// </summary>
    [Button]
    public void PauseAmbience()
    {
        bool isPaused;
        ambienceInstance.getPaused(out isPaused);

        if (!isPaused)
        {
            ambienceInstance.setPaused(true);
        }
    }

    /// <summary>
    /// Resume the ambience playback if paused.
    /// </summary>
    [Button]
    public void ResumeAmbience()
    {
        bool isPaused;
        ambienceInstance.getPaused(out isPaused);

        if (isPaused)
        {
            ambienceInstance.setPaused(false);
        }
    }

    /// <summary>
    /// Set a parameter on the ambience event.
    /// </summary>
    public void SetAmbienceParameter(string paramName, float value)
    {
        ambienceInstance.setParameterByName(paramName, value);
    }

    /// <summary>
    /// Set a global parameter that affects ambience.
    /// </summary>
    public void SetAmbienceGlobalParameter(string paramName, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(paramName, value);
    }

    #endregion

    #region SFX

    /// <summary>
    /// Play a one-shot SFX at a given position (3D) or no position (2D).
    /// </summary>
    /// <param name="eventReference">FMOD Event Reference for the SFX.</param>
    /// <param name="worldPosition">Optional position for 3D sound. If null, plays 2D.</param>
    public void PlaySFXOneShot(EventReference eventReference, Vector3? worldPosition = null)
    {
        if (worldPosition.HasValue)
        {
            // 3D SFX
            RuntimeManager.PlayOneShot(eventReference, worldPosition.Value);
        }
        else
        {
            // 2D SFX
            RuntimeManager.PlayOneShot(eventReference);
        }
    }

    /// <summary>
    /// Create a looping SFX event instance for more control.
    /// </summary>
    public void PlaySFXLoop(string sfxKey, EventReference eventReference)
    {
        // If already created and playing, do nothing
        if (sfxInstances.ContainsKey(sfxKey))
        {
            PLAYBACK_STATE playbackState;
            sfxInstances[sfxKey].getPlaybackState(out playbackState);

            if (playbackState == PLAYBACK_STATE.PLAYING)
                return;
        }

        // Create instance
        EventInstance instance = RuntimeManager.CreateInstance(eventReference);
        sfxInstances[sfxKey] = instance;
        instance.start();
    }

    /// <summary>
    /// Stop looping SFX by key.
    /// </summary>
    public void StopSFXLoop(string sfxKey, float fadeOutTime = 0f)
    {
        if (!sfxInstances.ContainsKey(sfxKey))
            return;

        if (fadeOutTime > 0f)
        {
            sfxInstances[sfxKey].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        else
        {
            sfxInstances[sfxKey].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        sfxInstances[sfxKey].release();
        sfxInstances.Remove(sfxKey);
    }

    /// <summary>
    /// Set a parameter on a looping SFX instance.
    /// </summary>
    public void SetSFXParameter(string sfxKey, string paramName, float value)
    {
        if (!sfxInstances.ContainsKey(sfxKey))
            return;
        sfxInstances[sfxKey].setParameterByName(paramName, value);
    }

    #endregion

    #region Volume Controls

    /// <summary>
    /// Sets the master volume (0-1 range) and saves the setting.
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Sets the music volume (0-1 range) and saves the setting.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Sets the SFX volume (0-1 range) and saves the setting.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Sets the ambience volume (0-1 range) and saves the setting.
    /// </summary>
    public void SetAmbienceVolume(float volume)
    {
        AmbienceVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Reset all volume settings to defaults.
    /// </summary>
    public void ResetVolumeSettings()
    {
        MasterVolume = 1f;
        MusicVolume = 1f;
        AmbienceVolume = 1f;
        SFXVolume = 1f;
        SaveVolumeSettings();
    }

    #endregion

    #region Mute Controls

    public bool IsVolumeMuted(VolumeType volumeType)
    {
        bool isMuted = false;

        // Get the appropriate bus and check its mute state
        switch (volumeType)
        {
            case VolumeType.Master:
                RuntimeManager.GetBus("bus:/").getMute(out isMuted);
                break;
            case VolumeType.Music:
                RuntimeManager.GetBus("bus:/Music").getMute(out isMuted);
                break;
            case VolumeType.Ambience:
                RuntimeManager.GetBus("bus:/Ambience").getMute(out isMuted);
                break;
            case VolumeType.SoundEffects:
                RuntimeManager.GetBus("bus:/SFX").getMute(out isMuted);
                break;
        }

        return isMuted;
    }

    public void MuteMaster()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.audioMasterMute = true;
#else
        RuntimeManager.MuteAllEvents(true);
#endif
    }

    public void UnmuteMaster()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.audioMasterMute = false;
#else
        RuntimeManager.MuteAllEvents(false);
#endif
    }

    /// <summary>
    /// Mute the ambience bus.
    /// </summary>
    public void MuteAmbience()
    {
        _ambienceBus.setMute(true);
    }

    /// <summary>
    /// Unmute the ambience bus.
    /// </summary>
    public void UnmuteAmbience()
    {
        _ambienceBus.setMute(false);
    }

    /// <summary>
    /// Mute the SFX bus.
    /// </summary>
    public void MuteSFX()
    {
        _SFXBus.setMute(true);
    }

    /// <summary>
    /// Unmute the SFX bus.
    /// </summary>
    public void UnmuteSFX()
    {
        _SFXBus.setMute(false);
    }

    /// <summary>
    /// Mute the music bus.
    /// </summary>
    public void MuteMusic()
    {
        _musicBus.setMute(true);
    }

    /// <summary>
    /// Unmute the music bus.
    /// </summary>
    public void UnmuteMusic()
    {
        _musicBus.setMute(false);
    }

    /// <summary>
    /// Sets a specific bus volume.
    /// </summary>
    public void SetBusVolume(string busPath, float volume)
    {
        Bus bus = RuntimeManager.GetBus(busPath);
        bus.setVolume(volume);
    }

    /// <summary>
    /// Stop all audio events.
    /// </summary>
    public void StopAll(float fadeOutTime = 0f)
    {
        if (fadeOutTime > 0f)
        {
            RuntimeManager.GetBus("bus:/").stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        else
        {
            RuntimeManager.GetBus("bus:/").stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    #endregion

    /// <summary>
    /// Clean up FMOD resources.
    /// </summary>
    public void CleanUp()
    {
        // Stop and release the music instance if it is valid
        if (musicInstance.isValid())
        {
            PLAYBACK_STATE musicState;
            musicInstance.getPlaybackState(out musicState);
            if (musicState == PLAYBACK_STATE.PLAYING)
            {
                musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
            musicInstance.release();
        }

        // Stop and release the ambience instance if it is valid
        if (ambienceInstance.isValid())
        {
            PLAYBACK_STATE ambienceState;
            ambienceInstance.getPlaybackState(out ambienceState);
            if (ambienceState == PLAYBACK_STATE.PLAYING)
            {
                ambienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
            ambienceInstance.release();
        }

        // Stop and release all looping SFX instances
        foreach (var sfx in sfxInstances.Values)
        {
            sfx.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sfx.release();
        }
        sfxInstances.Clear();
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
