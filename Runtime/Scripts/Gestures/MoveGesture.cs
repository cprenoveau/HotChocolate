using HotChocolate.Utils;
using UnityEngine;

namespace HotChocolate.Gestures
{
    public class MoveGesture
    {
        /// <summary>
        /// Expresses how much the resulting delta lags behind the actual touch. A MaxForce of 1 means no lag, 0 means no movement.
        /// </summary>
        public float MaxForce { get; set; } = 0.5f;

        /// <summary>
        /// Expresses the speed at which the resulting delta will stop moving after touch release. A Friction of 1 means immediate stopping, 0 means no stopping at all.
        /// </summary>
        public float Friction { get; set; } = 0.5f;

        public bool AllowMoveWithTwoFingers { get; set; }

        public float MouseWheelScaleSpeed = 30f;
        public float MouseRotationSpeed = 1f;

        public bool IsActive { get; private set; }
        public Vector2 LocalPos { get; private set; }
        public Vector2 ScreenPos { get; private set; }
        public Vector2 DeltaPos { get; private set; }
        public float LocalScale { get; private set; }
        public float DeltaScale { get; private set; }
        public float LocalAngle { get; private set; }
        public float DeltaAngle { get; private set; }
        public bool TwoFingers { get; private set; }

        private Camera _camera;
        private RectTransform _zone;
        private Vector2 _lastPosition;
        private Vector2 _lastAngle;
        private bool _wasDown;
        private TouchWrapper _soloTouch;

        public MoveGesture(RectTransform zone, Camera camera, int? soloTouchIndex = null)
        {
            _zone = zone;
            _camera = camera;

            if(soloTouchIndex.HasValue)
            {
                _soloTouch = new TouchWrapper(soloTouchIndex.Value);
            }
        }

        public void Reset()
        {
            DeltaPos = Vector2.zero;
            DeltaScale = 0;
            LocalScale = 0;
            DeltaAngle = 0;
            LocalAngle = 0;

            IsActive = false;
            _wasDown = false;
            _lastPosition = Vector2.zero;
            _lastAngle = Vector2.zero;
            _soloTouch?.Reset();
        }

        public void Update(float deltaTime)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            UpdatePc(deltaTime);
#else
            UpdateMobile(deltaTime);
#endif
        }

        private void UpdatePc(float deltaTime)
        {
            TwoFingers = false;

            ScreenPos = Input.mousePosition;
            DeltaScale = Input.mouseScrollDelta.y * MouseWheelScaleSpeed;

            if (DeltaScale != 0 || SecondFingerPc())
            {
                TwoFingers = true;
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 localPos = Positions.ToLocalPos(ScreenPos, _zone, _camera);

                if (IsActive)
                {
                    ApplyMovement(deltaTime, localPos);

                    if (SecondFingerPc())
                    {
                        DeltaAngle = DeltaPos.x * MouseRotationSpeed;
                        LocalAngle += DeltaAngle;
                    }
                }
                else
                {
                    DeltaPos = Vector2.zero;

                    if (!_wasDown && Positions.IsHitByRaycast2D(Input.mousePosition, _zone))
                    {
                        _lastPosition = LocalPos = localPos;
                        IsActive = true;
                    }

                    _wasDown = true;
                }

                if (TwoFingers && !AllowMoveWithTwoFingers)
                {
                    DeltaPos = Vector2.zero;
                }

                return;
            }

            _wasDown = false;
            IsActive = false;
            ApplyFriction(deltaTime);
        }

        private bool SecondFingerPc()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        private void UpdateMobile(float deltaTime)
        {
            if (_soloTouch != null)
            {
                UpdateMobile(_soloTouch, deltaTime);
            }
            else
            {
                UpdateMobile(
                    Input.touchCount,
                    Utils.ActiveTouchCount(),
                    Input.touchCount > 0 ? Input.GetTouch(0) : (Touch?)null,
                    Input.touchCount > 1 ? Input.GetTouch(1) : (Touch?)null,
                    deltaTime);
            }
        }

        private void UpdateMobile(TouchWrapper soloTouch, float deltaTime)
        {
            soloTouch.Refresh();

            int touchCount = soloTouch.Touch.HasValue ? 1 : 0;
            int activeTouches = soloTouch.Touch.HasValue && Utils.TouchIsActive(soloTouch.Touch.Value) ? 1 : 0;

            UpdateMobile(
                touchCount,
                activeTouches,
                soloTouch.Touch,
                null,
                deltaTime);
        }

        private void UpdateMobile(int touchCount, int activeTouches, Touch? touch1, Touch? touch2, float deltaTime)
        {
            TwoFingers = touchCount >= 2;

            //No touch
            if (activeTouches == 0)
            {
                _wasDown = false;
                IsActive = false;

                _lastAngle = Vector2.zero;
                DeltaScale = LocalScale = 0;
                ApplyFriction(deltaTime);
                return;
            }

            //One touch
            if (touchCount == 1)
            {
                _lastAngle = Vector2.zero;
                DeltaAngle = 0;

                ScreenPos = touch1.Value.position;
                Vector2 localPos = Positions.ToLocalPos(touch1.Value.position, _zone, _camera);

                if (IsActive)
                {
                    DeltaScale = LocalScale = 0;
                    ApplyMovement(deltaTime, localPos);
                }
                else
                {
                    DeltaPos = Vector2.zero;
                    DeltaScale = LocalScale = 0;

                    if (!_wasDown && Positions.IsHitByRaycast2D(touch1.Value.position, _zone))
                    {
                        _lastPosition = LocalPos = localPos;
                        IsActive = true;
                    }

                    _wasDown = true;
                }

                return;
            }

            //Two touches
            Vector2 localPos1 = Positions.ToLocalPos(touch1.Value.position, _zone, _camera);
            Vector2 localPos2 = Positions.ToLocalPos(touch2.Value.position, _zone, _camera);

            ScreenPos = Positions.Average(touch1.Value.position, touch2.Value.position);

            if (_lastAngle == Vector2.zero)
            {
                _lastAngle = localPos2 - localPos1;
            }

            if (IsActive)
            {
                ApplyMovement(deltaTime, Positions.Average(localPos1, localPos2));
                ApplyScale(localPos1, localPos2);
                ApplyRotation(localPos2 - localPos1);

                if (!AllowMoveWithTwoFingers)
                {
                    DeltaPos = Vector2.zero;
                }
            }
            else
            {
                DeltaPos = Vector2.zero;
                DeltaScale = 0;
                DeltaAngle = 0;

                if (!_wasDown && Positions.IsHitByRaycast2D(ScreenPos, _zone))
                {
                    _lastPosition = Positions.Average(localPos1, localPos2);
                    LocalScale = (localPos1 - localPos2).magnitude;
                    LocalAngle = Trigo.PositionToAngle(_lastAngle).Degrees;
                    IsActive = true;
                }
                _wasDown = true;
                return;
            }

            if (activeTouches == 1)
            {
                //removed first finger
                if (touch1.Value.phase == TouchPhase.Ended || touch1.Value.phase == TouchPhase.Canceled)
                {
                    _lastPosition = localPos2;
                    DeltaPos = Vector2.zero;
                    DeltaScale = LocalScale = 0;
                    _lastAngle = Vector2.zero;

                    return;
                }

                //removed second finger
                if (touch2.Value.phase == TouchPhase.Ended || touch2.Value.phase == TouchPhase.Canceled)
                {
                    _lastPosition = localPos1;
                    DeltaPos = Vector2.zero;
                    DeltaScale = LocalScale = 0;
                    _lastAngle = Vector2.zero;

                    return;
                }
            }

            //added finger
            if (touch1.Value.phase == TouchPhase.Began || touch2.Value.phase == TouchPhase.Began)
            {
                DeltaPos = Vector2.zero;
                _lastPosition = Positions.Average(localPos1, localPos2);
                DeltaScale = 0;
                LocalScale = (localPos1 - localPos2).magnitude;
                DeltaAngle = 0;
                LocalAngle = Trigo.PositionToAngle(_lastAngle).Degrees;
            }
        }

        private void ApplyMovement(float deltaTime, Vector2 localPos)
        {
            float mult = Mathf.Clamp01(30f * deltaTime);

            Vector2 steering = ((localPos - _lastPosition) - DeltaPos) * MaxForce * MaxForce * mult;
            DeltaPos += steering;
            LocalPos += DeltaPos;

            _lastPosition = localPos;
        }

        private void ApplyScale(Vector2 localPos1, Vector2 localPos2)
        {
            float scale = (localPos1 - localPos2).magnitude;

            DeltaScale = (scale - LocalScale);
            LocalScale = scale;
        }

        private void ApplyRotation(Vector3 direction)
        {
            float angle = Vector3.Angle(_lastAngle, direction);

            Vector3 cross = Vector3.Cross(_lastAngle, direction).normalized;
            angle *= cross.z;

            DeltaAngle = angle;
            LocalAngle += angle;

            _lastAngle = direction;
        }

        private void ApplyFriction(float deltaTime)
        {
            float mult = Mathf.Clamp01(30f * deltaTime);

            Vector2 steering = DeltaPos * Friction * Friction * mult;
            DeltaPos -= steering;
            LocalPos += DeltaPos;

            float angleSteer = DeltaAngle * Friction * Friction * mult;
            DeltaAngle -= angleSteer;
            LocalAngle += DeltaAngle;
        }
    }
}
