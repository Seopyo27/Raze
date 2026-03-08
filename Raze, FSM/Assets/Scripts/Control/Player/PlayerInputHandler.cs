using UnityEngine;
using UnityEngine.InputSystem;

namespace MP1.Control
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [field: SerializeField] public Vector2 Move { get; set; }
        [field: SerializeField] public bool Sprint { get; set; }
        [field: SerializeField] public bool Rolling { get; set; }
        [field: SerializeField] public bool Attack { get; set; }
        [field: SerializeField] public bool Crouch { get; set; } = false;

        private void OnMove(InputValue value)
        {
            Move = value.Get<Vector2>();
        }

        private void OnSprint(InputValue value)
        {
            Sprint = value.isPressed;
            InitCrouch();
        }

        private void OnRolling(InputValue value)
        {
            Rolling = value.isPressed;
            InitCrouch();
        }

        private void OnAttack(InputValue value)
        {
            Attack = value.isPressed;
            InitCrouch();
        }

        private void OnCrouch(InputValue value)
        {
            Crouch = !Crouch;
            InitSprint();
        }

        private void InitSprint()
        {
            if (Sprint) Sprint = false;
        }

        private void InitCrouch()
        {
            if (Crouch) Crouch = false;
        }

    }
}
