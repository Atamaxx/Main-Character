using System;
using UnityEngine;

namespace MainController
{
    public struct FrameInput
    {
        public float X;
        public bool JumpRight;
        public bool JumpUpPressed;
        public bool JumpUpReleased;
        public bool JumpUpPressing;
        public bool JumpLeft;
        public bool Sprint;
        public bool Deceleration;
    }

    public interface IPlayerController
    {
        public Vector3 Velocity { get; }
        public FrameInput Input { get; }
        public bool LandingThisFrame { get; }
        public Vector2 RawMovement { get; }
        public bool Grounded { get; }

        public event Action<bool> GroundedChanged;
        public event Action Jumped;
        public event Action Stepped;

        public event Action Sprinting;
        public event Action Decelerating;

    }
}