using UnityEngine;

namespace MyApp.BowlingKit.Classes
{
    public class PivotAngleHandler
    {
        #region variable

        #endregion

        #region Pivot Vector
        #region vector
        public Vector3 getPivotVectorDirection(float angle)
        {
            Vector3 v = new Vector3(1f, 0f, 0f);
            return (getPivotRotation(angle) * v).normalized;
        }
        public Vector3 getPivotVector(Vector3 point, float angle, float magnitude = 1f)
        {
            return getPivotVectorDirection(angle) * magnitude + point;
        }
        #region vector(s) from pivot
        public Vector3 getVectorFromPivot(Vector3 point, float angle, float delta, float magnitude = 1f)
        {
            return getPivotVector(point, angle + delta, magnitude);
        }
        public void getVectorsFromPivot(Vector3 point, float angle, float delta, out Vector3 v1, out Vector3 v2, float magnitude = 1f)
        {
            v1 = getVectorFromPivot(point, angle, -delta, magnitude);
            v2 = getVectorFromPivot(point, angle, delta, magnitude);
        }
        #endregion
        #endregion
        #region Rotation
        public Quaternion getPivotRotation(float angle)
        {
            return Quaternion.AngleAxis(angle, Vector3.up);
        }
        public Quaternion getPivotRotation(Vector3 point, float angle, float magnitude = 1)
        {
            Vector3 v = getPivotVector(point, angle, magnitude);
            if (v == Vector3.zero) return Quaternion.identity;
            return Quaternion.LookRotation(-v);
        }
        #endregion
        #endregion
        #region Constructor
        #endregion
    }
}