using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerController : MonoBehaviour
{
    private InputActions inputActions; 
    
    [Header("Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput; 

    // 状态标记
    // ❗ 重点：请在 Inspector 中确保这个变量没有被勾选
    [SerializeField] public bool isMeleeAttack; 

    private void Awake()
    {
        inputActions = new InputActions();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 【新增日志 A】：检查 isMeleeAttack 的初始状态
        Debug.Log("【初始化检查】isMeleeAttack 初始值为: " + isMeleeAttack); 
    }

    private void OnEnable()
    {
        inputActions.Gameplay.Enable();
        inputActions.Gameplay.MeleeAttack.started += OnMeleeAttack;
    }

    private void OnDisable()
    {
        inputActions.Gameplay.MeleeAttack.started -= OnMeleeAttack;
        inputActions.Gameplay.Disable();
    }

    private void Update()
    {
        moveInput = inputActions.Gameplay.Move.ReadValue<Vector2>();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (moveInput.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);

        rb.velocity = moveInput * moveSpeed;
    }

    private void UpdateAnimation()
    {
        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);
        animator.SetFloat("Speed", moveInput.sqrMagnitude);
        
        animator.SetBool("isMeleeAttack", isMeleeAttack);
    }

    // 攻击回调方法（核心调试区域）
    private void OnMeleeAttack(InputAction.CallbackContext context)
    {
        // 【新增日志 B】：确认输入事件被触发
        Debug.Log("【输入检测】攻击按键被按下。"); 

        if (!isMeleeAttack)
        {
            // 【新增日志 C】：确认状态锁被解除
            Debug.Log("【状态解锁】isMeleeAttack 为 False，指令可以发送。"); 
            
            // 确保 animator 存在，避免空引用错误
            if (animator != null)
            {
                animator.SetTrigger("MeleeAttack");
                // 【新增日志 D】：确认 SetTrigger 被执行
                Debug.Log("【指令发送】成功发送 SetTrigger(\"MeleeAttack\")。"); 
            }
            else
            {
                Debug.LogError("Animator 组件丢失，无法触发动画！");
                return; // 如果组件丢失，直接返回，避免后续错误
            }

            isMeleeAttack = true;
            
            // 协程重置状态
            StartCoroutine(ResetAttackState(0.5f)); 
            // 【新增日志 E】：确认协程启动指令已发送
            Debug.Log("【协程启动】ResetAttackState 已发送启动指令。"); 
        }
        else
        {
            // 【新增日志 F】：检查是否被状态锁阻止
            Debug.LogWarning("【状态锁定】攻击指令被忽略，isMeleeAttack 状态为 True。");
        }
    }

    // 协程重置状态（核心调试区域）
    IEnumerator ResetAttackState(float time)
    {
        // 【新增日志 G】：确认协程开始运行
        Debug.Log("【协程运行】协程开始执行。等待 " + time + " 秒...");
        
        yield return new WaitForSeconds(time);
        
        isMeleeAttack = false;
        
        // 【新增日志 H】：确认协程运行结束，状态重置
        Debug.Log("【协程重置】isMeleeAttack 已重置为 False。");
    }
}