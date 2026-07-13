using UnityEngine;
using UnityEngine.SceneManagement; // Required for SceneManager

public class Finish : MonoBehaviour
{
    private enum Ending
    {
        LevelCompletion,
        SecretEnding,
    }

    [SerializeField]
    private Ending _endingType = Ending.LevelCompletion;

    [SerializeField]
    private string _secretLevelName = ""; // This can be used to specify a secret level directly from the Finish component

    [Header("Debug")]
    [SerializeField]
    private bool _debugMode = false;

    public void FinishLevel()
    {
        if (_debugMode)
        {
            Debug.Log($"FinishLevel called. Ending type: {_endingType}");
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        if (_endingType == Ending.LevelCompletion)
        {
            GameManager.Instance.ChangeState(GameState.LevelCompletion);
        }
        else // SecretEnding
        {
            // Pass the specific secret level name from this Finish instance,
            // or an empty string if it should be determined by LevelLoader.
            GameManager.Instance.CompleteSecretLevel(_secretLevelName);
        }
    }
}
