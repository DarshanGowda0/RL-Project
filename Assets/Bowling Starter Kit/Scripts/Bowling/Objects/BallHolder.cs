using UnityEngine;

namespace MyApp.BowlingKit
{
    public class BallHolder : MonoBehaviour
    {
        #region enum
        internal enum BallHolderState
        {
            Null = -1,
            Enable,
            Disable
        }
        #endregion
        #region variable
        public Transform ballLoadPosition;
        public Transform ballReleasePosition;
        [Min(0)] public float speed = 1f;
        [Min(0)] public float hitDeltaDistance = .1f;
        #region editor
#if UNITY_EDITOR
        [Min(0)] public float sphereRadius = .1f;
#endif
        #endregion
        #region HideInInspector
        [HideInInspector] public BowlingBall bowlingBall;
        public Vector3 StartPosition
        {
            get
            {
                return ballLoadPosition == null ? transform.position : ballLoadPosition.position;
            }
        }
        public Vector3 EndPosition
        {
            get
            {
                return ballReleasePosition == null ? transform.position : ballReleasePosition.position;
            }
        }
        #endregion
        #region private
        private BallHolderState ballHolderState = BallHolderState.Null;
        private bool[] ballDefaults;
        #endregion
        #endregion
        #region Functions
        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (ballLoadPosition != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(StartPosition, sphereRadius);
            }
            if (ballReleasePosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(EndPosition, sphereRadius);
            }
        }
        private void OnDrawGizmosSelected()
        {
            bool b1 = ballLoadPosition != null;
            bool b2 = ballReleasePosition != null;
            if (b1)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(StartPosition, sphereRadius);
            }
            if (b2)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(EndPosition, sphereRadius);
            }
            if (b1 && b1)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(StartPosition, EndPosition);
            }
        }
#endif
        #endregion
        private void FixedUpdate()
        {
            if (bowlingBall == null || ballHolderState != BallHolderState.Enable) return;
            //TODO: check ball distance
            if (Vector3.Distance(bowlingBall.Position, EndPosition) <= hitDeltaDistance)
            {
                bowlingBall.enable = ballDefaults[0];
                bowlingBall._rb.isKinematic= ballDefaults[1];
                bowlingBall.setBallState_PositionSelection();
                bowlingBall = null;
                ballDefaults = null;
                ballHolderState = BallHolderState.Disable;
                return;
            }
            //TODO calculate new ball position
            Vector3 v = Vector3.Normalize(EndPosition - bowlingBall.Position);
            bowlingBall.transform.Translate(v * speed * Time.deltaTime, Space.World);
        }
        #endregion
        #region functions
        public bool StartTransporting(BowlingBall ball)
        {
            if (ball == null || ballHolderState == BallHolderState.Enable) return false;
            bowlingBall = ball;
            //TODO: do change ball position and so on
            ballDefaults = new bool[]
            {
                bowlingBall.enable,
                bowlingBall._rb.isKinematic
            };
            bowlingBall.enable = bowlingBall._rb.isKinematic = false;
            bowlingBall.Position = StartPosition;
            ballHolderState = BallHolderState.Enable;
            return true;
        }
        #endregion
    }
}