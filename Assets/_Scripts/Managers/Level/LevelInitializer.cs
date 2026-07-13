using UnityEngine;
using UnityEngine.Events;

public class LevelInitializer : MonoBehaviour
{
    [SerializeField] private UnityEvent _onInit;

    private void Start()
    {
        // Change state to Starting - which will now transition to Gameplay automatically
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.Starting);
        }
        
        // Fire any custom initialization events
        _onInit?.Invoke();
        
        // Save game progress
        GameSession gameSession = GameSession.Instance;
        if (gameSession != null)
        {
            gameSession.SaveGame();
        }
    }
}