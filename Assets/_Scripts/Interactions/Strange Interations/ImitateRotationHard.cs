using UnityEngine;

public class ImitateRotationHard : MonoBehaviour
{
    [SerializeField]
    private Transform _target;

    [SerializeField]
    private bool _invertRotation = false;

    void Update()
    {
        if (_target != null)
        {
            if (_invertRotation)
                transform.localRotation = Quaternion.Inverse(_target.localRotation);
            else
                transform.localRotation = _target.localRotation;
        }
    }
}
