using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // --- 变量定义区域 ---

    [Header("基础设置 (Inspector中调整)")]
    public float moveSpeed = 5f;          // 移动速度
    public float attackDuration = 0.5f;   // 攻击动作持续时间（硬直时间）

    [Header("状态监测 (只读)")]
    public bool isDead;                   // 核心状态：是否死亡

    // --- 组件引用 ---
    private InputActions inputActions;    // 新版输入系统的实例
    private Rigidbody2D rb;
    private Animator animator;

    // --- 内部变量 ---
    private Vector2 moveInput;            // 存储玩家当前的输入方向
    private bool isMeleeAttack;           // 防止攻击连点（攻击冷却锁）

    // 1. 初始化组件
    private void Awake()
    {
        inputActions = new InputActions();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // 2. 激活输入系统 (事件订阅)
    private void OnEnable()
    {
        inputActions.Gameplay.Enable();
        // 当按下攻击键时，调用 OnMeleeAttack 函数
        inputActions.Gameplay.MeleeAttack.started += OnMeleeAttack;
    }

    // 3. 禁用输入系统 (取消订阅，防止内存泄漏)
    private void OnDisable()
    {
        inputActions.Gameplay.MeleeAttack.started -= OnMeleeAttack;
        inputActions.Gameplay.Disable();
    }

    // 4. 每帧逻辑：处理输入读取 和 动画更新
    private void Update()
    {
        // 【逻辑锁】如果你死了，就完全切断输入读取，也不再更新跑动动画
        if (isDead) return;

        // 实时读取手柄/键盘的 Vector2 输入
        moveInput = inputActions.Gameplay.Move.ReadValue<Vector2>();
        
        // 更新动画参数
        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);
        animator.SetFloat("Speed", moveInput.sqrMagnitude); // 用向量长度判断是否在动
        animator.SetBool("isMeleeAttack", isMeleeAttack);
    }

    // 5. 物理帧逻辑：处理刚体移动
    private void FixedUpdate()
    {
        // 【物理锁】如果你死了，必须强制停止物理运动
        if (isDead)
        {
            rb.velocity = Vector2.zero; // 防止尸体因惯性继续滑行
            return;
        }

        Move();
    }

    // 具体移动逻辑
    private void Move()
    {
        // 处理角色翻转 (Left/Right)
        // 如果向左走 (x < 0)
        if (moveInput.x < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        // 如果向右走 (x > 0)
        else if (moveInput.x > 0.01f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // 应用速度给刚体
        rb.velocity = moveInput * moveSpeed;
    }

    // 6. 攻击事件回调
    private void OnMeleeAttack(InputAction.CallbackContext context)
    {
        // 【攻击锁】死人不能打，正在攻击中也不能打（防止无限连击）
        if (isDead || isMeleeAttack) return;

        isMeleeAttack = true; // 上锁
        animator.SetTrigger("MeleeAttack"); // 播放动画
        
        // 开启协程，倒计时结束后解锁
        StartCoroutine(ResetAttackState());
    }

    // --- 外部接口 (供敌人/陷阱/UI调用) ---

    // 玩家受伤
    public void PlayerHurt()
    {
        // 死人不需要再播受伤动画
        if (isDead) return;
        animator.SetTrigger("hurt");
    }

    // 玩家死亡
    public void PlayerDie()
    {
        // 防止多次调用导致逻辑重复执行
        if (isDead) return;

        isDead = true; // 改变逻辑状态
        animator.SetBool("isDead", true); // 同步动画状态
        
        Debug.Log("【系统】玩家判定死亡");
        // 可以在此处添加弹出“游戏结束UI”的逻辑
    }

    // --- 协程工具 ---
    
    // 攻击硬直计时器
    IEnumerator ResetAttackState()
    {
        // 等待攻击动作播放完毕
        yield return new WaitForSeconds(attackDuration);
        isMeleeAttack = false; // 解锁，允许下一次攻击
    }
}