using UnityEngine;

public class LegMoveState : LegBaseState
{
    public override void EnterState(Leg leg)
    {
        //Debug.Log("Started moving leg: " + leg.legName);
    }

    public override void ExitState(Leg leg)
    {
        //Debug.Log("Leg stopped moving: " + leg.legName);
    }

    public override void FixedUpdate(Leg leg)
    {
        leg.UpdateMove();

        leg.transform.position = leg.currentPosition;
    }

    public override void Update(Leg leg)
    {
        leg.UpdatePositionToMove(); // Update target position every frame
    }
    
    // TODO: Implement this function
    /*public void UpdateMove()
    {
        if (moveTimer < moveDuration)
        {
            // Smoothly interpolate position over time
            float t = moveTimer / moveDuration;
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, t);
            tempPosition.y += stepCurve.Evaluate(t) * stepHeight; // Apply step height curve
            currentPosition = tempPosition;
            moveTimer += Time.deltaTime; // Increment timer
        }
        else
        {
            // Movement complete
            currentPosition = newPosition;
            oldPosition = newPosition;

            // TODO: Implement new state based system
            // Exit from Move State
            currentLegState.ExitState(this);
            currentLegState = IdleState;
            // Enter Idle State
            currentLegState.EnterState(this);

            OnLegMovementFinished?.Invoke(this); // Finished moving notification to the system
            // proceduralLegAnimation.NotifyLegMovementComplete(this);
            currentRotation = body.transform.eulerAngles.y;  // Reset rotation
        }
    }
    */
}