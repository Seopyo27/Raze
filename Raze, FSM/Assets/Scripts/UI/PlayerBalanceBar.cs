using MP1.Stats;
using UnityEngine;

namespace MP1.UI
{
    public class PlayerBalanceBar : MonoBehaviour
    {
        [SerializeField] private Balance balanceComponent = null;
        [SerializeField] private RectTransform foreGround = null;


        void Update()
        {
            foreGround.localScale = new Vector3(balanceComponent.GetFraction(), 1, 1);
        }
    }

}
