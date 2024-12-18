using UnityEngine;

public class LegDebug : MonoBehaviour
{
    public string legName;
    // [SerializeField] private float stepDuration = 0.25f;
    // [SerializeField] private float stepHeight = 0.4f;
    [SerializeField] private AnimationCurve stepCurve; // Define the curve in the inspector

    public GameObject body;
    private Vector3 oldPosition;
    private Vector3 newPosition;
    private Vector3 currentPosition;
    private float footSpacing;
    private float moveTimer = 0f;
    private float startMoveTime;
    private bool isMoving = false;
    private bool isDone = false;
    private int movesToPerform = 0;

    public string LegName => legName;
    public bool IsMoving => isMoving;
    public bool IsDone => isDone;
    public float MoveTimer => moveTimer;
    public int MovesToPerform
    {
        set { movesToPerform = value; }
        get { return movesToPerform; }
    }
    public float StartMoveTime
    {
        set { startMoveTime = value; }
        get { return startMoveTime; }
    }

    // DEBUG
    public GameObject newPositionToMove;

    void Start()
    {
        body = transform.parent.gameObject;
        footSpacing = transform.localPosition.x;
        isMoving = false;
        isDone = false;
        moveTimer = 1f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 30, LayerMask.NameToLayer("Ground")))
        {
            transform.position = currentPosition = newPosition = oldPosition = hit.point;
        }
    }

    void Update()
    {
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
        }
    }

    public void ResetLegState()
    {
        isMoving = false;
        isDone = false;
        moveTimer = 1f;  // Reset to what you consider as the "default" state
    }
}