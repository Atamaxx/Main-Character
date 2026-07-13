using UnityEngine;

public class GameExiter : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_EDITOR
        // Stop play mode in editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the application in build
        Application.Quit();
#endif
    }
}
