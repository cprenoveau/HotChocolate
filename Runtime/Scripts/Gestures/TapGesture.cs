using HotChocolate.Utils;
using UnityEngine;

namespace HotChocolate.Gestures
{
    public class TapGesture
    {
        /// <summary>
        /// Time in seconds after which a touch is no longer valid for tap.
        /// </summary>
        public float MaxDuration { get; set; } = 0.25f;

        /// <summary>
        /// The amount of movement in local space allowed before a touch is no longer valid for tap.
        /// </summary>
        public float DragTolerance { get; set; } = 50f;

        public bool IsTrue { get; private set; }
        public Vector2 LocalPos { get; private set; }
        public Vector2 ScreenPos { get; private set; }

        private Camera _camera;
        private RectTransform _zone;

        private bool _isValid;
        private float _touchDuration;
        private TouchWrapper _soloTouch;

        public TapGesture(RectTransform zone, Camera camera, int? soloTouchIndex = null)
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

            if(Input.GetMouseButtonDown(0))
            {
                if (Positions.IsHitByRaycast2D(Input.mousePosition, _zone))
                {
                    ScreenPos = Input.mousePosition;
                    LocalPos = Positions.ToLocalPos(Input.mousePosition, _zone, _camera);

                    _touchDuration = 0;
                    _isValid = true;
                }
            }
            else if(Input.GetMouseButton(0))
            {
                if (_isValid)
                {
                    _touchDuration += deltaTime;
                    if(_touchDuration > MaxDuration)
                    {
                        _isValid = false;
                    }
                }
            }
            else
            {
                if (_isValid)
                {
                    var pos = Positions.ToLocalPos(Input.mousePosition, _zone, _camera);
                    if ((LocalPos - pos).magnitude <= DragTolerance)
                    {
                        IsTrue = true;
                    }
                }

                _isValid = false;
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

            if(activeTouches > 1)
            {
                _isValid = false;
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
                        LocalPos = Positions.ToLocalPos(touch.position, _zone, _camera);

                        _touchDuration = 0;
                        _isValid = true;
                    }
                }
                else if(_isValid)
                {
                    _touchDuration += deltaTime;
                    if(_touchDuration > MaxDuration)
                    {
                        _isValid = false;
                    }
                }
            }
            else
            {
                if (_isValid && touch1.HasValue)
                {
                    var pos = Positions.ToLocalPos(touch1.Value.position, _zone, _camera);
                    if ((LocalPos - pos).magnitude <= DragTolerance)
                    {
                        IsTrue = true;
                    }
                }

                _isValid = false;
            }
        }
    }
}
