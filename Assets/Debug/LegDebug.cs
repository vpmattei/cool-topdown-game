using System;
using UnityEngine;

public class LegDebug : MonoBehaviour
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

    private MoveLegDebugger moveLegDebugger;

    #endregion

    #region Movement Settings

    [SerializeField] private float stepDistance = 2f;   // The threshold after which movement should start
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
        moveLegDebugger = body?.GetComponent<MoveLegDebugger>();
        // footOffset = transform.localPosition;
        isMoving = false;
        isDone = false;
        moveTimer = 1f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 30, terrainLayer))
        {
            footOffset = currentPosition = newPosition = oldPosition = hit.point;
        }
    }

    void Update()
    {
        if (Physics.Raycast(footOffset + body.transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit newHit, 10, terrainLayer))
        {
            if (Physics.Raycast(oldPosition + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit oldHit, 10, terrainLayer))
            {
                float distanceMoved = Vector3.Distance(oldHit.point, newHit.point);

                if (distanceMoved >= stepDistance)
                {
                    if (legIndexToMove == moveLegDebugger.currentLegIndex)
                    {
                        MoveLeg(newHit.point, moveDuration, stepHeight);
                    }
                }
            }
        }

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

            // Fire the event so MoveLegDebugger knows this leg is done
            OnLegMovementFinished?.Invoke(legIndexToMove);

            // ResetLegState();
        }
    }

    public void ResetLegState()
    {
        isMoving = false;
        isDone = false;
        moveTimer = 1f;  // Reset to what you consider as the "default" state
    }

    void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), Vector3.down * 10, Color.yellow);

        if (Physics.Raycast(footOffset + body.transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit hitSphere, 10, terrainLayer))
        {
            Gizmos.DrawSphere(hitSphere.point, .2f);
            if (Physics.Raycast(oldPosition + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit hit, 10, terrainLayer))
            {
                Debug.DrawLine(hitSphere.point, hit.point, Color.red);
            }
        }
    }
}