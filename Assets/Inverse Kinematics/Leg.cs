using System;
using UnityEngine;

public class Leg : MonoBehaviour
{
    #region Inspector Fields

    [Header("Basic Leg Info")]
    public string legName;
    [SerializeField] private AnimationCurve stepCurve;
    [SerializeField] private LayerMask terrainLayer;

    [Header("References")]
    public GameObject body;
    public Vector3 footOffset;

    #endregion

    #region Private Fields

    private Vector3 oldPosition;
    private Vector3 newPosition;
    private Vector3 currentPosition;
    private float moveTimer = 0f;
    private bool isMoving = false;
    private bool isDone = false;
    private int movesToPerform = 0;
    [SerializeField] private float currentRotation = 0f;
    [SerializeField] private float rotationAmount = 0f;

    private ProceduralLegAnimation proceduralLegAnimation;
    private PlayerController playerController;

    #endregion

    #region Movement Settings

    [SerializeField] private float stepDistance = 2f;   // The threshold after which movement should start
    [SerializeField] private float maxRotation = .15f;   // The rotation threshold after which movement should start
    [SerializeField] private float velocityFactor = 1f;   // Velocity factor that adds distance to the step distance
    [SerializeField] private float moveDuration = 0.25f; // How long each leg moves
    [SerializeField] private float stepHeight = 2f;     // How high each leg goes
    [SerializeField] private float legInterval = 0.125f; // Time interval between legs starting movement
    [Tooltip("Leg index that indicates at which order this leg will move (0 is first, 1 is second and so on...)")]
    [SerializeField] private int legIndexToMove = 0; // Leg index that indicates at which order this leg will move

    #endregion

    #region Events

    /// <summary>
    /// Fired when this leg finishes its movement.
    /// The int parameter can be the legIndexToMove, 
    /// or you can pass this script instance (LegDebug) if you prefer.
    /// </summary>
    public event Action<int> OnLegMovementFinished;

    #endregion

    #region Properties

    // Readonly properties for external checks
    public string LegName => legName;
    public bool IsMoving => isMoving;
    public bool IsDone => isDone;
    public float MoveTimer => moveTimer;

    // MovesToPerform as a standard property
    public int MovesToPerform
    {
        get => movesToPerform;
        set => movesToPerform = value;
    }

    // StartMoveTime can be a simple auto-property or standard property
    public float StartMoveTime { get; set; }

    // Movement settings: public getters/setters
    public float StepDistance { get => stepDistance; set => stepDistance = value; }

    public float MoveDuration { get => moveDuration; set => moveDuration = value; }

    public float StepHeight { get => stepHeight; set => stepHeight = value; }

    public float LegInterval { get => legInterval; set => legInterval = value; }

    public Vector3 FootOffset { get => footOffset; set => footOffset = value; }
    public int LegIndexToMove => legIndexToMove; // read-only for external usage

    #endregion

    void Start()
    {
        body = transform.parent.gameObject;
        proceduralLegAnimation = body?.GetComponent<ProceduralLegAnimation>();
        playerController = body?.GetComponent<PlayerController>();
        // footOffset = transform.localPosition;
        isMoving = false;
        isDone = false;
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
        // Move leg if distance moved is bigger than the step distance
        if (Physics.Raycast(footOffset + body.transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit newHit, 10, terrainLayer))
        {
            if (Physics.Raycast(oldPosition + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit oldHit, 10, terrainLayer))
            {
                float distanceMoved = Vector3.Distance(oldHit.point, newHit.point);

                if (distanceMoved >= stepDistance || isMoving)
                {
                    if (legIndexToMove == proceduralLegAnimation.currentLegIndex)
                    {
                        Vector3 predictedStepPosition = PredictStepPosition(oldHit.point, newHit.point, playerController.GetLinearVelocity(), velocityFactor);
                        MoveLeg(predictedStepPosition, moveDuration, stepHeight);
                    }
                }
            }
        }

        // Move leg if the leg rotated more than the rotation angle
        if (rotationAmount >= maxRotation)
        {
            Debug.Log("Rotate legs!");
            // Then reset the rotated amount to 0
            currentRotation = body.transform.rotation.y;
            // Read the current rotation in Euler angles
            Vector3 currentEuler = transform.rotation.eulerAngles;
            // Adjust the y-rotation
            currentEuler.y = currentRotation;
            // Convert back to a Quaternion
            transform.rotation = Quaternion.Euler(currentEuler);
        }

        rotationAmount = Mathf.Abs(body.transform.rotation.y - currentRotation);
        transform.position = currentPosition;
    }

    public void MoveLeg(Vector3 positionToMove, float moveDuration, float stepHeight)
    {
        // If we are starting a new movement sequence:
        if (!isMoving)
        {
            oldPosition = currentPosition;
            newPosition = positionToMove;
            isMoving = true;
            isDone = false;
            moveTimer = 0f; // Reset the move timer for this new step
        }

        // Only update the position while we haven't completed the step
        if (moveTimer < moveDuration)
        {
            float t = moveTimer / moveDuration;
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, t);

            // Use the animation curve instead of a sine function
            float curveValue = stepCurve.Evaluate(t);
            tempPosition.y += curveValue * stepHeight;

            currentPosition = tempPosition;
            moveTimer += Time.deltaTime;
        }
        else
        {
            // Movement complete
            currentPosition = newPosition;
            oldPosition = newPosition;
            isMoving = false;
            isDone = true;

            // Fire the event so ProceduralLegAnimation knows this leg is done
            OnLegMovementFinished?.Invoke(legIndexToMove);

            // ResetLegState();
        }
    }

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
        isMoving = false;
        isDone = false;
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