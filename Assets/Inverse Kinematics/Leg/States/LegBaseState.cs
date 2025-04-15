using System;
using UnityEngine;

public abstract class LegBaseState
{
    public abstract void EnterState(Leg leg);

    public abstract void ExitState(Leg leg);

    public abstract void FixedUpdate(Leg leg);

    public abstract void Update(Leg leg);
}