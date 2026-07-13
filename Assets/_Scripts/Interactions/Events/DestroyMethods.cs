using UnityEngine;

public class DestroyMethods : MonoBehaviour
{

    public void DestroyThisAfterTime(float delay)
    {
        Destroy(gameObject, delay);
    }
}
