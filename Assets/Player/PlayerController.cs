using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rgbody;

    #region Movement Variables
    [SerializeField]
    private float walkSpeed = 5f;
    [SerializeField]
    private float sprintSpeed = 7f;
    private float targetSpeed = -1f;

    [SerializeField]
    [Range(0.15f, 10)]
    private float acceleration = 5;
    [SerializeField]
    private float deceleration = 5;
    [SerializeField]
    private float dashImpulseMagnitude = 10;
    Vector3 currentVelocity;
    private bool isMoving = false;

    #region Jump Variables
    [SerializeField]
    private float jumpHeight = 10f;
    [SerializeField]
    private bool isGrounded = true;
    [SerializeField]
    private float groundCheckDistance = 0.5f;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private float gravityForce = 10f;
    #endregion
    private Vector3 moveDirection = new Vector3(0, 0, 0);
    #endregion

    private void Awake()
    {
        rgbody = GetComponent<Rigidbody>();
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

    private void OnDash()
    {
        rgbody.AddForce(dashImpulseMagnitude * rgbody.linearVelocity, ForceMode.Impulse);
    }

    private void OnJump()
    {
        rgbody.useGravity = true;
        if (isGrounded)
        {
            rgbody.AddForce(jumpHeight * Vector3.up, ForceMode.Impulse);
            print(rgbody.linearVelocity);
        }
    }

    private void CheckGrounded()
    {
        // Cast a ray downward
        isGrounded = Physics.Raycast(transform.position - new Vector3(0, .8f, 0), Vector3.down, groundCheckDistance, groundLayer);
        rgbody.useGravity = !isGrounded;

        // Visualize the ray in Scene view
        Debug.DrawRay(transform.position - new Vector3(0, .8f, 0), Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        CheckGrounded();
        if (isMoving) Move();
        else ApplyDeceleration();

        if (!isGrounded && rgbody.linearVelocity.y <= 10)
        {
            // Add force downwards so the player falls faster, it makes for a snapier jump
            rgbody.AddForce(gravityForce * Vector3.down, ForceMode.Acceleration);
        }
    }

    private void Move()
    {
        Vector3 targetVelocity = moveDirection * targetSpeed;
        rgbody.linearVelocity = Vector3.SmoothDamp(rgbody.linearVelocity, targetVelocity, ref currentVelocity, 0.1f / acceleration);
    }

    private void ApplyDeceleration()
    {
        // Implement drag or deceleration logic here
        if (rgbody.linearVelocity.magnitude > 0.1f) // Example threshold for stopping movement
        {
            rgbody.AddForce(deceleration * -rgbody.linearVelocity, ForceMode.Acceleration);
        }
        else
        {
            rgbody.linearVelocity = Vector3.zero; // Stop completely when below threshold
        }
    }
}
