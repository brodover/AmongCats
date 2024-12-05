using System;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float moveSpeed = 15;
    private Vector3 moveInput;
    private Rigidbody rb;
    private SpriteRenderer sr;

    private const int UPDATE_DELAY_MILISECOND = 100; // 50ms = 20hz
    private DateTime _lastUpdate;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sr = GetComponentInChildren<SpriteRenderer>();
        moveInput = Vector3.zero;
        _lastUpdate = DateTime.Now;
    }

    private async void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        rb.MovePosition(rb.position +
             (moveInput.normalized * moveSpeed *
              Time.deltaTime));

        if (sr.flipX && moveInput.x < 0)
            sr.flipX = false;
        else if (!sr.flipX && moveInput.x > 0)
            sr.flipX = true;

        if (DateTime.Now > _lastUpdate)
        {
            await SignalRConnectionManager.Instance.PlayerMove(transform.position.x, transform.position.y, transform.position.z, sr.flipX);
            _lastUpdate = DateTime.Now.AddMilliseconds(UPDATE_DELAY_MILISECOND);
        }
    }
}