using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// A copy of the leg's class, used as a reference
// in order for the ProceduralLegAnimation manager
// have control over each existing leg
public class LegCopy
{
    public enum State { Idle, Moving, DoneMoving }

    public string name;
    public State currentState;
    public String groupName;
    public bool hasMoved = false;

    public LegCopy(string legName, State state, String legGroupName)
    {
        name = legName;
        currentState = state;
        groupName = legGroupName;
    }

    public void SetState(State s)
    {
        currentState = s;
    }

    public State GetState()
    {
        return currentState;
    }

    public bool IsDoneMoving()
    {
        return currentState == State.DoneMoving;
    }
}

public class ProceduralLegAnimation : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private LayerMask terrainLayer;

    [Header("Movement Settings")]
    [SerializeField] private float stepDistance = 10f;   // The threshold after which movement should start
    [SerializeField] private float moveDuration = 2f;    // How long each leg moves
    [SerializeField] private float stepHeight = 4f;      // How high each leg goes
    [SerializeField] private float legInterval = 1f;     // Time interval between legs starting movement

    [Header("Legs")]
    [SerializeField] private List<Leg> legs = new List<Leg>();

    [SerializeField] private List<LegCopy> legCopies = new List<LegCopy>();

    [Header("Concurrency")]
    [SerializeField] private int maxConcurrentMoves = 2; // Set this in Inspector
    private List<Leg> activeMovingLegs = new List<Leg>();

    [Header("Stability")]
    [SerializeField] private int minGroundedLegs = 2;

    private bool useGroupA = true;

    // [Header("Separate Start Times")]
    // [SerializeField] private bool useSeparateStartTimes = false;
    // [SerializeField] private List<float> separateStartTimes = new List<float>();    // Make sure separateStartTimes matches legs.Count if useSeparateStartTimes is true


    private int GetGroundedLegs()
    {
        int grounded = 0;
        foreach (var leg in legs)
        {
            if (!leg.IsMoving) grounded++;
        }
        return grounded;
    }

    void Start()
    {

        // Subscribe to each leg's OnLegMovementStarted and OnLegMovementFinished event
        for (int i = 0; i < legs.Count; i++)
        {
            var leg = legs[i];
            leg.OnLegMovementStarted += OnLegStartedMoving;
            leg.OnLegMovementFinished += OnLegDoneMoving;

            // Set legCopies interfaces
            legCopies[i] = new LegCopy(leg.LegName, LegCopy.State.Idle, leg.legGroupName);
        }
    }

    void Update()
    {
        // Calculate Urgency for each leg group
        List<Leg> legs = new List<Leg>();
        foreach (var leg in legs)
        {

        }

        if (GetGroundedLegs() < minGroundedLegs) return;

        // Collect eligible legs in the current group
        List<Leg> eligibleLegs = new List<Leg>();
        foreach (var leg in legs)
        {
            if (!leg.IsMoving &&
                leg.CalculateUrgency() >= 1f &&
                leg.legGroupName == (useGroupA ? "A" : "B")
            )
            {
                eligibleLegs.Add(leg);
            }
        }

        eligibleLegs.Sort((a, b) => b.CalculateUrgency().CompareTo(a.CalculateUrgency()));

        // Move legs in the current group
        foreach (var leg in eligibleLegs)
        {
            if (activeMovingLegs.Count < maxConcurrentMoves)
            {
                leg.StartMove(leg.PositionToMove);
                activeMovingLegs.Add(leg);
            }
        }
    }

    private void SwitchActiveGroup()
    {
        useGroupA = !useGroupA;

        // Reset the leg state for the new active group
        foreach (Leg leg in legs)
        {
            if (leg.legGroupName == (useGroupA ? "A" : "B"))
            {
                leg.ResetLegState();
            }
        }

        // Reset the legCopies states for the new active group
        foreach (LegCopy playerLeg in legCopies)
        {
            if (playerLeg.groupName == (useGroupA ? "A" : "B"))
            {
                playerLeg.SetState(LegCopy.State.Idle);
                playerLeg.hasMoved = false;
            }
        }
    }

    /// <summary>
    /// Called once a leg has started moving
    /// </summary>
    private void OnLegStartedMoving(Leg leg)
    {
        // Find the this leg in the legCopies list
        for (int i = 0; i < legCopies.Count; i++)
        {
            if (String.Equals(legCopies[i].name, leg.LegName))
            {
                legCopies[i].SetState(LegCopy.State.Moving);
            }
        }
    }

    /// <summary>
    /// Called once a leg has finished moving,
    /// Removes the leg from the active moving legs list
    /// </summary>
    private void OnLegDoneMoving(Leg leg)
    {
        if (activeMovingLegs.Contains(leg))
        {
            activeMovingLegs.Remove(leg);
        }

        bool allLegsInCurrentGroupDone = true;
        // Find the this leg in the legCopies list
        for (int i = 0; i < legCopies.Count; i++)
        {
            // Check if the leg's state is not marked as 'Done Moving'
            if (String.Equals(legCopies[i].name, leg.LegName) && !legCopies[i].IsDoneMoving())
            {
                legCopies[i].SetState(LegCopy.State.DoneMoving);
                legCopies[i].hasMoved = true;
                Debug.Log(legCopies[i].GetState().ToString());
            }

            // Find all the legs in the same group as the leg that has called this event
            // And check if they have all completed as well
            if (legCopies[i].groupName == leg.legGroupName && !legCopies[i].hasMoved)
            {
                Debug.Log(legCopies[i].groupName);

                allLegsInCurrentGroupDone = false;
            }
        }

        if (allLegsInCurrentGroupDone) SwitchActiveGroup();
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            normal = { textColor = Color.white }
        };

        GUILayout.BeginVertical("box");

        // Add group priority display
        GUILayout.Label("Group Priority:", style);
        GUILayout.Label($"Group A: {(useGroupA ? "Priority" : "---")}", style);
        GUILayout.Label($"Group B: {(!useGroupA ? "Priority" : "---")}", style);

        GUILayout.Space(10);
        GUILayout.Label("Leg States:", style);

        for (int i = 0; i < legs.Count; i++)
        {
            string legStatus;
            if (legs[i].IsMoving) legStatus = "Moving";
            else if (legs[i].IsDone) legStatus = "Done";
            else legStatus = "Not Started";

            GUILayout.Label($"Leg {i} ({legs[i].legGroupName})", style);
            GUILayout.Label($"Status: {legStatus}", style);
            GUILayout.Label($"Urgency: {legs[i].CalculateUrgency()}", style);
            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}