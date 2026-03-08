using System;
using UnityEngine;

namespace MP1.Stats
{
    public class Stamina : MonoBehaviour
    {
        private CharacterStats _characterState;
        [SerializeField] private float _currentSP;
        [SerializeField] private float _recoveryDelayTime = 0.5f;
        [SerializeField] private float _recoveryRate = 10f;

        public event Action OnNoStamina;

        private float _recovertDelayTimer = 0;

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
            _recovertDelayTimer = Mathf.Min(_recovertDelayTimer + Time.deltaTime, _recoveryDelayTime);
            if (_currentSP == GetMaxStamina()) return;
            if (_recovertDelayTimer != _recoveryDelayTime) return;
            _currentSP = Mathf.Min(_currentSP + (_recoveryRate * Time.deltaTime), GetMaxStamina());
        }

        private void InitBalance()
        {
            _currentSP = GetMaxStamina();
        }

        private float GetMaxStamina()
        {
            return _characterState.GetStat(Stat.Stamina);
        }

        public float GetFraction()
        {
            return _currentSP / GetMaxStamina();
        }

        public void UseStamina(float use)
        {
            _currentSP = Mathf.Max(_currentSP - use, 0);
            _recovertDelayTimer = 0;
        }

        public bool CanUseStamina(float use)
        {
            if(_currentSP - use >= 0)
            {
                return true;
            }
            else
            {
                OnNoStamina?.Invoke();
                return false;
            }
        }
    }
}

