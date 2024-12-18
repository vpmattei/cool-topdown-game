using System;
using Unity.VisualScripting;
using UnityEngine;

public class CircleRenderer : MonoBehaviour
{
    LineRenderer circleRenderer;

    [Range(1, 100), SerializeField] int steps = 100;
    [Range(.01f, 10), SerializeField] float radius = 10;

    private void Awake()
    {
        circleRenderer = GetComponent<LineRenderer>();
    }

    public void DrawCircle(int steps, float radius, Vector3 pos)
    {
        this.transform.position = pos;

        circleRenderer.positionCount = steps;

        for (int currentStep = 0; currentStep < steps; currentStep++)
        {
            float circumferenceProgress = (float)currentStep / steps;

            float currentRadian = circumferenceProgress * 2 * MathF.PI;

            float xScaled = MathF.Cos(currentRadian);
            float yScaled = MathF.Sin(currentRadian);

            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPosition = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + y);

            circleRenderer.SetPosition(currentStep, currentPosition);
        }
    }
}
