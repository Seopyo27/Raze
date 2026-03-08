
using System.Collections.Generic;
using RPG.Stats;
using UnityEngine;
using UnityEngine.TextCore.Text;
namespace MP1.Stats
{
    public class CharacterStats : MonoBehaviour 
    {
        [SerializeField] Progression progression = null;
        [SerializeField] characterClass characterClass;
        private Dictionary<Stat, int> statLevels;

        void Awake()
        {
            statLevels = new Dictionary<Stat, int>();
            InitLevelAll();
        }

        private void InitLevelAll()
        {
            foreach (Stat stat in System.Enum.GetValues(typeof(Stat)))
            {
                statLevels[stat] = 1;
            }
        }

        public float GetStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetStatLevel(stat));
        }

        public int GetStatLevel(Stat stat)
        {
            return statLevels.ContainsKey(stat) ? statLevels[stat] : 0;
        }



        
    }
}