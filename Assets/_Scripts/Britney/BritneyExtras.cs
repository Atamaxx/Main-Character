using System;
using UnityEngine;

namespace Britney
{
    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
        public bool SpeedUp;
        public bool SpeedDown;
    }

    public interface IBritneyController
    {
        public FrameInput F_Input { get; set; }
        public bool IsFrozen { get; set; }
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public event Action Frozen;
        public event Action Unfrozen;
    }
}
//public struct FrameInput
// {
//     public float X;
//     public float Y;
//     public bool JumpRight;
//     public bool JumpUpPressed;
//     public bool JumpUpReleased;
//     public bool JumpUpPressing;
//     public bool JumpLeft;
//     public bool Sprint;
//     public bool Deceleration;
// }

// public interface IBritneyController
// {
//     public Vector3 Velocity { get; }
//     public FrameInput Input { get; }
//     public bool LandingThisFrame { get; }
//     public Vector2 RawMovement { get; }
//     public bool Grounded { get; }

//     public event Action<bool> GroundedChanged;
//     public event Action Jumped;
//     public event Action Stepped;

// }
