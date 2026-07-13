using System;
using NaughtyAttributes;
using UnityEngine;

public class TimeMusic : MonoBehaviour, ITimelineTask
{
    [SerializeField, BoxGroup("SETTINGS")] private bool _pause = false;
    [SerializeField, BoxGroup("SETTINGS")] private string _timeParam = "Time";

    #region INTERFACE
    public event Action Stopped;
    public event Action Resumed;

    public void OnUpdate(float currentTime)
    {
        if (_pause) return;
        
        AudioSystem.Instance.SetMusicGlobalParameter(_timeParam, currentTime);
    }

    private void OnStopped()
    {
        _pause = true;
    }

    private void OnResumed()
    {
        _pause = false;
    }
    #endregion

    private void OnEnable()
    {
        TimeManager.Instance.RegisterTask(this);

        Stopped += OnStopped;
        Resumed += OnResumed;
    }

    private void OnDisable()
    {
        TimeManager.Instance.UnregisterTask(this);

        Stopped -= OnStopped;
        Resumed -= OnResumed;
    }
}
