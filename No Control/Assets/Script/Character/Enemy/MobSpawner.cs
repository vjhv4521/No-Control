using Game.Character;
using UnityEngine;

// 仅保留这一份 MobSpawner 类定义，删除所有重复的
public class MobSpawner : MonoBehaviour
{
    public static MobSpawner Instance { get; private set; }
    [Header("生成配置")]
    public Vector2 SpawnAreaSize = new Vector2(5f, 5f);
    public GameObject MobPrefab;
    public int MobCountPerWave = 5;
    public float SpawnInterval = 2f;
    public int MobHealth = 100;
    [SerializeField] private int MaxSpawnWaves = 10;
    [SerializeField] private int currentWave = 0;

    private float spawnTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        spawnTimer = SpawnInterval;
        if (MobPrefab == null)
        {
            Debug.LogError("MobSpawner：怪物预制体未赋值！");
            enabled = false;
        }
    }

    private void Update()
    {
        if (GameApp.Instance == null) return;
        if (currentWave >= MaxSpawnWaves) return;
        if (GameApp.Instance.State is GameApp.GameState.Break or GameApp.GameState.Dead) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnMobs();
            currentWave++;
            spawnTimer = SpawnInterval;
        }
    }

    private void SpawnMobs()
    {
        for (int i = 0; i < MobCountPerWave; i++)
        {
            Vector2 spawnOffset = new Vector2(
                Random.Range(-SpawnAreaSize.x / 2, SpawnAreaSize.x / 2),
                Random.Range(-SpawnAreaSize.y / 2, SpawnAreaSize.y / 2)
            );
            Vector3 spawnPos = transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);

            GameObject mobObj = Instantiate(MobPrefab, spawnPos, Quaternion.identity);
            if (mobObj == null) continue;

            Enemy enemy = mobObj.GetComponent<Enemy>();
            if (enemy == null) enemy = mobObj.AddComponent<Enemy>();
            
            enemy.MaxHp = MobHealth;
            enemy.Init();
        }
    }

    public void ResetSpawner()
    {
        currentWave = 0;
        spawnTimer = SpawnInterval;
    }
}