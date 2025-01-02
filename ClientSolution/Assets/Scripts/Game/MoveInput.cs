using System;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class MoveInput : MonoBehaviour
    {
        private Rigidbody rb;
        private Transform spriteTransform;

        [SerializeField] private float moveSpeed = 13; // default human

        private float acceleration;

        private Vector2 _currentVelocity = Vector2.zero;   // Tracks current velocity

        private Vector2 _moveInput2;
        private InputSystem_Actions _playerInput;

        private const int UPDATE_DELAY_MILISECOND = 250; // 50ms = 20hz
        private DateTime _lastUpdate;
        private Vector3 _lastPos;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            spriteTransform = transform.GetChild(0).transform;

            _playerInput = new InputSystem_Actions();

            if (gameObject.CompareTag("Cat"))
                moveSpeed = ClientCommon.Game.CatMovementSpeed;
        }

        private void Start()
        {
            _moveInput2 = Vector2.zero;
            _lastUpdate = DateTime.Now;

            acceleration = moveSpeed / ClientCommon.Game.TimeToMaxSpeed;
        }

        private void OnEnable()
        {
            if (_playerInput == null)
                _playerInput = new InputSystem_Actions();
            _playerInput.Enable(); // Enable the input system.
        }

        private void OnDisable()
        {
            _playerInput.Disable(); // Disable the input system.
        }

        private void Update()
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
                    acceleration * Time.deltaTime
                );
            }
            else
            {
                // Decelerate when no input is detected
                _currentVelocity = Vector2.MoveTowards(
                    _currentVelocity,
                    Vector2.zero,
                    acceleration * Time.deltaTime
                );
            }
            // Apply movement using Rigidbody
            rb.MovePosition(rb.position + (Vector3)_currentVelocity * Time.deltaTime);

            if (_moveInput2.x != 0)
            {
                float direction = Mathf.Sign(_moveInput2.x); // 1 for positive, -1 for negative
                spriteTransform.localScale = new Vector3(Mathf.Abs(spriteTransform.localScale.x) * -direction, spriteTransform.localScale.y, spriteTransform.localScale.z);
            }
        }
    }
}