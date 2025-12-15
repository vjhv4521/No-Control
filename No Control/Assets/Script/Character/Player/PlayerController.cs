using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// 仅负责控制逻辑，不继承CharacterBase，引用Player类获取属性
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("控制配置")]
    public float attackDuration = 0.5f;
    public InputChaosManager chaosManager;

    [Header("状态")]
    public bool isDead;
    public bool isMeleeAttack;

    // 组件引用
    private InputActions inputActions;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private bool lastAttackPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 组件校验
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        if (rb == null) Debug.LogError("PlayerController：缺少Rigidbody2D组件！");
        if (animator == null) Debug.LogError("PlayerController：缺少Animator组件！");

        // 输入系统初始化（校验是否生成）
        try
        {
            inputActions = new InputActions();
        }
        catch
        {
            Debug.LogError("InputActions未生成！请在Edit -> Project Settings -> Input System中启用并生成C#类");
        }
    }

    private void OnEnable()
    {
        if (inputActions == null) return;
        inputActions.Gameplay.Enable();
        inputActions.Gameplay.MeleeAttack.performed += OnMeleeAttack;
    }

    private void OnDisable()
    {
        if (inputActions == null) return;
        inputActions.Gameplay.MeleeAttack.performed -= OnMeleeAttack;
        inputActions.Gameplay.Disable();
    }

    private void Update()
    {
        if (isDead || Player.Instance == null || !Player.Instance.status.Alive) return;
        HandleInput();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        Move();
    }

    private void HandleInput()
    {
        // 移动输入
        moveInput = Vector2.zero;
        if (chaosManager != null)
        {
            if (Keyboard.current[chaosManager.GetKeyForAction("MoveUp")]?.isPressed == true) moveInput.y += 1;
            if (Keyboard.current[chaosManager.GetKeyForAction("MoveDown")]?.isPressed == true) moveInput.y -= 1;
            if (Keyboard.current[chaosManager.GetKeyForAction("MoveLeft")]?.isPressed == true) moveInput.x -= 1;
            if (Keyboard.current[chaosManager.GetKeyForAction("MoveRight")]?.isPressed == true) moveInput.x += 1;
        }
        moveInput = moveInput.normalized;

        // 攻击输入
        if (chaosManager != null)
        {
            Key attackKey = chaosManager.GetKeyForAction("Attack");
            KeyControl kc = Keyboard.current[attackKey];
            bool currPressed = kc != null && kc.isPressed;
            if (currPressed && !lastAttackPressed) OnMeleeAttack(new InputAction.CallbackContext());
            lastAttackPressed = currPressed;
        }
    }

    private void Move()
    {
        if (Player.Instance == null) return;
        // 角色翻转
        if (moveInput.x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput.x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
        // 移动
        rb.velocity = moveInput * Player.Instance.moveSpeed;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);
        animator.SetFloat("Speed", moveInput.sqrMagnitude);
        animator.SetBool("isMeleeAttack", isMeleeAttack);
    }

    private void OnMeleeAttack(InputAction.CallbackContext context)
    {
        if (isDead || isMeleeAttack || animator == null) return;
        isMeleeAttack = true;
        animator.SetTrigger("MeleeAttack");
        StartCoroutine(ResetAttackState());
    }

    // 受击回调（供Player调用）
    public void PlayerHurt()
    {
        if (isDead || animator == null) return;
        animator.SetTrigger("hurt");
    }

    // 死亡回调（供Player调用）
    public void PlayerDie()
    {
        if (isDead) return;
        isDead = true;
        animator.SetBool("isDead", true);
        Debug.Log("【系统】玩家判定死亡");
    }

    // 触发混乱系统
    public void OnPlayerHurt()
    {
        if (chaosManager != null) chaosManager.OnPlayerHurt();
    }

    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(attackDuration);
        isMeleeAttack = false;
    }
}