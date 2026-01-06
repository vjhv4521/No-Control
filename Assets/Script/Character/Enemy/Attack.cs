using System.Collections;
using UnityEngine;

public class EnemyHitDetection : MonoBehaviour
{
    [Header("基础设置")]
    public int health = 1;                     // 敌人生命值（默认为1，碰一下就消失）
    public float deathDelay = 0.1f;            // 死亡延迟时间
    public bool destroyOnDeath = true;         // 死亡后是否销毁
    public bool playDeathAnimation = false;    // 是否播放死亡动画
    
    [Header("效果")]
    public GameObject deathEffect;             // 死亡特效
    public AudioClip hitSound;                 // 受击音效
    
    [Header("击退效果")]
    public bool canBeKnockedBack = true;       // 是否可以被击退
    public float knockbackForce = 3f;          // 击退力度
    public float knockbackDuration = 0.1f;     // 击退持续时间
    
    // 内部变量
    private bool isDead = false;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    
    void Awake()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
    }
    
    // 当被攻击时调用
    public void TakeDamage(int damage = 1, Vector2? attackDirection = null)
    {
        if (isDead) return;  // 如果已经死亡，不再处理
        
        health -= damage;
        
        // 播放受击音效
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
        
        // 击退效果
        if (canBeKnockedBack && rb != null && attackDirection.HasValue)
        {
            Vector2 direction = attackDirection.Value.normalized;
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
            StartCoroutine(StopKnockback());
        }
        
        // 闪烁效果（视觉反馈）
        StartCoroutine(FlashEffect());
        
        // 检查是否死亡
        if (health <= 0)
        {
            Die();
        }
    }
    
    // 死亡处理
    private void Die()
    {
        isDead = true;
        
        // 播放死亡动画（如果有）
        if (playDeathAnimation && animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // 生成死亡特效
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // 禁用碰撞体，防止继续被攻击
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        // 延迟销毁或禁用
        StartCoroutine(DieCoroutine());
    }
    
    // 协程：延迟处理死亡
    private IEnumerator DieCoroutine()
    {
        yield return new WaitForSeconds(deathDelay);
        
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
        else
        {
            // 如果不销毁，可以设置为不可见或禁用
            gameObject.SetActive(false);
        }
    }
    
    // 协程：闪烁效果
    private IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;  // 变红表示受伤
        
        yield return new WaitForSeconds(0.1f);
        
        spriteRenderer.color = originalColor;
    }
    
    // 协程：停止击退
    private IEnumerator StopKnockback()
    {
        yield return new WaitForSeconds(knockbackDuration);
        
        if (rb != null && !isDead)
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    // 碰撞检测（如果玩家的攻击有碰撞体）
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家攻击
        if (other.CompareTag("PlayerAttack") || other.CompareTag("Weapon"))
        {
            // 计算攻击方向（从攻击位置指向敌人）
            Vector2 attackDirection = transform.position - other.transform.position;
            TakeDamage(1, attackDirection);
        }
    }
    
    // 碰撞检测（如果玩家的攻击使用碰撞而不是触发器）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查是否是玩家攻击
        if (collision.gameObject.CompareTag("PlayerAttack") || 
            collision.gameObject.CompareTag("Weapon"))
        {
            // 计算攻击方向
            Vector2 attackDirection = transform.position - collision.transform.position;
            TakeDamage(1, attackDirection);
        }
    }
}
// 通用伤害接口
public interface IDamageable
{
    void TakeDamage(int damage, Vector2? attackDirection = null);
}

// 使用接口的简单敌人脚本
public class SimpleEnemy : MonoBehaviour, IDamageable
{
    public int health = 1;
    public GameObject deathEffect;
    
    public void TakeDamage(int damage, Vector2? attackDirection = null)
    {
        health -= damage;
        
        if (health <= 0)
        {
            // 死亡效果
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }
            
            // 销毁敌人
            Destroy(gameObject);
        }
    }
}
