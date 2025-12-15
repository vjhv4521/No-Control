using UnityEngine;

namespace Game.Character
{
    public class Enemy : CharacterBase
    {
        [Header("敌人配置")]
        public float moveSpeed = 2f;

        private void Update()
        {
            if (status == null || !status.Alive || Player.Instance == null) return;
            MoveToPlayer();
        }

        private void MoveToPlayer()
        {
            Vector2 direction = (Player.Instance.transform.position - transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public override void SetDead()
        {
            base.SetDead();

            // 死亡特效（空值保护）
            GameObject deadEffectPrefab = Resources.Load<GameObject>("Effects/Prefab/EffectDead");
            if (deadEffectPrefab != null)
            {
                GameObject deadEffect = Instantiate(deadEffectPrefab, transform.position, Quaternion.identity);
                Destroy(deadEffect, 7f);
            }

            // 死亡音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEffect("SFX/Dead");
            }

            // 玩家加经验
            if (Player.Instance != null)
            {
                Player.Instance.Exp += 1;
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (status == null || !status.Alive) return;

            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Player player = other.gameObject.GetComponent<Player>();
                if (player != null && player.status != null && player.status.Alive)
                {
                    player.status.Hit(10);
                    Debug.Log($"{player.gameObject.name} 被敌人攻击，受到10点伤害！");
                }
                SetDead();
            }
        }
    }
}