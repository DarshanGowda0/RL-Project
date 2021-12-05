using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MyApp.BowlingKit
{
    public class BowlingGameManager : MonoBehaviour
    {
        #region struct
        internal struct RotationPosition
        {
            #region constructor
            public RotationPosition(Transform t) : this(t.position, t.rotation) { }
            public RotationPosition(Vector3 pos, Quaternion r)
            {
                position = pos;
                rotation = r;
            }
            #endregion
            #region variable
            public Vector3 position;
            public Quaternion rotation;
            #endregion
        }
        #endregion
        #region enum
        public enum BowlingGameEvent
        {
            Null = -1, None, ShootsOver, AllPinsFallen
        }
        #endregion
        #region variable
        public bool startFromBallHolder = true;
        public bool infinityShoots = false;
        [Min(1)] public int maxShoots = 2;
        public bool isOverrideGroundLayer = true;
        public LayerMask groundMask;
        public bool updatePinsStateInRealTime = true;
        #region events
        [Min(0)] public float gameFirstStartEventDelay = 0f;
        public UnityEvent gameFirstStartEvent;
        [Min(0)] public float takeBallToBallHolderEventDelay = 0f;
        public UnityEvent takeBallToBallHolderEvent;
        [Min(0)] public float gameOverEventDelay = 0f;
        public UnityEvent gameOverEvent;
        [Min(0)] public float shootsOverEventDelay = 0f;
        public UnityEvent shootsOverEvent;
        [Min(0)] public float allPinsFallenEventDelay = 0f;
        public UnityEvent allPinsFallenEvent;
        #endregion
        #region editor 
#if UNITY_EDITOR
        [HideInInspector] public bool showParametersParts = true;
        [HideInInspector] public bool showInfoParts = true;
        [HideInInspector] public bool showEventsParts = true;
#endif
        #endregion
        #region HideInInspector
        public int FallenPinCount { get { return _fallenPinCount; } }
        public int StandedPinCount { get { return _standedPinCount; } }
        public int CurrentShoots { get { return _currentShoots; } }
        public bool GameIsOver { get { return _gameIsOver; } }
        public BowlingGameEvent GameEvent { get { return _gameEvent; } }
        [HideInInspector] public BowlingBall _ball;
        [HideInInspector] public BowlingPin[] _pins;
        [HideInInspector] public BallHolder _ballHolder;
        [HideInInspector] public int _currentShoots = 0;
        [HideInInspector] public bool _gameIsOver;
        [HideInInspector] public int _fallenPinCount;
        [HideInInspector] public int _standedPinCount;
        [HideInInspector] public BowlingGameEvent _gameEvent;
        #endregion
        #region private
        private RotationPosition[] pinsPosRotStruct;
        private BowlingBall.BowlingBallState lastBallState;
        #endregion
        #endregion
        #region Function
        private void Awake()
        {
            if ((_ball = FindObjectOfType<BowlingBall>()) == null)
            {
                Debug.LogError("No ball founded.");
                Extensions.Quit();
                return;
            }
            if ((_pins = FindObjectsOfType<BowlingPin>()) == null || _pins.Length < 1)
            {
                Debug.LogError("No pin founded.");
                Extensions.Quit();
                return;
            }
            _ballHolder = FindObjectOfType<BallHolder>();
            if (isOverrideGroundLayer)
            {
                for (int i = 0; i < _pins.Length; i++)
                {
                    _pins[i].Init(groundMask);
                }
            }
            pinsInitPosRot_sampling();
        }
        private void Start()
        {
            init();
        }
        private void FixedUpdate()
        {
            if (_gameIsOver) return;
            if (lastBallStateIsChanged())
            {
                switch (_ball.BallState)
                {
                    case BowlingBall.BowlingBallState.Released:
                        _currentShoots++;
                        break;
                }
                updateLastBallState();
            }
            if (lastBallState == BowlingBall.BowlingBallState.Finish)
            {
                if (!pinStateIsExist(BowlingPin.BowlingPinState.Involved))
                {
                    updatePinsState(); foreach (var node in _pins)
                    {
                        if (node.PinState != BowlingPin.BowlingPinState.Standed)
                        {
                            node.gameObject.SetActive(false);
                        }
                    }
                    Debug.Log("Valid pins state - <fallen, Standed>: <" + FallenPinCount + ", " + StandedPinCount + ">");
                    //all pins fallen
                    if (FallenPinCount == _pins.Length)
                    {
                        setGemeIsOver(BowlingGameEvent.AllPinsFallen);
                    }
                    else
                    {
                        if (!infinityShoots && _currentShoots >= maxShoots)
                        {
                            setGemeIsOver(BowlingGameEvent.ShootsOver);
                        }
                        else
                        {
                            TakeBallToStart();
                        }
                    }
                }
            }
            if (updatePinsStateInRealTime)
            {
                updatePinsState();
            }
        }
        #endregion
        #region function
        #region init
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            var node = FindObjectOfType<BowlingGameManager>();
            if (node == null)
            {
                new GameObject("_BowlingGameManager").AddComponent<BowlingGameManager>();
            }
        }
        private void init()
        {
            if (startFromBallHolder && _ballHolder != null)
            {
                _ballHolder.StartTransporting(_ball);
            }
            _currentShoots = 0;
            resetFallenStandedCound();
            _gameIsOver = false;
            lastBallState = BowlingBall.BowlingBallState.Null;
            invokeEvent(gameFirstStartEvent, gameFirstStartEventDelay);
            _gameEvent = BowlingGameEvent.None;
        }
        #endregion
        #region pinsPosRotStruct
        private void updatePinsState()
        {
            var result = pinsStateCount(new BowlingPin.BowlingPinState[] { BowlingPin.BowlingPinState.Fallen, BowlingPin.BowlingPinState.Standed });
            _fallenPinCount = result[0];
            _standedPinCount = result[1];
        }
        private void pinsInitPosRot_backToDefault()
        {
            bool[] ik = new bool[_pins.Length];
            for (int i = 0; i < _pins.Length; i++)
            {
                if (_pins[i] == null) continue;
                _pins[i].enable = false;
                ik[i] = _pins[i]._rb.isKinematic;
                _pins[i]._rb.isKinematic = true;
                _pins[i]._rb.velocity = Vector3.zero;
                _pins[i].setPositionAndRotation(pinsPosRotStruct[i].position, pinsPosRotStruct[i].rotation);
            }
            for (int i = 0; i < _pins.Length; i++)
            {
                if (_pins[i] == null) continue;
                _pins[i]._rb.isKinematic = ik[i];
                _pins[i].enable = true;
                _pins[i].gameObject.SetActive(true);
            }
            resetFallenStandedCound();
        }
        private void resetFallenStandedCound()
        {
            _fallenPinCount = _standedPinCount = -1;
        }
        private void pinsInitPosRot_sampling()
        {
            pinsPosRotStruct = new RotationPosition[_pins.Length];
            for (int i = 0; i < _pins.Length; i++)
            {
                if (_pins[i] == null) continue;
                pinsPosRotStruct[i] = new RotationPosition(_pins[i].gameObject.transform);
            }
        }
        #endregion
        #region pins
        public bool pinStateIsExist(BowlingPin.BowlingPinState state)
        {
            if (_pins == null || _pins.Length < 1) return false;
            for (int i = 0; i < _pins.Length; i++)
            {
                if (_pins[i] == null) continue;
                if (_pins[i].PinState == state) return true;
            }
            return false;
        }
        public int pinsStateCount(BowlingPin.BowlingPinState state)
        {
            var result = pinsStateCount(new BowlingPin.BowlingPinState[] { state });
            if (result == null) return -1;
            if (result.Length < 1) return 0;
            return result[0];
        }
        public int[] pinsStateCount(BowlingPin.BowlingPinState[] states)
        {
            if (states == null || states.Length < 1 || _pins == null) return null;
            int[] result = new int[states.Length];
            for (int i = 0; i < _pins.Length; i++)
            {
                if (_pins[i] == null) continue;
                for (int j = 0; j < states.Length; j++)
                {
                    if (_pins[i].PinState == states[j])
                    {
                        result[j]++;
                        break;
                    }
                }
            }
            return result;
        }
        public bool pinsStateEquals(BowlingPin.BowlingPinState state)
        {
            if (_pins == null || _pins.Length < 1) return false;
            for (int i = 0; i < _pins.Length; i++)
            {
                if (_pins[i] == null) continue;
                if (_pins[i].PinState != state) return false;
            }
            return true;
        }
        #endregion
        #region Logic
        #region lastBallState
        private void updateLastBallState()
        {
            lastBallState = _ball.BallState;
        }
        private bool lastBallStateIsChanged()
        {
            return lastBallState != _ball.BallState;
        }
        #endregion
        private void setGemeIsOver(BowlingGameEvent e)
        {
            _gameIsOver = true;
            _gameEvent = e;
            switch (e)
            {
                case BowlingGameEvent.ShootsOver:
                    invokeEvent(shootsOverEvent, shootsOverEventDelay);
                    break;
                case BowlingGameEvent.AllPinsFallen:
                    invokeEvent(allPinsFallenEvent, allPinsFallenEventDelay);
                    break;
            }
            invokeEvent(gameOverEvent, gameOverEventDelay);
        }
        public void TakeBallToStart()
        {
            _ball.ResetBall();
            _gameEvent = BowlingGameEvent.None;
            updateLastBallState();
            if (_ballHolder != null)
            {
                _ballHolder.StartTransporting(_ball);
                invokeEvent(takeBallToBallHolderEvent, takeBallToBallHolderEventDelay);
            }
        }
        public void ResetToStart()
        {
            _currentShoots = 0;
            TakeBallToStart();
            pinsInitPosRot_backToDefault();
            _gameIsOver = false;
        }
        #endregion

        #region delay
        public void invokeEvent(UnityEvent e, float delay)
        {
            if (e == null) return;
            if (delay > 0)
            {
                StartCoroutine(Coroutine(delay, e));
            }
            else
            {
                e.Invoke();
            }
        }
        private IEnumerator Coroutine(float seconds, UnityEvent e)
        {
            yield return new WaitForSeconds(seconds);
            if (e != null) e.Invoke();
        }
        #endregion
        #endregion
    }
}