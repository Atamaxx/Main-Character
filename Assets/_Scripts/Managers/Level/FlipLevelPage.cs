// Path: _Scripts/Managers/Level/FlipLevelPage.cs
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class FlipLevelPage : StaticInstance<FlipLevelPage> // Assuming StaticInstance is appropriate
{
    [SerializeField, Required]
    private Camera _mainCamera;

    [SerializeField, Required]
    private Material _pageFlipMaterial;

    [Header("Textures - Assign RenderTextures or Default Textures")]
    [Tooltip(
        "RenderTexture that holds the captured previous level screen. Should be the same as RenderTextureLevelCapture's output."
    )]
    [SerializeField, Required]
    private RenderTexture _frontPageTexture; // This should be the target of RenderTextureLevelCapture

    [SerializeField, Required]
    private Texture2D _defaultFrontPageTexture; // Fallback if frontPageTexture is black/unusable

    [Tooltip(
        "RenderTexture for the back of the page (e.g., next level preview or a generic texture)."
    )]
    [SerializeField]
    private RenderTexture _backPageTexture;

    [SerializeField, Required]
    private Texture2D _defaultBackPageTexture; // Fallback or default back page

    [Header("Animation Settings")]
    [SerializeField]
    private float _flipDuration = 1.0f;

    [Header("Visual Setup")]
    [SerializeField, Required]
    private Sprite _squareSprite; // A simple square sprite for the full-screen quad

    private GameObject _pageFlipGO;
    private Coroutine _flipCoroutine;

    public float GetFlipDuration() => _flipDuration;

    protected override void Awake()
    {
        base.Awake();

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
                Debug.LogError("Main Camera not found for FlipLevelPage.", this);
        }
        if (_pageFlipMaterial == null)
            Debug.LogError("PageFlipMaterial is not assigned.", this);
        if (_frontPageTexture == null)
            Debug.LogError("FrontPageTexture (RenderTexture) is not assigned.", this);
        if (_defaultFrontPageTexture == null)
            Debug.LogError("DefaultFrontPageTexture is not assigned.", this);
        if (_defaultBackPageTexture == null)
            Debug.LogError("DefaultBackPageTexture is not assigned.", this);
        if (_squareSprite == null)
            Debug.LogError("SquareSprite is not assigned.", this);
    }

    public void SetUpPageFlip()
    {
        if (_mainCamera == null || _pageFlipMaterial == null || _squareSprite == null)
        {
            Debug.LogError(
                "FlipLevelPage is not properly configured. Aborting SetUpPageFlip.",
                this
            );
            return;
        }
        SetTextures();
        CreateFullScreenPageFlipImage2D();
    }

    public void StartPageFlip()
    {
        if (_pageFlipGO == null)
        {
            Debug.LogWarning("Page flip GameObject is null. Attempting to set it up now.", this);
            SetUpPageFlip(); // Attempt to set up if not already
            if (_pageFlipGO == null) // If still null, abort
            {
                Debug.LogError(
                    "Failed to set up PageFlip GameObject. Aborting StartPageFlip.",
                    this
                );
                return;
            }
        }

        if (AudioSystem.Instance != null && FMODEvents.Instance != null)
        {
            AudioSystem.Instance.PlaySFXOneShot(FMODEvents.Instance.PageFlip);
        }
        AnimateTransition();
    }

    public void SetTextures()
    {
        if (_pageFlipMaterial == null)
            return;

        // Use the captured front page texture if it's valid, otherwise use default
        if (_frontPageTexture != null && !IsRenderTextureEffectivelyBlack(_frontPageTexture))
        {
            _pageFlipMaterial.SetTexture("_PageFront_Tex", _frontPageTexture);
            //Debug.Log("Using captured FrontPageTexture for page flip.");
        }
        else
        {
            _pageFlipMaterial.SetTexture("_PageFront_Tex", _defaultFrontPageTexture);
            //Debug.Log("FrontPageTexture is black or null, using DefaultFrontPageTexture.");
        }

        // Use the back page texture if valid, otherwise use default
        if (_backPageTexture != null && !IsRenderTextureEffectivelyBlack(_backPageTexture))
        {
            _pageFlipMaterial.SetTexture("_PageBack_Tex", _backPageTexture);
            //Debug.Log("Using captured BackPageTexture for page flip.");
        }
        else
        {
            _pageFlipMaterial.SetTexture("_PageBack_Tex", _defaultBackPageTexture);
            //Debug.Log("BackPageTexture is black or null, using DefaultBackPageTexture.");
        }

        // Reset progress to ensure we see the front texture initially (progress from 1 to 0)
        _pageFlipMaterial.SetFloat("_Progress", 1f);
    }

    public void AnimateTransition()
    {
        if (_pageFlipMaterial == null)
        {
            Debug.LogError("PageFlipMaterial is null. Cannot animate transition.", this);
            CleanupPageFlip(); // Cleanup if material is missing
            return;
        }

        if (_flipCoroutine != null)
        {
            StopCoroutine(_flipCoroutine);
        }
        _flipCoroutine = StartCoroutine(AnimatePageFlipCoroutine());
    }

    private IEnumerator AnimatePageFlipCoroutine()
    {
        if (_pageFlipGO == null)
        {
            Debug.LogWarning(
                "PageFlipGO is null at start of AnimatePageFlipCoroutine. Attempting setup."
            );
            SetUpPageFlip();
            if (_pageFlipGO == null)
            {
                Debug.LogError("Failed to setup PageFlipGO in AnimatePageFlipCoroutine. Aborting.");
                yield break;
            }
        }
        _pageFlipGO.SetActive(true); // Ensure it's active

        float elapsedTime = 0f;
        _pageFlipMaterial.SetFloat("_Progress", 1f); // Start with front page fully visible

        while (elapsedTime < _flipDuration)
        {
            // Progress from 1 (front) to 0 (back)
            float progress = Mathf.Lerp(1f, 0f, elapsedTime / _flipDuration);
            _pageFlipMaterial.SetFloat("_Progress", progress);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        _pageFlipMaterial.SetFloat("_Progress", 0f); // Ensure it ends on the back page

        CleanupPageFlip();
    }

    private void CleanupPageFlip()
    {
        if (_pageFlipGO != null)
        {
            Destroy(_pageFlipGO);
            _pageFlipGO = null;
        }
        _flipCoroutine = null;
        // Potentially trigger GameManager to change state to Gameplay here if this script is responsible
        // Example: if (GameManager.Instance != null) GameManager.Instance.ChangeState(GameState.Gameplay);
    }

    public void CreateFullScreenPageFlipImage2D()
    {
        if (_mainCamera == null || _squareSprite == null || _pageFlipMaterial == null)
        {
            Debug.LogError(
                "Cannot create PageFlipImage: Missing MainCamera, SquareSprite, or PageFlipMaterial.",
                this
            );
            return;
        }

        if (_pageFlipGO != null)
            Destroy(_pageFlipGO);

        _pageFlipGO = new GameObject("PageFlipSpriteQuad");
        SpriteRenderer spriteRenderer = _pageFlipGO.AddComponent<SpriteRenderer>();
        spriteRenderer.material = _pageFlipMaterial;
        spriteRenderer.sprite = _squareSprite;

        // Ensure it's on a layer that's rendered by the main camera and sorts in front
        spriteRenderer.sortingLayerName = "Front"; // Or any appropriate high-sorting layer
        spriteRenderer.sortingOrder = 1000; // A high value to ensure it's on top

        // Position and scale to fit screen (relative to camera)
        _pageFlipGO.transform.SetParent(_mainCamera.transform, false); // Set parent, worldPositionStays = false
        _pageFlipGO.transform.localPosition = new Vector3(0, 0, _mainCamera.nearClipPlane + 0.1f); // Slightly in front of near clip plane
        _pageFlipGO.transform.localRotation = Quaternion.identity;

        float screenHeight = _mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * _mainCamera.aspect;

        // Assuming _squareSprite is a 1x1 unit sprite. If not, adjust spriteSize.x/y accordingly.
        Vector2 spriteSize = _squareSprite.bounds.size;
        if (Mathf.Approximately(spriteSize.x, 0) || Mathf.Approximately(spriteSize.y, 0))
        {
            Debug.LogWarning(
                "SquareSprite bounds size is zero. Defaulting to 1x1 for scaling.",
                this
            );
            spriteSize = Vector2.one;
        }

        float scaleX = screenWidth / spriteSize.x;
        float scaleY = screenHeight / spriteSize.y;
        _pageFlipGO.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        _pageFlipGO.SetActive(false); // Initially inactive, activated by StartPageFlip
    }

    public bool IsRenderTextureEffectivelyBlack(
        RenderTexture renderTexture,
        float threshold = 0.01f,
        int samplePoints = 5
    )
    {
        if (renderTexture == null)
            return true; // Treat null as black

        // Create a temporary texture to read pixels
        Texture2D tempTex = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            TextureFormat.RGB24,
            false
        );
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        tempTex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tempTex.Apply();

        RenderTexture.active = currentActiveRT;

        int nonBlackPixelCount = 0;
        for (int i = 0; i < samplePoints; i++)
        {
            for (int j = 0; j < samplePoints; j++)
            {
                int x = (i * renderTexture.width) / (samplePoints - 1);
                int y = (j * renderTexture.height) / (samplePoints - 1);
                x = Mathf.Clamp(x, 0, renderTexture.width - 1);
                y = Mathf.Clamp(y, 0, renderTexture.height - 1);

                Color pixel = tempTex.GetPixel(x, y);
                // Check if any color component is above the threshold
                if (pixel.r > threshold || pixel.g > threshold || pixel.b > threshold)
                {
                    nonBlackPixelCount++;
                }
            }
        }

        DestroyImmediate(tempTex); // Use DestroyImmediate if called from editor context, Destroy otherwise

        // If a small percentage of sampled pixels are non-black, consider it effectively black.
        // Example: if less than 10% of samples are non-black.
        return nonBlackPixelCount < (samplePoints * samplePoints * 0.1f);
    }
}
