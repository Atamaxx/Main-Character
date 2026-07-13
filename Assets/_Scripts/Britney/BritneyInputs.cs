using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Britney
{
    public class BritneyInputs : MonoBehaviour
    {
        // Move
        public Vector2 MoveInput { get; private set; }

        // Jump
        /// <summary>
        /// True the exact frame the jump button transitions from
        /// not pressed to pressed.
        /// </summary>
        public bool JumpUpPressedThisFrame { get; private set; }

        /// <summary>
        /// True while the jump button is held down.
        /// </summary>
        public bool JumpUpPressing { get; private set; }

        /// <summary>
        /// True the frame the jump is released.
        /// </summary>
        public bool JumpUpReleased { get; private set; }

        // Other actions
        public bool SpeedUpPressing { get; private set; }
        public bool SpeedDownPressing { get; private set; }


        // --- Movement callback ---
        public void OnMove(InputValue inputValue)
        {
            Vector2 raw = inputValue.Get<Vector2>();
            MoveInput = new Vector2(Mathf.RoundToInt(raw.x), Mathf.RoundToInt(raw.y));
        }

        // --- Jump callback ---
        public void OnJumpUp(InputValue inputValue)
        {
            // If it's pressed this frame AND previously we weren't pressing, we know it got pressed down this frame
            if (inputValue.isPressed && !JumpUpPressing)
            {
                JumpUpPressedThisFrame = true;
            }

            // Update "pressing" and "released" states
            JumpUpPressing = inputValue.isPressed;
            JumpUpReleased = !inputValue.isPressed;
        }

        // --- Speed Up/Down callbacks ---
        public void OnSpeedUp(InputValue inputValue)
        {
            SpeedUpPressing = inputValue.isPressed;
        }

        public void OnSpeedDown(InputValue inputValue)
        {
            SpeedDownPressing = inputValue.isPressed;
        }

        // --- Reset "pressed this frame" at the end of each frame ---
        private void LateUpdate()
        {
            // We set this false so that it’s only “true” on the single frame the button transitions from up to down
            JumpUpPressedThisFrame = false;
        }
    }
}
