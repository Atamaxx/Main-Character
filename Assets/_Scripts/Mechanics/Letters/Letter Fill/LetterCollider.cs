using TMPro;
using UnityEngine;

namespace Letters
{
    public class LetterCollider : MonoBehaviour
    {
        private enum ColliderType { Box, Circle, Capsule }

        [Header("Collider Settings")]
        [SerializeField] private ColliderType colliderType = ColliderType.Box;
        [SerializeField] private Vector2 colliderSize = new(0.2f, 0.5f);

        [Header("Layer Mask")]
        [SerializeField] private LayerMask colliderObjectLayer;
        [SerializeField] private LayerMask inkObjectsLayer;
        [Header("Additional")]
        [SerializeField] private float _cooldownDuration = 0.1f;

        [Header("Optional Prefab for Letter Colliders")]
        [Tooltip("If set, this prefab will be used instead of creating a new GameObject.")]
        [SerializeField] private GameObject letterColliderPrefab;

        public void CreateCollidersForCharacters(TMP_Text tmpText, LetterFiller letterFiller)
        {
            TMP_TextInfo textInfo = tmpText.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                Vector3 charMidBot = (charInfo.bottomLeft + charInfo.topRight) / 2f;
                Vector3 worldPos = tmpText.transform.TransformPoint(charMidBot);

                // If a prefab is provided, instantiate it; otherwise, create a new GameObject.
                GameObject letterColliderGO;
                if (letterColliderPrefab != null)
                {
                    letterColliderGO = Instantiate(letterColliderPrefab, worldPos, tmpText.transform.rotation, tmpText.transform);
                    letterColliderGO.name = "LetterCollider_" + i;
                    letterColliderGO.layer = colliderObjectLayer;
                }
                else
                {
                    // Fallback: Create a new GameObject and add the desired collider component.
                    letterColliderGO = new GameObject("LetterCollider_" + i);
                    letterColliderGO.transform.position = worldPos;
                    letterColliderGO.transform.rotation = tmpText.transform.rotation;
                    letterColliderGO.transform.SetParent(tmpText.transform);

                    // Add collider based on selected collider type
                    switch (colliderType)
                    {
                        case ColliderType.Box:
                            {
                                BoxCollider2D box = letterColliderGO.AddComponent<BoxCollider2D>();
                                box.isTrigger = true;
                                box.size = colliderSize;
                                break;
                            }
                        case ColliderType.Circle:
                            {
                                CircleCollider2D circle = letterColliderGO.AddComponent<CircleCollider2D>();
                                circle.isTrigger = true;
                                // Use the smaller dimension as the circle's radius
                                circle.radius = Mathf.Min(colliderSize.x, colliderSize.y) * 0.5f;
                                break;
                            }
                        case ColliderType.Capsule:
                            {
                                CapsuleCollider2D capsule = letterColliderGO.AddComponent<CapsuleCollider2D>();
                                capsule.isTrigger = true;
                                capsule.size = colliderSize;
                                // capsule.direction = CapsuleDirection2D.Vertical; // Optionally set direction
                                break;
                            }
                    }
                }

                // Regardless of how we got our LetterColliderGO, ensure it has a LetterHitDetector.
                // (If the prefab already has one, this call won't duplicate it, but you may want
                // to check for an existing component first.)
                LetterHitDetector detector = letterColliderGO.GetComponent<LetterHitDetector>();
                if (detector == null)
                {
                    detector = letterColliderGO.AddComponent<LetterHitDetector>();
                }
                
                detector.characterIndex = i;
                detector.letterFiller = letterFiller;
                detector.layerMask = inkObjectsLayer;
                detector.cooldownDuration = _cooldownDuration;
            }
        }
    }
}
