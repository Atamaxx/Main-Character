using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Letters
{
    public class LetterHitDetector : MonoBehaviour
    {
        public int characterIndex;
        public LetterFiller letterFiller;
        public LayerMask layerMask;
        public float cooldownDuration;
        private bool _onCooldown = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((layerMask.value & (1 << collision.gameObject.layer)) != 0)
            {
                if (collision.TryGetComponent(out InkManager inkManager))
                {
                    if (_onCooldown)
                    {
                        return;
                    }

                    letterFiller.OnLetterTouched(characterIndex, inkManager);
                    StartCoroutine(StartCooldown());
                }
                else
                {
                    Debug.LogWarning($"InkManager component not found on {collision.gameObject.name}");
                }
            }
        }


        private IEnumerator StartCooldown()
        {
            _onCooldown = true;
            yield return new WaitForSeconds(cooldownDuration);
            _onCooldown = false;
        }
    }
}
