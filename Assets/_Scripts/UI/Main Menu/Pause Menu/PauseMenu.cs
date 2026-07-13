using Unity.Cinemachine;
using UnityEngine;
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private CinemachineManager _cmManager;
    private bool _isPaused = false;
    private CinemachineCamera _lastLevelCamera;
    public void Pause()
    {
        if (!_isPaused && GameManager.Instance.State == GameState.Pause) return;

        _lastLevelCamera = _cmManager.GetActiveCamera();
        GameManager.Instance.ChangeState(GameState.Pause);
        _isPaused = true;
    }

    public void Resume()
    {
        if (_isPaused && GameManager.Instance.State == GameState.Gameplay) return;

        _cmManager.SwitchToCamera(_lastLevelCamera);
        GameManager.Instance.ChangeState(GameState.Gameplay);
        _isPaused = false;
    }
}
