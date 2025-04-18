using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LegsManager : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private LayerMask terrainLayer;

    [Header("Movement Settings")]
    [SerializeField] private float stepDistance = 10f;   // The threshold after which movement should start
    [SerializeField] private float moveDuration = 2f;    // How long each leg moves
    [SerializeField] private float stepHeight = 4f;      // How high each leg goes
    [SerializeField] private float legInterval = 1f;     // Time interval between legs starting movement

    [Header("Leg Group Settings")]
    [Tooltip("Define the group names available for the legs.")]
    public List<string> legGroups = new List<string>();
    // TODO: Create legGroupUrgency for each legGroup
    private List<float> legGroupUrgency;
    // TODO: Implement new mostUrgentLegGroup logic
    private string mostUrgentLegGroup = null;
    //private bool useGroupA = true;


    [Header("Legs")]
    [SerializeField] private List<Leg> legs = new List<Leg>();

    // TODO: Replace playerLegs with new state system
    //[SerializeField] private List<PlayerLeg> playerLegs = new List<PlayerLeg>();

    [Header("Concurrency")]
    [SerializeField] private int maxConcurrentMoves = 2; // Set this in Inspector
    private List<Leg> activeMovingLegs = new List<Leg>();

    [Header("Stability")]
    [SerializeField] private int minGroundedLegs = 2;


    // [Header("Separate Start Times")]
    // [SerializeField] private bool useSeparateStartTimes = false;
    // [SerializeField] private List<float> separateStartTimes = new List<float>();    // Make sure separateStartTimes matches legs.Count if useSeparateStartTimes is true

    void Start()
    {
        // Set the current active leg group to first in the list
        mostUrgentLegGroup = legGroups[0];
        
        // Define the size of the legGroupUrgency List
        legGroupUrgency = new List<float>(legGroups.Count);

        // Subscribe to each leg's OnLegMovementStarted and OnLegMovementFinished event
        for (int i = 0; i < legs.Count; i++)
        {
            var leg = legs[i];
            leg.OnLegMovementStarted += OnLegStartedMoving;
            leg.OnLegMovementFinished += OnLegDoneMoving;

            // TODO: Replace playerLegs with new state system
            // Set playerLegs interfaces
            //playerLegs[i] = new PlayerLeg(leg.LegName, PlayerLeg.State.Idle, leg.legGroup);
        }
    }

    void Update()
    {
        // TODO: Update every leg group urgency
        UpdateLegGroupUrgency();

        // TODO: Update the current active leg group based on the overall urgency of the group
        mostUrgentLegGroup = MostUrgentLegGroup;

        // If we don't have the minimum amount of legs grounded, don't bother moving any more extra legs, so we return
        if (GetGroundedLegs() < minGroundedLegs) return;

        // TODO: Then for each leg of the current active leg group, move a leg if they have surpassed the urgency limit = 1

        // Collect eligible legs in the current group
        /*
        List<Leg> eligibleLegs = new List<Leg>();
        foreach (var leg in legs)
        {
            // For each leg, we first
            // Check if they are idle
            // Then if they have an urgency >= 1 
            // Then if the current active group is the one from the leg 
            if (leg.currentLegState == leg.IdleState && leg.CalculateUrgency() >= 1f &&
                leg.SelectedGroupName == (useGroupA ? Leg.LegGroup.GroupA : Leg.LegGroup.GroupB))
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
        */
    }

    private int GetGroundedLegs()
    {
        int grounded = 0;
        foreach (var leg in legs)
        {
            if (leg.currentLegState == leg.IdleState)
            {
                grounded++;
            }
            //if (!leg.IsMoving) grounded++;
        }
        return grounded;
    }


    // TODO: Remove this deprecated method
    private void SwitchActiveGroup()
    {
        //useGroupA = !useGroupA;

        // Reset the leg state for the new active group
        /*
        foreach (Leg leg in legs)
        {
            if (leg.legGroup == (useGroupA ? Leg.LegGroup.GroupA : Leg.LegGroup.GroupB))
            {
                leg.ResetLegState();
            }
        }
        */

        // TODO: Replace playerLegs with new state system
        // Reset the playerLegs states for the new active group
        /*foreach (PlayerLeg playerLeg in playerLegs)
        {
            if (playerLeg.group == (useGroupA ? Leg.LegGroup.GroupA : Leg.LegGroup.GroupB))
            {
                playerLeg.SetState(PlayerLeg.State.Idle);
                playerLeg.hasMoved = false;
            }
        }*/
    }

    /// <summary>
    /// Called once a leg has started moving
    /// </summary>
    private void OnLegStartedMoving(Leg leg)
    {
        // TODO: Replace playerLegs with new state system
        // Find the this leg in the playerLegs list
        /*for (int i = 0; i < playerLegs.Count; i++)
        {
            if (String.Equals(playerLegs[i].name, leg.LegName))
            {
                playerLegs[i].SetState(PlayerLeg.State.Moving);
            }
        }*/
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

        //bool allLegsInCurrentGroupDone = true;

        // TODO: Replace playerLegs with new state system
        // Find the this leg in the playerLegs list
        /*for (int i = 0; i < playerLegs.Count; i++)
        {
            // Check if the leg's state is not marked as 'Done Moving'
            if (String.Equals(playerLegs[i].name, leg.LegName) && !playerLegs[i].IsDoneMoving())
            {
                playerLegs[i].SetState(PlayerLeg.State.DoneMoving);
                playerLegs[i].hasMoved = true;
                Debug.Log(playerLegs[i].GetState().ToString());
            }

            // Find all the legs in the same group as the leg that has called this event
            // And check if they have all completed as well
            if (playerLegs[i].group == leg.legGroup && !playerLegs[i].hasMoved)
            {
                Debug.Log(playerLegs[i].group.ToString());

                allLegsInCurrentGroupDone = false;
            }
        }*/

        //if (allLegsInCurrentGroupDone) SwitchActiveGroup();
    }

    private void UpdateLegGroupUrgency()
    {
        int index = 0;

        foreach (string legGroupUrgencyToUpdate in legGroups)
        {
            float groupUrgency = 0;

            foreach (Leg leg in legs)
            {
                if (leg.SelectedGroupName == legGroupUrgencyToUpdate)
                {
                    groupUrgency += leg.CalculateUrgency();
                }
            }

            legGroupUrgency[index] = groupUrgency;
            index++;
        }
    }

    private string MostUrgentLegGroup
    {
        get
        {
            string mostUrgentLegGroup = null;
            float groupUrgency = 0;

            int i = 0;
            foreach (float legGrUcy in legGroupUrgency)
            {
                if (groupUrgency <= legGrUcy)
                {
                    groupUrgency = legGrUcy;

                    mostUrgentLegGroup = legGroups[i];
                }
                i++;
            }

            return mostUrgentLegGroup;
        }
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
        GUILayout.Label($"Group A: {(legGroupUrgency[0] >= 2 ? "PRIORITY" : legGroupUrgency[0])}", style);
        GUILayout.Label($"Group B: {(legGroupUrgency[1] >= 2 ? "PRIORITY" : legGroupUrgency[1])}", style);

        GUILayout.Space(10);
        GUILayout.Label("Leg States:", style);

        for (int i = 0; i < legs.Count; i++)
        {
            string legStatus;

            if (legs[i].currentLegState == legs[i].MoveState) legStatus = "Moving";
            else legStatus = "Idle";

            GUILayout.Label($"Leg {i} ({legGroupUrgency[legs[i].selectedGroupIndex]})", style);
            GUILayout.Label($"Status: {legStatus}", style);
            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}