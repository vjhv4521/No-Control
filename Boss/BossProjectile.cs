using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    // 追踪设置
    public Transform target;
    public float speed = 5f;
    public float acceleration = 1f;
    public float turnSpeed = 5f;
    private float currentSpeed = 0f;

    // 生命周期设置
    public float lifetime = 3f;
    private float spawnTime = 0f;

    // 伤害设置
    public float damage = 25f;

    // 组件引用
    private Rigidbody2D rb;
    private Collider2D projectileCollider;

    // 外部引用
    [System.NonSerialized] // 避免序列化问题
    public GameObject bossController;

    // 碰撞设置
    public LayerMask collisionLayers;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        spawnTime = Time.time;
        currentSpeed = speed * 0.5f; // 初始速度

        // 默认碰撞层设置
        if (collisionLayers.value == 0)
        {
            collisionLayers = LayerMask.GetMask("Player", "Wall");
        }

        // 确保碰撞体启用
        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
        }
    }

    private void Update()
    {
        // 检查生命周期
        if (Time.time - spawnTime >= lifetime)
        {
            DestroyProjectile();
            return;
        }

        // 追踪玩家
        if (target != null)
        {
            ChaseTarget();
        }
        else
        {
            // 如果目标丢失，继续向前移动
            if (rb != null)
            {
                rb.velocity = transform.right * currentSpeed;
            }
        }

        // 平滑加速
        if (currentSpeed < speed)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, speed);
        }
    }

    // 追踪目标
    private void ChaseTarget()
    {
        Vector2 direction = (target.position - transform.position).normalized;

        // 平滑转向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // 移动
        if (rb != null)
        {
            rb.velocity = transform.right * currentSpeed;
        }
        else
        {
            transform.position += transform.right * currentSpeed * Time.deltaTime;
        }
    }

    // 碰撞检测
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 对玩家造成伤害
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                DestroyProjectile();
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // 碰到墙壁，销毁
            DestroyProjectile();
        }
    }

    // 销毁投射物
    private void DestroyProjectile()
    {
        // 可以添加粒子效果或音效
        Destroy(gameObject);
    }

    // 绘制Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        if (target != null)
        {
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}