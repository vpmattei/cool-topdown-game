using System;
using UnityEngine;

public class LegIdleState : LegBaseState
{
    public override void EnterState(Leg leg)
    {
        Debug.Log("Leg in Idle state: " + leg.legName);
    }

    public override void ExitState(Leg leg)
    {
        Debug.Log("Leg exiting Idle state: " + leg.legName);
    }

    public override void FixedUpdate(Leg leg)
    {

    }

    public override void Update(Leg leg)
    {

    }
}