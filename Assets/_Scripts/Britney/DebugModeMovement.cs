using UnityEngine;

namespace Britney
{
    /// <summary>
    /// This script toggles a debug mode that lets you move the character using WASD.
    /// When debug mode is enabled, it optionally disables a target movement script to avoid conflicts.
    /// </summary>
    public class DebugModeMovement : MonoBehaviour
    {
        [Header("Debug Mode Settings")]
        [Tooltip("Key used to toggle debug mode on/off.")]
        public KeyCode toggleDebugKey = KeyCode.F1;

        [Tooltip("Movement speed when in debug mode.")]
        public float debugSpeed = 20f;

        [Tooltip("Is debug mode currently enabled?")]
        public bool debugMode = false;

        [Header("Optional Movement Script")]
        [Tooltip("If assigned, this movement script will be disabled when debug mode is active.")]
        public MonoBehaviour movementScriptToDisable;

        void Update()
        {
            // Toggle debug mode when the toggle key is pressed.
            if (Input.GetKeyDown(toggleDebugKey))
            {
                debugMode = !debugMode;
                Debug.Log("Debug Mode: " + (debugMode ? "Enabled" : "Disabled"));

                // Optionally disable/enable the normal movement script.
                if (movementScriptToDisable != null)
                {
                    movementScriptToDisable.enabled = !debugMode;
                }
            }

            // If debug mode is active, read explicit key input for movement.
            if (debugMode)
            {
                Vector3 move = Vector3.zero;

                // Use explicit key checks to ensure correct mapping:
                if (Input.GetKey(KeyCode.W))
                    move += Vector3.up; // W moves up.
                if (Input.GetKey(KeyCode.S))
                    move += Vector3.down; // S moves down.
                if (Input.GetKey(KeyCode.A))
                    move += Vector3.left; // A moves left.
                if (Input.GetKey(KeyCode.D))
                    move += Vector3.right; // D moves right.

                // Normalize to prevent faster diagonal movement.
                if (move.magnitude > 1f)
                {
                    move.Normalize();
                }

                // Move the transform directly.
                transform.position += move * debugSpeed * Time.deltaTime;
            }
        }
    }
}
