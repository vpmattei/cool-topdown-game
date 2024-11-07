using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rgbody;
    [SerializeField]
    private float walkSpeed = 5f;
    [SerializeField]
    private float sprintSpeed = 7f;
    [SerializeField]
    [Range(0.15f, 10)]
    private float acceleration = 5;
    Vector2 currentVelocity;
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
        Move(sprintSpeed);
    }

    private void OnSprintEnd()
    {
        isSprinting = false;
        Move(walkSpeed);
    }

    private void OnMove(InputValue value)
    {
        isMoving = true;
        Vector2 v2 = value.Get<Vector2>();
        moveDirection = new Vector3(v2.x, 0, v2.y);
    }

    private void OnMoveEnd(InputValue value)
    {
        isMoving = false;
    }

    void FixedUpdate()
    {
        if (isMoving) Move(walkSpeed);
    }

    private void Move(float speed)
    {
        print("Move direction(1):" + moveDirection * speed);
        rgbody.linearVelocity = Vector2.SmoothDamp(rgbody.linearVelocity, moveDirection * speed, ref currentVelocity, 0.1f / acceleration);
        print("Move direction(2):" + moveDirection * speed);
        print(rgbody.linearVelocity);
    }

    private void OnJump()
    {
        rgbody.AddForce(new Vector3(0, 10, 0) * jumpHeight);
    }
}
