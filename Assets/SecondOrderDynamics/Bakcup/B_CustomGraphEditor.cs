//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(CustomGraph))]
//public class CustomGraphEditor : Editor
//{
//    private Texture2D graphTexture;

//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        if (GUILayout.Button("Update Graph"))
//        {
//            Repaint();
//        }
//    }

//    void OnSceneGUI()
//    {
//        CustomGraph graph = (CustomGraph)target;

//        Handles.color = Color.white;

//        float halfWidth = graph.width / 2f;
//        float halfHeight = graph.height / 2f;
//        Vector3 bottomLeft = new Vector3(graph.width - halfWidth, graph.height - halfHeight);
//        Vector3 topRight = new Vector3(graph.width + halfWidth, graph.height + halfHeight);

//        Handles.DrawLine(new Vector3(bottomLeft.x, bottomLeft.y), new Vector3(topRight.x, bottomLeft.y));
//        Handles.DrawLine(new Vector3(topRight.x, bottomLeft.y), new Vector3(topRight.x, topRight.y));

//        float handleSize = 0.04f;
//        float timeStep = 0.1f;

//        float currentTime = 0f;
//        Vector2 previousPosition = Vector2.zero;
//        Vector2 currentPosition = Vector2.zero;

//        Handles.color = Color.green;

//        while (currentTime <= 10f)
//        {
//            currentPosition.x = currentTime;
//            currentPosition.y = graph.dynamicsFunction.Evaluate(currentTime);

//            currentPosition.x = Mathf.Lerp(bottomLeft.x, topRight.x, Mathf.InverseLerp(0f, 10f, currentPosition.x));
//            currentPosition.y = Mathf.Lerp(bottomLeft.y, topRight.y, Mathf.InverseLerp(-graph.height / 2f, graph.height / 2f, currentPosition.y));

//            if (currentTime > 0f)
//            {
//                Handles.DrawLine(previousPosition, currentPosition);
//            }

//            previousPosition = currentPosition;
//            currentTime += timeStep;
//        }
//    }
//}