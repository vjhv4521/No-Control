using UnityEngine;

namespace Game.Character
{
    public abstract class CharacterBase : MonoBehaviour
    {
        public Status status { get; protected set; }
        [SerializeField] public int MaxHp; 
        protected Material material;

        public virtual void Init()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"[{gameObject.name}] 缺少SpriteRenderer组件！");
                return;
            }
            material = spriteRenderer.material;
            status = new Status(this);
            
            // 动态添加血条组件
            var hpBar = gameObject.GetComponent<HealthBarController>();
            if (hpBar == null) hpBar = gameObject.AddComponent<HealthBarController>();
            hpBar.Init(status);
        }

        public virtual void SetDead()
        {
            // 如果是玩家，只标记死亡，不销毁物体
            if (gameObject.CompareTag("Player"))
            {
                Debug.Log("玩家进入死亡状态，等待复活...");
                return;
            }

            // 普通敌人延迟销毁
            // 注意：此时 status.Alive 已经被 Status.DeadCheck 设为 false 了，所以不能用 !status.Alive 来判断
            // 只要调用了 SetDead，就说明已经死了，直接执行销毁
            Destroy(gameObject, 0.1f); 
        }

        public void HitEffect()
        {
            if (material == null) return;
            material.SetFloat("_Blend", 1f);
            Invoke(nameof(ResetHitEffect), 0.2f);
        }

        private void ResetHitEffect()
        {
            if (material == null) return;
            material.SetFloat("_Blend", 0f);
        }

        public virtual void TakeDamage(float damage)
        {
            if (status == null || !status.Alive) return;
            status.Hit(Mathf.RoundToInt(damage));
        }
    }
}