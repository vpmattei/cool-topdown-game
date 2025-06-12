using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class BoneRotationLimiter : MonoBehaviour
{
    [Header("Limits (Local Euler °)")]
    public Vector3 minLocalEuler = new Vector3(-30, -10, -10);
    public Vector3 maxLocalEuler = new Vector3(60, 10, 10);

    [Header("Gizmo Settings")]
    public float gizmoRadius = 0.2f;
    public int gizmoSteps = 60;
    public float arrowSize = 0.05f;

    void LateUpdate()
    {
        Vector3 e = transform.localEulerAngles;
        e.x = (e.x > 180f) ? e.x - 360f : e.x;
        e.y = (e.y > 180f) ? e.y - 360f : e.y;
        e.z = (e.z > 180f) ? e.z - 360f : e.z;

        e.x = Mathf.Clamp(e.x, minLocalEuler.x, maxLocalEuler.x);
        e.y = Mathf.Clamp(e.y, minLocalEuler.y, maxLocalEuler.y);
        e.z = Mathf.Clamp(e.z, minLocalEuler.z, maxLocalEuler.z);

        transform.localEulerAngles = e;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Vector3 center = transform.position;
        int steps = Mathf.Max(4, gizmoSteps);

        DrawSector(center, transform.right, transform.up, minLocalEuler.x, maxLocalEuler.x, Color.red, steps);
        DrawSector(center, transform.up, transform.forward, minLocalEuler.y, maxLocalEuler.y, Color.green, steps);
        DrawSector(center, transform.forward, transform.right, minLocalEuler.z, maxLocalEuler.z, Color.blue, steps);
    }

    private void DrawSector(Vector3 center, Vector3 axis, Vector3 startDir, float minAngle, float maxAngle, Color col, int steps)
    {
        // 1) Solid fill
        Handles.color = new Color(col.r, col.g, col.b, 0.1f);

        // compute the actual "zero" of the wedge by rotating startDir by minAngle
        Vector3 realStartDir = Quaternion.AngleAxis(minAngle, axis) * startDir.normalized;
        Handles.DrawSolidArc(center, axis, realStartDir, maxAngle - minAngle, gizmoRadius);

        // 2) Outline
        Handles.color = col;
        float delta = (maxAngle - minAngle) / steps;
        Vector3 prev = center + Quaternion.AngleAxis(minAngle, axis) * startDir.normalized * gizmoRadius;

        for (int i = 1; i <= steps; i++)
        {
            float ang = minAngle + delta * i;
            Vector3 next = center + Quaternion.AngleAxis(ang, axis) * startDir.normalized * gizmoRadius;
            Handles.DrawLine(prev, next);
            prev = next;
        }

        // 3) Arrows at both ends
        DrawArrow(center, axis, startDir, minAngle, col);
        DrawArrow(center, axis, startDir, maxAngle, col);
    }

    private void DrawArrow(Vector3 center, Vector3 axis, Vector3 startDir, float angleDeg, Color col)
    {
        Vector3 dir = Quaternion.AngleAxis(angleDeg, axis) * startDir.normalized;
        Vector3 pos = center + dir * gizmoRadius;

        Handles.color = col;
        Vector3 tangent = Quaternion.AngleAxis(90, axis) * dir;
        Quaternion rot = Quaternion.LookRotation(dir, tangent);
        Handles.ConeHandleCap(0, pos, rot, arrowSize, EventType.Repaint);
    }
#endif

    /// <summary>
    /// Rotates 'point' around 'center' by 'angleDeg' degrees around 'axis', preserving distance.
    /// </summary>
    public Vector3 RotatePointAroundCenter(Vector3 point, Vector3 center, float angleDeg, Vector3 axis = default)
    {
        if (axis == default) axis = Vector3.up;
        // Compute direction from center to point
        Vector3 dir = point - center;
        // Rotate that direction
        Vector3 rotatedDir = Quaternion.AngleAxis(angleDeg, axis) * dir;
        // Build the new point
        return center + rotatedDir;
    }
}
