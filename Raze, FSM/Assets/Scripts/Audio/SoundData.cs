using UnityEngine;
namespace MP1.Audio
{
    [CreateAssetMenu(fileName = "NewSoundData", menuName = "Sound/SoundData")]
    public class SoundData : ScriptableObject
    {
        public AudioClip[] hitSounds;      // 피격음
        public AudioClip[] attackSounds;
        public AudioClip deathSound;    // 죽는 소리
        public AudioClip[] footSounds;
        public AudioClip JumpSound;
        public AudioClip BattleCry;
    }
}

