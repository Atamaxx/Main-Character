using NaughtyAttributes;
using UnityEngine;

public class AudioMethods : MonoBehaviour
{
    [SerializeField] private bool _stopAudioOnDestroy = true;

    public void PlayMusic()
    {
        AudioSystem.Instance.PlayMusic();
    }

    public void PlayAmbience()
    {
        AudioSystem.Instance.PlayAmbience();
    }

    public void StopMusic()
    {
        if (AudioSystem.Instance != null)
            AudioSystem.Instance.StopMusic(3f);
    }

    public void StopAmbience()
    {
        if (AudioSystem.Instance != null)
            AudioSystem.Instance.StopAmbience(3f);
    }

    public void PauseMusic()
    {
        AudioSystem.Instance.PauseMusic();
    }
    public void PauseAmbience()
    {
        AudioSystem.Instance.PauseAmbience();
    }

    public void ResumeMusic()
    {
        AudioSystem.Instance.ResumeMusic();
    }
    public void ResumeAmbience()
    {
        AudioSystem.Instance.ResumeAmbience();
    }



    [Button]
    public void PlayAllAudio()
    {
        PlayMusic();
        PlayAmbience();
    }
    [Button]
    public void StopAllAudio()
    {
        StopMusic();
        StopAmbience();
    }
    public void RenewLongLivingEvents()
    {
        AudioSystem.Instance.RenewLongLivingEvents();
    }
    public void RenewMusic()
    {
        AudioSystem.Instance.RenewMusicEvent();
    }

    public void RenewAmbience()
    {
        AudioSystem.Instance.RenewAmbienceEvent();
    }


    public void MuteMaster()
    {
        AudioSystem.Instance.MuteMaster();
    }

    public void UnmuteMaster()
    {
        AudioSystem.Instance.UnmuteMaster();
    }
    public void MuteMusic()
    {
        AudioSystem.Instance.MuteMusic();
    }

    public void UnmuteMusic()
    {
        AudioSystem.Instance.UnmuteMusic();
    }

    public void MuteAmbience()
    {
        AudioSystem.Instance.MuteAmbience();
    }

    public void UnmuteAmbience()
    {
        AudioSystem.Instance.UnmuteAmbience();
    }
    public void MuteSFX()
    {
        AudioSystem.Instance.MuteSFX();
    }

    public void UnmuteSFX()
    {
        AudioSystem.Instance.UnmuteSFX();
    }

    public void SaveVolumeSettings()
    {
        AudioSystem.Instance.SaveVolumeSettings();
    }


    void OnDestroy()
    {
        if (_stopAudioOnDestroy)
            StopAllAudio();
    }
}
