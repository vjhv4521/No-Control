using UnityEngine;
using Game.Character;

// 敌人攻击组件
public class Attack : MonoBehaviour
{
    [Header("攻击设置")]
    public float attackDamage = 1f;
    public float attackRange = 0.1f;
    public float attackCooldown = 1f;

    [Header("攻击效果")]
    public GameObject attackEffect;
    public AudioClip attackSound;

    private float lastAttackTime;
    private Animator animator;
    private Transform target;

    private void Start()
    {
        animator = GetComponent<Animator>();
        FindPlayer();
    }

    private void Update()
    {
        if (target == null)
        {
            FindPlayer();
        }
    }

    // 攻击方法，供外部调用
    public void PerformAttack()
    {
        // 检查冷却时间
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

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

        // 生成攻击效果
        if (attackEffect != null)
        {
            Instantiate(attackEffect, transform.position, Quaternion.identity);
        }

        // 检测攻击范围内的玩家
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                // 对玩家造成伤害
                if (Player.Instance != null && Player.Instance.status.Alive)
                {
                    Player.Instance.TakeDamage(attackDamage);
                }
            }
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    // 在场景中绘制攻击范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}