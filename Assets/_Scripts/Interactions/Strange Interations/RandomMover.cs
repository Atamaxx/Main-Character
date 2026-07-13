using System.Collections.Generic;
using UnityEngine;

public class RandomMover : MonoBehaviour
{
    [SerializeField] private bool _pause = false;
    [SerializeField] private List<Transform> objectsToMove;
    [SerializeField] private Vector2 movementBounds = new Vector2(5f, 5f);
    [SerializeField] private Vector2 rotationSpeedRange = new Vector2(10f, 50f);
    [SerializeField] private Vector2 moveSpeedRange = new Vector2(1f, 5f);

    private Dictionary<Transform, Vector2> targetPositions;
    private Dictionary<Transform, float> rotationSpeeds;
    private Dictionary<Transform, float> moveSpeeds;

    private void Start()
    {
        targetPositions = new Dictionary<Transform, Vector2>();
        rotationSpeeds = new Dictionary<Transform, float>();
        moveSpeeds = new Dictionary<Transform, float>();

        foreach (var obj in objectsToMove)
        {
            ClampObjectToBounds(obj);
            SetNewTargetPosition(obj);
            SetRandomRotationSpeed(obj);
            SetRandomMoveSpeed(obj);
        }
    }

    private void Update()
    {
        if (_pause) return;

        foreach (var obj in objectsToMove)
        {
            MoveObject(obj);
            RotateObject(obj);
        }
    }

    public void SetPause(bool pause)
    {
        _pause = pause;
    }

    private void MoveObject(Transform obj)
    {
        obj.position = Vector2.MoveTowards((Vector2)obj.position, targetPositions[obj], moveSpeeds[obj] * Time.deltaTime);

        if (Vector2.Distance(obj.position, targetPositions[obj]) < 0.1f)
        {
            SetNewTargetPosition(obj);
        }
    }

    private void RotateObject(Transform obj)
    {
        obj.Rotate(0, 0, rotationSpeeds[obj] * Time.deltaTime);
    }

    private void SetNewTargetPosition(Transform obj)
    {
        Vector2 randomPosition = new Vector2(
            Random.Range(-movementBounds.x, movementBounds.x),
            Random.Range(-movementBounds.y, movementBounds.y)
        ) + (Vector2)transform.position;
        targetPositions[obj] = randomPosition;
    }

    private void SetRandomRotationSpeed(Transform obj)
    {
        float randomRotationSpeed = Random.Range(rotationSpeedRange.x, rotationSpeedRange.y);
        rotationSpeeds[obj] = randomRotationSpeed;
    }

    private void SetRandomMoveSpeed(Transform obj)
    {
        float randomMoveSpeed = Random.Range(moveSpeedRange.x, moveSpeedRange.y);
        moveSpeeds[obj] = randomMoveSpeed;
    }

    private void ClampObjectToBounds(Transform obj)
    {
        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(obj.position.x, transform.position.x - movementBounds.x, transform.position.x + movementBounds.x),
            Mathf.Clamp(obj.position.y, transform.position.y - movementBounds.y, transform.position.y + movementBounds.y),
            obj.position.z
        );
        obj.position = clampedPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(movementBounds.x * 2, movementBounds.y * 2, 0));
    }
}