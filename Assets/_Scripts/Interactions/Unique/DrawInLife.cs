using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class DrawInLife : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _visualsToToggle = new();

    [SerializeField]
    private List<VisualEffect> _vfx = new();

    [SerializeField]
    private VisualEffect _headVFX;

    [SerializeField]
    private UnityEvent _onDrawInLife;

    [SerializeField]
    private MenuCursor _menuCursor;

    private Coroutine _holdCoroutine;

    private bool _finishDrawInLife = false;

    public void StartHold()
    {
        if (_finishDrawInLife)
            return;
        _menuCursor.SetIsCursorEnabled(false);
        _holdCoroutine = StartCoroutine(HoldResetCoroutine());
    }

    public void EndHold()
    {
        if (_finishDrawInLife)
            return;

        if (_holdCoroutine != null)
        {
            AudioSystem.Instance.StopSFXLoop("lifeHold");
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }

        foreach (var visual in _visualsToToggle)
        {
            visual.SetActive(false);
        }

        _menuCursor.SetIsCursorEnabled(true);
    }

    public void Reset()
    {
        print("RERERERAERAERASEWET");
        _menuCursor.RepositionCursor(_headVFX.transform);
        _finishDrawInLife = false;
        foreach (var visual in _visualsToToggle)
        {
            visual.SetActive(false);
        }
    }

    private IEnumerator HoldResetCoroutine()
    {
        AudioSystem.Instance.PlaySFXLoop("lifeHold", FMODEvents.Instance.LevelResetHoldSFX);

        yield return new WaitForSeconds(0.32f);
        _visualsToToggle[0].SetActive(true);
        StartCoroutine(TemporarilyIncreaseSpawnRate(_vfx[0]));
        yield return new WaitForSeconds(0.33f);
        _visualsToToggle[1].SetActive(true);
        StartCoroutine(TemporarilyIncreaseSpawnRate(_vfx[1]));
        yield return new WaitForSeconds(0.32f);
        _visualsToToggle[2].SetActive(true);
        StartCoroutine(TemporarilyIncreaseSpawnRate(_vfx[2]));
        yield return new WaitForSeconds(0.1f);
        _visualsToToggle[3].SetActive(true);
        StartCoroutine(TemporarilyIncreaseSpawnRate(_vfx[3]));

        _finishDrawInLife = true;
        _onDrawInLife.Invoke();
    }

    private IEnumerator TemporarilyIncreaseSpawnRate(VisualEffect vfx)
    {
        vfx.SetInt("SpawnRate", 400);
        yield return new WaitForSeconds(0.4f);
        vfx.SetInt("SpawnRate", 100);
    }
}
