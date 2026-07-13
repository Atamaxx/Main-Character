using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
public class LoadSceneOnButton : MonoBehaviour
{
    [Scene] public string sceneToLoad = "YourSceneName";
    public KeyCode loadSceneKey = KeyCode.F2;

    void Update()
    {
        if (Input.GetKeyDown(loadSceneKey))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
