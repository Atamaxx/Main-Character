using UnityEngine;
using System.Collections;
namespace Utilities.Materials
{
    public static class PropertyChangeHelper
    {

        public static IEnumerator ChangeColorCoroutine(Material targetMaterial, int propertyID, Color from, Color to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Color currentColor = Color.Lerp(from, to, elapsed / duration);
                targetMaterial.SetColor(propertyID, currentColor);
                yield return null;
            }
            targetMaterial.SetColor(propertyID, to);
        }
        public static IEnumerator ChangeColorFromCurrent(Material material, int propertyID, Color end, float duration)
        {
            // Capture the starting time of the transition.
            float startTime = Time.time;

            Color start = material.GetColor(propertyID);
            
            while (Time.time - startTime < duration)
            {
                // Calculate the interpolation factor based on elapsed time.
                float t = (Time.time - startTime) / duration;

                // Interpolate between the start and end colors.
                Color currentColor = Color.Lerp(start, end, t);

                // Update the material's color property.
                material.SetColor(propertyID, currentColor);

                yield return null;
            }

            // Ensure the final color is set.
            material.SetColor(propertyID, end);
        }

        public static IEnumerator ChangeFloatCoroutine(Material targetMaterial, int propertyID, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float currentFloat = Mathf.Lerp(from, to, elapsed / duration);
                targetMaterial.SetFloat(propertyID, currentFloat);
                yield return null;
            }
            targetMaterial.SetFloat(propertyID, to);
        }


        public static IEnumerator ChangeVector2Coroutine(Material targetMaterial, int propertyID, Vector2 from, Vector2 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector2 currentVector2 = Vector2.Lerp(from, to, elapsed / duration);
                targetMaterial.SetVector(propertyID, currentVector2);
                yield return null;
            }
            targetMaterial.SetVector(propertyID, to);
        }
    }
}