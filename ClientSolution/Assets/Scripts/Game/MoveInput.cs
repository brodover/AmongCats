using System;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class MoveInput : MonoBehaviour
    {
        private Rigidbody rb;
        private Transform st;

        [SerializeField] private float moveSpeed = 13; // default human

        private float acceleration;

        private Vector2 _currentVelocity = Vector2.zero;   // Tracks current velocity

        private Vector2 _moveInput2;
        private InputSystem_Actions _playerInput;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            st = transform.GetChild(0).transform;

            _playerInput = new InputSystem_Actions();

            if (gameObject.CompareTag("Cat"))
                moveSpeed = ClientCommon.Game.CatMovementSpeed;
        }

        void Start()
        {
            _moveInput2 = Vector2.zero;

            acceleration = moveSpeed / ClientCommon.Game.TimeToMaxSpeed;
        }

        void OnEnable()
        {
            if (_playerInput == null)
                _playerInput = new InputSystem_Actions();
            _playerInput.Enable(); // Enable the input system.
        }

        void OnDisable()
        {
            _playerInput.Disable(); // Disable the input system.
        }

        void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            // Read player input
            _moveInput2 = _playerInput.Player.Move.ReadValue<Vector2>();

            // Target velocity based on input
            Vector2 targetVelocity = _moveInput2.normalized * moveSpeed;

            if (_moveInput2 != Vector2.zero)
            {
                // Accelerate towards the target velocity
                _currentVelocity = Vector2.MoveTowards(
                    _currentVelocity,
                    targetVelocity,
                    acceleration * Time.fixedDeltaTime
                );
            }
            else
            {
                // Decelerate when no input is detected
                _currentVelocity = Vector2.MoveTowards(
                    _currentVelocity,
                    Vector2.zero,
                    acceleration * Time.fixedDeltaTime
                );
            }
            // Apply movement using Rigidbody
            rb.MovePosition(rb.position + (Vector3)_currentVelocity * Time.fixedDeltaTime);

            // sprite flip-x direction
            if (_moveInput2.x != 0)
            {
                float direction = Mathf.Sign(_moveInput2.x); // 1 for positive, -1 for negative
                st.localScale = new Vector3(
                    Mathf.Abs(st.localScale.x) * -direction, 
                    st.localScale.y, st.localScale.z);
            }
        }
    }
}