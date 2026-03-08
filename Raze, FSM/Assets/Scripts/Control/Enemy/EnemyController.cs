using MP1.Actions.Enemy;
using MP1.Audio;
using MP1.Combat;
using MP1.Control.Core;
using MP1.Event;
using MP1.Stats;
using RPG.Control;
using UnityEngine;
using UnityEngine.AI;

namespace MP1.Control.Enemey
{
    public class EnemyController :MonoBehaviour, IHearable {

        private EnemyActionManager _enemyActionManager;
        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private AudioSource _audioSource;
        private Health _health;
        private Balance _balance;

        public NavMeshAgent NavMeshAgent => _navMeshAgent;
        public Animator Animator => _animator;
        public AudioSource AudioSource => _audioSource;
        public Health Health => _health;
        public Balance Balance => _balance;

        [field: Header("기본 설정")]
        [field: SerializeField] public Weapon Weapon { get; private set; } // 무기 정보
        [field: SerializeField] public Vision Vision { get; private set; } // 시야 범위에 대한 값
        [field: SerializeField] public SoundData soundData { get; private set; } // 상황에 따른 오디오 클립 정보
        public Health targetPlayer;

        [field: Header("이동")]
        [field: SerializeField] public float MaxSpeed { get; private set; } = 5; // 최대 속력
        [Range(0, 1)]
        [field: SerializeField] public float ChaseSpeedFraction { get; private set; } = 0.7f; // 추격 속력
        [Range(0, 1)]
        [field: SerializeField] public float PatrolSpeedFraction { get; private set; } = 0.5f; // 정찰 속력
        [Range(0, 1)]
        [field: SerializeField] public float ConfrontationSpeedFraction { get; private set; } = 0.2f; // 대치 속력
        [Range(0, 1)]
        [field: SerializeField] public float AttackMoveSpeedFraction { get; private set; } = 1f; // 공격 하기위해 붙는 속력
        [field: SerializeField] public float RotationSpeed { get; private set; } = 3f; // NavMeshAgent가 아닌 캐릭터를 수동으로 회전시킬 떄 속도

        [field:Header("순찰")]
        [field: SerializeField] public PatrolPath PatrolPath { get; private set; } // 순찰 경로
        [field: SerializeField] public float WaypointTolerance { get; private set; } = 3; // 순찰시 웨이포인트 접근 오차
        [field: SerializeField] public float WaypintStayTime { get; private set; } = 3; // 각 웨이포인트에 머무르는 시간
        
        [field: Header("추격")]
        [field: SerializeField] public float MinimumChasingTime { get; private set; } = 5; // 이 시간만큼 플레이어를 무조건 추격
        [field: SerializeField] public float SuspicionTime { get; private set; } = 5; // 플레이어가 보이지 않을때 복귀 전 의심하는 시간
        
        [field: Header("대치")]
        [field: SerializeField] public float ConfrontationRadius { get; private set; } = 3f; // 이 거리만큼 플레이어와 거리를 유지

        // 최소시간~최대시간 범위내에서 랜덤으로 거리를 유지하다가 공격을 시도
        [field: SerializeField] public float ConfrontationMinimumTIme { get; private set; } = 1f; // 최소 이 시간만큼 거리유지
        [field: SerializeField] public float ConfrontationMaximumTIme { get; private set; } = 5f; // 최대 이 시간만큼 거리유지
        
        // 거리를 유지할 때 최소시간~최대시간 범위내에서 랜덤으로 이동 방향을 유지하다가 이동 방향 전환
        [field: SerializeField] public float StrafeChangeIntervalMinimumTime { get; private set; } = 1f; // 최소 이 시간만큼 방향유지
        [field: SerializeField] public float StrafeChangeIntervalMaximumTime { get; private set; } = 5f; // 최대 이 시간만큼 방향유지

        [field: Header("전투의 함성 범위")]
        [field: SerializeField] public float BattleCryRadius { get; private set; } = 10f; // 전투의 함성 범위

        [field: Header("청각 상태")]
        [field: SerializeField] public bool HasHeardFootSound { get; private set; } // 발자국 소리를 들었는지 여부
        [field: SerializeField] public bool HasHeardBattleCrySound { get; private set; } // 동료의 전투의 함성 소리를 들었는지 여부

        void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _health = GetComponent<Health>();
            _balance = GetComponent<Balance>();
            _audioSource = GetComponent<AudioSource>();

            _enemyActionManager = new EnemyActionManager(this);
            _health.OnHit += _enemyActionManager.TransitionToEnemyHitAction;
            _health.OnDie += _enemyActionManager.TransitionToEnemyDieAction;

            _animator.runtimeAnimatorController = Weapon.animatorOverride;
        }

        void Start()
        {
            targetPlayer = GameObject.FindWithTag("Player").GetComponent<Health>();

            _enemyActionManager.SetStartingAction(_enemyActionManager.EnemyPatrolAction);
            _enemyActionManager.Initialize();
        }

        void Update()
        {
            _enemyActionManager.Update();
        }

        // ──────────────────────────────────────────
        // 애니메이션 이벤트
        // ──────────────────────────────────────────
        public void OnEvent(string eventName)
        {
            var currentAction = _enemyActionManager.currentAction;
            if (currentAction is IAnimationEventReceiver receiver)
            {
                receiver.OnAnimationEvent(eventName);
            }
        }

        // ──────────────────────────────────────────
        // 이동, 회전 및 유틸
        // ──────────────────────────────────────────
        public void SetSpeed(float speedFraction)
        {
            NavMeshAgent.speed = MaxSpeed * Mathf.Clamp01(speedFraction);
        }

        public void RotateCharacter(Vector3 direction, float rotationSpeed)
        {
            direction.y = 0;
            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float angle = Quaternion.Angle(transform.rotation, targetRotation);

            if (angle < 1.0f) 
            {
                transform.rotation = targetRotation;
            }
            else 
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        public bool IsLookAtDirection(Vector3 direction, float threshold = 0.95f)
        {
            if (direction.sqrMagnitude < 0.01f) return true;

            direction.y = 0;
            Vector3 targetDir = direction.normalized;
            Vector3 forward = transform.forward;

            return Vector3.Dot(forward, targetDir) > threshold;
        }

        public void Despawn(float delayTime = 0)
        {
            Destroy(gameObject, delayTime);
        }
        
        // ──────────────────────────────────────────
        // 청각 인터페이스
        // ──────────────────────────────────────────
        public void OnHearFootSound() =>  HearFootSound(); 
        public void OnHearBattleCrySound() => HearBattleCrySound();
        public void HearFootSound() => HasHeardFootSound = true;
        public void HearBattleCrySound() => HasHeardBattleCrySound = true;
        public void ResetFootSoundState() =>  HasHeardFootSound = false;
        public void ResetBattleCrySoundState() => HasHeardBattleCrySound = false;


        // ──────────────────────────────────────────
        // 시각화
        // ──────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            Vision.DrawGizmos(transform); //시야 범위
            AttackRange.DrawAttackGizmos(transform, Weapon.attackRangeOffset, Weapon.attackRangeSize, Color.red); //공격 범위
        }
    }
}