using System.Collections.Generic;
using RPG.Stats;
using UnityEngine;

namespace MP1.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]

    public class Progression : ScriptableObject
    {
        [SerializeField] ProgressionCharacterClass[] characterClasses = null;

        [System.Serializable]
        class ProgressionCharacterClass
        {
            public characterClass characterClass;
            public ProgressionStat[] stats;
        
        }

        [System.Serializable]
        class ProgressionStat
        {
            public Stat stat;
            public float[] levels;
        }

        Dictionary<characterClass, Dictionary<Stat, float[]>> lookupTable = null;

        public float GetStat(Stat stat, characterClass characterClass, int level)
        {
            BuildLookup();
            if(level <= 0 || GetMaxLevels(stat, characterClass) < level)
            {
                return 0;
            }

            return lookupTable[characterClass][stat][level-1];
        }


        private void BuildLookup()
        {
            if(lookupTable != null) return;

            lookupTable = new Dictionary<characterClass, Dictionary<Stat, float[]>>();

            foreach (ProgressionCharacterClass progressionClass in characterClasses)
            {
                var statLookupTable = new Dictionary<Stat, float[]>();

                foreach (ProgressionStat progressionStat in progressionClass.stats)
                {
                    statLookupTable[progressionStat.stat] = progressionStat.levels;
                }

                lookupTable[progressionClass.characterClass] = statLookupTable;
            }
        }

        public int GetMaxLevels(Stat stat, characterClass characterClass)
        {
            BuildLookup();

            float[] levels = lookupTable[characterClass][stat];
            return levels.Length;
        }




    }
    
}