using System;
using UnityEngine;

public class LegMoveState : LegBaseState
{
    public override void EnterState(Leg leg)
    {
        Debug.Log("Started moving leg: " + leg.legName);
    }

    public override void ExitState(Leg leg)
    {
        Debug.Log("Leg stopped moving: " + leg.legName);
    }

    public override void FixedUpdateState(Leg leg)
    {

    }

    public override void UpdateState(Leg leg)
    {
        
    }
}