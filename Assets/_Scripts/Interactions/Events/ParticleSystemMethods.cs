using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    // Reference to the Particle System and its Renderer.
    [SerializeField] private ParticleSystem _ps;
    [SerializeField] private ParticleSystemRenderer _psRenderer;

    // Serialized fields for properties that are less event-friendly.
    [Header("Serialized Settings")]
    [Tooltip("Set via ApplySerializedShapeType()")]
    [SerializeField] private ParticleSystemShapeType serializedShapeType = ParticleSystemShapeType.Cone;
    [Tooltip("Set via ApplySerializedShapeRadius()")]
    [SerializeField] private float serializedShapeRadius = 1f;
    [Tooltip("Set via ApplySerializedEmissionRate()")]
    [SerializeField] private float serializedEmissionRate = 10f;

 

    #region Main Module Methods
    /// <summary>
    /// Sets the particle system's start lifetime.
    /// </summary>
    /// <param name="lifetime">Lifetime in seconds.</param>
    public void SetStartLifetime(float lifetime)
    {
        var main = _ps.main;
        main.startLifetime = lifetime;
    }

    /// <summary>
    /// Sets the particle system's start speed.
    /// </summary>
    /// <param name="speed">Speed value.</param>
    public void SetStartSpeed(float speed)
    {
        var main = _ps.main;
        main.startSpeed = speed;
    }

    /// <summary>
    /// Sets the particle system's start size.
    /// </summary>
    /// <param name="size">Size value.</param>
    public void SetStartSize(float size)
    {
        var main = _ps.main;
        main.startSize = size;
    }

    /// <summary>
    /// Sets the particle system's start color.
    /// </summary>
    /// <param name="color">Color value.</param>
    public void SetStartColor(Color color)
    {
        var main = _ps.main;
        main.startColor = color;
    }

    /// <summary>
    /// Sets the particle system's start delay.
    /// </summary>
    /// <param name="delay">Delay in seconds.</param>
    public void SetStartDelay(float delay)
    {
        var main = _ps.main;
        main.startDelay = delay;
    }

    /// <summary>
    /// Sets the particle system's duration.
    /// </summary>
    /// <param name="duration">Duration in seconds.</param>
    public void SetDuration(float duration)
    {
        var main = _ps.main;
        main.duration = duration;
    }

    /// <summary>
    /// Sets the particle system's simulation speed.
    /// </summary>
    /// <param name="speed">Simulation speed multiplier.</param>
    public void SetSimulationSpeed(float speed)
    {
        var main = _ps.main;
        main.simulationSpeed = speed;
    }

    /// <summary>
    /// Sets the gravity modifier.
    /// </summary>
    /// <param name="gravityModifier">Gravity modifier value.</param>
    public void SetGravityModifier(float gravityModifier)
    {
        var main = _ps.main;
        main.gravityModifier = gravityModifier;
    }

    /// <summary>
    /// Enables or disables looping.
    /// </summary>
    /// <param name="loop">True to loop, false otherwise.</param>
    public void SetLoop(bool loop)
    {
        var main = _ps.main;
        main.loop = loop;
    }

    /// <summary>
    /// Sets the maximum number of particles.
    /// </summary>
    /// <param name="maxParticles">Maximum particles.</param>
    public void SetMaxParticles(int maxParticles)
    {
        var main = _ps.main;
        main.maxParticles = maxParticles;
    }
    #endregion

    #region Emission Module Methods
    /// <summary>
    /// Sets the emission rate over time.
    /// </summary>
    /// <param name="rate">Emission rate (particles per second).</param>
    public void SetEmissionRate(float rate)
    {
        var emission = _ps.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(rate);
    }
    #endregion

    #region Shape Module Methods
    /// <summary>
    /// Sets the shape module's type.
    /// </summary>
    /// <param name="shapeType">A ParticleSystemShapeType value.</param>
    public void SetShapeType(ParticleSystemShapeType shapeType)
    {
        var shape = _ps.shape;
        shape.shapeType = shapeType;
    }

    /// <summary>
    /// Sets the shape module's radius.
    /// </summary>
    /// <param name="radius">Radius value.</param>
    public void SetShapeRadius(float radius)
    {
        var shape = _ps.shape;
        shape.radius = radius;
    }

    /// <summary>
    /// Sets the shape module's angle.
    /// </summary>
    /// <param name="angle">Angle value in degrees.</param>
    public void SetShapeAngle(float angle)
    {
        var shape = _ps.shape;
        shape.angle = angle;
    }

    /// <summary>
    /// Sets the shape module's arc (for shapes like cones).
    /// </summary>
    /// <param name="arc">Arc in degrees.</param>
    public void SetShapeArc(float arc)
    {
        var shape = _ps.shape;
        shape.arc = arc;
    }
    #endregion

    #region Renderer Module Methods
    /// <summary>
    /// Sets the sorting layer name of the particle system's renderer.
    /// </summary>
    /// <param name="sortingLayerName">The sorting layer name.</param>
    public void SetSortingLayer(string sortingLayerName)
    {
        if (_psRenderer != null)
        {
            _psRenderer.sortingLayerName = sortingLayerName;
        }
    }

    /// <summary>
    /// Sets the sorting order of the particle system's renderer.
    /// </summary>
    /// <param name="order">Sorting order (integer).</param>
    public void SetSortingOrder(int order)
    {
        if (_psRenderer != null)
        {
            _psRenderer.sortingOrder = order;
        }
    }
    #endregion

    #region Serialized Value Application Methods
    /// <summary>
    /// Applies the serialized shape type to the particle system.
    /// </summary>
    public void ApplySerializedShapeType()
    {
        var shape = _ps.shape;
        shape.shapeType = serializedShapeType;
    }

    /// <summary>
    /// Applies the serialized shape radius to the particle system.
    /// </summary>
    public void ApplySerializedShapeRadius()
    {
        var shape = _ps.shape;
        shape.radius = serializedShapeRadius;
    }

    /// <summary>
    /// Applies the serialized emission rate to the particle system.
    /// </summary>
    public void ApplySerializedEmissionRate()
    {
        var emission = _ps.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(serializedEmissionRate);
    }
    #endregion
}
