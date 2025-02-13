using HotChocolate.Utils;
using UnityEngine;

namespace HotChocolate.Gestures
{
    public class SwipeGesture
    {
        public float MinSpeed { get; set; } = 100;
        public float MinDistance { get; set; } = 100;
        public bool MustStayWithinZone { get; set; }

        public bool IsActive { get; private set; }
        public bool IsTrue { get; private set; }
        public Vector2 Direction { get; private set; }
        public Vector2 ScreenPos { get; private set; }
        public Vector2 LocalPos { get; private set; }
        public float Duration { get; private set; }

        private Camera _camera;
        private RectTransform _zone;
        private Vector2 _startPosition;
        private TouchWrapper _soloTouch;

        public SwipeGesture(RectTransform zone, Camera camera, int? soloTouchIndex = null)
        {
            _zone = zone;
            _camera = camera;

            if (soloTouchIndex.HasValue)
            {
                _soloTouch = new TouchWrapper(soloTouchIndex.Value);
            }
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
            IsTrue = false;

            if (Input.GetMouseButtonDown(0))
            {
                if (Positions.IsHitByRaycast2D(Input.mousePosition, _zone))
                {
                    ScreenPos = Input.mousePosition;
                    _startPosition = LocalPos = Positions.ToLocalPos(Input.mousePosition, _zone, _camera);

                    Duration = 0;
                    IsActive = true;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (IsActive)
                {
                    if (MustStayWithinZone && !Positions.IsHitByRaycast2D(Input.mousePosition, _zone))
                    {
                        IsActive = false;
                    }
                    else
                    {
                        Duration += deltaTime;

                        ScreenPos = Input.mousePosition;
                        LocalPos = Positions.ToLocalPos(Input.mousePosition, _zone, _camera);

                        float distance = (_startPosition - LocalPos).magnitude;

                        if (distance > MinDistance)
                        {
                            if (distance > MinSpeed * Duration)
                                IsTrue = true;

                            IsActive = false;
                            Direction = (LocalPos - _startPosition).normalized;
                        }
                    }
                }
            }
            else
            {
                IsActive = false;
            }
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
                deltaTime);
        }

        private void UpdateMobile(int touchCount, int activeTouches, Touch? touch1, float deltaTime)
        {
            IsTrue = false;

            if (activeTouches > 1)
            {
                IsActive = false;
                return;
            }

            if (activeTouches == 1)
            {
                var touch = touch1.Value;
                if (touch.phase == TouchPhase.Began)
                {
                    if (Positions.IsHitByRaycast2D(touch.position, _zone))
                    {
                        ScreenPos = touch.position;
                        _startPosition = LocalPos = Positions.ToLocalPos(touch.position, _zone, _camera);

                        Duration = 0;
                        IsActive = true;
                    }
                }
                else if (IsActive)
                {
                    if (MustStayWithinZone && !Positions.IsHitByRaycast2D(touch.position, _zone))
                    {
                        IsActive = false;
                    }
                    else
                    {
                        Duration += deltaTime;

                        ScreenPos = touch.position;
                        LocalPos = Positions.ToLocalPos(touch.position, _zone, _camera);

                        float distance = (_startPosition - LocalPos).magnitude;
                        if (distance > MinDistance)
                        {
                            if (distance > MinSpeed * Duration)
                                IsTrue = true;

                            IsActive = false;
                            Direction = (LocalPos - _startPosition).normalized;
                        }
                    }
                }
            }
            else
            {
                IsActive = false;
            }
        }
    }
}
