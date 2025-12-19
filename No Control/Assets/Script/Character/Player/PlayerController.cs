using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("控制配置")]
    public float attackDuration = 0.5f;
    public InputChaosManager chaosManager;

    [Header("状态")]
    public bool isDead;
    public bool isMeleeAttack;

    private InputActions inputActions;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private bool lastAttackPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        try { inputActions = new InputActions(); } catch { }
    }

    private void OnEnable()
    {
        if (inputActions != null) {
            inputActions.Gameplay.Enable();
            inputActions.Gameplay.MeleeAttack.performed += OnMeleeAttack;
        }
    }

    private void OnDisable()
    {
        if (inputActions != null) {
            inputActions.Gameplay.MeleeAttack.performed -= OnMeleeAttack;
            inputActions.Gameplay.Disable();
        }
    }

    private void Update()
    {
        // 关键：如果逻辑判定死亡，禁止输入
        if (isDead || Player.Instance == null || !Player.Instance.status.Alive) return;
        HandleInput();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDead) { rb.velocity = Vector2.zero; return; }
        Move();
    }

    private void HandleInput()
    {
        moveInput = Vector2.zero;
        if (chaosManager != null)
        {
            if (Keyboard.current[chaosManager.GetKeyForAction("MoveUp")]?.isPressed == true) moveInput.y += 1;
            if (Keyboard.current[chaosManager.GetKeyForAction("MoveDown")]?.isPressed == true) moveInput.y -= 1;
            if (Keyboard.current[chaosManager.GetKeyForAction("MoveLeft")]?.isPressed == true) moveInput.x -= 1;
            if (Keyboard.current[chaosManager.GetKeyForAction("MoveRight")]?.isPressed == true) moveInput.x += 1;
        }
        moveInput = moveInput.normalized;

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
        if (moveInput.x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput.x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
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
        if (isDead || isMeleeAttack) return;
        isMeleeAttack = true;
        animator.SetTrigger("MeleeAttack");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D enemy in hitEnemies) {
            if (enemy.CompareTag("Enemy")) Destroy(enemy.gameObject);
        }
        StartCoroutine(ResetAttackState());
    }

    public void PlayerHurt() { if (!isDead) animator.SetTrigger("hurt"); }

    public void PlayerDie()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        rb.velocity = Vector2.zero;
    }

    // 复活重置
    public void PlayerRespawn()
    {
        isDead = false;
        animator.SetBool("isDead", false);
        // 强制播放 Idle 动画（请确保名字正确）
        animator.Play("Idle", 0, 0f); 
    }

    public void OnPlayerHurt() => chaosManager?.OnPlayerHurt();
    private IEnumerator ResetAttackState() { yield return new WaitForSeconds(attackDuration); isMeleeAttack = false; }
}