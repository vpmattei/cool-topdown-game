//using UnityEngine;

//[CreateAssetMenu(fileName = "New Custom Graph", menuName = "Custom Graph")]
//public class CustomGraph : ScriptableObject
//{
//    [System.Serializable]
//    public class SecondOrderDynamicsFunction
//    {
//        public float A;
//        public float B;
//        public float C;
//        public float D;
//        public float E;

//        public float Evaluate(float t)
//        {
//            return A * Mathf.Exp(-B * t) * Mathf.Cos(C * t) + D * Mathf.Exp(-E * t);
//        }
//    }

//    public SecondOrderDynamicsFunction dynamicsFunction = new SecondOrderDynamicsFunction();
//    public float width = 1f;
//    public float height = 1f;

//    public AnimationCurve curve
//    {
//        get
//        {
//            float timeStep = 0.1f;
//            float endTime = 10f;
//            float currentTime = 0f;
//            Vector2 previousPosition = Vector2.zero;
//            Vector2 currentPosition = Vector2.zero;

//            AnimationCurve curve = new AnimationCurve();
//            while (currentTime <= endTime)
//            {
//                currentPosition.x = currentTime;
//                currentPosition.y = dynamicsFunction.Evaluate(currentTime);

//                curve.AddKey(new Keyframe(currentTime, currentPosition.y));

//                currentTime += timeStep;
//            }

//            return curve;
//        }
//    }
//}