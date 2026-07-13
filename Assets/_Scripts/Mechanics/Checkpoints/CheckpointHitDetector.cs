using UnityEngine;

public class CheckpointHitDetector : MonoBehaviour
{
    public Checkpoint Check;
    public LayerMask layerMask;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((layerMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            if (ResetManager.Instance.CanReset && collision.TryGetComponent(out InkManager inkManager))
            {
                Check.OnTouched(
                    inkManager
                );
            }
            else
            {
                Debug.LogWarning($"InkManager component not found on {collision.gameObject.name}");
            }
        }
    }


}
