using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Game
{
    public class MoveInput : MonoBehaviour
    {
        private Rigidbody rb;
        private Transform childT;
        public Button speedBtn = null;

        [SerializeField] 
        private float moveSpeed = ClientCommon.Game.HumanMovementSpeed;

        private float acceleration;
        private bool isSpeedUp = false;
        private float speedUpCooldownTimer = 0f;
        private float speedUpDurationTimer = 0f;

        private Vector2 _currentVelocity = Vector2.zero;   // Tracks current velocity

        private Vector2 _moveInput2;
        private InputSystem_Actions _playerInput;

        private float _tempSpeedMultiplier = 1.7f;
        private float _tempSpeedDuration = 3f;
        private float _tempSpeedCooldown = 7f;
        private Color _selectedColor = new Color(0.95f, 0.59f, 0.03f);

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            childT = transform.GetChild(0).transform;

            _playerInput = new InputSystem_Actions();

            if (gameObject.CompareTag("Cat"))
                moveSpeed = ClientCommon.Game.CatMovementSpeed;
        }

        void Start()
        {
            _moveInput2 = Vector2.zero;

            //acceleration = moveSpeed / ClientCommon.Game.TimeToMaxSpeed;
        }

        public void ChangeSpeed()
        {
            if (isSpeedUp)
            {
                StopSpeedUp();
            }
            else if (speedUpCooldownTimer <= 0f)
            {
                isSpeedUp = true;
                speedUpDurationTimer = _tempSpeedDuration;
                speedBtn.GetComponent<Image>().color = _selectedColor;
            }
            else
            {
                Debug.Log("Speed Up is still on cooldown.");
            }
        }

        private void StopSpeedUp()
        {
            isSpeedUp = false;
            speedUpCooldownTimer = _tempSpeedCooldown;
            speedBtn.interactable = false;
            speedBtn.GetComponent<Image>().color = Color.white;
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

        void Update()
        {
            if (speedUpCooldownTimer > 0f)
            {
                speedUpCooldownTimer -= Time.deltaTime;
                if (speedUpCooldownTimer <= 0f)
                {
                    speedBtn.interactable = true;
                }
            }

            if (speedUpDurationTimer > 0f)
            {
                speedUpDurationTimer -= Time.deltaTime;
                if (speedUpDurationTimer <= 0f)
                {
                    StopSpeedUp();
                }
            }
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
            if (isSpeedUp)
            {
                targetVelocity = targetVelocity * _tempSpeedMultiplier;
            }

            if (_moveInput2 != Vector2.zero)
            {
                // Accelerate towards the target velocity
                /*_currentVelocity = Vector2.MoveTowards(
                    _currentVelocity,
                    targetVelocity,
                    acceleration * Time.fixedDeltaTime
                );*/

                _currentVelocity = targetVelocity;
            }
            else
            {
                // Decelerate when no input is detected
                /*_currentVelocity = Vector2.MoveTowards(
                    _currentVelocity,
                    Vector2.zero,
                    acceleration * Time.fixedDeltaTime
                );*/

                _currentVelocity = Vector2.zero;
                return;
            }
            // Apply movement using Rigidbody
            rb.MovePosition(rb.position + (Vector3)_currentVelocity * Time.fixedDeltaTime);

            // sprite flip-x direction
            if (_moveInput2.x != 0)
            {
                float direction = Mathf.Sign(_moveInput2.x); // 1 for positive, -1 for negative
                childT.localScale = new Vector3(
                    Mathf.Abs(childT.localScale.x) * -direction, 
                    childT.localScale.y, childT.localScale.z);
            }
        }
    }
}