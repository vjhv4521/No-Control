using UnityEngine;

namespace Game.Character
{
    public class Status
    {
        public bool Alive { get; private set; } = true;
        private int maxHp;
        private int curHp;
        private readonly CharacterBase character;

        public int MaxHp
        {
            get => maxHp;
            set
            {
                maxHp = value;
                if (CurHp > MaxHp) CurHp = MaxHp;
                DeadCheck();
            }
        }

        public int CurHp
        {
            get => curHp;
            set
            {
                curHp = value;
                DeadCheck();
            }
        }

        public Status(CharacterBase character)
        {
            if (character == null)
            {
                Debug.LogError("Status初始化失败：CharacterBase实例为null！");
                return;
            }
            this.character = character;
            maxHp = character.MaxHp;
            curHp = maxHp;
        }

        private void DeadCheck()
        {
            if (character == null || !Alive) return;
            if (CurHp <= 0)
            {
                Alive = false; // 自动标记死亡
                character.SetDead(); // 触发角色死亡逻辑
            }
        }

        public void Hit(int damage)
        {
            if (character == null || !Alive) return;
            damage = Mathf.Max(damage, 0);
            CurHp -= damage;
            character.HitEffect();
        }

        public void Respawn()
        {
            if (character == null) return;
            Alive = true;
            curHp = maxHp;
            character.transform.position = Vector3.zero;
            character.gameObject.SetActive(true);
        }
    }
}