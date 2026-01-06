using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MinimapPrefabCreator
{
    [MenuItem("Tools/Create Minimap Prefab")]
    public static void CreateMinimapPrefab()
    {
        var canvasGO = new GameObject("MinimapCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var containerGO = new GameObject("MinimapContainer");
        containerGO.transform.SetParent(canvasGO.transform, false);
        var containerRect = containerGO.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.sizeDelta = new Vector2(Mathf.Round(Screen.width * 0.2f), Mathf.Round(Screen.height * 0.25f));
        containerRect.anchoredPosition = new Vector2(10, -10);
        var bgImage = containerGO.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.8f);

        var rawGO = new GameObject("MinimapImage");
        rawGO.transform.SetParent(containerGO.transform, false);
        var rawRect = rawGO.AddComponent<RectTransform>();
        rawRect.anchorMin = new Vector2(0, 0);
        rawRect.anchorMax = new Vector2(1, 1);
        rawRect.offsetMin = new Vector2(2, 2);
        rawRect.offsetMax = new Vector2(-2, -2);
        var raw = rawGO.AddComponent<RawImage>();

        var playerMarkerGO = new GameObject("PlayerMarker");
        playerMarkerGO.transform.SetParent(containerGO.transform, false);
        var playerRect = playerMarkerGO.AddComponent<RectTransform>();
        playerRect.anchorMin = new Vector2(0, 0);
        playerRect.anchorMax = new Vector2(0, 0);
        playerRect.pivot = new Vector2(0.5f, 0.5f);
        playerRect.sizeDelta = new Vector2(6, 6);
        var playerImg = playerMarkerGO.AddComponent<Image>();
        playerImg.color = Color.red;
        playerImg.raycastTarget = false;

        var fovGO = new GameObject("FOVOverlay");
        fovGO.transform.SetParent(containerGO.transform, false);
        var fovRect = fovGO.AddComponent<RectTransform>();
        fovRect.anchorMin = new Vector2(0, 0);
        fovRect.anchorMax = new Vector2(0, 0);
        fovRect.pivot = new Vector2(0.5f, 0.5f);
        var fovImg = fovGO.AddComponent<Image>();
        fovImg.color = new Color(1f, 1f, 1f, 0.15f);
        fovImg.raycastTarget = false;
        var defaultSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        fovImg.sprite = defaultSprite;

        var camGO = new GameObject("MinimapCamera");
        camGO.transform.SetParent(containerGO.transform, false);
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        var uiLayer = LayerMask.NameToLayer("UI");
        var mask = ~0;
        if (uiLayer >= 0) mask = mask & ~(1 << uiLayer);
        cam.cullingMask = mask;
        cam.depth = 100;

        var ctrl = containerGO.AddComponent<MinimapController>();
        ctrl.minimapCamera = cam;
        ctrl.minimapImage = raw;
        ctrl.minimapRect = containerRect;
        ctrl.backgroundImage = bgImage;
        ctrl.playerMarker = playerImg;
        ctrl.fovOverlay = fovImg;
        ctrl.borderWidth = 2;
        ctrl.screenWidthPercent = 0.2f;
        ctrl.screenHeightPercent = 0.25f;
        ctrl.safeMarginPixels = 10f;
        ctrl.updateInterval = 0.1f;

        var folder = "Assets/Prefabs/Minimap";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Prefabs", "Minimap");
        var path = folder + "/Minimap.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasGO, path);
        Object.DestroyImmediate(canvasGO);
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(path));
    }
}
