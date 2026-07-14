using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameAspectRatioFitter : MonoBehaviour
{
    public static GameAspectRatioFitter Instance { get; private set; }

    [SerializeField]
    private Camera targetCamera;

    [SerializeField]
    private RectTransform[] uiRoots;

    [SerializeField]
    private float targetWidth = 13f;

    [SerializeField]
    private float targetHeight = 28f;

    [SerializeField]
    private bool applyToCamera = true;

    [SerializeField]
    private bool applyToUiRoots = true;

    [SerializeField]
    private bool renderBlackBars = true;

    [SerializeField]
    private Color blackBarColor = Color.black;

    [SerializeField]
    private bool dontDestroyOnLoad = true;

    private Canvas blackBarCanvas;
    private RectTransform leftBlackBar;
    private RectTransform rightBlackBar;
    private RectTransform topBlackBar;
    private RectTransform bottomBlackBar;
    private int lastScreenWidth;
    private int lastScreenHeight;
    private Rect lastViewportRect = new(0f, 0f, 1f, 1f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        ApplyAspectRatio(true);
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        ApplyAspectRatio();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (blackBarCanvas != null)
        {
            Destroy(blackBarCanvas.gameObject);
        }
    }

    private void OnValidate()
    {
        targetWidth = Mathf.Max(1f, targetWidth);
        targetHeight = Mathf.Max(1f, targetHeight);
    }

    private void ApplyAspectRatio(bool force = false)
    {
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        if (!force && lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
        {
            return;
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastViewportRect = CalculateViewportRect();

        if (applyToCamera)
        {
            ApplyCameraRect();
        }

        if (applyToUiRoots)
        {
            ApplyUiRootRects();
        }

        if (renderBlackBars)
        {
            ApplyBlackBars();
        }
        else if (blackBarCanvas != null)
        {
            blackBarCanvas.gameObject.SetActive(false);
        }
    }

    private Rect CalculateViewportRect()
    {
        float targetAspect = targetWidth / targetHeight;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect > targetAspect)
        {
            float width = targetAspect / screenAspect;
            return new Rect((1f - width) * 0.5f, 0f, width, 1f);
        }

        float height = screenAspect / targetAspect;
        return new Rect(0f, (1f - height) * 0.5f, 1f, height);
    }

    private void ApplyCameraRect()
    {
        if (targetCamera == null)
        {
            return;
        }

        targetCamera.rect = lastViewportRect;
    }

    private void ApplyUiRootRects()
    {
        if (uiRoots == null)
        {
            return;
        }

        foreach (RectTransform uiRoot in uiRoots)
        {
            if (uiRoot == null)
            {
                continue;
            }

            uiRoot.anchorMin = new Vector2(lastViewportRect.xMin, lastViewportRect.yMin);
            uiRoot.anchorMax = new Vector2(lastViewportRect.xMax, lastViewportRect.yMax);
            uiRoot.offsetMin = Vector2.zero;
            uiRoot.offsetMax = Vector2.zero;
        }
    }

    private void ApplyBlackBars()
    {
        EnsureBlackBarCanvas();
        blackBarCanvas.gameObject.SetActive(true);

        SetBar(leftBlackBar, 0f, 0f, lastViewportRect.xMin, 1f);
        SetBar(rightBlackBar, lastViewportRect.xMax, 0f, 1f, 1f);
        SetBar(
            bottomBlackBar,
            lastViewportRect.xMin,
            0f,
            lastViewportRect.xMax,
            lastViewportRect.yMin
        );
        SetBar(
            topBlackBar,
            lastViewportRect.xMin,
            lastViewportRect.yMax,
            lastViewportRect.xMax,
            1f
        );
    }

    private void EnsureBlackBarCanvas()
    {
        if (blackBarCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new($"{nameof(GameAspectRatioFitter)} Black Bars");
        canvasObject.transform.SetParent(transform, false);

        blackBarCanvas = canvasObject.AddComponent<Canvas>();
        blackBarCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        blackBarCanvas.sortingOrder = short.MaxValue;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        canvasObject.AddComponent<GraphicRaycaster>();

        leftBlackBar = CreateBlackBar("Left");
        rightBlackBar = CreateBlackBar("Right");
        topBlackBar = CreateBlackBar("Top");
        bottomBlackBar = CreateBlackBar("Bottom");
    }

    private RectTransform CreateBlackBar(string barName)
    {
        GameObject barObject = new($"{barName} Black Bar");
        barObject.transform.SetParent(blackBarCanvas.transform, false);

        Image image = barObject.AddComponent<Image>();
        image.color = blackBarColor;
        image.raycastTarget = true;

        return image.rectTransform;
    }

    private static void SetBar(
        RectTransform bar,
        float anchorMinX,
        float anchorMinY,
        float anchorMaxX,
        float anchorMaxY
    )
    {
        bar.anchorMin = new Vector2(anchorMinX, anchorMinY);
        bar.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        bar.offsetMin = Vector2.zero;
        bar.offsetMax = Vector2.zero;
    }
}
