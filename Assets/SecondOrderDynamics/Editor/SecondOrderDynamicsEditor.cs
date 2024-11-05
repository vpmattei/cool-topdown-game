using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SecondOrderDynamics))]
public class SecondOrderDynamicsEditor : Editor
{
    SecondOrderDynamics secondOrderDynamics;
    AnimationCurve curve = new();

    void OnEnable()
    {
        secondOrderDynamics = target as SecondOrderDynamics;
        UpdateCurve();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            UpdateCurve();
        }
    }

    void OnSceneGUI()
    {
        Handles.BeginGUI();
        Rect screenRect = new(10, 10, Screen.width / 4, Screen.height / 4);

        EditorGUI.CurveField(screenRect, curve, Color.red, new Rect(0, -1, secondOrderDynamics.duration, 4));

        Handles.EndGUI();
    }

    public void UpdateCurve()
    {
        // Clear old curve data
        curve = new AnimationCurve();

        // Get public variables from script target
        float f = secondOrderDynamics.frequency;
        float d = secondOrderDynamics.dampingRatio;
        float r = secondOrderDynamics.anticipationRatio;

        // Initialize variables
        float target = 1;
        float y = 0f;           // Current Position
        float yd = 0f;          // Current Velocity

        // Compute constants
        float k1 = d / (Mathf.PI * f);
        float k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
        float k3 = r * d / (2 * Mathf.PI * f);

        int duration = secondOrderDynamics.duration;
        float timeStep = 0.01f;
        int numSteps = Mathf.FloorToInt(duration / timeStep);
        float k2_stable = Mathf.Max(k2, 1.1f * (timeStep * timeStep / 4 + timeStep * k1 / 2)); // Clamp k2 to guarantee stability

        for (int i = 0; i < numSteps; i++)
        {
            float time = i * timeStep;

            float targetVelocity = i == 0 ? 1 / timeStep : 0;

            y = y + timeStep * yd; // Integrate position by velocity
            yd = yd + timeStep * (target + k3*targetVelocity - y - k1*yd) / k2_stable;  // Integrate velocity by acceleration

            curve.AddKey(time, y);
        }

        secondOrderDynamics.m_FunctionCurve = curve;
    }
}
