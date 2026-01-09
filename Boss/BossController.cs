using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    // Boss属性
    [Header("Boss属性")]
    public int maxHealth = 1000;
    private int currentHealth;
    public float moveSpeed = 3f;
    public float attackDamage = 50f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    // 房间检测
    [Header("房间设置")]
    public Collider2D bossRoomCollider;
    public Transform player;
    private bool playerInRoom = false;
    private bool isInCombat = false;

    // 巡逻设置
    [Header("巡逻设置")]
    public Vector2[] patrolPoints;
    private int currentPatrolIndex = 0;
    public float patrolSpeed = 1.5f;
    public float patrolWaitTime = 2f;
    private float patrolTimer = 0f;

    // 战斗设置
    [Header("战斗设置")]
    public float detectionRange = 20f;
    private List<Vector2> chasePath;
    private int currentWaypointIndex = 0;
    public float pathUpdateInterval = 0.5f;
    private float lastPathUpdateTime = 0f;

    // 远程攻击
    [Header("远程攻击")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 5f;
    public float projectileLifetime = 3f;
    public float projectileCooldown = 3f;
    private float lastProjectileTime = 0f;
    public float projectileSpawnOffset = 1f;
    public int projectilesPerShot = 3;
    public float projectileSpread = 15f;

    // 镜头移动
    [Header("镜头设置")]
    public Camera mainCamera;
    public float cameraTransitionTime = 2f;
    private Vector3 originalCameraPosition;
    private bool isCameraTransitioning = false;

    // 组件引用
    private Rigidbody2D rb;
    private Collider2D bossCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // 伤害播报设置
    [Header("伤害播报")]
    public float damageTextDuration = 1f;
    public float damageTextOffset = 1f;
    public float damageTextSpeed = 2f;

    // 调试设置
    [Header("调试设置")]
    public bool debugMode = false;

    // 状态管理
    private enum BossState
    {
        Patrol,
        Chase,
        Attack,
        Idle,
        Transition
    }
    private BossState currentState = BossState.Patrol;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        originalCameraPosition = mainCamera.transform.position;

        // 自动获取玩家对象
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (bossRoomCollider == null)
        {
            bossRoomCollider = GetComponentInParent<Collider2D>();
        }

        // 设置初始巡逻点
        if (patrolPoints.Length == 0)
        {
            patrolPoints = new Vector2[] { transform.position, transform.position + Vector3.right * 5f, transform.position - Vector3.up * 5f };
        }
    }

    private void Update()
    {
        if (isCameraTransitioning) return;

        CheckPlayerInRoom();
        UpdateBossState();
        HandleBossState();

        // 调试日志
        if (debugMode && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            Debug.Log($"Boss调试信息: 状态={currentState}, 玩家在房间内={playerInRoom}, 距离={distanceToPlayer:F2}, 检测范围={detectionRange}");
        }
    }

    // 检查玩家是否在房间内
    private void CheckPlayerInRoom()
    {
        if (player != null)
        {
            bool isInRoom = false;

            // 方法1: 使用OverlapPoint检测
            if (bossRoomCollider != null)
            {
                isInRoom = bossRoomCollider.OverlapPoint(player.position);
            }

            // 方法2: 直接检测距离（备用方案）
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            bool isCloseEnough = distanceToPlayer <= detectionRange;

            // 综合判断：如果有房间碰撞体，使用房间检测；否则使用距离检测
            playerInRoom = bossRoomCollider != null ? isInRoom : isCloseEnough;
        }
    }

    // 更新Boss状态
    private void UpdateBossState()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case BossState.Patrol:
                // 多种激活条件：玩家在房间内或距离足够近
                bool shouldActivate = playerInRoom || distanceToPlayer <= detectionRange;
                if (shouldActivate)
                {
                    StartCombatTransition();
                }
                break;
            case BossState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    currentState = BossState.Attack;
                }
                else if (distanceToPlayer > detectionRange * 2f) // 增大脱离范围
                {
                    // 尝试重新计算路径
                    UpdateChasePath();
                }
                break;
            case BossState.Attack:
                if (distanceToPlayer > attackRange * 1.5f)
                {
                    currentState = BossState.Chase;
                }
                break;
        }
    }

    // 处理Boss状态
    private void HandleBossState()
    {
        switch (currentState)
        {
            case BossState.Patrol:
                Patrol();
                break;
            case BossState.Chase:
                ChasePlayer();
                break;
            case BossState.Attack:
                AttackPlayer();
                break;
            case BossState.Idle:
                Idle();
                break;
        }
    }

    // 巡逻行为
    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Vector2 targetPosition = patrolPoints[currentPatrolIndex];
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float distanceToPoint = Vector2.Distance(transform.position, targetPosition);

        if (distanceToPoint < 0.5f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                patrolTimer = 0f;
            }
            rb.velocity = Vector2.zero;
        }
        else
        {
            rb.velocity = direction * patrolSpeed;
        }

        // 更新朝向
        UpdateFacingDirection(direction);
    }

    // 追逐玩家
    private void ChasePlayer()
    {
        if (player == null) return;

        if (chasePath == null || chasePath.Count == 0 || Time.time - lastPathUpdateTime > pathUpdateInterval)
        {
            UpdateChasePath();
        }

        if (chasePath != null && chasePath.Count > 0)
        {
            MoveAlongPath();
        }
        else
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            UpdateFacingDirection(direction);
        }

        // 尝试远程攻击
        TryRemoteAttack();
    }

    // 更新追逐路径
    private void UpdateChasePath()
    {
        if (player == null) return;

        chasePath = FindPath(transform.position, player.position);
        currentWaypointIndex = 0;
        lastPathUpdateTime = Time.time;
    }

    // 沿着路径移动
    private void MoveAlongPath()
    {
        if (currentWaypointIndex >= chasePath.Count)
        {
            chasePath = null;
            return;
        }

        Vector2 currentWaypoint = chasePath[currentWaypointIndex];
        float distanceToWaypoint = Vector2.Distance(transform.position, currentWaypoint);

        if (distanceToWaypoint < 0.3f)
        {
            currentWaypointIndex++;
        }
        else
        {
            Vector2 direction = (currentWaypoint - (Vector2)transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            UpdateFacingDirection(direction);
        }
    }

    // A*寻路算法（无视墙体）
    private List<Vector2> FindPath(Vector2 start, Vector2 target)
    {
        List<Vector2> path = new List<Vector2>();
        path.Add(target);
        return path;
    }

    // 攻击玩家
    private void AttackPlayer()
    {
        if (player == null) return;

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        // 近战攻击
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                // 执行攻击
                player.GetComponent<Player>().TakeDamage(attackDamage);
                lastAttackTime = Time.time;

                // 播放攻击动画
                if (animator != null)
                {
                    animator.SetTrigger("Attack");
                }
            }
        }

        // 尝试远程攻击
        TryRemoteAttack();
    }

    // 远程攻击
    private void TryRemoteAttack()
    {
        if (Time.time - lastProjectileTime >= projectileCooldown)
        {
            lastProjectileTime = Time.time;
            StartCoroutine(FireProjectiles());
        }
    }

    // 发射多个追踪弹
    private IEnumerator FireProjectiles()
    {
        for (int i = 0; i < projectilesPerShot; i++)
        {
            FireSingleProjectile(i);
            yield return new WaitForSeconds(0.2f);
        }
    }

    // 发射单个追踪弹
    private void FireSingleProjectile(int index)
    {
        if (projectilePrefab == null || player == null) return;

        // 计算发射位置和角度
        Vector3 spawnPosition = transform.position + (Vector3)GetProjectileSpawnOffset(index);
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // 设置追踪弹属性
        BossProjectile projectileScript = projectile.AddComponent<BossProjectile>();
        projectileScript.target = player.transform;
        projectileScript.speed = projectileSpeed;
        projectileScript.lifetime = projectileLifetime;
        projectileScript.damage = attackDamage * 0.5f;
        projectileScript.bossController = gameObject;
    }

    // 获取追踪弹生成偏移
    private Vector2 GetProjectileSpawnOffset(int index)
    {
        float angle = (index - (projectilesPerShot - 1) / 2f) * projectileSpread;
        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
        return direction * projectileSpawnOffset;
    }

    // 空闲行为
    private void Idle()
    {
        rb.velocity = Vector2.zero;
    }

    // 更新朝向
    private void UpdateFacingDirection(Vector2 direction)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    // 开始战斗过渡
    private void StartCombatTransition()
    {
        if (isInCombat || isCameraTransitioning) return;

        isCameraTransitioning = true;
        StartCoroutine(BossCombatTransition());
    }

    // Boss战斗过渡动画
    private IEnumerator BossCombatTransition()
    {
        // 停止时间
        Time.timeScale = 0f;
        float fixedDeltaTime = Time.fixedDeltaTime;
        Time.fixedDeltaTime = 0f;

        // 镜头从玩家移动到Boss
        Vector3 cameraStartPos = mainCamera.transform.position;
        Vector3 cameraBossPos = transform.position + Vector3.back * 10f;
        float transitionProgress = 0f;

        while (transitionProgress < 1f)
        {
            transitionProgress += Time.unscaledDeltaTime / cameraTransitionTime;
            mainCamera.transform.position = Vector3.Lerp(cameraStartPos, cameraBossPos, transitionProgress);
            yield return null;
        }

        // 短暂停留
        yield return new WaitForSecondsRealtime(1f);

        // 镜头从Boss移动回玩家
        transitionProgress = 0f;
        while (transitionProgress < 1f)
        {
            transitionProgress += Time.unscaledDeltaTime / cameraTransitionTime;
            mainCamera.transform.position = Vector3.Lerp(cameraBossPos, cameraStartPos, transitionProgress);
            yield return null;
        }

        // 恢复时间
        Time.timeScale = 1f;
        Time.fixedDeltaTime = fixedDeltaTime;
        isCameraTransitioning = false;
        isInCombat = true;
        currentState = BossState.Chase;
    }

    // 玩家进入/离开房间检测
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 使用标签检测更可靠
        if (other.CompareTag("Player"))
        {
            playerInRoom = true;
            // 更新player引用
            if (player == null)
            {
                player = other.transform;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 使用标签检测更可靠
        if (other.CompareTag("Player"))
        {
            playerInRoom = false;
            if (isInCombat)
            {
                // 玩家离开房间，结束战斗
                isInCombat = false;
                currentState = BossState.Patrol;
            }
        }
    }

    // 检测实体碰撞（物理碰撞）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 目前不检测玩家攻击
    }

    // 受到伤害
    private bool isTakingDamage = false;
    public void TakeDamage(int damage)
    {
        // 防止重入，避免无限递归
        if (isTakingDamage) return;

        try
        {
            isTakingDamage = true;
            currentHealth -= damage;

            // 播放受伤动画和音效
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }

            // 生成伤害播报
            ShowDamageText(damage);

            // 检查是否死亡
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        finally
        {
            isTakingDamage = false;
        }
    }

    // 显示伤害文本
    private void ShowDamageText(int damage)
    {
        // 创建伤害文本游戏对象
        GameObject damageTextObj = new GameObject("DamageText");

        // 设置位置
        Vector3 textPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), damageTextOffset, -1f);
        damageTextObj.transform.position = textPosition;

        // 添加Canvas组件
        Canvas canvas = damageTextObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 1000;

        // 设置Canvas尺寸
        RectTransform canvasRect = damageTextObj.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(200, 50);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }

        // 添加Text组件
        Text damageText = damageTextObj.AddComponent<Text>();

        // 设置Text属性
        damageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        damageText.fontSize = 24;
        damageText.color = Color.red;
        damageText.alignment = TextAnchor.MiddleCenter;
        damageText.raycastTarget = false;
        damageText.text = damage.ToString();

        // 添加Canvas Renderer
        if (damageTextObj.GetComponent<CanvasRenderer>() == null)
        {
            damageTextObj.AddComponent<CanvasRenderer>();
        }

        // 添加简单的动画效果
        StartCoroutine(AnimateDamageText(damageTextObj, damageText));
    }

    // 伤害文本动画
    private IEnumerator AnimateDamageText(GameObject damageTextObj, Text damageText)
    {
        if (damageTextObj == null || damageText == null) yield break;

        Vector3 startPosition = damageTextObj.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0f, 2f, 0f);
        float elapsedTime = 0f;

        Color startColor = damageText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < damageTextDuration)
        {
            float t = elapsedTime / damageTextDuration;

            // 移动位置
            damageTextObj.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            // 淡入淡出效果
            damageText.color = Color.Lerp(startColor, endColor, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 销毁伤害文本
        Destroy(damageTextObj);
    }

    // 死亡
    private void Die()
    {
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // 禁用Boss
        rb.velocity = Vector2.zero;
        enabled = false;

        // 播放死亡音效和粒子效果
        // 触发胜利条件

        // 延迟后销毁Boss
        StartCoroutine(DestroyAfterDelay(1f));
    }

    // 延迟后销毁Boss
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // 绘制Gizmos
    private void OnDrawGizmosSelected()
    {
        // 绘制巡逻路径
        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < patrolPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
            }
            Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1], patrolPoints[0]);
        }

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 绘制检测范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}