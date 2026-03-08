using System;
using MP1.Actions.Player;
using MP1.Audio;
using MP1.Combat;
using MP1.Control.Core;
using MP1.Event;
using MP1.Stats;
using Unity.Cinemachine;
using UnityEngine;

namespace MP1.Control
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerInputHandler _Input;
        private CharacterController _Controller;
        private PlayerActionManager _playerActionManager;
        private Camera _mainCamera;
        private Animator _animator;
        private TerrainJump _terrainJump;
        private Health _health;
        private Stamina _stamina;
        private Balance _balance;
        private AudioSource _audioSource;
        private CinemachineImpulseSource _impulseSource;

        public PlayerInputHandler Input => _Input;
        public CharacterController Controller => _Controller;
        public Camera MainCamera => _mainCamera;
        public Animator Animator => _animator;
        private TerrainJump TerrainJump => _terrainJump;
        public Health Health => _health;
        public Stamina Stamina => _stamina;
        public Balance Balance => _balance;
        public AudioSource AudioSource => _audioSource;
        public CinemachineImpulseSource ImpulseSource  => _impulseSource;

        [field: Header("기본 설정")]
        [field: SerializeField] public SoundData SoundData { get; private set; }
        [field: SerializeField] public Weapon Weapon { get; private set; }       

        [field: Header("이동")]
        [field: SerializeField] public float CurrentSpeed { get; private set; } = 0; // 현재 속력
        [field: SerializeField] public float Crouchspeed { get; private set; } = 3f; // 앉아 걷기
        [field: SerializeField] public float WalkSpeed { get; private set; } = 5f; // 걷기 속력
        [field: SerializeField] public float SprintSpeed { get; private set; } = 8.5f; // 달리기 속력
        [field: SerializeField] public float CrouchTransitionSpeed { get; private set; } = 1f; // 앉기 전환 속력
        [field: SerializeField] public float RotationSpeed { get; private set; } = 10f; // 방향 전환 속력

        [field: Header("현재 캐릭터 속도 및 중력")]
        [field: SerializeField] public Vector3 VerticalVelocity{ get; set; }
        [field: SerializeField] public Vector3 HorizontalVelocity{ get; set; }
        [field: SerializeField] public float Gravity { get; set; } = -9.8f;

        [field: Header("구르기")]
        [field: SerializeField] public float RollingDistacne { get; private set; } = 0; // 구르기 거리
        [field: SerializeField] public float RollingTime { get; private set; } = 1; // 구르기 시간
        [field: SerializeField] public float RollingStaminaCost { get; private set; } = 10; // 구르기 스태미나 소모량
        [Range(0, 1)]
        [field: SerializeField] public float InvincibleTimeFraction { get; private set; } = 0.5f; // 구르기 무적 시간 비율
        [field: SerializeField] public bool IsInvincible { get; set; } = false; // 현재 무적 상태

        [field: Header("지형 점프")]
        [field: SerializeField] public float TerrainJumpTime { get; private set; } = 1; // 지형 점프 시간
        [field: SerializeField] public float TerrainJumpHeight { get; private set; } = 5; // 지형 점프 높이
        [field: SerializeField] public Vector3 PossibleTerrainJumpPosition { get; private set; } // 현재 이동 가능한 지형 점프 지점
        
        [field: Header("공격")]
        [field: SerializeField] public int CurrentComboCount { get; set; } = 0; // 현재 콤보 수

        [field: Header("캐릭터 상태 판정")]
        [field: SerializeField] public bool IsFalling { get; private set; } // 현재 캐릭터가 떨어지고 았는지 여부
        [field: SerializeField] public bool IsGround { get; private set; } // 현재 캐릭터가 땅을 밟고 있는지 여부
        [field: SerializeField] public float GroundedOffset { get; private set; } = -0.14f; // 땅 판정 구 위치
        [field: SerializeField] public float GroundedRadius { get; private set; } = 0.5f; // 땅 판정 구 반지름
        [field: SerializeField] public LayerMask GroundLayers { get; private set; } // 땅 레이어

        void Awake()
        {
            _mainCamera = Camera.main;
            _Input = GetComponent<PlayerInputHandler>();
            _Controller = GetComponent<CharacterController>();
            _terrainJump = GetComponent<TerrainJump>();
            _health = GetComponent<Health>();
            _stamina = GetComponent<Stamina>();
            _animator = GetComponent<Animator>();
            _balance = GetComponent<Balance>();
            _audioSource = GetComponent<AudioSource>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            
            _playerActionManager = new PlayerActionManager(this);
            _health.OnHit += _playerActionManager.TransitionToPlayerHitAction;
            _health.OnDie += _playerActionManager.TransitionToPlayerDieAction;

            _animator.runtimeAnimatorController = Weapon.animatorOverride;
        }
        
        void Start()
        {
            _playerActionManager.SetStartingAction(_playerActionManager.PlayerIdleAction);
            _playerActionManager.Initialize();
        }

        void Update()
        {
            //상태 판정
            CheckGround();
            CheckFalling();

            //지형 점프 가능 위치 확인
            UpdatePossibleJumpPosition();

            //현재 속도 업데이트
            UpdateCurrentSpeed();

            //공격 콤보 업데이트
            CountAttackCombo();

            //액션 업데이트 진행
            _playerActionManager.Update();

            //중력 적용
            ApplyGravity();
            
            //최종 움직임
            MovePlayer();
        }

        private void CountAttackCombo()
        {
            if (_playerActionManager.currentAction != _playerActionManager.PlayerAttackAction) CurrentComboCount = 0;
        }

        // ──────────────────────────────────────────
        // 애니메이션 이벤트
        // ──────────────────────────────────────────
        public void OnEvent(string eventName)
        {
            var currentAction = _playerActionManager.currentAction;
            if (currentAction is IAnimationEventReceiver receiver)
            {
                receiver.OnAnimationEvent(eventName);
            }
        }

        // ──────────────────────────────────────────
        // 이동, 회전 및 유틸
        // ──────────────────────────────────────────
        public void InitHorizonVelocity() => HorizontalVelocity = Vector3.zero;
        public void InitVerticalVelocity() => VerticalVelocity = Vector3.zero;

        private void MovePlayer()
        {
            Vector3 finalVelocity = VerticalVelocity + HorizontalVelocity;
            _Controller.Move(finalVelocity * Time.deltaTime);
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

        private void ApplyGravity()
        {
            if (_playerActionManager.currentAction == _playerActionManager.PlayerTerrainJumpAction) return;

            if (IsGround && VerticalVelocity.y < 0)
            {
                VerticalVelocity = new Vector3(0f, -10f, 0f);
            }
            else
            {
                VerticalVelocity += new Vector3(0f, Gravity * Time.deltaTime, 0f);
            }
        }
        
        private void UpdateCurrentSpeed()
        {
            if (_Input.Sprint) CurrentSpeed = SprintSpeed;
            else if(_Input.Crouch) CurrentSpeed = Crouchspeed;
            else  CurrentSpeed = WalkSpeed;
        }

        private void UpdatePossibleJumpPosition()
        {
            Vector3 climbJumpPosition = _terrainJump.GetClimbJumpPosition();
            Vector3 leapJumpPosition = _terrainJump.GetLeapJumpPosition();

            if(climbJumpPosition != Vector3.zero) PossibleTerrainJumpPosition = climbJumpPosition;
            else if(leapJumpPosition != Vector3.zero) PossibleTerrainJumpPosition = leapJumpPosition;
            else PossibleTerrainJumpPosition = Vector3.zero;
        }

        // ──────────────────────────────────────────
        // 상태 판정
        // ──────────────────────────────────────────
        private void CheckGround()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            IsGround = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void CheckFalling()
        {
            IsFalling = !IsGround && VerticalVelocity.y < 0;
        }

        // ──────────────────────────────────────────
        // 시각화
        // ──────────────────────────────────────────
        private void DrawGroundGizoms()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (IsGround) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;


            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }

        private void OnDrawGizmos()
        {
            DrawGroundGizoms();
            AttackRange.DrawAttackGizmos(transform, Weapon.attackRangeOffset, Weapon.attackRangeSize, Color.red);
        }
    }
}

