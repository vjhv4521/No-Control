using UnityEngine;
using UnityEngine.UI; // 使用标准UI组件，避免TMP中文乱码问题

public class LevelEndTrigger : MonoBehaviour
{
    [Header("过关提示配置")]
    public GameObject winTipPanel; // 过关提示面板（UGUI）
    public Text winTipText; // 使用 Unity 原生 Text 组件
    public string winTipContent = "恭喜过关！";
    public float tipShowDelay = 0.5f; // 延迟显示提示（可选）

    // 确保终点的Collider2D是Trigger
    private void Awake()
    {
        Collider2D coll = GetComponent<Collider2D>();
        if (coll == null)
        {
            Debug.LogError("终点对象缺少Collider2D组件！", this);
            enabled = false;
            return;
        }
        coll.isTrigger = true; // 强制设为触发模式
    }

    // 触发检测（只检测玩家）
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 替换成你的玩家标签（比如Player），确保玩家对象标签正确
        if (other.CompareTag("Player") && GameApp.Instance.State != GameApp.GameState.Win)
        {
            StartCoroutine(LevelWinLogicCoroutine());
        }
    }

    // 过关核心逻辑
    private System.Collections.IEnumerator LevelWinLogicCoroutine()
    {
        // 1. 更新游戏状态为“过关”
        GameApp.Instance.State = GameApp.GameState.Win;
        
        // 2. 停止时间（冻结游戏画面）
        Time.timeScale = 0f;
        
        // 3. 停止怪物生成
        if (MobSpawner.Instance != null)
        {
            MobSpawner.Instance.enabled = false;
        }

        // 4. 延迟显示过关提示（使用不受TimeScale影响的等待）
        if (tipShowDelay > 0)
        {
            yield return new WaitForSecondsRealtime(tipShowDelay);
        }
        
        ShowWinTip();
        Debug.Log("游戏过关！");
    }

    // 显示过关提示
    private void ShowWinTip()
    {
        if (winTipPanel == null)
        {
            var canvasGO = new GameObject("WinCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var panelGO = new GameObject("WinPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var rt = panelGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(600, 200);
            var img = panelGO.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.6f);

            var textGO = new GameObject("WinText");
            textGO.transform.SetParent(panelGO.transform, false);
            var trt = textGO.AddComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 0.5f);
            trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(580, 180);
            
            // 使用标准 Text 组件
            var tmp = textGO.AddComponent<Text>();
            // 尝试加载新版 Unity 的内置字体
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            tmp.font = font;
            tmp.alignment = TextAnchor.MiddleCenter;
            tmp.fontSize = 48;
            tmp.color = Color.white; // 确保字体颜色可见
            
            winTipPanel = panelGO;
            winTipText = tmp;
        }
        winTipPanel.SetActive(true);
        if (winTipText != null) winTipText.text = winTipContent;
    }

    // 可选：重置游戏（比如点击按钮重新玩）
    public void RestartGame()
    {
        Time.timeScale = 1f;
        GameApp.Instance.State = GameApp.GameState.Normal;
        if (MobSpawner.Instance != null)
        {
            MobSpawner.Instance.ResetSpawner();
            MobSpawner.Instance.enabled = true;
        }
        if (winTipPanel != null)
        {
            winTipPanel.SetActive(false);
        }
        // 可选：重置玩家状态、怪物等
    }
}
