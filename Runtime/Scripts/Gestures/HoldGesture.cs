using HotChocolate.Utils;
using UnityEngine;

namespace HotChocolate.Gestures
{
    public class HoldGesture
    {        
        /// <summary>
        /// Time in seconds for a hold to be true.
        /// </summary>
        public float MinDuration { get; set; } = 2f;
        
        /// <summary>
        /// The amount of movement in local space allowed before a touch is no longer valid for hold.
        /// </summary>
        public float DragTolerance { get; set; } = 1f;

        /// <summary>
        /// Should the touch stay within the input zone in order to be valid.
        /// </summary>
        public bool MustStayWithinZone { get; set; }

        public bool IsTrue { get; private set; }
        public bool IsValid { get; private set; }
        public Vector2 LocalPos { get; private set; }
        public Vector2 ScreenPos { get; private set; }
        public float TouchDuration { get; private set; }

        private Camera _camera;
        private RectTransform _zone;

        private bool _isActive;
        private TouchWrapper _soloTouch;

        public HoldGesture(RectTransform zone, Camera camera, int? soloTouchIndex = null)
        {
            _zone = zone;
            _camera = camera;

            if (soloTouchIndex.HasValue)
            {
                _soloTouch = new TouchWrapper(soloTouchIndex.Value);
            }
        }

        public void Release()
        {
            IsTrue = false;
            _isActive = false;
            IsValid = false;
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

            if (Input.GetMouseButton(0))
            {
                if (!_isActive)
                {
                    _isActive = true;

                    if (Positions.IsHitByRaycast2D(Input.mousePosition, _zone))
                    {
                        ScreenPos = Input.mousePosition;
                        LocalPos = Positions.ToLocalPos(Input.mousePosition, _zone, _camera);

                        TouchDuration = 0;
                        IsValid = true;

                        if (MinDuration == 0)
                            IsTrue = true;
                    }
                }
                else if (IsValid)
                {
                    if (MustStayWithinZone && !Positions.IsHitByRaycast2D(Input.mousePosition, _zone))
                    {
                        Release();
                    }
                    else
                    {
                        var pos = Positions.ToLocalPos(Input.mousePosition, _zone, _camera);
                        if ((LocalPos - pos).magnitude > DragTolerance)
                        {
                            IsValid = false;
                            IsTrue = false;
                        }
                        else
                        {
                            TouchDuration += deltaTime;
                            IsTrue = TouchDuration >= MinDuration;
                        }
                    }
                }
            }
            else
            {
                Release();
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

            if (activeTouches == 1)
            {
                var touch = touch1.Value;

                if (!_isActive)
                {
                    _isActive = true;

                    if (Positions.IsHitByRaycast2D(touch.position, _zone))
                    {
                        ScreenPos = touch.position;
                        LocalPos = Positions.ToLocalPos(touch.position, _zone, _camera);

                        TouchDuration = 0;
                        IsValid = true;

                        if (MinDuration == 0)
                            IsTrue = true;
                    }
                }
                else if (IsValid)
                {
                    if (MustStayWithinZone && !Positions.IsHitByRaycast2D(touch.position, _zone))
                    {
                        Release();
                    }
                    else
                    {
                        var pos = Positions.ToLocalPos(touch.position, _zone, _camera);
                        if ((LocalPos - pos).magnitude > DragTolerance)
                        {
                            IsValid = false;
                            IsTrue = false;
                        }
                        else
                        {
                            TouchDuration += deltaTime;
                            IsTrue = TouchDuration >= MinDuration;
                        }
                    }
                }
                else
                {
                    Release();
                }
            }
        }
    }
}
