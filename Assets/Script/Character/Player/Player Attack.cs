using System.Collections;
using UnityEngine;
using Game.Character;

public class PlayerAttackDetector : MonoBehaviour
{
    [Header("攻击设置")]
    public float attackDuration = 0.3f;        // 攻击检测持续时间
    public int attackDamage = 10;               // 攻击伤害
    public float attackRange = 0.8f;           // 攻击范围
    
    [Header("攻击效果")]
    public GameObject slashEffect;             // 攻击特效
    public AudioClip attackSound;              // 攻击音效
    
    [Header("攻击检测")]
    public Transform attackPoint;              // 攻击检测点（通常是武器位置）
    public LayerMask enemyLayer;               // 敌人层级
    
    // 内部变量
    private bool isAttacking = false;
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        // 如果没指定攻击点，使用角色位置
        if (attackPoint == null)
        {
            attackPoint = transform;
        }
    }
    
    // 攻击函数（由玩家控制器调用）
    public void Attack()
    {
        if (isAttacking) return;
        
        StartCoroutine(AttackCoroutine());
    }
    
    // 攻击协程
    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        
        // 播放攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 播放攻击音效
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, transform.position);
        }
        
        // 生成攻击特效
        if (slashEffect != null)
        {
            GameObject effect = Instantiate(slashEffect, attackPoint.position, attackPoint.rotation);
            Destroy(effect, 1f); // 1秒后销毁特效
        }
        
        // 等待一小段时间，让动画开始播放
        yield return new WaitForSeconds(0.1f);
        
        // 检测攻击范围内的敌人
        DetectEnemies();
        
        // 等待攻击持续时间结束
        yield return new WaitForSeconds(attackDuration - 0.1f);
        
        isAttacking = false;
    }
    
    // 检测范围内的敌人
    private void DetectEnemies()
    {
        // 强制检测所有层，避免 LayerMask 设置错误导致打不到怪
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        
        Debug.Log($"[攻击检测] 位置:{attackPoint.position}, 半径:{attackRange}, 检测到物体数:{hitColliders.Length}");

        foreach (Collider2D collider in hitColliders)
        {
            // 排除自己
            if (collider.gameObject == gameObject) continue;
            if (collider.transform.root == transform.root) continue;
            
            // 排除玩家阵营（通过 Tag 或 Layer）
            if (collider.CompareTag("Player")) continue;

            Debug.Log($"[攻击检测] 命中: {collider.name}, Tag: {collider.tag}, Layer: {LayerMask.LayerToName(collider.gameObject.layer)}");

            // 尝试获取血量组件
            // 1. 尝试 CharacterBase (新系统)
            CharacterBase targetChar = collider.GetComponentInParent<CharacterBase>();
            if (targetChar != null)
            {
                 Debug.Log($"[攻击检测] -> 找到 CharacterBase: {targetChar.name}, 当前血量: {targetChar.status?.CurHp}");
                 targetChar.TakeDamage(attackDamage);
                 continue; // 处理下一个
            }

            // 2. 尝试 EnemyHitDetection (旧系统)
            EnemyHitDetection enemyHit = collider.GetComponentInParent<EnemyHitDetection>();
            if (enemyHit != null)
            {
                Debug.Log($"[攻击检测] -> 找到 EnemyHitDetection: {enemyHit.name}");
                Vector2 dir = collider.transform.position - transform.position;
                enemyHit.TakeDamage(attackDamage, dir);
            }
        }
    }
    
    // 在Unity编辑器中绘制攻击范围（便于调试）
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
