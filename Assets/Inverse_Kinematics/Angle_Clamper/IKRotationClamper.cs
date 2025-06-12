using UnityEngine;

[DefaultExecutionOrder(100)]
public class IKRotationClamper : MonoBehaviour
{
    [Tooltip("For each bone you want to clamp, assign its Transform and the min/max Local Euler angles.")]
    [SerializeField] private BoneLimit[] limits;

    void LateUpdate()
    {
        foreach (var bl in limits)
            ClampBone(bl);
    }

    private void ClampBone(BoneLimit bl)
    {
        if (bl.bone == null) return;
        // Grab current local rotation as a –180..+180 Vector3
        Vector3 e = bl.bone.localEulerAngles;
        
        e.x = Normalize(e.x);
        e.y = Normalize(e.y);
        e.z = Normalize(e.z);

        // Clamp against your pasted values
        e.x = Mathf.Clamp(e.x, bl.minLocalEuler.x, bl.maxLocalEuler.x);
        e.y = Mathf.Clamp(e.y, bl.minLocalEuler.y, bl.maxLocalEuler.y);
        e.z = Mathf.Clamp(e.z, bl.minLocalEuler.z, bl.maxLocalEuler.z);

        // Write back
        bl.bone.localEulerAngles = e;
    }

    private float Normalize(float angle)
    {
        return (angle > 180f) ? angle - 360f : angle;
    }
}