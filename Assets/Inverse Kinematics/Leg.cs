using UnityEngine;

public class Leg : MonoBehaviour
{
    [HideInInspector] public Vector3 oldPosition, currentPosition, newPosition;
    [HideInInspector] public float footSpacing;
    [HideInInspector] public float lerp;

    void Start()
    {
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
        lerp = 1;
    }

    public bool IsMoving()
    {
        return lerp < 1;
    }
}
