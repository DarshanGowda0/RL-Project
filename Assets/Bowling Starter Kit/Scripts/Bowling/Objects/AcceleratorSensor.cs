using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MyApp.BowlingKit.Extera
{
    [RequireComponent(typeof(Collider))]
    public class AcceleratorSensor : MonoBehaviour
    {
        #region variable
        public float forcePower = 10f;
        public float handleArrowSize = 2f;
        public Color arrowColor = Color.white;
        [HideInInspector] public Collider _collider;
        #endregion
        #region Function
        #region Gizmo
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = arrowColor;
            Quaternion q = _collider.transform.up == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(_collider.transform.up);
            Handles.ArrowHandleCap(0, _collider.transform.position, q, handleArrowSize, EventType.Repaint);
        }
#endif
        #endregion
        private void OnValidate()
        {
            _collider = GetComponent<Collider>();
        }
        private void Awake()
        {
            _collider.isTrigger = true;
        }
        private void OnTriggerStay(Collider other)
        {
            if (other == this) return;
            BowlingBall ball = other.gameObject.GetComponent<BowlingBall>();
            if (ball == null) return;
            ball.AddForce(_collider.transform.up.normalized * ball._rb.mass * forcePower);
        }
        #endregion
    }
}