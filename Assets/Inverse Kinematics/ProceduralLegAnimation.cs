using System.Collections.Generic;
using UnityEngine;
public class ProceduralLegAnimation : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private LayerMask terrainLayer;

    [Header("Movement Settings")]
    [SerializeField] private float stepDistance = 10f;   // The threshold after which movement should start
    [SerializeField] private float moveDuration = 2f;    // How long each leg moves
    [SerializeField] private float stepHeight = 4f;      // How high each leg goes
    [SerializeField] private float legInterval = 1f;     // Time interval between legs starting movement
    public int currentLegIndex = 0;     // Current Leg index that indicates which leg(s) should move
    private int maxLegIndex = 0;

    [Header("Legs")]
    [SerializeField] private List<Leg> legs = new List<Leg>();

    [Header("Concurrency")]
    [SerializeField] private int maxConcurrentMoves = 2; // Set this in Inspector
    private List<Leg> activeMovingLegs = new List<Leg>();

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

        // Subscribe to each leg's OnLegMovementFinished event
        foreach (var leg in legs)
        {
            leg.OnLegMovementFinished += OnLegDoneMoving;
        }
    }

    void Update()
    {
        List<Leg> eligibleLegs = new List<Leg>();

        foreach (var leg in legs)
        {
            if (!leg.IsMoving && leg.CalculateUrgency() >= 1f)
            {
                eligibleLegs.Add(leg);
            }
        }

        eligibleLegs.Sort((a, b) => b.CalculateUrgency().CompareTo(a.CalculateUrgency()));

        // Move legs only if under concurrency limit
        foreach (var leg in eligibleLegs)
        {
            if (activeMovingLegs.Count < maxConcurrentMoves && !leg.IsMoving)
            {
                leg.StartMove(leg.PositionToMove);
                activeMovingLegs.Add(leg);
            }
        }
    }

    public void NotifyLegMovementComplete(Leg leg)
    {
        if (activeMovingLegs.Contains(leg))
        {
            activeMovingLegs.Remove(leg);
        }
    }

    /// <summary>
    /// Called every time a leg finishes moving. The leg passes its own legIndex.
    /// Once all legs of the "currentLegIndex" have moved, we can increment currentLegIndex.
    /// </summary>
    private void OnLegDoneMoving()
    {
        // Do nothing, for now..
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
            GUILayout.Label($"Distance Moved: {legs[i].DistanceMoved}", style);
            GUILayout.Label(legStatus, style);
        }
        GUILayout.EndVertical();
    }
}