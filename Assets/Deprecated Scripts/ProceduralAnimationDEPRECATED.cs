using TMPro;
using UnityEngine;

public class ProceduralAnimationDEPRECATED : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private float stepDuration = 0.25f;
    [SerializeField] private float stepDistance = 0.6f;
    [SerializeField] private float stepHeight = 0.4f;
    [SerializeField] private float stepInterval = 0.25f;
    [SerializeField] private float raycastDistance = 30f;
    [SerializeField] private Vector3 footOffset = new Vector3(0, 0.1f, 0);

    [Header("Legs")]
    [SerializeField] private LegDEPRECATED[] legs;

    [Header("Visualizations")]
    [SerializeField] private GameObject circleRendererObject;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text leftLegDistanceText;
    [SerializeField] private TMP_Text rightLegDistanceText;
    [SerializeField] private TMP_Text currentLegText;

    private Vector3 oldBodyPosition;
    private Vector3 currentBodyPosition;
    private CircleRenderer circleRenderer;
    private float currentStepInterval;
    private int legIndex = 0;

    private void Start()
    {
        // Setting old body position
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit hit, raycastDistance, terrainLayer))
        {
            currentBodyPosition = oldBodyPosition = hit.point;
        }

        currentStepInterval = 0f;

        // Cache the CircleRenderer component
        if (circleRendererObject != null)
        {
            circleRenderer = circleRendererObject.GetComponent<CircleRenderer>();
        }
        else
        {
            Debug.LogWarning("CircleRendererObject is not assigned.");
        }

        // UI checks
        if (leftLegDistanceText == null)
            Debug.LogWarning("LeftLegDistanceText is not assigned.");
        if (rightLegDistanceText == null)
            Debug.LogWarning("RightLegDistanceText is not assigned.");
        if (currentLegText == null)
            Debug.LogWarning("CurrentLegText is not assigned.");
    }

    void Update()
    {
        Debug.DrawRay(transform.position + new Vector3(0, 0.5f, 0), Vector3.down * raycastDistance, Color.green);
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down, out RaycastHit hit, raycastDistance, terrainLayer))
        {
            currentBodyPosition = hit.point;

            float distance = Vector3.Distance(oldBodyPosition, currentBodyPosition);
            UpdateStepDistanceUI(distance);

            if (distance > stepDistance)
            {
                for (int i = 0; i < legs.Length; i++) legs[i].shouldMove = true;

                oldBodyPosition = currentBodyPosition;
            }

            for (int i = 0; i < legs.Length; i++)
            {
                // Maintain the leg's transform position
                legs[i].transform.position = legs[i].currentPosition;

                // Move the leg if needed
                if (legs[i].shouldMove)
                {
                    MoveLeg(legs[i]);
                }
            }
        }

        // After checking legs, if interval is surpassed, possibly switch legs
        if (currentStepInterval >= stepInterval)
        {
            currentStepInterval = 0f;

            // Check if the current leg completed its move before switching
            if (!legs[legIndex].IsMoving() && legs[legIndex].shouldMove == false)
            {
                legIndex = (legIndex + 1) % legs.Length;
            }
        }

        currentStepInterval += Time.deltaTime;

        UpdateCurrentLegUI();
    }

    private void MoveLeg(LegDEPRECATED leg)
    {
        leg.isMoving = true;

        Vector3 bodyPosition = transform.position + Vector3.up * 0.5f + transform.right * leg.footSpacing;
        if (Physics.Raycast(bodyPosition, Vector3.down, out RaycastHit hit, raycastDistance, terrainLayer))
        {
            float distance = Vector3.Distance(leg.newPosition, hit.point);


            if (distance > stepDistance)
            {
                leg.newPosition = hit.point + footOffset;
                leg.lerp = 0f; // reset lerp for new movement
            }

            if (leg.lerp < stepDuration)
            {
                float t = leg.lerp / stepDuration;
                Vector3 tempPosition = Vector3.Lerp(leg.oldPosition, leg.newPosition, t);
                // Add vertical offset using a sine curve for the step arc
                tempPosition.y += Mathf.Sin(t * Mathf.PI) * stepHeight;

                leg.currentPosition = tempPosition;
                leg.lerp += Time.deltaTime;
            }
            else
            {
                // Movement complete
                leg.oldPosition = leg.newPosition;
                leg.isMoving = false;
                leg.shouldMove = false;

                if (circleRenderer != null)
                {
                    circleRenderer.DrawCircle(100, stepDistance, transform.position);
                }
            }
        }
    }

    private void UpdateStepDistanceUI(float distance)
    {
        // Only update UI if references are assigned
        leftLegDistanceText.text = $"Distance: {distance:F2}";
    }

    private void UpdateCurrentLegUI()
    {
        if (currentLegText != null && legIndex >= 0 && legIndex < legs.Length)
        {
            currentLegText.text = $"Current LegDEPRECATED: {legs[legIndex].legType}";
        }
    }

    private void DrawDebugRay()
    {
    }

    private void OnDrawGizmos()
    {
        if (legs == null) return;
        foreach (LegDEPRECATED leg in legs)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leg.newPosition, 0.05f);
        }
    }
}