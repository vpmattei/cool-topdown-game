using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rgbody;
    [SerializeField]
    private float walkSpeed = 5f;
    [SerializeField]
    private float sprintSpeed = 7f;
    private float targetSpeed = -1f;
    [SerializeField]
    [Range(0.15f, 10)]
    private float acceleration = 5;
    Vector3 currentVelocity;
    private bool isMoving = false;
    private bool isSprinting = false;
    [SerializeField]
    private int jumpHeight = 10;
    private Vector3 moveDirection = new Vector3(0, 0, 0);

    private void Awake()
    {
        rgbody = GetComponent<Rigidbody>();
    }

    private void OnSprintStart()
    {
        isSprinting = true;
        targetSpeed = sprintSpeed;
    }

    private void OnSprintEnd()
    {
        isSprinting = false;
        targetSpeed = walkSpeed;
    }

    private void OnMove(InputValue value)
    {
        isMoving = true;
        Vector2 v2 = value.Get<Vector2>();
        moveDirection = new Vector3(v2.x, 0, v2.y);

        if (targetSpeed == -1f) targetSpeed = walkSpeed;
    }

    private void OnMoveEnd(InputValue value)
    {
        isMoving = false;
    }

    void FixedUpdate()
    {
        if (isMoving) Move();
    }

    private void Move()
    {
        Vector3 targetVelocity = moveDirection * targetSpeed;
        rgbody.linearVelocity = Vector3.SmoothDamp(rgbody.linearVelocity, targetVelocity, ref currentVelocity, 0.1f / acceleration);
    }

    private void OnJump()
    {
        rgbody.AddForce(new Vector3(0, 10, 0) * jumpHeight);
    }
}
