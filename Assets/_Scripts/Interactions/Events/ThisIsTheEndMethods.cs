using System.Collections;
using Britney;
using UnityEngine;

public class ThisIsTheEndMethods : MonoBehaviour
{
    [SerializeField] private BritneyMovement _controller;

    // This method disables controls and starts the quit timer.
    public void DisableControls()
    {
        _controller.enabled = false;
    }
    public void EnableControls()
    {
        _controller.enabled = false;
    }
    public void QuitAfterDelay(float delay)
    {
        StartCoroutine(QuitAfterDelayCoroutine(delay));
    }

    private IEnumerator QuitAfterDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Application.Quit();
    }
}
