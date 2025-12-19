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
            if (status == null || !status.Alive) return;
            Destroy(gameObject, 1f); 
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