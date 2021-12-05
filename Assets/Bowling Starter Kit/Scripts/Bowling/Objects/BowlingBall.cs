using UnityEngine;
using UnityEngine.UI;
using MyApp.BowlingKit.Classes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MyApp.BowlingKit
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class BowlingBall : MonoBehaviour
    {
        #region enum
        public enum ShootingMode { Touching, Tapping }
        public enum BowlingBallEvent { Null = -1, None, FarDistance, BallStopped }
        public enum BowlingBallState { Null = -1, Idle, PositionSelection, Tapped_FindDelta, Tapped_FindPower, Touched, Released, Finish }
        public enum ShadowMode { ByTime, ByDistance }
        #endregion
        #region variable
        #region const
        public const float Zero = 0f;
        #endregion
        public bool enable = true;
        #region Parameter(s)
        public Collider placementCollider;
        public ShootingMode shootingMode = ShootingMode.Touching;
        [Min(0)] public float maxForceDistance = 1.0f;
        [Min(0)] public float maxMovementDistance = 10f;
        //Touching
        [Min(0)] public float minForceDistance = 1.0f;
        //Tapping
        [Range(0, 360)] public float pivotAngle;
        [Range(0, 180)] public float theta = 30f;
        [Min(0)] public float orbitDampening = 100f;
        [Min(0)] public float powerDampening = 5f;
        private PivotAngleHandler pivotAngleHandler = new PivotAngleHandler();
        #endregion
        #region Force
        [Min(0)] public float potentialForceCoefficient = 1.0f;
        [Min(0)] public float potentialForceSensitivity = 1.0f;
        public ForceMode forceMode = ForceMode.Force;
        // force to stop
        public bool forceToStop = false;
        [Min(0)] public float velocityThreshold = .1f;
        [Min(0)] public float stopVelocityLerp = .01f;
        #endregion
        #region UI
        public GameObject sliderCanvas;
        public Slider slider;
        public Image sliderFillAreaImage;
        public bool solidColor = false;
        public Color color1 = Color.green;
        public Color color2 = Color.red;
        #endregion
        #region Shadow
        public bool shadowEnable;
        public GameObject shadowPrefab;
        public Vector3 shadowScale = Vector3.one;
        public ShadowMode shadowMode = ShadowMode.ByDistance;
        public float shadowTTL = 1f;
        public float shadowInstantiateRate = .5f;
        public float shadowInstantiateDistance = 1f;
        #endregion
        #region HideInInspector
        public Vector3 PotentialForceDirection { get; protected set; }
        public float PotentialEnergy { get; protected set; }
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        [HideInInspector] public Camera _camera;
        [HideInInspector] public Rigidbody _rb;
        [HideInInspector] public Collider _collider;
        [HideInInspector] public RectTransform _sliderRT;
        [HideInInspector] public BowlingBallState _ballState = BowlingBallState.Null;
        [HideInInspector] public BowlingBallEvent _ballEvent = BowlingBallEvent.Null;

        public BowlingBallState BallState { get { return _ballState; } }
        public BowlingBallEvent BallEvent { get { return _ballEvent; } }
        #endregion
        #region private
        private bool potentialForceReleased;
        private Vector3 startPosition = Vector3.zero;
        private Vector3 p2;
        private float releaseDeltaTime;
        #region PositionSelection
        private bool secondTouch_PositionSelection;
        private Vector3 startPosition_secondTouchPositionSelection;
        #endregion
        #region shadow
        private float _shadowInstantiateRate = 0f;
        private Vector3 lastShadowPosition = Vector3.zero;
        #endregion
        #region Touching Tapping
        private bool secondTouch = false;
        private float _theta;
        private bool _reductionalAngle;
        private float _power;
        private bool _reductionalPower;
        #endregion
        #endregion
        #region editor 
#if UNITY_EDITOR
        [HideInInspector] public bool showParametersParts = true;
        [HideInInspector] public bool showInfoParts = true;
        [HideInInspector] public bool showForceParts = true;
        [HideInInspector] public bool showUIParts = true;
        [HideInInspector] public bool showShadowParts = true;
#endif
        #endregion
        #endregion
        #region Functions
        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // draws the direction line and sphere in scene while playing
            if (shootingMode == ShootingMode.Touching)
            {
                //draw Potential Energy
                Gizmos.color = Color.white;
                if (_ballState == BowlingBallState.Touched || _ballState == BowlingBallState.Released)
                {
                    Gizmos.DrawWireSphere(transform.position, PotentialEnergy);
                }
                //Draw movement flow
                if (_ballState == BowlingBallState.Touched && PotentialForceDirection != Vector3.zero)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawRay(Position, PotentialForceDirection * maxMovementDistance);
                }
            }
            else
            {

            }

            //draw velocity
            Debug.DrawRay(Position, _rb.velocity);
        }
        private void OnDrawGizmosSelected()
        {

            if (_ballState == BowlingBallState.Null || _ballState == BowlingBallState.Idle)
            {
                //max force distance
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(Position, Vector3.up, maxForceDistance);
                //min force distance
                if (shootingMode == ShootingMode.Touching)
                {
                    Handles.color = Color.blue;
                    Handles.DrawWireDisc(Position, Vector3.up, minForceDistance);
                }
            }
            //pivot vector and delta
            if (shootingMode == ShootingMode.Tapping)
            {
                Gizmos.color = Color.red;
                var v = pivotAngleHandler.getPivotVector(Position, pivotAngle, maxMovementDistance);
                Gizmos.DrawLine(Position, v);
                if (theta != 0f)
                {
                    Vector3 v1, v2;
                    pivotAngleHandler.getVectorsFromPivot(Position, pivotAngle, theta, out v1, out v2, maxMovementDistance);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(Position, v1);
                    Gizmos.DrawLine(Position, v2);
                }
            }
            //Max Distance
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(Extensions.isPlaying() ? startPosition : Position, Vector3.up, maxMovementDistance);
            }
        }
#endif
        #endregion
        private void OnValidate()
        {
            // called when a value is changed in the inspector
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }
        private void Awake()
        {
            init();
        }
        private void Update()
        {
            if (!enable) return;
            switch (_ballState)
            {
                case BowlingBallState.Tapped_FindDelta:
                    ballState_Tapped_FindDelta();
                    break;
                case BowlingBallState.Tapped_FindPower:
                    ballState_Tapped_FindPower();
                    break;
                case BowlingBallState.Touched:
                    ballState_Touched();
                    break;
                case BowlingBallState.Released:
                    checkForceToStop();
                    ballState_Released();
                    break;
                case BowlingBallState.Finish:
                    ballState_Finish();
                    break;
                case BowlingBallState.PositionSelection:
                    ballState_PositionSelection();
                    break;
                default:
                    ballState_Idle();
                    break;
            }
        }
        private void FixedUpdate()
        {
            if (!enable) return;
            if (shootingMode == ShootingMode.Touching)
            {
                if (secondTouch)
                {
                    calculateForce(Position, p2, minForceDistance);
                }
            }
            else
            {

            }
        }
        #endregion
        #region functions
        #region BallState
        private void ballState_Idle()
        {
            if (shootingMode == ShootingMode.Touching)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (secondTouch = isHit())
                    {
                        p2 = Position;
                        setSliderActive(true);
                        setSliderPosition(Position);
                        setBallState(BowlingBallState.Touched);
                    }
                }
            }
            else
            {
                //TODO: goto find delta
                setSliderActive(true);
                setSliderPosition(Position);
                setBallState(BowlingBallState.Tapped_FindDelta);
                calculatePotentialForceValue(0f);
            }
        }
        private void ballState_Released()
        {
            if (potentialForceReleased)
            {
                bool b1 = _rb.IsSleeping();
                if (!b1)
                {
                    if (Time.realtimeSinceStartup - releaseDeltaTime > 2f)
                    {
                        b1 = _rb.velocity == Vector3.zero;
                    }
                }
                bool b2 = Vector3.Distance(startPosition, Position) > maxMovementDistance;
                if (b1 || b2)
                {
                    if (b1)
                    {
                        _ballEvent = BowlingBallEvent.BallStopped;
                    }
                    else if (b2)
                    {
                        _ballEvent = BowlingBallEvent.FarDistance;
                    }
                    releaseDeltaTime = Zero;
                    potentialForceReleased = false;
                    setBallState(BowlingBallState.Finish);
                }
                else
                {
                    if (shadowEnable && isShadowInstantiateRateCompleted(Position))
                    {
                        createShadow(Position, transform.rotation);
                    }
                }
            }
            else
            {
                var force = getReleasedPotentialForce();
                if (force == Vector3.zero)
                {
                    potentialForceReleased = false;
                    setBallState(BowlingBallState.Idle);
                }
                else
                {
                    startPosition = Position;
                    AddForce(force);
                    PotentialEnergy = Zero;
                    potentialForceReleased = true;
                    releaseDeltaTime = Time.realtimeSinceStartup;
                }
            }
        }
        private void ballState_Touched()
        {
            if (Input.GetMouseButton(0))
            {
                if (mousePositionOnPlane(out p2, maxForceDistance))
                {

                }
                else
                {
                    p2 = Position;
                }
            }
            else
            {
                if (secondTouch)
                {
                    setBallState(BowlingBallState.Released);
                    setSliderActive(false);
                    secondTouch = false;
                }
                else
                {
                    PotentialEnergy = Zero;
                    setBallState(BowlingBallState.Idle);
                }
            }
        }
        private void ballState_Tapped_FindPower()
        {
            if (Input.GetMouseButtonDown(0))
            {
                setBallState(BowlingBallState.Released);
                setSliderActive(false);
            }
            else
            {
                //calculate power
                var speed = powerDampening * Time.deltaTime;
                if (_reductionalPower)
                {
                    _power -= speed;
                    if (_power <= 0f)
                    {
                        _reductionalPower = !_reductionalPower;
                        _power = 0f;
                    }
                }
                else
                {
                    _power += speed;
                    if (_power >= maxForceDistance)
                    {
                        _reductionalPower = !_reductionalPower;
                        _power = maxForceDistance;
                    }
                }
                //set potential force value
                calculatePotentialForceValue(_power);
            }
        }
        private void ballState_Tapped_FindDelta()
        {
            if (Input.GetMouseButtonDown(0))
            {
                setBallState(BowlingBallState.Tapped_FindPower);
            }
            else
            {
                //calculate _theta
                var speed = orbitDampening * Time.deltaTime;
                if (_reductionalAngle)
                {
                    _theta -= speed;
                    if (_theta <= -theta)
                    {
                        _reductionalAngle = !_reductionalAngle;
                        _theta = -theta;
                    }
                }
                else
                {
                    _theta += speed;
                    if (_theta >= theta)
                    {
                        _reductionalAngle = !_reductionalAngle;
                        _theta = theta;
                    }
                }
                //set position and potential force direction
                calculatePotentialForceDirection(Position + (pivotAngleHandler.getPivotRotation(_theta) * pivotAngleHandler.getPivotVector(Position, pivotAngle)));
            }
        }
        private void ballState_PositionSelection()
        {
            if (secondTouch_PositionSelection)
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 position;
                    if (mousePositionOnPlane(out position, float.MaxValue))
                    {
                        Position = position;
                    }
                    else
                    {
                        Position = startPosition_secondTouchPositionSelection;
                        secondTouch_PositionSelection = false;
                    }
                }
                else
                {
                    if (placementCollider == null)
                    {
                        setBallState(BowlingBallState.Idle);
                        Position = startPosition_secondTouchPositionSelection;
                        secondTouch_PositionSelection = false;
                    }
                    else
                    {
                        RaycastHit hit;
                        if (placementCollider.Raycast(cameraRay(), out hit, Mathf.Infinity))
                        {
                            Position = hit.point;// new Vector3(hit.point.x, hit.point.y + (_collider.bounds.size.y / 2), hit.point.z);
                            secondTouch_PositionSelection = false;
                            setBallState(BowlingBallState.Idle);
                        }
                        else
                        {
                            Position = startPosition_secondTouchPositionSelection;
                        }
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    startPosition_secondTouchPositionSelection = Position;
                    secondTouch_PositionSelection = true;
                }
            }
        }
        private void ballState_Finish()
        {
            //throw new System.Exception("Not handled.");
        }
        #endregion
        #region init & constructors
        private void init()
        {
            _ballEvent = BowlingBallEvent.None;
            lastShadowPosition = startPosition = Position;
            setBallState(BowlingBallState.Null);
            potentialForceReleased = false;
            PotentialEnergy = _shadowInstantiateRate = Zero;
            setSliderActive(false);
            if (slider != null)
            {
                _sliderRT = slider.GetComponent<RectTransform>();
            }
            if (_camera == null) _camera = Camera.main;
            if (_camera == null)
            {
                Debug.LogError("Camera not found.");
                Extensions.Quit();
            }
            if (_rb == null)
            {
                Debug.LogError("Ball Rigidbody not found.");
                Extensions.Quit();
            }
            if (_collider == null)
            {
                Debug.LogError("Ball Collider not found.");
                Extensions.Quit();
            }
            {
                var ik = _rb.isKinematic;
                _rb.isKinematic = true;
                _rb.velocity = Vector3.zero;
                _rb.isKinematic = ik;
            }
            if (placementCollider != null) placementCollider.isTrigger = true;
            if (maxForceDistance < minForceDistance)
            {
                Extensions.Swap<float>(ref maxForceDistance, ref minForceDistance);
            }
            if (shootingMode == ShootingMode.Tapping)
            {
                pivotAngleHandler = new PivotAngleHandler();
            }
        }
        #endregion
        public void ResetBall()
        {
            init();
        }
        public void AutoCalculateMinForceDistance()
        {
            minForceDistance = (_collider.bounds.size.x + _collider.bounds.size.z) / 4;
        }
        #region logic
        public void setBallState_PositionSelection()
        {
            setBallState(BowlingBallState.PositionSelection);
        }
        private void setBallState(BowlingBallState state)
        {
            // when we're moving ball onto board, disable gravity
            switch (_ballState = state)
            {
                case BowlingBallState.Null:
                case BowlingBallState.Touched:
                case BowlingBallState.Idle:
                case BowlingBallState.Tapped_FindDelta:
                case BowlingBallState.Tapped_FindPower:
                case BowlingBallState.PositionSelection:
                    _rb.useGravity = false;
                    break;
                case BowlingBallState.Released:
                    _rb.useGravity = true;
                    break;
            }
        }
        #region physic
        #region force
        private void checkForceToStop()
        {
            if (!forceToStop) return;
            if (_rb.velocity != Vector3.zero && _rb.velocity.magnitude < velocityThreshold)
            {
                _rb.velocity = Vector3.Lerp(Vector3.zero, _rb.velocity, stopVelocityLerp);
            }
        }
        private Vector3 getReleasedPotentialForce()
        {
            return potentialForceCoefficient * PotentialEnergy * PotentialForceDirection;
        }
        private void calculateForce(Vector3 point1, Vector3 point2, float minD)
        {
            var distance = Vector3.Distance(point2, point1);
            //calc PotentialEnergy 
            {
                var dis = distance - minD;
                if (dis < 0) dis = 0;
                PotentialEnergy = Mathf.Clamp(dis * potentialForceSensitivity, Zero, maxForceDistance);
            }
            setSliderValue(PotentialEnergy, maxForceDistance - minD);
            //var realForce = Mathf.Clamp(distance * potentialForceSensitivity, Zero, maxForceDistance);

            //calc PotentialEnergy Direction
            Vector3 potentialForceHitPosition;
            if (mousePositionOnPlane(out potentialForceHitPosition, PotentialEnergy))
            {
                calculatePotentialForceDirection(potentialForceHitPosition);
            }
            else
            {
                PotentialForceDirection = Vector3.zero;
            }
        }
        private void calculatePotentialForceValue(float power)
        {
            PotentialEnergy = Mathf.Clamp(power * potentialForceSensitivity, Zero, maxForceDistance);
            setSliderValue(PotentialEnergy, maxForceDistance);
        }
        private void calculatePotentialForceDirection(Vector3 hitPosition)
        {
            PotentialForceDirection = (Position - hitPosition).normalized;
            if (sliderCanvas != null && PotentialForceDirection != Vector3.zero)
            {
                sliderCanvas.transform.rotation = Quaternion.LookRotation(PotentialForceDirection);
            }
        }
        public void AddForce(Vector3 force)
        {
            _rb.AddForce(force, forceMode);
        }
        #endregion
        protected bool isHit()
        {
            Vector3 v;
            return isHit(out v);
        }
        protected bool isHit(out Vector3 hitPosition)
        {
            RaycastHit hit;
            if (_collider.Raycast(cameraRay(), out hit, Mathf.Infinity))
            {
                hitPosition = hit.point;
                return true;
            }
            hitPosition = default;
            return false;
        }
        protected Ray cameraRay()
        {
            return _camera.ScreenPointToRay(Input.mousePosition);
        }
        #endregion
        #region vectors
        public bool mousePositionOnPlane(out Vector3 hitPosition, float d)
        {
            Plane plane = new Plane(Vector3.up, Position);

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            float enter = 0.0f;
            if (plane.Raycast(ray, out enter))
            {
                //Get the point that is clicked
                var result = ray.GetPoint(enter);
                if (Vector3.Distance(result, Position) > d)
                {
                    result = (result - Position).normalized * d + Position;
                }
                hitPosition = result;
                return true;
            }
            hitPosition = default;
            return false;
        }
        public Vector3 getMouse2dPosition()
        {
            return _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.z));
        }
        #endregion
        #endregion
        #region UI
        private void setSliderActive(bool val)
        {
            if (sliderCanvas == null) return;
            sliderCanvas.SetActive(val);
        }
        private void setSliderPosition(Vector3 position)
        {
            if (sliderCanvas == null) return;
            sliderCanvas.transform.position = position;
        }
        private void setSliderValue(float val, float max, float min = 0)
        {
            if (slider == null) return;
            float result = 0f;
            {
                var delta = max - min;
                if (delta != 0)
                {
                    result = (val - min) / delta;
                }
            }
            slider.value = result;


            if (sliderFillAreaImage != null)
            {
                if (result > 0)
                {
                    sliderFillAreaImage.enabled = true;
                    sliderFillAreaImage.color = solidColor ? color1 : Color.Lerp(color1, color2, result);
                }
                else
                {
                    sliderFillAreaImage.enabled = false;
                }
            }
        }
        #endregion
        #region shadow
        private void createShadow(Vector3 position, Quaternion q)
        {
            if (shadowPrefab == null) return;
            var node = Object.Instantiate(shadowPrefab, position, q);
            node.transform.localScale = shadowScale;
            node.hideFlags = HideFlags.HideAndDontSave;
            Object.Destroy(node, shadowTTL);
        }
        private bool isShadowInstantiateRateCompleted(Vector3 position)
        {
            if (shadowMode == ShadowMode.ByTime)
            {
                if (_shadowInstantiateRate > shadowInstantiateRate)
                {
                    _shadowInstantiateRate = Zero;
                    return true;
                }
                _shadowInstantiateRate += Time.deltaTime;
            }
            else if (shadowMode == ShadowMode.ByDistance)
            {
                if (Vector3.Distance(lastShadowPosition, Position) > shadowInstantiateDistance)
                {
                    lastShadowPosition = Position;
                    return true;
                }
            }
            return false;
        }
        #endregion
        #endregion
    }
}