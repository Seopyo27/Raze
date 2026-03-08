using MP1.Stats;
using UnityEngine;

namespace MP1.UI
{
    public class PlayerStaminaBar : MonoBehaviour
    {
        [SerializeField] private Stamina staminaComponent = null;
        [SerializeField] private RectTransform foreGround = null;

        void Update()
        {
            foreGround.localScale = new Vector3(staminaComponent.GetFraction(), 1, 1);
        }
    }

}
