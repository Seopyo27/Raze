using MP1.Stats;
using UnityEngine;

namespace MP1.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Health healthComponent = null;
        [SerializeField] private RectTransform foreGround = null;

        void Update()
        {
            foreGround.localScale = new Vector3(healthComponent.GetFraction(), 1, 1);
        }
    }

}
