using Unity.VisualScripting;
using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    [SerializeField] LayerMask terrainLayer;
    [SerializeField] Transform body = default;
    [SerializeField] float stepDuration = .25f;
    [SerializeField] float stepDistance = 0.6f;
    [SerializeField] float stepHeight = 0.4f;
    [SerializeField] float stepInterval = .25f;
    private float currentStepInterval;
    private int legIndex = 0;
    [SerializeField] Vector3 footOffset = new Vector3(0, 0.1f, 0);

    [SerializeField] Leg[] legs;
    // float footSpacing;


    // Vector3 oldPosition, currentPosition, newPosition;
    // Vector3 oldNormal, currentNormal, newNormal;
    // float lerp;

    private void Start()
    {
        currentStepInterval = 0;
    }

    void Update()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            legs[i].transform.position = legs[i].currentPosition;   // Keeps the leg in the same position

            if (legIndex == i && currentStepInterval >= stepInterval && !legs[i].IsMoving())
            {
                legs[i].moveLeg = true;
            }

            if (legs[i].moveLeg) MoveLeg(legs[i]);    // Moves the leg with the current leg index
        }

        if (currentStepInterval >= stepInterval)
        {
            currentStepInterval = 0;
            legIndex += 1;
            if (legIndex + 1 > legs.Length) legIndex = 0;   // Make leg index go back to the start of the leg list so we restart
        }

        currentStepInterval += Time.deltaTime;
    }

    public void MoveLeg(Leg leg)
    {
        leg.isMoving = true;
        // transform.up = currentNormal;

        Ray bodyRay = new Ray(body.position + new Vector3(0, .5f, 0) + (body.right * leg.footSpacing), Vector3.down);
        Debug.DrawRay(body.position + new Vector3(0, .5f, 0), new Vector3(0, -30, 0), Color.green);

        if (Physics.Raycast(bodyRay, out RaycastHit hit, 30, terrainLayer.value))
        {
            // Debug.Log("Distance: " + Vector3.Distance(newPosition, hit.point));
            if (Vector3.Distance(leg.newPosition, hit.point) > stepDistance)
            {
                int direction = transform.InverseTransformPoint(hit.point).z > transform.InverseTransformPoint(leg.newPosition).z ? 1 : -1;
                leg.newPosition = hit.point + footOffset; //+ (transform.forward * stepDistance * direction);
                leg.lerp = 0;
                // newNormal = hit.normal;
            }

            if (leg.lerp < stepDuration)
            {
                Vector3 tempPosition = Vector3.Lerp(leg.oldPosition, leg.newPosition, leg.lerp / stepDuration);
                tempPosition.y += Mathf.Sin(leg.lerp / stepDuration * Mathf.PI) * stepHeight;

                leg.currentPosition = tempPosition;

                // currentNormal = Vector3.Lerp(oldNormal, newNormal, leg.lerp);
                leg.lerp += Time.deltaTime;
            }
            else
            {
                leg.oldPosition = leg.newPosition;
                leg.isMoving = false;
                leg.moveLeg = false;

                // oldNormal = newNormal;
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (Leg leg in legs)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leg.newPosition, 0.05f);
        }
    }
}
