using UnityEngine;

public class LegIdleState : LegBaseState
{
    public override void EnterState(Leg leg)
    {
        //Debug.Log("Leg in Idle state: " + leg.legName);
    }

    public override void ExitState(Leg leg)
    {
        //Debug.Log("Leg exiting Idle state: " + leg.legName);
    }

    public override void FixedUpdate(Leg leg)
    {
        //leg.transform.position = leg.currentPosition;
    }

    public override void Update(Leg leg)
    {
        leg.transform.position = leg.currentPosition;   // Stay at the same position relative to the world position
        leg.transform.eulerAngles = new Vector3(0, leg.currentRotation, 0);

        leg.UpdatePositionToMove(); // Update target position every frame
    }
}