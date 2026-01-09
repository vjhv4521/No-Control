using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 单例实例
    public static UIManager Instance { get; private set; }

    [Header("Start Menu")]
    public GameObject startMenu;
    public Button startButton;
    public Button quitButton;
    public Image humanSprite;
    public Image orcSprite;

    [Header("Game UI")]
    public GameObject gameUI;
    public Text healthText;
    public Text scoreText;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Game Over Menu")]
    public GameObject gameOverMenu;
    public Text finalScoreText;
    public Button retryButton;
    public Button quitGameOverButton;

    private bool isPaused = false;
    private bool isGameOver = false;

    private void Awake()
    {
        // 确保单例唯一
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初始化UI状态（使用空条件运算符避免未赋值错误）
        ShowStartMenu();
        HideGameUI();
        HidePauseMenu();
        HideGameOverMenu();

        // 注册按钮事件
        RegisterButtonEvents();
    }

    // 显示开始菜单
    private void ShowStartMenu()
    {
        if (startMenu != null)
        {
            startMenu.SetActive(true);
        }
        else
        {
            Debug.LogWarning("StartMenu is not assigned in UIManager!");
        }
    }

    // 隐藏开始菜单
    private void HideStartMenu()
    {
        if (startMenu != null)
        {
            startMenu.SetActive(false);
        }
    }

    // 显示游戏UI
    private void ShowGameUI()
    {
        if (gameUI != null)
        {
            gameUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameUI is not assigned in UIManager!");
        }
    }

    // 隐藏游戏UI
    private void HideGameUI()
    {
        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }
    }

    // 显示暂停菜单
    private void ShowPauseMenu()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PauseMenu is not assigned in UIManager!");
        }
    }

    // 隐藏暂停菜单
    private void HidePauseMenu()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
    }

    // 显示游戏结束菜单
    private void ShowGameOverMenu()
    {
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameOverMenu is not assigned in UIManager!");
        }
    }

    // 隐藏游戏结束菜单
    private void HideGameOverMenu()
    {
        if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(false);
        }
    }

    private void Update()
    {
        // 处理暂停/继续输入
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // 注册按钮事件
    private void RegisterButtonEvents()
    {
        // 开始菜单按钮
        startButton?.onClick.AddListener(StartGame);
        quitButton?.onClick.AddListener(QuitGame);

        // 暂停菜单按钮
        resumeButton?.onClick.AddListener(ResumeGame);
        restartButton?.onClick.AddListener(RestartGame);
        mainMenuButton?.onClick.AddListener(ReturnToMainMenu);

        // 游戏结束菜单按钮
        retryButton?.onClick.AddListener(RestartGame);
        quitGameOverButton?.onClick.AddListener(QuitGame);
    }

    // 开始游戏
    public void StartGame()
    {
        HideStartMenu();
        ShowGameUI();
        HidePauseMenu();
        HideGameOverMenu();

        // 重置游戏状态
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;

        // 通知游戏管理器开始游戏
        GameManager.Instance?.StartGame();
    }

    // 暂停游戏
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        ShowPauseMenu();
    }

    // 继续游戏
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        HidePauseMenu();
    }

    // 重新开始游戏
    public void RestartGame()
    {
        HideStartMenu();
        ShowGameUI();
        HidePauseMenu();
        HideGameOverMenu();

        // 重置游戏状态
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;

        // 通知游戏管理器重新开始游戏
        GameManager.Instance?.RestartGame();
    }

    // 返回主菜单
    public void ReturnToMainMenu()
    {
        ShowStartMenu();
        HideGameUI();
        HidePauseMenu();
        HideGameOverMenu();

        // 重置游戏状态
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;
    }

    // 游戏结束
    public void GameOver(int finalScore)
    {
        isGameOver = true;
        Time.timeScale = 1f;

        // 更新最终分数
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore.ToString();
        }

        HideGameUI();
        ShowGameOverMenu();

        // 通知游戏管理器游戏结束
        GameManager.Instance?.GameOver();
    }



    // 退出游戏
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 更新健康值显示
    public void UpdateHealth(int health, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + health.ToString() + "/" + maxHealth.ToString();
        }
    }

    // 更新分数显示
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }
}