using MP1.Stats;
using UnityEngine;

namespace MP1.UI
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Health healthComponent = null;
        [SerializeField] private RectTransform foreGround = null;
        [SerializeField] Canvas rootCanvas = null;

        void Update()
        {
            if(Mathf.Approximately(healthComponent.GetFraction(), 0)
            || Mathf.Approximately(healthComponent.GetFraction(), 1))
            {
                rootCanvas.enabled = false;
                return;
            }
            
            rootCanvas.enabled = true;
            foreGround.localScale = new Vector3(healthComponent.GetFraction(), 1, 1);
        }
    }

}
