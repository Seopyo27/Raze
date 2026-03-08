using UnityEngine;
using MP1.Actions.Core;
using MP1.Control;
using MP1.Combat;
using MP1.Control.Core;

namespace MP1.Actions.Player
{
    public class PlayerMoveAction : IAction
    {
        private PlayerController _player;
        private PlayerActionManager _playerActionManager;

        public PlayerMoveAction(PlayerActionManager playerActionManager)
        {
            _playerActionManager = playerActionManager;
            _player = playerActionManager.Player;
        }

        private float _stepTimer;

        public void Enter()
        {
            PlayMoveAnimation();
        }

        public void Update()
        {
            UpdateMoveSpeedValueForAnimation();
            PlayFootStepSoundBySpeed(_player.CurrentSpeed, _player.SprintSpeed);
            _playerActionManager.UpdateCrouchLayerWeight();
            _playerActionManager.SwapFallingAnimation("Move");

            Vector3 moveVelocity = CalMoveVelocity();
            _player.RotateCharacter(moveVelocity, _player.RotationSpeed);
            ApplyMoveVelocity(moveVelocity);

            if(_playerActionManager.CheckUnbalanceActionAndTransition()) return;
            if(_playerActionManager.CheckAttackActionAndTransition()) return;
            if(_playerActionManager.CheckRollingActionAndTransition()) return;
            if(_playerActionManager.CheckIdleActionAndTransition()) return;
            if(_playerActionManager.CheckTerrainJumpActionAndTransition()) return;
        }

        public void Exit()
        {
            _player.InitHorizonVelocity();
          
        }

        private void PlayMoveAnimation()
        {
            _player.Animator.CrossFade("Move", 0.1f);
        }

        private void UpdateMoveSpeedValueForAnimation()
        {
            float moveSpeed = _player.Input.Sprint ? 1f : 0f;
            _player.Animator.SetFloat("moveSpeed", moveSpeed, 0.1f, Time.deltaTime);
        }

        private Vector3 CalMoveVelocity()
        {
            Transform camTransform = _player.MainCamera.transform;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            
            Vector3 direction = (camRight.normalized * _player.Input.Move.x + camForward.normalized * _player.Input.Move.y).normalized;
            
            return direction * _player.CurrentSpeed;
        }

        private void ApplyMoveVelocity(Vector3 moveVelocity)
        {
            _player.HorizontalVelocity = moveVelocity;
        }

        public void PlayFootStepSoundBySpeed(float currentSpeed, float sprintSpeed)
        {
            if (currentSpeed < 0.1f) return;

            float ratio = currentSpeed / sprintSpeed;
    
            float interval = Mathf.Lerp(0.7f, 0.5f, ratio);

            _stepTimer += Time.deltaTime;
            if (_stepTimer >= interval)
            {
                PlayFootstepSound();
                NoiseFootStep(GetNoiseRadius());
                _stepTimer = 0;
            }
        }

        private void PlayFootstepSound()
        {
           
            if (_player.SoundData?.footSounds == null || _player.SoundData.footSounds.Length == 0) return;
            int index = Random.Range(0, _player.SoundData.footSounds.Length);
            _player.AudioSource.PlayOneShot(_player.SoundData.footSounds[index]);
        }

        private float GetNoiseRadius()
        {
            if(_player.CurrentSpeed == _player.Crouchspeed)
            {
                return 1.0f;
            }
            else if(_player.CurrentSpeed == _player.WalkSpeed)
            {
                return 5.0f;
            }
            else if(_player.CurrentSpeed == _player.SprintSpeed)
            {
                return 7.0f;
            }
            else
            {
                return 0f;
            } 
        }

        private void NoiseFootStep(float radius)
        {
            Collider[] colliders = Physics.OverlapSphere(_player.transform.position, radius, LayerMask.GetMask("Enemy"));

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<IHearable>(out var enemy))
                {
                    enemy.OnHearFootSound();
                }
            }
        }
    }
}