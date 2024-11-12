using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rgbody;

    #region Movement Variables
    [SerializeField, Range(1, 15)] private float walkSpeed = 5f;
    [SerializeField, Range(0.15f, 30)] private float acceleration = 5f;
    [SerializeField, Range(0.15f, 30)] private float deceleration = 5f;
    [SerializeField] private float dashImpulseMagnitude = 10f;

    private float targetSpeed = -1f;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 currentVelocity;
    private bool isMoving = false;
    #endregion

    #region Jump and Gravity Variables
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float gravityForce = 60f;
    [SerializeField] private float applyGravityForceThreshold = 10f;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        rgbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (isMoving)
            Move();
        else
            ApplyDeceleration();

        if (!isGrounded && rgbody.linearVelocity.y < applyGravityForceThreshold)
        {
            rgbody.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }

        DrawDebugVectors();
    }
    #endregion

    #region Input Handlers
    private void OnMove(InputValue value)
    {
        isMoving = true;
        Vector2 input = value.Get<Vector2>();
        moveDirection = new Vector3(input.x, 0, input.y);
        targetSpeed = walkSpeed;
    }

    private void OnMoveEnd(InputValue value)
    {
        isMoving = false;
    }

    private void OnDash()
    {
        rgbody.linearVelocity = new Vector3(rgbody.linearVelocity.x, 0, rgbody.linearVelocity.z);
        rgbody.AddForce(moveDirection * dashImpulseMagnitude, ForceMode.Impulse);
    }

    private void OnJump()
    {
        if (isGrounded)
        {
            rgbody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            Debug.Log("JUMPED");
        }
    }
    #endregion

    #region Movement and Deceleration
    private void Move()
    {
        Vector3 targetVelocity = moveDirection * targetSpeed;
        Vector3 currentXZVelocity = new Vector3(rgbody.linearVelocity.x, 0, rgbody.linearVelocity.z);
        Vector3 force = (targetVelocity - currentXZVelocity) * acceleration;

        rgbody.AddForce(force, ForceMode.Acceleration);
    }

    private void ApplyDeceleration()
    {
        Vector3 horizontalVelocity = new Vector3(rgbody.linearVelocity.x, 0, rgbody.linearVelocity.z);

        if (horizontalVelocity.magnitude > 0.25f)
        {
            Vector3 decelerationForce = -horizontalVelocity.normalized * deceleration;
            rgbody.AddForce(decelerationForce, ForceMode.Acceleration);
        }
        else
        {
            rgbody.linearVelocity = new Vector3(0, rgbody.linearVelocity.y, 0);
        }
    }
    #endregion

    #region Ground Check and Debugging
    private void CheckGrounded()
    {
        Vector3 rayOrigin = transform.position - new Vector3(0, 0.8f, 0);
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayer);
        rgbody.useGravity = !isGrounded;

        Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    private void DrawDebugVectors()
    {
        Vector3 playerPosition = transform.position;

        Debug.DrawRay(playerPosition + new Vector3(0, 1f, 0), rgbody.linearVelocity, Color.cyan); // Velocity
        Vector3 acceleration = (rgbody.linearVelocity - currentVelocity) / Time.fixedDeltaTime;
        Debug.DrawRay(playerPosition + new Vector3(0, 0.5f, 0), acceleration, Color.magenta); // Acceleration
        Vector3 force = rgbody.mass * acceleration;
        Debug.DrawRay(playerPosition, force, Color.yellow); // Force
    }
    #endregion
}