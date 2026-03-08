using UnityEngine;

namespace MP1.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapon", order = 0)]
    public class Weapon : ScriptableObject {
        [SerializeField] public AnimatorOverrideController animatorOverride = null;
        [SerializeField] public GameObject weaponPrefab = null;
        [SerializeField] public float damage = 0f;
        [SerializeField] public float balanceDamage = 0f;
        [SerializeField] public float attackSpeed = 0.5f;
        [SerializeField] public int maxComboCount = 1;
        [SerializeField] public AudioClip[] attackAudioClips;
        [SerializeField] public int staminaCost = 1;
        [SerializeField] public AttackRangeShape attackRangeShape = AttackRangeShape.box;
        [SerializeField] public Vector3 attackRangeOffset;
        [SerializeField] public Vector3 attackRangeSize;
    }
}