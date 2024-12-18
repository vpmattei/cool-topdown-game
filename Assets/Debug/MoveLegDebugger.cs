using System.Collections.Generic;
using UnityEngine;
public class MoveLegDebugger : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private LayerMask terrainLayer;

    [Header("Movement Settings")]
    [SerializeField] private float stepDistance = 10f;   // The threshold after which movement should start
    [SerializeField] private float moveDuration = 2f;    // How long each leg moves
    [SerializeField] private float stepHeight = 4f;    // How high each leg goes
    [SerializeField] private float legInterval = 1f;     // Time interval between legs starting movement

    [Header("Legs")]
    [SerializeField] private List<LegDebug> legs = new List<LegDebug>();

    [Header("Debug Variables")]
    [SerializeField] private float raycastDistance = 30f;

    [Tooltip("When true, the code runs through the sequence of moving legs.")]
    public int movesToPerform = 0;

    // Internal state
    private float movementStartTime;

    // Body position states
    private Vector3 currentBodyPosition;
    private Vector3 oldBodyPosition;

    void Start()
    {
        // Setting old body position
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit hit, raycastDistance, terrainLayer))
        {
            currentBodyPosition = oldBodyPosition = hit.point;
        }
    }

    private void Update()
    {
        Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), Vector3.down * raycastDistance, Color.green);
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit hit, raycastDistance, terrainLayer))
        {
            currentBodyPosition = hit.point;

            float distance = Vector3.Distance(oldBodyPosition, currentBodyPosition);
            // UpdateStepDistanceUI(distance);

            // Check if we should start moving legs
            if (distance >= stepDistance)
            {
                oldBodyPosition = currentBodyPosition;

                if (movesToPerform == 0) movementStartTime = 0; // If this is the first move, then set the timer to 0
                // distanceMoved = 0f; // Reset the distance

                for (int i = 0; i < legs.Count; i++)
                {
                    movesToPerform += 1;
                    var leg = legs[i];

                    leg.StartMoveTime = i * legInterval;
                    leg.MovesToPerform += 1;
                    Debug.Log(leg.LegName + " Leg interval: " + leg.StartMoveTime);
                }
            }
        }

        if (movesToPerform > 0)
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
                    leg.MoveLeg(currentBodyPosition + new Vector3(0, .5f, 0), moveDuration, stepHeight);
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
        movesToPerform = 0;

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
        // GUILayout.Label($"Distance Moved: {distanceMoved:F2}", style);
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