using Game.Character;
using UnityEngine;
using System.Collections;

public class Player : CharacterBase
{
    public static Player Instance { get; private set; }
    public int Exp { get; set; } = 0;
    public float moveSpeed = 5f; 

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() { Init(); }

    public override void TakeDamage(float damage)
    {
        // 死亡期间免疫伤害
        if (status != null && !status.Alive) return;

        base.TakeDamage(damage);
        GetComponent<PlayerController>()?.OnPlayerHurt();
        GetComponent<PlayerController>()?.PlayerHurt();
    }

    public override void SetDead()
    {
        // 注意：不调用 base.SetDead()，防止触发基类的延迟销毁
        // 我们只执行玩家特有的死亡逻辑
        
        GetComponent<PlayerController>()?.PlayerDie();
        
        Debug.Log("【逻辑】玩家已死亡，启动2秒复活计时...");
        
        // 停止所有正在运行的协程，防止复活逻辑多次触发
        StopAllCoroutines();
        StartCoroutine(RespawnRoutine(2f));
    }

    private IEnumerator RespawnRoutine(float delay)
    {
        // 使用真时间，防止 Time.timeScale 为 0 时卡死
        yield return new WaitForSecondsRealtime(delay);

        Debug.Log("【逻辑】正在执行复活重置...");

        // 1. 暴力重置状态：重新 new 一个 Status 强制回满血并设置 Alive 为 true
        Init(); 

        // 2. 额外保险：如果你的 Status 类有内部变量，确保它们被重置
        if (status != null) {
            // 这里根据你 Status 类的具体实现，可能需要手动设为 true
            // status.Alive = true; 
        }

        // 3. 通知控制器解除锁定并切换动画
        GetComponent<PlayerController>()?.PlayerRespawn();

        Debug.Log("【逻辑】复活完成！");
    }
}