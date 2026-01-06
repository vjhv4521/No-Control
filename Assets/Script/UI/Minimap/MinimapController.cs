using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("相机与贴图")]
    public Camera minimapCamera;
    public RawImage minimapImage;
    public RectTransform minimapRect;
    public LayerMask minimapCullingMask;

    [Header("分辨率设置")] // 新增：分辨率配置项
    public int minimapResolutionWidth = 256; // 小地图宽度（像素）
    public int minimapResolutionHeight = 256; // 小地图高度（像素）
    public bool useScreenPercentage = false; // 是否使用屏幕百分比模式
    [Range(0.05f, 0.5f)] public float resolutionScreenPercent = 0.2f; // 屏幕百分比

    [Header("样式与标记")]
    public Image backgroundImage;
    public Image playerMarker;
    public Image fovOverlay;
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.8f);
    public Color borderColor = Color.white;
    public int borderWidth = 2;
    public float screenWidthPercent = 0.2f;
    public float screenHeightPercent = 0.25f;
    public float safeMarginPixels = 10f;

    [Header("缩放")]
    public float defaultOrthographicSize = 12f;
    public float zoomDuration = 0.35f;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("性能")]
    public float updateInterval = 0.01f;
    public int initialMarkerPoolSize = 16;
    public float lodZoomThreshold = 6f;
    public float lodHideDistance = 30f;

    RenderTexture rt;
    Camera mainCamera;
    readonly List<Image> markerPool = new List<Image>();
    readonly HashSet<Image> activeMarkers = new HashSet<Image>();
    Coroutine updateRoutine;
    float targetOrtho;

    void Awake()
    {
        mainCamera = Camera.main;
        EnsureUIExists();
        if (minimapRect == null && minimapImage != null) minimapRect = minimapImage.rectTransform;
        ApplyLayout();
        SetupRenderTexture(); // 这里会使用新的分辨率配置
        SetupCamera();
        SetupBackground();
        SetupMarkerPool();
        targetOrtho = defaultOrthographicSize;
    }

    void OnEnable()
    {
        if (updateRoutine == null) updateRoutine = StartCoroutine(UpdateLoop());
    }

    void OnDisable()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }

    void ApplyLayout()
    {
        if (minimapRect == null) return;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            minimapRect.anchorMin = new Vector2(0, 1);
            minimapRect.anchorMax = new Vector2(0, 1);
            minimapRect.pivot = new Vector2(0, 1);
            var w = Mathf.Round(Screen.width * screenWidthPercent);
            var h = Mathf.Round(Screen.height * screenHeightPercent);
            minimapRect.sizeDelta = new Vector2(w, h);
            minimapRect.anchoredPosition = new Vector2(safeMarginPixels, -safeMarginPixels);
        }
    }

    // 核心修改：重构SetupRenderTexture方法，支持自定义分辨率
    void SetupRenderTexture()
    {
        if (rt != null) return;
        
        // 根据配置选择分辨率计算方式
        int w, h;
        if (useScreenPercentage)
        {
            // 按屏幕百分比计算
            w = Mathf.Max(64, Mathf.RoundToInt(Screen.width * resolutionScreenPercent));
            h = Mathf.Max(64, Mathf.RoundToInt(Screen.height * resolutionScreenPercent));
        }
        else
        {
            // 使用固定像素尺寸
            w = Mathf.Max(64, minimapResolutionWidth); // 最小64像素避免出错
            h = Mathf.Max(64, minimapResolutionHeight);
        }

        // 创建RenderTexture时使用计算好的分辨率
        rt = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);
        rt.name = "MinimapRT";
        rt.Create();
        if (minimapImage != null) minimapImage.texture = rt;
    }

    void SetupCamera()
    {
        if (minimapCamera == null)
        {
            var go = new GameObject("MinimapCamera");
            go.transform.SetParent(transform, false);
            minimapCamera = go.AddComponent<Camera>();
        }
        minimapCamera.orthographic = true;
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = Color.black;
        if (minimapCullingMask.value == 0)
        {
            int ui = LayerMask.NameToLayer("UI");
            int mask = ~0;
            if (ui >= 0) mask &= ~(1 << ui);
            minimapCamera.cullingMask = mask;
        }
        else
        {
            minimapCamera.cullingMask = minimapCullingMask.value;
        }
        minimapCamera.depth = 100;
        minimapCamera.targetTexture = rt;
        minimapCamera.orthographicSize = defaultOrthographicSize;
    }

    void SetupBackground()
    {
        if (backgroundImage != null) backgroundImage.color = backgroundColor;
        var outline = backgroundImage != null ? backgroundImage.GetComponent<Outline>() : null;
        if (outline != null) outline.enabled = false;
    }

    void SetupMarkerPool()
    {
        if (playerMarker != null) playerMarker.gameObject.SetActive(true);
        for (int i = markerPool.Count; i < initialMarkerPoolSize; i++)
        {
            var go = new GameObject("MinimapMarker");
            go.transform.SetParent(minimapRect, false);
            var img = go.AddComponent<Image>();
            img.color = Color.yellow;
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.sizeDelta = new Vector2(6, 6);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0.5f, 0.5f);
            img.gameObject.SetActive(false);
            markerPool.Add(img);
        }
        if (fovOverlay != null)
        {
            fovOverlay.color = new Color(1f, 1f, 1f, 0.15f);
            fovOverlay.raycastTarget = false;
        }
    }

    IEnumerator UpdateLoop()
    {
        while (true)
        {
            UpdateMinimap();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    void UpdateMinimap()
    {
        var player = Player.Instance;
        if (player != null)
        {
            var p = player.transform.position;
            minimapCamera.transform.position = new Vector3(p.x, p.y, -10f);
            UpdatePlayerMarker(p);
            UpdateFov(p);
        }
        ApplyLOD();
    }

    void UpdatePlayerMarker(Vector3 worldPos)
    {
        if (playerMarker == null || minimapRect == null) return;
        var vp = minimapCamera.WorldToViewportPoint(worldPos);
        var size = minimapRect.rect.size;
        var anchored = new Vector2(vp.x * size.x, vp.y * size.y);
        playerMarker.rectTransform.anchoredPosition = anchored;
        playerMarker.color = Color.red;
        playerMarker.rectTransform.sizeDelta = new Vector2(6, 6);
    }

    void UpdateFov(Vector3 worldPos)
    {
        if (fovOverlay == null || minimapRect == null) return;
        float radiusWorld = Mathf.Max(4f, defaultOrthographicSize * 0.6f);
        var centerVp = minimapCamera.WorldToViewportPoint(worldPos);
        var size = minimapRect.rect.size;
        var centerPx = new Vector2(centerVp.x * size.x, centerVp.y * size.y);
        float pixelsPerUnit = size.y / (minimapCamera.orthographicSize * 2f);
        float radiusPx = radiusWorld * pixelsPerUnit;
        fovOverlay.rectTransform.anchoredPosition = centerPx;
        fovOverlay.rectTransform.sizeDelta = new Vector2(radiusPx * 2f, radiusPx * 2f);
    }

    void ApplyLOD()
    {
        bool lowDetail = minimapCamera.orthographicSize >= lodZoomThreshold;
        foreach (var m in activeMarkers)
        {
            var wp = m.rectTransform.position;
            var player = Player.Instance;
            bool far = false;
            if (player != null)
            {
                far = Vector2.Distance(player.transform.position, wp) > lodHideDistance;
            }
            m.enabled = !(lowDetail && far);
        }
        if (playerMarker != null) playerMarker.enabled = true;
    }

    public void ZoomTo(float size)
    {
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        targetOrtho = Mathf.Max(1f, size);
        zoomRoutine = StartCoroutine(ZoomRoutine(minimapCamera.orthographicSize, targetOrtho, zoomDuration));
    }

    Coroutine zoomRoutine;
    IEnumerator ZoomRoutine(float from, float to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Mathf.Clamp01(Time.unscaledDeltaTime / duration);
            float k = zoomCurve.Evaluate(t);
            minimapCamera.orthographicSize = Mathf.Lerp(from, to, k);
            yield return null;
        }
        minimapCamera.orthographicSize = to;
    }

    void EnsureUIExists()
    {
        if (minimapImage != null && minimapRect != null && backgroundImage != null && playerMarker != null && fovOverlay != null) return;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("MinimapCanvas");
            canvasGO.transform.SetParent(transform, false);
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        GameObject containerGO = new GameObject("MinimapContainer");
        containerGO.transform.SetParent(canvas.transform, false);
        minimapRect = containerGO.AddComponent<RectTransform>();
        minimapRect.anchorMin = new Vector2(0, 1);
        minimapRect.anchorMax = new Vector2(0, 1);
        minimapRect.pivot = new Vector2(0, 1);
        backgroundImage = containerGO.AddComponent<Image>();
        backgroundImage.raycastTarget = false;
        GameObject rawGO = new GameObject("MinimapImage");
        rawGO.transform.SetParent(containerGO.transform, false);
        minimapImage = rawGO.AddComponent<RawImage>();
        var rawRect = minimapImage.rectTransform;
        rawRect.anchorMin = new Vector2(0, 0);
        rawRect.anchorMax = new Vector2(1, 1);
        rawRect.offsetMin = new Vector2(2, 2);
        rawRect.offsetMax = new Vector2(-2, -2);
        GameObject playerGO = new GameObject("PlayerMarker");
        playerGO.transform.SetParent(containerGO.transform, false);
        playerMarker = playerGO.AddComponent<Image>();
        var pmRect = playerMarker.rectTransform;
        pmRect.anchorMin = new Vector2(0, 0);
        pmRect.anchorMax = new Vector2(0, 0);
        pmRect.pivot = new Vector2(0.5f, 0.5f);
        pmRect.sizeDelta = new Vector2(6, 6);
        playerMarker.color = Color.red;
        playerMarker.raycastTarget = false;
        GameObject fovGO = new GameObject("FOVOverlay");
        fovGO.transform.SetParent(containerGO.transform, false);
        fovOverlay = fovGO.AddComponent<Image>();
        var fovRect = fovOverlay.rectTransform;
        fovRect.anchorMin = new Vector2(0, 0);
        fovRect.anchorMax = new Vector2(0, 0);
        fovRect.pivot = new Vector2(0.5f, 0.5f);
        var defaultSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        fovOverlay.sprite = defaultSprite;
        fovOverlay.raycastTarget = false;
    }

    public Image AcquireMarker()
    {
        Image img = null;
        for (int i = 0; i < markerPool.Count; i++)
        {
            if (!markerPool[i].gameObject.activeSelf)
            {
                img = markerPool[i];
                break;
            }
        }
        if (img == null)
        {
            var go = new GameObject("MinimapMarker");
            go.transform.SetParent(minimapRect, false);
            img = go.AddComponent<Image>();
            img.color = Color.yellow;
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.sizeDelta = new Vector2(6, 6);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0.5f, 0.5f);
            img.gameObject.SetActive(false);
            markerPool.Add(img);
        }
        img.gameObject.SetActive(true);
        activeMarkers.Add(img);
        return img;
    }

    public void ReleaseMarker(Image marker)
    {
        if (marker == null) return;
        marker.gameObject.SetActive(false);
        activeMarkers.Remove(marker);
    }

    public void SetMarkerWorldPosition(Image marker, Vector3 worldPos)
    {
        if (marker == null || minimapRect == null) return;
        var vp = minimapCamera.WorldToViewportPoint(worldPos);
        var size = minimapRect.rect.size;
        marker.rectTransform.anchoredPosition = new Vector2(vp.x * size.x, vp.y * size.y);
    }
}