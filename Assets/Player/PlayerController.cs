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
    [SerializeField, Range(0, 15)] private float rotationSpeed = 10;
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

    private float groundDistanceAtStart;
    [SerializeField, Range(1, 5)] private float desiredPlayerHeight = 2;
    [SerializeField, Range(0.001f, .25f)] private float heightSmoothSpeed = 1;
    [SerializeField, Range(1, 5)] private float tiltSpeed = 3;


    #region Unity Methods
    private void Awake()
    {
        rgbody = GetComponent<Rigidbody>();
        cam = Camera.main;
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        groundDistanceAtStart = GetGroundDistance;
    }

    private void Update()
    {
        // Get the player's current rotation as a quaternion
        Quaternion playerRotation = transform.rotation;

        // Inverse the player's rotation to transform the world movement direction into the player's local space
        Vector3 localMovementDirection = Quaternion.Inverse(playerRotation) * moveDirection;

        // Return as a 2D vector in the player's local space
        // Debug.Log(new Vector2(localMovementDirection.x, localMovementDirection.z));

        animator.SetFloat("XDir", localMovementDirection.x);
        animator.SetFloat("YDir", localMovementDirection.z);
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

        //FollowTerrain(,)
        FollowTerrain(heightSmoothSpeed, tiltSpeed);
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

    #region Ground Distance
    /// <summary>
    /// Keeps the player at the same distance from the ground and the same angle as the ground normal
    /// </summary>
    /// <param name="heightSmoothSpeed">The smoothness speed at which the player changes height</param>
    /// <param name="tiltSpeed">The tilt speed at which the player adapts to the terrain's angle</param>
    private void FollowTerrain(float heightSmoothSpeed, float tiltSpeed)
    {
        Vector3 curVelocity = new Vector3();

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100, groundLayer))
        {
            Vector3 newPos = transform.position;
            newPos.y = hit.point.y + groundDistanceAtStart;

            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref curVelocity, heightSmoothSpeed);

            Vector3 groundNormal = hit.normal;

            // 2. Flattened forward (so you keep looking where you were looking)
            Vector3 flattenedForward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

            // 3. Build rotation: forward=flattenedForward, up=groundNormal
            Quaternion targetRot = Quaternion.LookRotation(flattenedForward, groundNormal);

            // 4. Apply or smooth
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * tiltSpeed);
        }
    }

    /// <summary>
    /// Returns the ground distance,
    /// Based on this game object transform's position and the 'ground' (any object with the layer 'Ground').
    /// </summary>
    private float GetGroundDistance
    {
        get
        {
            float groundDist = 0;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 100, groundLayer))
            {
                Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.blue : Color.red);
                groundDist = transform.position.y - hitInfo.point.y;
            }

            return groundDist;
        }
    }

    /// <summary>
    /// Checks if the player is grounded using a downward raycast.
    /// </summary>
    private void CheckGrounded()
    {
        Vector3 rayOrigin = transform.position - new Vector3(0, rayGroundCheckStartPos, 0);
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayer);
        rgbody.useGravity = !isGrounded;

        //Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, isGrounded ? Color.blue : Color.red);
    }

    /// <summary>
    /// Draws velocity, acceleration, and force vectors in the Scene view for debugging.
    /// </summary>
    private void DrawDebugVectors()
    {
        Vector3 playerPosition = transform.position;

        Debug.DrawRay(playerPosition + new Vector3(0, 1f, 0), rgbody.linearVelocity, Color.cyan); // Velocity

        // [0,1] && 90° -> [-1,0]
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

    public Vector3 GetLinearVelocity()
    {
        return rgbody.linearVelocity;
    }
}