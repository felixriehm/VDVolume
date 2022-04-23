using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Benchmark
{
    public class ConicalHelix : MonoBehaviour
    {
        [SerializeField]
        [Range(0.1f, 0.8f)]
        private float stepSize = 0.1f;
        [SerializeField]
        private float xLower = 1f;
        [SerializeField]
        private float xUpper = 1f;
        [SerializeField]
        private float yLower = 1f;
        [SerializeField]
        private float yUpper = 1f;
        [SerializeField]
        private float zLower = 1f;
        [SerializeField]
        private float zUpper = 1f;

        public float stepLowerLimit => 0;
        public float stepUpperLimit => (float) (8*Math.PI);

        private void OnDrawGizmos()
        {
            Vector3 lastFrom = CalcOneStep(stepUpperLimit);
            for (float t = stepUpperLimit - stepSize; t >= stepLowerLimit; t-=stepSize)
            {
                Vector3 to = CalcOneStep(t);
                Gizmos.DrawLine(lastFrom, to);
                lastFrom = to;
            }
        }

        public Vector3 CalcOneStep(float t)
        {
            double spiralStepper = 0.5 * Math.Exp(0.15 * t);
            double x = spiralStepper * Math.Cos(2 * t);
            double y = spiralStepper * Math.Sin(2 * t);
            double z = spiralStepper;

            Transform transform1 = transform;
            Vector3 position = transform1.position;
            float newXPosition = (float) LinearInterpolation(position.x - xLower, position.x + xUpper, -17.184, 13.576, x);
            float newYPosition = (float) LinearInterpolation(position.y - yLower, position.y + yUpper, -19.332, 15.274, y);
            float newZPosition = (float) LinearInterpolation(position.z - zLower, position.z + zUpper, 0.5, 21.688, z);

            return position + transform1.TransformVector(new Vector3(newXPosition, newYPosition, newZPosition));
        }

        double LinearInterpolation(double lower, double upper, double tLower,double tUpper, double t)
        {
            return lower + ((upper - lower) / (tUpper - tLower)) * (t - tLower);
        }

        public float GetStepSize()
        {
            return stepSize;
        }
    }
}

