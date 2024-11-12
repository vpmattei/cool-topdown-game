using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rgbody;

    #region Movement Variables
    [SerializeField]
    [Range(1, 15)]
    private float walkSpeed = 5f;
    private float targetSpeed = -1f;

    [SerializeField]
    [Range(0.15f, 30)]
    private float acceleration = 5;
    [SerializeField]
    [Range(0.15f, 10)]
    private float deceleration = 5;
    [SerializeField]
    private float dashImpulseMagnitude = 10;
    Vector3 currentVelocity;
    private bool isMoving = false;

    #region Jump Variables
    [SerializeField]
    private float jumpHeight = 10;
    [SerializeField]
    private bool isGrounded = true;
    [SerializeField]
    private float groundCheckDistance = 0.5f;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private float gravityForce = 60;
    [SerializeField]
    private float applyGravityForceThreshold = 10;
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

        targetSpeed = walkSpeed;
    }

    private void OnMoveEnd(InputValue value)
    {
        isMoving = false;
    }

    private void OnDash()
    {
        rgbody.AddForce(moveDirection * dashImpulseMagnitude, ForceMode.Impulse);
    }

    private void OnJump()
    {
        rgbody.useGravity = true;
        if (isGrounded)
        {
            // Preserve current x and z velocity, reset y velocity, then add jump force
            //rgbody.linearVelocity = new Vector3(rgbody.linearVelocity.x, 0, rgbody.linearVelocity.z);
            rgbody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            print("JUMPED");
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

        if (isMoving)
        {
            Move();
        }
        else
        {
            ApplyDeceleration();
        }

        if (!isGrounded)
        {
            // Ensure gravity accelerates the player downward naturally
            if (rgbody.linearVelocity.y < applyGravityForceThreshold)
            {
                rgbody.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
            }
        }

        // Draw vectors in the Scene View for debugging
        DrawDebugVectors();
        print("Velocity: " + rgbody.linearVelocity);
    }

    private void Move()
    {
        Vector3 targetVelocity = moveDirection * targetSpeed;
        print("Move direction: " + moveDirection);

        // Current velocity in XZ plane (ignoring Y)
        Vector3 currentXZVelocity = new Vector3(rgbody.linearVelocity.x, 0, rgbody.linearVelocity.z);

        // Calculate force required to reach target XZ velocity
        Vector3 force = (targetVelocity - currentXZVelocity) * acceleration;
        print("Force applied: " + force);

        // Apply force only on X and Z axes
        rgbody.AddForce(force, ForceMode.Acceleration);
    }

    private void ApplyDeceleration()
    {
        // Get current horizontal velocity (ignore y-axis)
        Vector3 horizontalVelocity = new Vector3(rgbody.linearVelocity.x, 0, rgbody.linearVelocity.z);

        // Apply deceleration only if horizontal speed is above the threshold
        if (horizontalVelocity.magnitude > 0.1f)
        {
            // Apply force to reduce horizontal velocity
            Vector3 decelerationForce = -horizontalVelocity.normalized * deceleration;
            rgbody.AddForce(decelerationForce, ForceMode.Acceleration);
        }
        else
        {
            // Zero out horizontal velocity when below threshold, preserve y velocity
            rgbody.linearVelocity = new Vector3(0, rgbody.linearVelocity.y, 0);
        }
    }

    private void DrawDebugVectors()
    {
        Vector3 playerPosition = transform.position;

        // Draw velocity vector (cyan)
        Debug.DrawRay(playerPosition + new Vector3(0, 1f, 0), rgbody.linearVelocity, Color.cyan);

        // Draw acceleration vector (magenta)
        Vector3 acceleration = (rgbody.linearVelocity - currentVelocity) / Time.fixedDeltaTime;
        Debug.DrawRay(playerPosition + new Vector3(0, .5f, 0), acceleration, Color.magenta);

        // Draw force vector (yellow, includes gravity when falling)
        Vector3 force = rgbody.mass * acceleration;
        Debug.DrawRay(playerPosition, force, Color.yellow);
    }
}
