using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Canvas startCanvas;
    public Button startButton;
    public Button quitButton;
    public Image humanSprite;
    public Image orcSprite;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public Transform playerTransform;
    public float cameraTransitionTime = 2f;

    [Header("Dungeon Generation")]
    public CorridorFirstDungeonGenerator corridorGenerator;
    public SimpleRandomWalkDungeonGenerator simpleGenerator;
    public bool useCorridorGenerator = true;

    [Header("Game State")]
    private bool isGameStarted = false;

    [Header("UI Manager")]
    public UIManager uiManager;

    private void Start()
    {
        // 确保开始界面可见
        startCanvas.gameObject.SetActive(true);

        // 注册按钮事件
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);

        // 自动查找玩家对象
        if (playerTransform == null)
        {
            FindPlayer();
        }

        // 自动查找UIManager实例
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogWarning("UIManager not found! UI switching will not work.");
            }
        }

        // 将摄像机位置设置到Canvas上
        mainCamera.transform.position = startCanvas.transform.position + new Vector3(0, 0, -10f);
    }

    private void OnDisable()
    {
        // 当GameObject被禁用时，停止所有协程
        StopAllCoroutines();
    }

    // 自动查找玩家对象
    private void FindPlayer()
    {
        // 方法1: 使用标签查找
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player with tag 'Player' not found! Using default position.");
        }
    }

    // 开始游戏
    public void StartGame()
    {
        if (isGameStarted) return;

        isGameStarted = true;

        // 确保游戏对象是活跃的
        gameObject.SetActive(true);
        ActivateGameObjectAndParents(gameObject);

        // 确保游戏对象是活跃的，然后启动协程
        if (gameObject.activeInHierarchy)
        {
            // 开始摄像机过渡
            StartCoroutine(TransitionToGame());
        }
        else
        {
            Debug.LogWarning("StartMenu gameObject is inactive, cannot start coroutine");
            // 如果游戏对象不活跃，直接执行过渡逻辑的关键部分
            TransitionToGameImmediately();
        }
    }

    // 当无法启动协程时的备用方法
    private void TransitionToGameImmediately()
    {
        // 直接移动摄像机到玩家位置
        Vector3 targetPos;
        if (playerTransform != null)
        {
            targetPos = playerTransform.position + new Vector3(0, 0, -10f);
        }
        else
        {
            targetPos = mainCamera.transform.position;
        }
        mainCamera.transform.position = targetPos;

        // 隐藏开始界面
        startCanvas.gameObject.SetActive(false);

        // 通知UIManager开始游戏
        if (uiManager != null)
        {
            uiManager.StartGame();
        }
    }

    // 摄像机过渡到游戏
    private IEnumerator TransitionToGame()
    {
        // 记录起始位置
        Vector3 startPos = mainCamera.transform.position;

        // 获取目标位置，处理playerTransform可能为null的情况
        Vector3 targetPos;
        if (playerTransform != null)
        {
            targetPos = playerTransform.position + new Vector3(0, 0, -10f);
        }
        else
        {
            // 使用默认位置或当前位置
            targetPos = startPos + new Vector3(0, 0, 0); // 保持当前Z轴位置
            Debug.LogWarning("Using default camera position because playerTransform is null!");
        }

        float elapsedTime = 0f;

        // 平滑移动摄像机
        while (elapsedTime < cameraTransitionTime)
        {
            float t = elapsedTime / cameraTransitionTime;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保摄像机到达目标位置
        mainCamera.transform.position = targetPos;

        // 隐藏开始界面
        startCanvas.gameObject.SetActive(false);

        // 通知UIManager开始游戏，自动切换到游戏主界面
        if (uiManager != null)
        {
            uiManager.StartGame();
        }
        else
        {
            Debug.LogWarning("UIManager is null! Cannot automatically switch to game UI.");
        }
    }

    // 生成地牢
    private void GenerateDungeon()
    {
        if (useCorridorGenerator && corridorGenerator != null)
        {
            // 使用走廊优先地牢生成器
            corridorGenerator.GenerateDungeon();
        }
        else if (simpleGenerator != null)
        {
            // 使用简单随机游走地牢生成器
            simpleGenerator.GenerateDungeon();
        }
        else
        {
            Debug.LogError("No dungeon generator assigned!");
        }
    }

    // 生成怪物
    private void SpawnMonsters()
    {
        // 这里可以添加怪物生成逻辑
        // 例如：查找所有可能的怪物生成点，然后生成怪物
        Debug.Log("Monsters spawned!");
    }

    // 退出游戏
    public void QuitGame()
    {
        // 在编辑器中停止游戏
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在构建版本中退出游戏
        Application.Quit();
#endif
    }

    // 返回开始菜单
    public void ReturnToStartMenu()
    {
        // 确保脚本所在的GameObject是活跃的（如果脚本和Canvas是同一个对象）
        gameObject.SetActive(true);

        // 激活Canvas及其所有父对象，确保activeInHierarchy为true
        ActivateGameObjectAndParents(startCanvas.gameObject);

        // 确保游戏对象是活跃的，然后启动协程
        if (gameObject.activeInHierarchy)
        {
            // 将摄像机移动回Canvas位置
            StartCoroutine(MoveCameraToCanvas());
        }
        else
        {
            Debug.LogWarning("StartMenu gameObject is inactive, cannot start coroutine");
            // 如果游戏对象不活跃，直接移动摄像机
            MoveCameraToCanvasImmediately();
        }

        isGameStarted = false;
    }

    // 激活游戏对象及其所有父对象
    private void ActivateGameObjectAndParents(GameObject obj)
    {
        if (obj == null) return;

        // 激活当前对象
        obj.SetActive(true);

        // 递归激活所有父对象
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            parent.gameObject.SetActive(true);
            parent = parent.parent;
        }
    }

    // 当无法启动协程时的备用方法
    private void MoveCameraToCanvasImmediately()
    {
        // 直接移动摄像机到Canvas位置
        Vector3 targetPos = startCanvas.transform.position + new Vector3(0, 0, -10f);
        mainCamera.transform.position = targetPos;
    }

    // 移动摄像机到Canvas
    private IEnumerator MoveCameraToCanvas()
    {
        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = startCanvas.transform.position + new Vector3(0, 0, -10f);

        float elapsedTime = 0f;

        while (elapsedTime < cameraTransitionTime)
        {
            float t = elapsedTime / cameraTransitionTime;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPos;
    }
}