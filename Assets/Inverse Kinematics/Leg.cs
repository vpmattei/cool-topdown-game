using UnityEngine;

public class Leg : MonoBehaviour
{
    [HideInInspector] public Vector3 oldPosition, currentPosition, newPosition;
    [HideInInspector] public float footSpacing;
    [HideInInspector] public float lerp;
    [HideInInspector] public bool moveLeg;
    [HideInInspector] public bool isMoving;

    void Start()
    {
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
        lerp = 1;
        moveLeg = false;
        isMoving = false;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}
