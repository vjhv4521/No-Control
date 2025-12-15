using UnityEngine;

namespace Game.Character
{
    public abstract class CharacterBase : MonoBehaviour
    {
        public Status status { get; protected set; }
        [SerializeField] public int MaxHp; // 与Status的MaxHp映射
        protected Material material;

        // 初始化：空值校验 + 状态系统初始化
        public virtual void Init()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError($"[{gameObject.name}] 缺少SpriteRenderer组件！");
                return;
            }
            material = spriteRenderer.material;
            status = new Status(this); // 基类统一初始化Status
        }

        // 死亡逻辑：适配Status的DeadCheck自动标记
        public virtual void SetDead()
        {
            if (status == null || !status.Alive) return;
            Destroy(gameObject, 1f); // 延迟销毁保证特效/音效执行
        }

        // 受击特效：空值保护
       // 受击特效：移除DoTween，改为简单的颜色闪烁（无插件版）
        public void HitEffect()
        {
            if (material == null) return;
            // 替代方案：直接设置材质透明度（或颜色），无需渐变
            material.SetFloat("_Blend", 1f); // 显示受击效果
            Invoke(nameof(ResetHitEffect), 0.2f); // 延迟恢复
        }

        // 恢复材质默认状态
        private void ResetHitEffect()
        {
            if (material == null) return;
            material.SetFloat("_Blend", 0f);
        }

        // 补充Player调用的TakeDamage方法（核心逻辑不变）
        public virtual void TakeDamage(float damage)
        {
            if (status == null || !status.Alive) return;
            status.Hit(Mathf.RoundToInt(damage)); // 映射到Status的Hit方法
        }
    }
}