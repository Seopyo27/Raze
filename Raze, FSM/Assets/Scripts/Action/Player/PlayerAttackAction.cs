using UnityEngine;
using MP1.Control;
using UnityEngine.InputSystem;
using MP1.Combat;
using MP1.Stats;
using MP1.Actions.Core;
using MP1.Event;

namespace MP1.Actions.Player
{
    public class PlayerAttackAction : IAction, IAnimationEventReceiver
    {
        private PlayerController _player;
        private PlayerActionManager _playerActionManager;
        private float _attackMoveTime = 0f;
        private const float AttackMoveDuration = 0.3f; 
        private const float AttackMoveSpeed = 8f;    
        private bool _isHit; 
        private bool _isFinishAttack;
        private bool _isSlowMotion;

        public PlayerAttackAction(PlayerActionManager playerActionManager)
        {
            _playerActionManager = playerActionManager;
            _player = playerActionManager.Player;
        }

        public void Enter()
        {
            ResetAttackActionValues();
            UseStaminaForAttack();

            if (_player.CurrentComboCount >= 3)
            {
                StartSlowMotion();
            }

            PlayAttackAnimation();
            _player.AudioSource.PlayOneShot(_player.Weapon.attackAudioClips[_player.CurrentComboCount-1]);
        }

        public void Update()
        {

            if (_isSlowMotion && _isHit)
            {
                StopSlowMotion();
            }

            _player.RotateCharacter(GetLookDirection(_player.transform, _player.MainCamera), _player.RotationSpeed);
            ApplyAttackMoveVelocity();
            _playerActionManager.UpdateCrouchLayerWeight();
            
            if (_playerActionManager.CheckUnbalanceActionAndTransition()) return;
            if (!_isFinishAttack) return;
            if (_playerActionManager.CheckAttackActionAndTransition()) return;
            if (_playerActionManager.CheckIdleActionAndTransition()) return;
            if (_playerActionManager.CheckRollingActionAndTransition()) return;
            if (_playerActionManager.CheckMoveActionAndTransition()) return;
            if (_playerActionManager.CheckTerrainJumpActionAndTransition()) return;
        }

        public void Exit()
        {
            StopSlowMotion();
            if (_player.CurrentComboCount >= _player.Weapon.maxComboCount) _player.CurrentComboCount = 0;
            _player.InitHorizonVelocity();
        }

        public void OnAnimationEvent(string eventName)
        {
            if (eventName == "FinishAttack") FinishAttack();
            else if (eventName == "Hit")
            {
                _isHit = true;
                ShakeCamera(_player.CurrentComboCount);
                ScanAttackRange();
            }
        }

        private void ResetAttackActionValues()
        {
            _player.CurrentComboCount += 1;
            _attackMoveTime = AttackMoveDuration;
            _isFinishAttack = false;
            _isHit = false;
            _isSlowMotion = false;
        }

        private void UseStaminaForAttack()
        {
            _player.Stamina.UseStamina(_player.Weapon.staminaCost);
        }

        private void PlayAttackAnimation()
        {
             _player.Animator.CrossFade("Attack" + _player.CurrentComboCount, 0.1f);
        }

        private void StartSlowMotion()
        {
            _isSlowMotion = true;
            Time.timeScale = 0.3f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            if (_player.AudioSource != null)
            {
                _player.AudioSource.pitch = Time.timeScale;
            }
        }

        private void StopSlowMotion()
        {
            if (!_isSlowMotion) return;
            _isSlowMotion = false;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            if (_player.AudioSource != null)
            {
                _player.AudioSource.pitch = 1.0f;
            }
        }

        private void ApplyAttackMoveVelocity()
        {
            if (IsPlayerInAttackRange()) 
            {
                _player.HorizontalVelocity = Vector3.zero;
                return;
            }

            if (_attackMoveTime <= 0f) return;
            _attackMoveTime -= Time.deltaTime;
            float ratio = _attackMoveTime / AttackMoveDuration;          
            float speed = AttackMoveSpeed * ratio;                         
            _player.HorizontalVelocity = _player.transform.forward * speed;
        }

        private bool IsPlayerInAttackRange()
        {
            Collider[] colliders = AttackRange.GetEnemiesInBox(_player.transform, _player.Weapon.attackRangeOffset, _player.Weapon.attackRangeSize, LayerMask.GetMask("Enemy"));
            if (colliders.Length > 0)
            {
                return true;
            }
            return false;
        }

        private void ShakeCamera(int comboIndex)
        {
            if (_player.ImpulseSource == null) return;

             Vector3 shakeDirection = _player.transform.forward;
             

            if (comboIndex < 3)
            {
                _player.ImpulseSource.GenerateImpulseWithVelocity(shakeDirection * 0.1f);
            }
            else
            {
                _player.ImpulseSource.GenerateImpulseWithVelocity(shakeDirection * 0.2f);
            }
        }

        public void FinishAttack()
        {
            _isFinishAttack = true;
        }

        public void ScanAttackRange()
        {
            Collider[] colliders = AttackRange.GetEnemiesInBox(_player.transform, _player.Weapon.attackRangeOffset, _player.Weapon.attackRangeSize, LayerMask.GetMask("Enemy"));
            foreach(Collider collider in colliders)
            {
                Health enemyHealth = collider.GetComponent<Health>();
                Balance enemyBalance = collider.GetComponent<Balance>();
                
                if(enemyHealth != null)
                {
                    enemyHealth.TakeDamage(_player.Weapon.damage);
                }

                if(enemyBalance != null)
                {
                    enemyBalance.TakeBalanceDamage(_player.Weapon.balanceDamage);
                }
                
            }
        }

        private Vector3 GetLookDirection(Transform character, Camera mainCamera)
        {
            if (Physics.Raycast(GetMouseRay(mainCamera), out RaycastHit hit))
            {
                Vector3 dir = hit.point - character.position;
                dir.y = 0;
                return dir;
            }
            return Vector3.zero;
        }

        private Ray GetMouseRay(Camera mainCamera)
            => mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
    }
}