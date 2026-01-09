using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 单例实例
    public static GameManager Instance { get; private set; }

    [Header("Dungeon Generation")]
    public CorridorFirstDungeonGenerator corridorGenerator;
    public SimpleRandomWalkDungeonGenerator simpleGenerator;
    public bool useCorridorGenerator = true;

    [Header("Monster Spawning")]
    public GameObject enemyPrefab;
    public int maxMonsters = 10;
    public float monsterSpawnDelay = 0.5f;

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;
    public int playerMaxHealth = 100;
    private int playerCurrentHealth;

    [Header("Game State")]
    private int score = 0;
    private bool isGameStarted = false;

    [Header("References")]
    public Camera mainCamera;
    public Canvas startCanvas;

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
        // 初始化游戏状态
        playerCurrentHealth = playerMaxHealth;
        score = 0;

        // 更新UI（使用空条件运算符避免未赋值错误）
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(playerCurrentHealth, playerMaxHealth);
            UIManager.Instance.UpdateScore(score);
        }
        else
        {
            Debug.LogWarning("UIManager.Instance is null!");
        }
    }

    // 开始游戏
    public void StartGame()
    {
        if (isGameStarted) return;

        isGameStarted = true;

        // 生成地牢
        GenerateDungeon();

        // 生成玩家（如果没有的话）
        SpawnPlayer();

        // 生成怪物
        StartCoroutine(SpawnMonsters());

        // 摄像机跟随玩家
        FollowPlayer();
    }

    // 重新开始游戏
    public void RestartGame()
    {
        // 重置游戏状态
        isGameStarted = false;
        playerCurrentHealth = playerMaxHealth;
        score = 0;

        // 更新UI
        UIManager.Instance?.UpdateHealth(playerCurrentHealth, playerMaxHealth);
        UIManager.Instance?.UpdateScore(score);

        // 重新加载场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 返回主菜单
    public void ReturnToMainMenu()
    {
        // 重置游戏状态
        isGameStarted = false;
        playerCurrentHealth = playerMaxHealth;
        score = 0;

        // 更新UI
        UIManager.Instance?.UpdateHealth(playerCurrentHealth, playerMaxHealth);
        UIManager.Instance?.UpdateScore(score);

        // 不重新加载场景，而是通过UIManager显示开始菜单
        UIManager.Instance?.ReturnToMainMenu();
    }

    // 游戏结束
    public void GameOver()
    {
        // 重置游戏状态
        isGameStarted = false;
    }

    // 游戏胜利
    public void GameWin()
    {
        // 重置游戏状态
        isGameStarted = false;

        Debug.Log("游戏胜利!");

        // 延迟后重新加载场景
        StartCoroutine(GameWinCoroutine());
    }

    private IEnumerator GameWinCoroutine()
    {
        // 等待3秒，让玩家看到胜利信息
        yield return new WaitForSeconds(3f);

        // 重新加载场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 生成地牢
    private void GenerateDungeon()
    {
        // 生成新地牢（GenerateDungeon方法会自动清除现有的地牢）
        if (useCorridorGenerator && corridorGenerator != null)
        {
            corridorGenerator.GenerateDungeon();
        }
        else if (simpleGenerator != null)
        {
            simpleGenerator.GenerateDungeon();
        }
        else
        {
            Debug.LogError("No dungeon generator assigned!");
        }
    }

    // 生成玩家
    private void SpawnPlayer()
    {
        // 查找现有玩家
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer == null && playerPrefab != null)
        {
            // 生成新玩家，使用默认位置或预设的出生点
            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // 生成怪物
    private IEnumerator SpawnMonsters()
    {
        if (enemyPrefab == null) yield break;

        // 简单的怪物生成逻辑：在玩家周围生成
        int monstersSpawned = 0;
        while (monstersSpawned < maxMonsters)
        {
            // 查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) break;

            // 在玩家周围随机位置生成怪物（距离玩家3-10单位）
            Vector3 spawnPosition = player.transform.position + new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f),
                0f
            );

            // 确保生成位置与玩家保持一定距离
            if (Vector3.Distance(spawnPosition, player.transform.position) < 3f)
            {
                continue;
            }

            // 生成怪物
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            monstersSpawned++;

            // 等待一段时间再生成下一个怪物
            yield return new WaitForSeconds(monsterSpawnDelay);
        }
    }

    // 获取所有地板位置
    private List<Vector3> GetAllFloorPositions()
    {
        List<Vector3> floorPositions = new List<Vector3>();

        // 简单的实现：返回玩家周围的一些位置
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 在玩家周围生成一些随机位置
            for (int i = 0; i < 50; i++)
            {
                Vector3 pos = player.transform.position + new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(-10f, 10f),
                    0f
                );
                floorPositions.Add(pos);
            }
        }
        else
        {
            // 如果没有玩家，返回原点周围的一些位置
            for (int i = 0; i < 50; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(-10f, 10f),
                    0f
                );
                floorPositions.Add(pos);
            }
        }

        return floorPositions;
    }

    // 打乱列表
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // 摄像机跟随玩家
    private void FollowPlayer()
    {
        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && mainCamera != null)
        {
            // 将摄像机位置设置到玩家身上
            mainCamera.transform.position = player.transform.position + new Vector3(0, 0, -10f);

            // 这里可以添加摄像机跟随逻辑，例如平滑跟随
        }
    }

    // 更新玩家健康值
    public void UpdatePlayerHealth(int healthChange)
    {
        playerCurrentHealth += healthChange;
        playerCurrentHealth = Mathf.Clamp(playerCurrentHealth, 0, playerMaxHealth);

        // 更新UI
        UIManager.Instance?.UpdateHealth(playerCurrentHealth, playerMaxHealth);

        // 检查游戏结束
        if (playerCurrentHealth <= 0)
        {
            UIManager.Instance?.GameOver(score);
        }
    }

    // 更新分数
    public void UpdateScore(int scoreChange)
    {
        score += scoreChange;

        // 更新UI
        UIManager.Instance?.UpdateScore(score);
    }

    // 获取当前分数
    public int GetScore()
    {
        return score;
    }

    // 获取玩家当前健康值
    public int GetPlayerHealth()
    {
        return playerCurrentHealth;
    }

    // 获取玩家最大健康值
    public int GetPlayerMaxHealth()
    {
        return playerMaxHealth;
    }
}