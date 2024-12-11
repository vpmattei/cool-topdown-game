using TMPro;
using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    [SerializeField] LayerMask terrainLayer;
    [SerializeField] float stepDuration = 0.25f;
    [SerializeField] float stepDistance = 0.6f;
    [SerializeField] float stepHeight = 0.4f;
    [SerializeField] float stepInterval = 0.25f;
    private float currentStepInterval;
    private int legIndex = 0;
    [SerializeField] Vector3 footOffset = new Vector3(0, 0.1f, 0);

    [SerializeField] Leg[] legs;
    [SerializeField] GameObject circleRenderer;
    [SerializeField] GameObject[] circles;

    // UI Elements for leg distances
    [SerializeField] private TMP_Text leftLegDistanceText;
    [SerializeField] private TMP_Text rightLegDistanceText;

    private void Start()
    {
        currentStepInterval = 0;
    }

    void Update()
    {
        Debug.DrawRay(this.transform.position + new Vector3(0, 0.5f, 0), new Vector3(0, -30, 0), Color.yellow);

        for (int i = 0; i < legs.Length; i++)
        {
            legs[i].transform.position = legs[i].currentPosition; // Keeps the leg in the same position

            if (legIndex == i && currentStepInterval >= stepInterval && !legs[i].IsMoving())
            {
                legs[i].moveLeg = true;
            }

            if (legs[i].moveLeg) MoveLeg(legs[i]); // Moves the leg with the current leg index
        }

        if (currentStepInterval >= stepInterval)
        {
            currentStepInterval = 0;
            legIndex = (legIndex + 1) % legs.Length; // Cycle leg index
        }

        currentStepInterval += Time.deltaTime;
    }

    public void MoveLeg(Leg leg)
    {
        leg.isMoving = true;

        Ray bodyRay = new Ray(this.transform.position + new Vector3(0, 0.5f, 0) + (this.transform.right * leg.footSpacing), Vector3.down);

        if (Physics.Raycast(bodyRay, out RaycastHit hit, 30, terrainLayer.value))
        {
            float distance = Vector3.Distance(leg.newPosition, hit.point);

            // Update UI based on leg name
            if (leg.name == "left")
            {
                leftLegDistanceText.text = $"Left Leg Distance: {distance:F2}"; // Update left leg UI
            }
            else if (leg.name == "right")
            {
                rightLegDistanceText.text = $"Right Leg Distance: {distance:F2}"; // Update right leg UI
            }

            if (distance > stepDistance)
            {
                leg.newPosition = hit.point + footOffset;
                leg.lerp = 0;
            }

            if (leg.lerp < stepDuration)
            {
                Vector3 tempPosition = Vector3.Lerp(leg.oldPosition, leg.newPosition, leg.lerp / stepDuration);
                tempPosition.y += Mathf.Sin(leg.lerp / stepDuration * Mathf.PI) * stepHeight;

                leg.currentPosition = tempPosition;
                leg.lerp += Time.deltaTime;
            }
            else
            {
                leg.oldPosition = leg.newPosition;
                leg.isMoving = false;
                leg.moveLeg = false;

                int i = (leg.name == "left") ? 0 : 1;
                circles[i].GetComponent<CircleRenderer>().DrawCircle(100, stepDistance, leg.transform.position);
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