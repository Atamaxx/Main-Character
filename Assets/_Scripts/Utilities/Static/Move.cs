using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

namespace My
{
    public class Move : MonoBehaviour
    {
        public delegate void CoroutineEndAction();
        //public static event CoroutineEndAction OnCoroutineEnd;



        public static async void ToTargetByTime(Transform target, Vector3 startPosition, float speed)
        {
            float journeyLength = Vector3.Distance(startPosition, target.position);
            float journeyTime = journeyLength / speed;
            float elapsedTime = 0f;

            while (elapsedTime < journeyTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / journeyTime);
                Vector3 newPosition = Vector3.Lerp(startPosition, target.position, t);
                target.position = newPosition;
                await Task.Yield();
            }

            // Ensure reaching the exact target position
            target.position = target.position;

        }

        public static async void ToTarget(Transform toMove, Vector3 startPosition, Vector3 targetPosition, float speed)
        {
            float journeyLength = Vector3.Distance(startPosition, targetPosition);
            float startTime = Time.time;

            while (toMove.position != targetPosition)
            {
                float distCovered = (Time.time - startTime) * speed;
                float fractionOfJourney = distCovered / journeyLength;
                toMove.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

                await Task.Yield();
            }
        }
        public static IEnumerator MoveObject(Transform ObjectToMove, Vector3 targetPosition, float moveSpeed)
        {
            while (Vector2.Distance(ObjectToMove.position, targetPosition) > 1f)
            {
                // Calculate the direction towards the target
                Vector3 direction = targetPosition - ObjectToMove.position;

                // Normalize the direction to have a magnitude of 1
                direction.Normalize();

                // Move the object towards the target
                ObjectToMove.position += moveSpeed * Time.deltaTime * direction;

                // Wait for the next frame
                yield return null;
            }
        }

        public static IEnumerator MoveObjectByTime(Transform objectToMove, Vector2 destination, float moveDuration)
        {
            float startTime = Time.time;
            Vector2 startPosition = objectToMove.position;

            while (Time.time - startTime < moveDuration)
            {
                float progress = (Time.time - startTime) / moveDuration;
                objectToMove.position = Vector2.Lerp(startPosition, destination, progress);
                yield return null; // Wait for the next frame update
            }

            // Ensure final position is reached
            objectToMove.position = destination;
        }

    }

}