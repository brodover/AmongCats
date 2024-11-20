using UnityEngine;

public class Move : MonoBehaviour
{
    public float moveSpeed = 15;
    private Vector3 moveInput;
    private Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveInput = Vector3.zero;
    }
    private void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        rb.MovePosition(rb.position +
             (moveInput.normalized * moveSpeed *
              Time.deltaTime));
    }
    //private void FixedUpdate()
    //{
    //    rb.MovePosition(rb.position +
    //         (moveInput.normalized * moveSpeed *
    //          Time.fixedDeltaTime));
    //}
}