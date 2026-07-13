using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [field: SerializeField] public float CurrentTime { private set; get; } = 0f;
    [SerializeField] private int _currentLineIndex = 0;
    [SerializeField] private List<TimeLine> _timeLines = new();

    private List<ITimelineTask> _tasks = new();
    public static TimeManager Instance { get; private set; }



    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Time instance in the scene.");
        }
        Instance = this;
    }

    private void Update()
    {
        CurrentTime = _timeLines[_currentLineIndex].CurrentLength;
        UpdateTasks();
    }

    public void RegisterTask(ITimelineTask task)
    {
        if (!_tasks.Contains(task))
            _tasks.Add(task);
    }
    public void UnregisterTask(ITimelineTask task)
    {
        if (_tasks.Contains(task))
            _tasks.Remove(task);
    }

    private void UpdateTasks()
    {
        foreach (ITimelineTask task in _tasks)
        {
            task.OnUpdate(CurrentTime);
        }
    }
}


public interface ITimelineTask
{
    public event Action Stopped;
    public event Action Resumed;

    // Called whenever the timeline manager updates the current progress.
    void OnUpdate(float currentProgress);
}
