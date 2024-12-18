using System.Collections.Generic;
using UnityEngine;
public class MoveLegDebugger : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private LayerMask terrainLayer;

    [Header("Movement Settings")]
    [SerializeField] private float stepDistance = 10f;   // The threshold after which movement should start
    [SerializeField] private float moveDuration = 2f;    // How long each leg moves
    [SerializeField] private float stepHeight = 4f;      // How high each leg goes
    [SerializeField] private float legInterval = 1f;     // Time interval between legs starting movement

    [Header("Legs")]
    [SerializeField] private List<LegDebug> legs = new List<LegDebug>();

    [Header("Separate Start Times")]
    [SerializeField] private bool useSeparateStartTimes = false;
    [SerializeField] private List<float> separateStartTimes = new List<float>();    // Make sure separateStartTimes matches legs.Count if useSeparateStartTimes is true

    [Header("Debug Variables")]
    [SerializeField] private float raycastDistance = 30f;
    [SerializeField] private GameObject circleRendererObject;
    private CircleRenderer circleRenderer;

    [Tooltip("When true, the code runs through the sequence of moving legs.")]
    public bool shouldMove = false;

    // Internal state
    private float movementStartTime;

    // Body position states
    private Vector3 currentBodyPosition;
    private Vector3 oldBodyPosition;
    private float distanceMoved;

    void Start()
    {
        // Setting old body position
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit hit, raycastDistance, terrainLayer))
        {
            currentBodyPosition = oldBodyPosition = hit.point;
        }

        // Cache the CircleRenderer component
        if (circleRendererObject != null)
        {
            circleRenderer = circleRendererObject.GetComponent<CircleRenderer>();
        }
        else
        {
            Debug.LogWarning("CircleRendererObject is not assigned.");
        }
    }

    private void Update()
    {
        Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), Vector3.down * raycastDistance, Color.green);
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit hit, raycastDistance, terrainLayer))
        {
            currentBodyPosition = hit.point;

            distanceMoved = Vector3.Distance(oldBodyPosition, currentBodyPosition);

            // Check if we should start moving legs
            if (distanceMoved >= stepDistance && !shouldMove)
            {
                oldBodyPosition = currentBodyPosition;

                circleRenderer.DrawCircle(100, stepDistance, oldBodyPosition + new Vector3(0, .5f, 0));

                if (!shouldMove) movementStartTime = 0; // If this is the first move, then set the timer to 0
                // distanceMoved = 0f; // Reset the distance

                for (int i = 0; i < legs.Count; i++)
                {
                    shouldMove = true;

                    var leg = legs[i];

                    if (useSeparateStartTimes && i < separateStartTimes.Count)
                    {
                        leg.StartMoveTime = separateStartTimes[i];
                    }
                    else
                    {
                        leg.StartMoveTime = i * legInterval;
                    }
                    
                    leg.MovesToPerform += 1;
                }
            }
        }

        if (shouldMove)
        {
            UpdateLegMovements();
        }
    }

    private void UpdateLegMovements()
    {
        bool allLegsDone = true;

        for (int i = 0; i < legs.Count; i++)
        {
            var leg = legs[i];

            // Check if it's time for this leg to start moving
            if (movementStartTime >= leg.StartMoveTime)
            {
                if (!leg.IsDone && leg.MovesToPerform > 0)
                {
                    leg.MoveLeg(currentBodyPosition + leg.FootOffset, moveDuration, stepHeight);
                    allLegsDone = false;
                }
                else
                {
                    leg.ResetLegState();
                    if (leg.MovesToPerform > 0)
                    {
                        leg.MovesToPerform -= 1;
                    }
                }
            }
        }

        if (allLegsDone) ResetMovement();

        movementStartTime += Time.deltaTime;
    }

    private void ResetMovement()
    {
        shouldMove = false;

        // Reset each leg so it can move again next time
        for (int i = 0; i < legs.Count; i++)
        {
            var leg = legs[i];
            // Reset relevant fields
            // You may need to adjust which states are reset based on your logic
            leg.ResetLegState();
        }
    }

    private void OnGUI()
    {
        int totalMovesRemaining = 0;
        for (int i = 0; i < legs.Count; i++)
        {
            totalMovesRemaining += legs[i].MovesToPerform;
        }
        totalMovesRemaining /= legs.Count;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            normal = { textColor = Color.white }
        };

        GUILayout.BeginVertical("box");
        GUILayout.Label($"Distance Moved: {distanceMoved:F2}", style);
        GUILayout.Label($"Moves to perform: {totalMovesRemaining}", style);
        GUILayout.Label($"Movement Start Time: {movementStartTime:F2}", style);
        GUILayout.Label($"Leg Interval: {legInterval:F2}", style);

        GUILayout.Label("Leg States:", style);
        for (int i = 0; i < legs.Count; i++)
        {
            string legStatus;
            if (legs[i].IsMoving) legStatus = "Moving";
            else if (legs[i].IsDone) legStatus = "Done";
            else legStatus = "Not Started";
            GUILayout.Label(legStatus, style);
        }
        GUILayout.EndVertical();
    }
}