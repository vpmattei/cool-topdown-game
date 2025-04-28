using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LegsManager : MonoBehaviour
{
    public event Action<float> OnStepDistanceValueChanged;
    public event Action<float> OnMoveDurationValueChanged;
    public event Action<float> OnStepHeightValueChanged;
    public event Action<float> OnLegIntervalValueChanged;

    [Header("Terrain Settings")]
    [SerializeField] private LayerMask terrainLayer;

    [Header("Movement Settings")]
    [Tooltip("The threshold after which movement should start")]
    [SerializeField] private float stepDistance = 2;      // The threshold after which movement should start
    
    [Tooltip("How long it takes for each leg to move")]
    [SerializeField] private float moveDuration = .2f;      // How long each leg moves

    [Tooltip("How high each leg goes")]
    [SerializeField] private float stepHeight = 4f;         // How high each leg goes

    [Tooltip("Time interval between legs starting movement")]
    [SerializeField] private float legInterval = .1f;       // Time interval between legs starting movement

    [Tooltip("Maximum amount of Urgency that each leg can tolerate, the smaller the value, the more 'sensitive' the leg becomes, meaning it will trigger a movement more easily", order = 1)]
    [SerializeField, Range(.1f, 1f)]
    private float legSesitivity = 1f;     // Maximum amount of Urgency that each leg can tolerate

    [Header("Leg Group Settings")]
    [Tooltip("Define the group names available for the legs.")]
    public List<string> legGroups = new List<string>();
    private List<float> legGroupUrgency;
    private string mostUrgentLegGroup = null;


    [Header("Legs")]
    [SerializeField] private List<Leg> legs = new List<Leg>();

    [Header("Concurrency")]
    private int movingLegs;
    [SerializeField] private int maxConcurrentMovingLegs = 2; // Set this in Inspector

    [Header("Stability")]
    private int groundedLegs;
    [SerializeField] private int minGroundedLegs = 3;

    void Start()
    {
        // Set the current active leg group to first in the list
        mostUrgentLegGroup = legGroups[0];

        // Define the size of the legGroupUrgency List
        legGroupUrgency = new List<float>(legGroups.Count);
        for (int i = 0; i < legGroups.Count; i++)
        {
            legGroupUrgency.Add(0f);   // or whatever default you like
        }
    }

    void Update()
    {
        // Update every leg group urgency
        UpdateLegGroupUrgency();

        // Update the current active leg group based on the overall urgency of the group
        mostUrgentLegGroup = MostUrgentLegGroup;

        UpdateGroundedAndMovingLegs();
        // If we don't have the minimum amount of legs grounded, don't bother moving any more extra legs, so we return
        if (groundedLegs < minGroundedLegs) return;

        // If we surpass the max cocurrent moving legs, don't bother moving any more extra legs, so we return
        if (movingLegs > maxConcurrentMovingLegs) return;

        // Then for each leg of the current active leg group, move a leg if they have surpassed the urgency limit = 1
        foreach (Leg urgentLeg in legs)
        {
            if (urgentLeg.SelectedGroupName == mostUrgentLegGroup &&
                urgentLeg.LegUrgency >= legSesitivity &&
                urgentLeg.currentLegState == urgentLeg.IdleState)
            {
                urgentLeg.StartMovevement();
            }
        }
    }

    private void UpdateGroundedAndMovingLegs()
    {
        int grounded = 0;
        int moving = 0;
        foreach (var leg in legs)
        {
            if (leg.currentLegState == leg.IdleState)
            {
                grounded++;
            }
            else if (leg.currentLegState == leg.MoveState)
            {
                moving++;
            }
        }

        groundedLegs = grounded;
        movingLegs = moving;
    }

    private void UpdateLegGroupUrgency()
    {
        int index = 0;

        foreach (string legGroupWithUrgencyToUpdate in legGroups)
        {
            float groupUrgency = 0;

            foreach (Leg leg in legs)
            {
                if (leg.SelectedGroupName == legGroupWithUrgencyToUpdate)
                {
                    groupUrgency += leg.LegUrgency;
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
        GUILayout.Label($"Most Urgent Leg Group: {mostUrgentLegGroup}", style);
        GUILayout.Label($"Group A: {(legGroupUrgency[0] >= 2 ? "PRIORITY" : legGroupUrgency[0])}", style);
        GUILayout.Label($"Group B: {(legGroupUrgency[1] >= 2 ? "PRIORITY" : legGroupUrgency[1])}", style);

        GUILayout.Space(10);
        GUILayout.Label($"Grounded Legs: {groundedLegs}", style);

        GUILayout.Space(10);
        GUILayout.Label("Leg States:", style);

        for (int i = 0; i < legs.Count; i++)
        {
            string legStatus;

            if (legs[i].currentLegState == legs[i].MoveState) legStatus = "Moving";
            else legStatus = "Idle";

            GUILayout.Label($"Leg {legs[i].legName} ({legs[i].LegUrgency})", style);
            GUILayout.Label($"Status: {legStatus}", style);
            GUILayout.Space(5);
        }

        GUILayout.EndVertical();
    }
}