// MonsterAI.cs
using UnityEngine;
using UnityEngine.AI;  // 导航系统命名空间

public class MonsterAI : MonoBehaviour
{
    [Header("目标设置")]
    public Transform playerTarget;      // 玩家引用

    [Header("AI参数")]
    public float detectionRange = 10f;  // 检测范围
    public float attackRange = 2f;      // 攻击范围
    public float patrolSpeed = 3.5f;    // 巡逻速度
    public float chaseSpeed = 5f;       // 追逐速度

    private NavMeshAgent navAgent;
    private Animator animator;
    private bool isChasing = false;

    void Start()
    {
        // 获取组件
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // 如果没有手动指定玩家，自动查找
        if (playerTarget == null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // 初始设置
        navAgent.speed = patrolSpeed;
    }

    void Update()
    {
        if (playerTarget == null) return;

        // 计算与玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // AI逻辑状态机
        if (distanceToPlayer <= detectionRange)
        {
            // 进入追逐状态
            isChasing = true;
            navAgent.speed = chaseSpeed;
            navAgent.SetDestination(playerTarget.position);

            // 如果进入攻击范围
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
        }
        else if (isChasing && distanceToPlayer > detectionRange * 1.5f)
        {
            // 丢失目标，停止追逐
            isChasing = false;
            navAgent.speed = patrolSpeed;
            StopChasing();
        }

        // 更新动画
        UpdateAnimation();
    }

    void AttackPlayer()
    {
        // 停止移动，执行攻击
        navAgent.isStopped = true;

        // 这里添加攻击逻辑
        Debug.Log("攻击玩家！");

        // 1秒后恢复移动
        Invoke("ResumeMovement", 1f);
    }

    void ResumeMovement()
    {
        navAgent.isStopped = false;
    }

    void StopChasing()
    {
        navAgent.ResetPath();  // 清除当前路径
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            // 根据速度设置动画
            float speed = navAgent.velocity.magnitude / navAgent.speed;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsChasing", isChasing);
        }
    }

    // 可视化检测范围（编辑器调试用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}