using System;
using UnityEngine;

[Serializable]
public class Leg : MonoBehaviour
{
    #region Inspector Fields
    // Leg State
    public LegBaseState currentLegState;
    public LegIdleState IdleState = new LegIdleState();
    public LegMoveState MoveState = new LegMoveState();

    [SerializeField] private AnimationCurve stepCurve;
    [SerializeField] private LayerMask terrainLayer;

    [Header("References")]
    public GameObject body;
    private Vector3 footOffset;

    [Header("Group Selection")]
    // This int stores the index of the group this leg belongs to.
    [LegGroupDropdown]
    public int selectedGroupIndex;

    #endregion

    #region Private Fields

    private Vector3 positionToMove;
    private Vector3 oldPosition;
    private Vector3 newPosition;
    public Vector3 currentPosition {  get; private set; }
    private float moveTimer = 0f;   // Timer that increments each frame, after the movement of the leg starts, moveTimer = [0 ... moveDuration]
    private float distanceMoved = 0f;
    public float currentRotation { get; private set; } = 0f;
    private float rotationAmount = 0f;

    [SerializeField] private LegsManager legsManager;
    public LegsManager LegsManager => legsManager;
    private PlayerController playerController;

    #endregion

    #region Movement Settings
    private float stepDistance = 2f;   // The threshold after which movement should start
    [SerializeField] private float maxRotation = .15f;   // The rotation threshold after which movement should start
    private float velocityFactor = 1f;   // Velocity factor that adds distance to the step distance
    private float moveDuration = 0.25f; // How long each leg moves
    private float stepHeight = 2f;     // How high each leg goes
    private float legInterval = 0.125f; // Time interval between legs starting movement
    #endregion

    #region Events

    public event Action<Leg> OnLegMovementStarted;
    public event Action<Leg> OnLegMovementFinished;

    #endregion

    #region Properties

    // Readonly properties for external checks
    public float MoveTimer => moveTimer;

    // StartMoveTime can be a simple auto-property or standard property
    public float StartMoveTime { get; set; }

    // Movement settings: public getters/setters
    public float StepDistance { get => stepDistance; set => stepDistance = value; }
    public float DistanceMoved { get => distanceMoved; }
    public Vector3 PositionToMove { get => positionToMove; }

    public float MoveDuration { get => moveDuration; set => moveDuration = value; }

    public float StepHeight { get => stepHeight; set => stepHeight = value; }

    public float LegInterval { get => legInterval; set => legInterval = value; }

    public Vector3 FootOffset { get => footOffset; set => footOffset = value; }
    // public int LegIndexToMove => legIndexToMove; // read-only for external usage

    #endregion

    void Start()
    {
        // body = transform.parent.gameObject;
        legsManager = body?.GetComponent<LegsManager>();
        playerController = body?.GetComponent<PlayerController>();
        // footOffset = transform.localPosition;

        currentLegState = IdleState;
        currentLegState.EnterState(this);

        moveTimer = 1f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 30, terrainLayer))
        {
            footOffset = currentPosition = newPosition = oldPosition = hit.point;
        }
        currentRotation = body.transform.rotation.y;
        rotationAmount = Mathf.Abs(body.transform.rotation.y - currentRotation);
    }

    void Update()
    {
        //UpdatePositionToMove(); // Update only in idle state

        currentLegState.Update(this);

        // Update rotation tracking
        rotationAmount = Mathf.Abs(body.transform.eulerAngles.y - currentRotation);
        //transform.position = currentPosition; // TODO: Update only in move state
    }

    void FixedUpdate()
    {
        currentLegState.FixedUpdate(this);
    }

    public void UpdatePositionToMove()
    {
        Vector3 pivot = body.transform.position + new Vector3(0, 0.5f, 0);
        Vector3 rotatedOffset = body.transform.rotation * footOffset;
        Vector3 sphereRayOrigin = pivot + rotatedOffset;

        if (Physics.Raycast(sphereRayOrigin, Vector3.down, out RaycastHit hitSphere, 10, terrainLayer))
        {
            positionToMove = hitSphere.point; // Updated in real-time
        }
    }

    public void StartMovement()
    {
        oldPosition = currentPosition;
        newPosition = positionToMove + new Vector3(0, transform.localScale.y/2, 0); // We add the height of the object divided by two, to make sure the bottom of the foot touches the ground and that it is not clipping the ground

        moveTimer = 0f; // Reset timer
        currentRotation = body.transform.eulerAngles.y;  // Reset rotation

        // Exit Idle State
        currentLegState.ExitState(this);
        currentLegState = MoveState;
        // Enter Move State
        currentLegState.EnterState(this);

        //OnLegMovementStarted?.Invoke(this); // Started moving notification to the system
    }

    public void UpdateMove()
    {
        if (moveTimer < moveDuration)
        {
            // Smoothly interpolate position over time
            float t = moveTimer / moveDuration;
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, t);
            tempPosition.y += stepCurve.Evaluate(t) * stepHeight; // Apply step height curve
            currentPosition = tempPosition;
            moveTimer += Time.deltaTime; // Increment timer
        }
        else
        {
            // Movement complete
            currentPosition = newPosition;
            oldPosition = newPosition;

            currentRotation = body.transform.eulerAngles.y;  // Reset rotation

            // Exit from Move State
            currentLegState.ExitState(this);
            currentLegState = IdleState;
            // Enter Idle State
            currentLegState.EnterState(this);

            //OnLegMovementFinished?.Invoke(this); // Finished moving notification to the system
        }
    }

    /// <summary>
    /// The actual name of the group this leg belongs to,
    /// looked up from the leg manager’s list.
    /// </summary>
    public string SelectedGroupName
    {
        get
        {
            if (legsManager == null
                || legsManager.legGroups == null
                || selectedGroupIndex < 0
                || selectedGroupIndex >= legsManager.legGroups.Count)
            {
                return string.Empty;
            }
            return legsManager.legGroups[selectedGroupIndex];
        }
    }

    public float LegUrgency
    {
        get
        {    
            // Distance urgency (normalized to stepDistance)
            float distanceUrgency = Vector3.Distance(oldPosition, positionToMove) / stepDistance;
            // Rotation urgency (normalized to maxRotation)
            float rotationUrgency = rotationAmount / maxRotation;
            // Total urgency (clamped to avoid overshooting)
            return Mathf.Clamp01(distanceUrgency + rotationUrgency * 0.1f);
        }
    }

    // TODO Fix this method
    private Vector3 PredictStepPosition(Vector3 oldHitPoint, Vector3 newHitPoint, Vector3 bodyVelocity, float velocityFactor)
    {
        // 1. Get the base direction (from oldHit to newHit)
        Vector3 baseDirection = (newHitPoint - oldHitPoint).normalized;

        // 2. Get the distance from oldHit to newHit if needed
        float stepDistance = Vector3.Distance(oldHitPoint, newHitPoint);

        // 3. Decide how much velocity should influence the step
        float velocityInfluence = bodyVelocity.magnitude * velocityFactor;

        // 4. Combine them to find the final predicted position
        Vector3 finalPosition = oldHitPoint + baseDirection * (stepDistance + velocityInfluence);

        return finalPosition;
    }

    public void ResetLegState()
    {
        // Exit from Any State
        currentLegState.ExitState(this);
        currentLegState = IdleState;
        // Enter Idle State
        currentLegState.EnterState(this);

        moveTimer = 1f;  // Reset to what you consider as the "default" state
    }

    void OnDrawGizmos()
    {
        // 1. The pivot is the player's position (plus any desired y offset).
        Vector3 pivot = body.transform.position + new Vector3(0, 0.5f, 0);

        // 2. Rotate the footOffset by the player’s current rotation.
        Vector3 rotatedOffset = body.transform.rotation * footOffset;

        // 3. Determine the final position from which you want to raycast downward.
        Vector3 sphereRayOrigin = pivot + rotatedOffset;

        // Draw a debug ray for clarity (optional).
        Debug.DrawRay(sphereRayOrigin, Vector3.down * 10, Color.yellow);

        // 4. Cast from the rotated offset in world space downward.
        if (Physics.Raycast(sphereRayOrigin, Vector3.down, out RaycastHit hitSphere, 10, terrainLayer))
        {
            // 5. Draw the sphere at the hit point.
            Gizmos.DrawSphere(hitSphere.point, 0.2f);

            // If you need to draw a line from that hit to another point (like oldPosition),
            // make sure oldPosition is also transformed properly if it's meant to rotate.
            if (Physics.Raycast(oldPosition + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit hit, 10, terrainLayer))
            {
                // Draw a line from the new hitSphere to the old one
                Debug.DrawLine(hitSphere.point, hit.point, Color.red);
            }
        }
    }
}