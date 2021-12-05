using UnityEngine;

namespace MyApp.BowlingKit
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class BowlingPin : MonoBehaviour
    {
        #region enum
        public enum BowlingPinState
        {
            Null = -1, Standed, Involved, Fallen
        }
        #endregion
        #region variable
        public bool enable = true;
        #region Parameters
        [Min(0)] public float maxDistance = 10f;
        #region bottom
        public Vector3 bottomVector = -Vector3.up;
        [Min(0)] public float bottomConnectorRadius = .01f;
        public LayerMask groundMask;
        #endregion
        #endregion
        #region force
        public bool forceToStop = false;
        [Min(0)] public float velocityThreshold = .1f;
        [Min(0)] public float stopVelocityLerp = .01f;
        #endregion
        #region HideInInspector
        [HideInInspector] public Rigidbody _rb;
        [HideInInspector] public Collider _collider;
        [HideInInspector] public BowlingPinState _pinState = BowlingPinState.Null;
        public BowlingPinState PinState { get { return _pinState; } }
        public Vector3 Position { get { return transform.position; } set { transform.position = value; } }
        public Quaternion Rotation { get { return transform.rotation; } set { transform.rotation = value; } }
        public Vector3 BottomPosition { get { return Position + bottomVector; } }

        #endregion
        #region editor 
#if UNITY_EDITOR
        [HideInInspector] public bool showParametersParts = true;
        [HideInInspector] public bool showInfoParts = true;
        [HideInInspector] public bool showForceParts = true;
#endif
        #endregion
        #region private
        private Vector3 initPosition;
        #endregion
        #endregion
        #region Functions
        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            //draw bottom
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(BottomPosition, bottomConnectorRadius.toVector3());
            //draw region
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Position, maxDistance);
        }
        #endregion
        private void OnValidate()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }
        private void Start()
        {
            initPosition = Position;
        }
        private void Update()
        {
            if (!enable) return;
            getUpdatedPinState();
        }
        #endregion
        #region functions
        public void setPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
        public void Init(LayerMask groundLayerMask)
        {
            _pinState = BowlingPinState.Null;
            groundMask = groundLayerMask;
            initPosition = Position;
        }
        public BowlingPinState getUpdatedPinState()
        {
            bool b1 = _rb.IsSleeping();
            bool b2 = _rb.velocity == Vector3.zero;
            bool b3 = forceToStop && _rb.velocity.magnitude < velocityThreshold;
            if (b1 || b2 || b3)
            {
                if (isGroundTouched())
                {
                    _pinState = BowlingPinState.Standed;
                }
                else
                {
                    _pinState = BowlingPinState.Fallen;
                }
            }
            else
            {
                checkForceToStop();
                if (Vector3.Distance(Position, initPosition) > maxDistance)
                {
                    _pinState = BowlingPinState.Fallen;
                }
                else
                {
                    _pinState = BowlingPinState.Involved;
                }
            }
            return _pinState;
        }
        private void checkForceToStop()
        {
            if (!forceToStop) return;
            if (_rb.velocity != Vector3.zero && _rb.velocity.magnitude < velocityThreshold)
            {
                _rb.velocity = Vector3.Lerp(Vector3.zero, _rb.velocity, stopVelocityLerp);
            }
        }
        public bool isGroundTouched()
        {
            return Physics.CheckBox(BottomPosition, bottomConnectorRadius.toVector3(), transform.rotation, groundMask);
        }
        #endregion
    }
}