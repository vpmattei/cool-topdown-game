using UnityEngine;

public class SecondOrderDynamics : MonoBehaviour
{
    [Range(1, 20)]
    public int duration = 10;
    [Range(0.01f, 10f)]
    public float frequency = 1;
    [Range(0f, 10f)]
    public float dampingRatio = .5f;   // zeta
    [Range(-10f, 10f)]
    public float anticipationRatio = -2;

    public GameObject target;
    public AnimationCurve m_FunctionCurve = new AnimationCurve();

    // Target
    private Vector3 targetCurrentPos, targetPreviousPos, targetVelocity;    // Target's current position, previous position and velocity
    [SerializeField] private Quaternion targetCurrentRot, targetPreviousRot;                 // Target's current rotation, previous rotation and angular velocity
    [SerializeField] private Vector3 targetAngVelocity;

    // Object
    private Vector3 pos, vel;       // Position and velocity
    public Quaternion rot;         // Rotation
    public Vector3 angVel;         // Angular velocity
    private float k1, k2, k3;       // Dynamics constants

    public void Start()
    {
        // Compute constants
        k1 = dampingRatio / (Mathf.PI * frequency);
        k2 = 1 / ((2 * Mathf.PI * frequency) * (2 * Mathf.PI * frequency));
        k3 = anticipationRatio * dampingRatio / (2 * Mathf.PI * frequency);

        // Initialize variables
        targetCurrentPos = target.transform.position;
        targetCurrentRot = target.transform.rotation;

        pos = target.transform.position;
        vel = Vector3.zero;
        rot = target.transform.rotation;
        angVel = Vector3.zero;
    }

    public void Update()
    {
        // Re-Compute constants
        k1 = dampingRatio / (Mathf.PI * frequency);
        k2 = 1 / ((2 * Mathf.PI * frequency) * (2 * Mathf.PI * frequency));
        k3 = anticipationRatio * dampingRatio / (2 * Mathf.PI * frequency);
    }

    public void FixedUpdate()
    {
        // Get target's new position and rotation
        targetCurrentPos = target.transform.position;
        targetCurrentRot = target.transform.rotation;

        // Estimate velocity
        targetVelocity = (targetCurrentPos - targetPreviousPos) / Time.deltaTime;
        targetPreviousPos = targetCurrentPos;

        // Estimate Angular Velovity
        targetAngVelocity = (targetCurrentRot.eulerAngles - targetPreviousRot.eulerAngles) / Time.deltaTime;
        targetPreviousRot = targetCurrentRot;

        float k2_stable = Mathf.Max(k2, 1.1f * (Time.deltaTime * Time.deltaTime / 4 + Time.deltaTime * k1 / 2)); // Clamp k2 to guarantee stability

        pos = pos + Time.deltaTime * vel;                                                                   // Integrate position by velocity
        vel = vel + Time.deltaTime * (targetCurrentPos + k3 * targetVelocity - pos - k1 * vel) / k2_stable; // Integrate velocity by acceleration

        rot = Quaternion.Euler(rot.eulerAngles + Time.deltaTime * angVel);                                                                      // Integrate rotation by angular velocity
        angVel = angVel + Time.deltaTime * (targetCurrentRot.eulerAngles + k3 * targetAngVelocity - rot.eulerAngles - k1 * angVel) / k2_stable; // Integrate angular velocity by angular acceleration

        this.transform.position = pos;
        this.transform.rotation = rot;
    }
}