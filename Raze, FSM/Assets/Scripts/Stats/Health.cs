using System;
using UnityEngine;

namespace MP1.Stats
{
    public class Health : MonoBehaviour
    {
        private CharacterStats _characterState;
        [SerializeField] private float currentHP;
        [SerializeField] private bool isDead;

        public event Action OnHit;
        public event Action OnDie;
        

        void Awake()
        {
            _characterState = GetComponent<CharacterStats>();
        }

        void Start()
        {
            InitCurrentHp();
        }

        public void InitCurrentHp()
        {
            currentHP = GetMaxHealth();
        }

        public float GetMaxHealth()
        {
            return _characterState.GetStat(Stat.Health);
        }

        public void TakeDamage(float damage)
        {
            if(isDead) return;
            
            currentHP = Mathf.Max(currentHP - damage, 0);

            if(currentHP == 0)
            {
                Die();
            }
            else
            {
                OnHit?.Invoke();
            }
        }

        public bool IsDead()
        {
            return isDead;
        }

        public float GetFraction()
        {
            return currentHP / GetMaxHealth();
        }

        private void Die()
        {
            if(isDead) return;

            isDead = true;
            OnDie?.Invoke();
        }
        
    }




}

