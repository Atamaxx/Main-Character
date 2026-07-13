using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ConstellationSystem converts particles (snowflakes) into stars and connects them with lines
/// (like constellations) when the character stops moving. It also removes the lines when the
/// character moves again.
/// </summary>
public class ConstellationSystem : MonoBehaviour
{
    #region Inspector Settings

    [Header("References")]
    [Tooltip("The particle system that is used for the snow effect.")]
    public ParticleSystem SnowParticleSystem;

    [Tooltip("Material used to render the constellation lines.")]
    public Material LineMaterial;

    [Header("Constellation Settings")]
    [Tooltip("Maximum distance to connect stars with a line.")]
    [SerializeField] private float _maxConnectionDistance = 2f;

    [Tooltip("Number of nearest neighbors each star should connect to.")]
    [SerializeField] private int _connectionsPerStar = 1;

    [Tooltip("The grid cell size used in spatial partitioning. Defaults to the max connection distance.")]
    [SerializeField] private float _cellSize = 2f;

    [Tooltip("If true, generation is spread over several frames; otherwise, generation is done in one frame.")]
    [SerializeField] private bool _generateOverTime = true;

    // Time (in seconds) allotted for processing per frame.
    [Tooltip("Maximum time (in seconds) allotted for processing per frame.")]
    [SerializeField] private float _maxProcessingTime = 0.005f;
    [SerializeField] private float _delay = 0f;

    #endregion

    #region Private Fields

    // Holds the generated line vertices.
    private readonly List<Vector3> _lineVertices = new List<Vector3>();

    // Reference to the coroutine that generates the constellation.
    private Coroutine _generationCoroutine;

    #endregion

    #region Public Methods

    /// <summary>
    /// Called when the character stops. Stops the snow particle system,
    /// extracts the particle positions, and begins generating the constellation lines.
    /// </summary>
    public void OnCharacterStopped()
    {
        // Stop any running generation coroutine.
        if (_generationCoroutine != null)
        {
            StopCoroutine(_generationCoroutine);
            _generationCoroutine = null;
        }

        // Stop the snow particle system.
        if (SnowParticleSystem.isPlaying)
        {
            SnowParticleSystem.Stop();
        }

        // Retrieve particles from the system.
        int maxParticles = SnowParticleSystem.main.maxParticles;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[maxParticles];
        int particleCount = SnowParticleSystem.GetParticles(particles);

        if (particleCount == 0)
        {
            return;
        }

        // Convert particle positions from local to world space.
        Vector3[] starPositions = new Vector3[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            starPositions[i] = SnowParticleSystem.transform.TransformPoint(particles[i].position);
        }

        // Build a spatial grid for fast neighbor lookups.
        Dictionary<Vector2Int, List<int>> spatialGrid = new Dictionary<Vector2Int, List<int>>();
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 pos = starPositions[i];
            Vector2Int cell = new Vector2Int(
                Mathf.FloorToInt(pos.x / _cellSize),
                Mathf.FloorToInt(pos.y / _cellSize)
            );

            if (!spatialGrid.TryGetValue(cell, out List<int> cellIndices))
            {
                cellIndices = new List<int>();
                spatialGrid[cell] = cellIndices;
            }
            cellIndices.Add(i);
        }

        // Clear any previous constellation lines.
        _lineVertices.Clear();

        // Start generation over time if desired.
        _generationCoroutine = StartCoroutine(GenerateConstellationCoroutine(starPositions, spatialGrid, particleCount));
    }

    /// <summary>
    /// Called when the character moves. Stops any ongoing constellation generation
    /// and removes the generated constellation lines.
    /// </summary>
    public void OnCharacterMoved()
    {
        // Stop the generation coroutine if it's running.
        if (_generationCoroutine != null)
        {
            StopCoroutine(_generationCoroutine);
            _generationCoroutine = null;
        }

        // Remove the constellation line mesh if it exists.
        GameObject lineObject = GameObject.Find("ConstellationLineMesh");
        if (lineObject != null)
        {
            Destroy(lineObject);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Coroutine that computes connections between stars and builds the constellation lines.
    /// Instead of processing a fixed number of stars per frame, it yields based on a time
    /// budget so that the work done each frame is independent of the frame rate.
    /// </summary>
    private IEnumerator GenerateConstellationCoroutine(Vector3[] starPositions, Dictionary<Vector2Int, List<int>> spatialGrid, int particleCount)
    {
        yield return new WaitForSeconds(_delay);
        int i = 0;
        // Loop until all stars are processed.
        while (i < particleCount)
        {
            // Record the start time for this processing chunk.
            float frameStartTime = Time.realtimeSinceStartup;

            // Process stars until the time budget for this frame is exceeded.
            while (i < particleCount && Time.realtimeSinceStartup - frameStartTime < _maxProcessingTime)
            {
                Vector3 currentPos = starPositions[i];
                Vector2Int cell = new Vector2Int(
                    Mathf.FloorToInt(currentPos.x / _cellSize),
                    Mathf.FloorToInt(currentPos.y / _cellSize)
                );

                // Find all candidate neighbors in this cell and adjacent cells.
                List<(int index, float distance)> candidates = new List<(int, float)>();
                for (int x = cell.x - 1; x <= cell.x + 1; x++)
                {
                    for (int y = cell.y - 1; y <= cell.y + 1; y++)
                    {
                        Vector2Int adjacentCell = new Vector2Int(x, y);
                        if (spatialGrid.TryGetValue(adjacentCell, out List<int> candidateIndices))
                        {
                            foreach (int j in candidateIndices)
                            {
                                if (i == j)
                                    continue;

                                float dist = Vector3.Distance(currentPos, starPositions[j]);
                                if (dist <= _maxConnectionDistance)
                                {
                                    candidates.Add((j, dist));
                                }
                            }
                        }
                    }
                }

                // Sort candidates by distance (closest first).
                candidates.Sort((a, b) => a.distance.CompareTo(b.distance));

                // Connect to up to _connectionsPerStar nearest candidates.
                int connectionsAdded = 0;
                foreach (var candidate in candidates)
                {
                    if (connectionsAdded >= _connectionsPerStar)
                        break;

                    _lineVertices.Add(currentPos);
                    _lineVertices.Add(starPositions[candidate.index]);
                    connectionsAdded++;
                }

                i++; // Move on to the next star.
            }

            // Update the mesh with the vertices processed so far.
            CreateLineMesh(_lineVertices);

            // Yield control so the next processing chunk occurs on the next frame.
            yield return null;
        }

        _generationCoroutine = null;
    }

    /// <summary>
    /// Creates or updates the mesh used to render the constellation lines.
    /// </summary>
    private void CreateLineMesh(List<Vector3> lineVertices)
    {
        GameObject lineObject = GameObject.Find("ConstellationLineMesh");
        Mesh mesh;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        if (lineObject == null)
        {
            lineObject = new GameObject("ConstellationLineMesh");
            lineObject.transform.parent = this.transform;
            meshFilter = lineObject.AddComponent<MeshFilter>();
            meshRenderer = lineObject.AddComponent<MeshRenderer>();
            meshRenderer.material = LineMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.sortingLayerName = "Hideout";
            meshRenderer.sortingOrder = -10;
            mesh = new Mesh { name = "ConstellationLines" };
            meshFilter.mesh = mesh;
        }
        else
        {
            meshFilter = lineObject.GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;
        }

        // Each pair of vertices represents one line segment.
        int[] indices = new int[lineVertices.Count];
        for (int j = 0; j < indices.Length; j++)
        {
            indices[j] = j;
        }

        mesh.Clear();
        mesh.SetVertices(lineVertices);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
    }

    #endregion
}
