using System;
using UnityEngine;

namespace MP1.Stats
{
    public class Balance : MonoBehaviour {
        private CharacterStats _characterState;
        [SerializeField] private float _currentBA;
        [SerializeField] private float _recoveryDelayTime = 0.5f;
        [SerializeField] private float _recoveryRate = 10f;
        [SerializeField] private bool _isUnbalanced;

        public event Action OnUnbalance;
        private float _recoverDelayTimer = 0;
        

        void Awake()
        {
            _characterState = GetComponent<CharacterStats>();
        }

        void Start()
        {
            InitBalance();
        }

        void Update()
        {
            RecoverBalacne();
         
            if(_isUnbalanced == true && _currentBA == GetMaxBalance())
            {
                _isUnbalanced = false;
            }
        }

        public void TakeBalanceDamage(float damage)
        {
            if(_isUnbalanced) return;

            _currentBA = Mathf.Max(_currentBA - damage, 0);
            _recoverDelayTimer = 0;

            if(_currentBA == 0)
            {
                _isUnbalanced = true;
                OnUnbalance?.Invoke();
            }            
        }

        public bool IsUnbalance()
        {
            return _isUnbalanced;
        }

        private void RecoverBalacne()
        {
            _recoverDelayTimer = Mathf.Min(_recoverDelayTimer + Time.deltaTime, _recoveryDelayTime);
            if (_currentBA == GetMaxBalance()) return;
            if (_recoverDelayTimer != _recoveryDelayTime) return;
            _currentBA = Mathf.Min(_currentBA + (_recoveryRate * Time.deltaTime), GetMaxBalance());
        }

        private void InitBalance()
        {
            _currentBA = GetMaxBalance();
        }

        private float GetMaxBalance()
        {
            return _characterState.GetStat(Stat.Balance);
        }

        public float GetFraction()
        {
            return _currentBA / GetMaxBalance();
        }


    }
}