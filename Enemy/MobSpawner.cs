using UnityEngine;
using UnityEngine.AI;

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
        if (GameApp.Instance.State is GameApp.GameState.Break or GameApp.GameState.Dead or GameApp.GameState.Win) return;

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
            Vector3 spawnPos = FindSafeSpawnPosition();

            if (spawnPos != Vector3.zero)
            {
                GameObject mobObj = Instantiate(MobPrefab, spawnPos, Quaternion.identity);
                if (mobObj == null) continue;

                Enemy enemy = mobObj.GetComponent<Enemy>();
                if (enemy == null) enemy = mobObj.AddComponent<Enemy>();
            }
        }
    }

    private Vector3 FindSafeSpawnPosition()
    {
        const int maxAttempts = 10;
        const float safeDistance = 1.5f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 spawnOffset = new Vector2(
                Random.Range(-SpawnAreaSize.x / 2, SpawnAreaSize.x / 2),
                Random.Range(-SpawnAreaSize.y / 2, SpawnAreaSize.y / 2)
            );
            Vector3 spawnPos = transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);

            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(spawnPos, safeDistance, LayerMask.GetMask("Enemy"));
            if (nearbyEnemies.Length == 0)
            {
                Collider2D[] obstacles = Physics2D.OverlapCircleAll(spawnPos, safeDistance / 2, LayerMask.GetMask("Wall"));
                if (obstacles.Length == 0)
                {
                    // 检查该位置是否在NavMesh上
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(spawnPos, out hit, 2.0f, NavMesh.AllAreas))
                    {
                        // 返回NavMesh上的最近位置
                        return hit.position;
                    }
                }
            }
        }

        // 如果所有尝试都失败，找到生成器附近最近的NavMesh位置
        NavMeshHit fallbackHit;
        if (NavMesh.SamplePosition(transform.position, out fallbackHit, 5.0f, NavMesh.AllAreas))
        {
            return fallbackHit.position;
        }

        return transform.position;
    }

    public void ResetSpawner()
    {
        currentWave = 0;
        spawnTimer = SpawnInterval;
    }
}
