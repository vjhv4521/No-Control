using Game.Character;
using UnityEngine;

// 玩家核心属性：生命值、经验、状态（继承CharacterBase）
public class Player : CharacterBase
{
    public static Player Instance { get; private set; }
    public int Exp { get; set; } = 0;
    public float moveSpeed = 5f; // 移动速度

    private void Awake()
    {
        // 单例防重复
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Init(); // 初始化状态系统
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        // 受击触发混乱系统（核心逻辑保留）
        GetComponent<PlayerController>()?.OnPlayerHurt();
        GetComponent<PlayerController>()?.PlayerHurt();
    }

    public override void SetDead()
    {
        base.SetDead();
        GetComponent<PlayerController>()?.PlayerDie();
        Debug.Log($"玩家死亡，最终经验：{Exp}");
    }
}