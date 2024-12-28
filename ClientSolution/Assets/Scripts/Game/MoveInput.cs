using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace Assets.Scripts.Game
{
    public class MoveInput : MonoBehaviour
    {
        public float moveSpeed = 13; // default human
        private Vector2 _moveInput2;
        private Vector3 _moveInput3;
        private InputSystem_Actions _playerInput;

        private Rigidbody rb;
        private SpriteRenderer sr;

        private const int UPDATE_DELAY_MILISECOND = 250; // 50ms = 20hz
        private DateTime _lastUpdate;
        private Vector3 _lastPos;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            sr = GetComponentInChildren<SpriteRenderer>();

            _moveInput2 = Vector2.zero;
            _moveInput3 = Vector3.zero;
            _playerInput = new InputSystem_Actions();
            _lastUpdate = DateTime.Now;

            if (gameObject.CompareTag("Cat"))
                moveSpeed = 20;
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
            _moveInput2 = _playerInput.Player.Move.ReadValue<Vector2>();

            if (_moveInput2.x == 0 && _moveInput2.y == 0)
                return;

            _moveInput3.x = _moveInput2.x;
            _moveInput3.y = _moveInput2.y;
            rb.MovePosition(rb.position + (_moveInput3.normalized * moveSpeed * Time.deltaTime));

            if (sr.flipX && _moveInput3.x < 0)
                sr.flipX = false;
            else if (!sr.flipX && _moveInput3.x > 0)
                sr.flipX = true;

            /*if (DateTime.Now > _lastUpdate)
            {
                await SignalRConnectionManager.Instance.PlayerMove(rb.position.x, rb.position.y, rb.position.z, sr.flipX);
                _lastUpdate = DateTime.Now.AddMilliseconds(UPDATE_DELAY_MILISECOND);
            }*/
        }
    }
}