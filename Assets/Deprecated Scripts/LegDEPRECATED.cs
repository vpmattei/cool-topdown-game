using UnityEngine;

public enum LegType { Left, Right, Other }

public class LegDEPRECATED : MonoBehaviour
{
    [Header("Identification")]
    public LegType legType = LegType.Other; // Assign in inspector instead of using strings.

    [HideInInspector] public Vector3 oldPosition;
    [HideInInspector] public Vector3 currentPosition;
    [HideInInspector] public Vector3 newPosition;
    [HideInInspector] public float footSpacing;
    [HideInInspector] public float lerp;
    [HideInInspector] public bool shouldMove; // renamed from moveLeg for clarity
    [HideInInspector] public bool isMoving;

    void Start()
    {
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
        lerp = 1f;
        shouldMove = false;
        isMoving = false;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}