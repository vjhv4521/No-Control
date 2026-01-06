using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 3f;
    private NavMeshAgent navMeshAgent;
    private bool navMeshReady = false;

    [Header("攻击设置")]
    public float attackDamage = 1f;
    public float attackCooldown = 1f;
    public float detectionRange = 10f;

    private Transform target;
    private Collider2D enemyCollider;

    private float lastAttackTime;
    private bool isAttacking;
    private float pathUpdateInterval = 0.5f;
    private float lastPathUpdateTime;

    private void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.isKinematic = true;
        }
    }

    private void Start()
    {
        enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null)
        {
            enemyCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        SetEnemyNoCollision();

        FindPlayer();
        lastAttackTime = -attackCooldown;
        isAttacking = false;
        lastPathUpdateTime = -pathUpdateInterval;

        StartCoroutine(InitializeNavMeshAgent());
    }

    private System.Collections.IEnumerator InitializeNavMeshAgent()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return null;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        navMeshAgent.speed = moveSpeed;
        navMeshAgent.angularSpeed = 120f;
        navMeshAgent.acceleration = 8f;
        navMeshAgent.stoppingDistance = 0f;
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.updateRotation = false;

        NavMeshHit hit;
        float navMeshZ = 0f;

        if (NavMesh.SamplePosition(new Vector3(transform.position.x, transform.position.y, navMeshZ), out hit, 3.0f, NavMesh.AllAreas))
        {
            transform.position = new Vector3(hit.position.x, hit.position.y, navMeshZ);
            navMeshAgent.enabled = true;

            for (int i = 0; i < 5; i++)
            {
                yield return null;
            }

            if (navMeshAgent.isOnNavMesh)
            {
                navMeshReady = true;
            }
            else
            {
                navMeshAgent.enabled = false;
                navMeshReady = false;
                Debug.LogWarning("Enemy未在NavMesh上，位置: " + transform.position);
            }
        }
        else
        {
            navMeshAgent.enabled = false;
            navMeshReady = false;
            Debug.LogWarning("Enemy无法找到NavMesh位置，位置: " + transform.position);
        }
    }

    private void SetEnemyNoCollision()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
        }
    }

    private void Update()
    {
        if (!navMeshReady || navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
        {
            return;
        }

        if (target == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (Time.time - lastPathUpdateTime > pathUpdateInterval)
            {
                navMeshAgent.SetDestination(target.position);
                lastPathUpdateTime = Time.time;
            }
        }
        else
        {
            navMeshAgent.isStopped = true;
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time - lastAttackTime > attackCooldown)
            {
                AttackPlayer();
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time - lastAttackTime > attackCooldown)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        if (isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        if (Player.Instance != null && Player.Instance.status.Alive)
        {
            Player.Instance.TakeDamage(attackDamage);
        }

        Attack attackComponent = GetComponent<Attack>();
        if (attackComponent != null)
        {
            attackComponent.PerformAttack();
        }

        StartCoroutine(EndAttack());
    }

    private IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }
}
