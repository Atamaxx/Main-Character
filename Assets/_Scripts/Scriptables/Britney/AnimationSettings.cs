using UnityEngine;

namespace Britney
{
    [CreateAssetMenu(fileName = "AnimationSettings", menuName = "Britney/AnimationSettings")]
    public class AnimationSettings : ScriptableObject
    {
        [Header("General Settings")]
        public float SpeedBaseMultiplier = 1f;

        // public float SpeedUpMultiplier = 1.5f;
        // public float SpeedDownMultiplier = 0.5f;
        [Header("Leg Settings")]
        public float HalfStepLength = 1f;
        public float MaxLegsHeight = 2f;
        public float MaxStepHeight = 0.5f;
        public AnimationCurve StepCurve;

        [Header("Arms Settings")]
        public int NumberOfArmPoints = 100;

        [Header("Tilt Settings")]
        public float TiltAmount = 4f;

        [Header("Air Animation Settings")]
        [Tooltip("Vertical offset for leg target in air")]
        public float AirLegYOffset = 1.0f;

        [Tooltip("Lateral offset for leg target in air")]
        public float AirLegLateralOffset = 0.5f;

        [Tooltip("Speed at which leg targets move to their air positions")]
        public float AirLegLerpSpeed = 5f;

        [Header("Landing Transition Settings")]
        [Tooltip("Duration over which to blend from air to running leg positions")]
        public float LandingTransitionDuration = 0.3f;

        [Tooltip("Lateral offset for grounded leg positions")]
        public float GroundedLateralOffset = 0.5f;
    }
}
