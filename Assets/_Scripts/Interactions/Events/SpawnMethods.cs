using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMethods : MonoBehaviour
{
    [SerializeField] private GameObject _prefabToSpawn;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private int _maxObjects = 5; // Maximum number of spawned objects

    // List to store all spawned objects
    private List<GameObject> _spawnedObjects = new List<GameObject>();

    private void Start()
    {
        // If no spawn point is provided, default to this object's transform.
        if (_spawnPoint == null)
        {
            _spawnPoint = transform;
        }
    }

    /// <summary>
    /// Spawns the prefab instantly at the spawn point.
    /// If the maximum number of objects is reached, the oldest is destroyed.
    /// </summary>
    public void Spawn()
    {
        // Remove the oldest spawned object if we've reached the maximum limit.
        if (_spawnedObjects.Count >= _maxObjects)
        {
            Destroy(_spawnedObjects[0]);
            _spawnedObjects.RemoveAt(0);
        }

        // Instantiate and store the new object.
        GameObject newObj = Instantiate(_prefabToSpawn, _spawnPoint.position, _spawnPoint.rotation);
        _spawnedObjects.Add(newObj);
    }

    public void Spawn(Vector2 spawnPostion)
    {
        // Remove the oldest spawned object if we've reached the maximum limit.
        if (_spawnedObjects.Count >= _maxObjects)
        {
            Destroy(_spawnedObjects[0]);
            _spawnedObjects.RemoveAt(0);
        }

        // Instantiate and store the new object.
        GameObject newObj = Instantiate(_prefabToSpawn, spawnPostion, Quaternion.identity);
        _spawnedObjects.Add(newObj);
    }

    /// <summary>
    /// Spawns the prefab and smoothly scales it from zero to its original size.
    /// If the maximum number of objects is reached, the oldest is destroyed.
    /// </summary>
    public void SpawnSmooth()
    {
        if (_spawnedObjects.Count >= _maxObjects)
        {
            Destroy(_spawnedObjects[0]);
            _spawnedObjects.RemoveAt(0);
        }

        // Instantiate the prefab.
        GameObject newObj = Instantiate(_prefabToSpawn, _spawnPoint.position, _spawnPoint.rotation);
        _spawnedObjects.Add(newObj);

        // Capture the object's intended scale and set its current scale to zero.
        Vector3 originalScale = newObj.transform.localScale;
        newObj.transform.localScale = Vector3.zero;

        // Animate scaling up to the original size.
        StartCoroutine(AnimateSpawnScale(newObj.transform, originalScale));
    }

    /// <summary>
    /// Immediately destroys all spawned objects.
    /// </summary>
    public void ResetSpawn()
    {
        foreach (GameObject obj in _spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        _spawnedObjects.Clear();
    }

    /// <summary>
    /// Smoothly scales down all spawned objects before destroying them.
    /// </summary>
    public void ResetSpawnSmooth()
    {
        foreach (GameObject obj in _spawnedObjects)
        {
            if (obj != null)
            {
                StartCoroutine(AnimateDestroyScale(obj));
            }
        }
        _spawnedObjects.Clear();
    }

    /// <summary>
    /// Coroutine that smoothly scales up the target transform from zero to targetScale.
    /// </summary>
    /// <param name="targetTransform">The transform to animate.</param>
    /// <param name="targetScale">The final scale to reach.</param>
    private IEnumerator AnimateSpawnScale(Transform targetTransform, Vector3 targetScale)
    {
        Vector3 startScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            targetTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        targetTransform.localScale = targetScale;
    }

    /// <summary>
    /// Coroutine that smoothly scales down the object before destroying it.
    /// </summary>
    /// <param name="obj">The GameObject to scale down and destroy.</param>
    private IEnumerator AnimateDestroyScale(GameObject obj)
    {
        Transform targetTransform = obj.transform;
        Vector3 initialScale = targetTransform.localScale;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            targetTransform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(obj);
    }
}
