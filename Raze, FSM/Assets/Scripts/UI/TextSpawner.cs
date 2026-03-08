using MP1.Stats;
using UnityEngine;

namespace MP1.UI
{
    public class TextSpawner : MonoBehaviour
    {
        [SerializeField] GameObject UnbalacneTextPrefab = null;
        [SerializeField] GameObject NoStaminaTextPrefab = null;
        private Balance _balance;
        private Stamina _stamina;

        private bool IsFadeNoStaminaText;

        public void Awake()
        {
            _balance = GetComponentInParent<Balance>();
            if(_balance != null)  _balance.OnUnbalance += SpawnUnbalanceText;

            _stamina = GetComponentInParent<Stamina>();
            if(_stamina != null) _stamina.OnNoStamina += SpawnNoStaminaText;
        }

        public void SpawnUnbalanceText()
        {
            Instantiate(UnbalacneTextPrefab, transform);
        }

        public void SpawnNoStaminaText()
        {
            if(IsFadeNoStaminaText) return;
            Instantiate(NoStaminaTextPrefab, transform).GetComponent<FadeText>().OnDestroyed += DestroyedStaminaText;
            IsFadeNoStaminaText = true;
        }

        public void DestroyedStaminaText()
        {
            IsFadeNoStaminaText = false;
        }
    }
}

