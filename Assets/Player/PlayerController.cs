using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rgbody;
    private Camera cam;
    private Animator animator;

    #region Movement Variables
    [SerializeField, Range(1, 15)] private float walkSpeed = 7;
    [SerializeField, Range(0.15f, 50)] private float acceleration = 20;
    [SerializeField, Range(0.15f, 50)] private float deceleration = 20;
    [SerializeField, Range(2, 30)] private float rotationSpeed = 10;
    [SerializeField] private float dashImpulseMagnitude = 10;

    private float targetSpeed = -1f;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 currentVelocity;
    #endregion

    #region Jump and Gravity Variables
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float rayGroundCheckStartPos = 0;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float gravityForce = 60f;
    [SerializeField] private float applyGravityForceThreshold = 10f;
    #endregion

    #region State Variables
    private bool isMoving = false;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        rgbody = GetComponent<Rigidbody>();
        cam = Camera.main;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetFloat("XDir", rgbody.linearVelocity.x / walkSpeed);
        animator.SetFloat("YDir", rgbody.linearVelocity.z / walkSpeed);
        Debug.Log(rgbody.linearVelocity);
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
        RotatePlayerToMousePoint();
    }
    #endregion

    #region Input Handlers
    private void OnMove(InputValue value)
    {
        isMoving = true;
        animator.SetBool("IsMoving", true);
        Vector2 input = value.Get<Vector2>();
        moveDirection = new Vector3(input.x, 0, input.y);
        targetSpeed = walkSpeed;
    }

    private void OnMoveEnd(InputValue value)
    {
        isMoving = false;
        animator.SetBool("IsMoving", false);
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
    /// <summary>
    /// Applies movement forces to move the player using the direction from the move input.
    /// </summary>
    private void Move()
    {
        Vector3 targetVelocity = moveDirection * targetSpeed;
        Vector3 currentXZVelocity = new Vector3(rgbody.linearVelocity.x, 0, rgbody.linearVelocity.z);
        Vector3 force = (targetVelocity - currentXZVelocity) * acceleration;

        rgbody.AddForce(force, ForceMode.Acceleration);
    }

    /// <summary>
    /// Gradually slows down the player when movement input stops.
    /// </summary>
    private void ApplyDeceleration()
    {
        Vector3 horizontalVelocity = new Vector3(rgbody.linearVelocity.x, 0, rgbody.linearVelocity.z);

        if (horizontalVelocity.magnitude > 0.35f)
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
    /// <summary>
    /// Checks if the player is grounded using a downward raycast.
    /// </summary>
    private void CheckGrounded()
    {
        Vector3 rayOrigin = transform.position - new Vector3(0, rayGroundCheckStartPos, 0);
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayer);
        rgbody.useGravity = !isGrounded;

        Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    /// <summary>
    /// Draws velocity, acceleration, and force vectors in the Scene view for debugging.
    /// </summary>
    private void DrawDebugVectors()
    {
        Vector3 playerPosition = transform.position;

        Debug.DrawRay(playerPosition + new Vector3(0, 1f, 0), rgbody.linearVelocity, Color.cyan); // Velocity
        Vector3 acceleration = (rgbody.linearVelocity - rgbody.linearVelocity) / Time.fixedDeltaTime;
        Debug.DrawRay(playerPosition + new Vector3(0, 0.5f, 0), acceleration, Color.magenta); // Acceleration
        Vector3 force = rgbody.mass * acceleration;
        Debug.DrawRay(playerPosition, force, Color.yellow); // Force
    }
    #endregion

    private void RotatePlayerToMousePoint()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 100f;
        mousePos = cam.ScreenToWorldPoint(mousePos);
        Debug.DrawRay(cam.transform.position, mousePos - cam.transform.position, Color.green);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            Vector3 directionToLookAt = (hit.point - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToLookAt);

            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  new Quaternion(0, lookRotation.y, 0, lookRotation.w),
                                                  Time.deltaTime * rotationSpeed);
        }
    }
}