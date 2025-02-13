using HotChocolate.Utils;
using UnityEngine;

namespace HotChocolate.Gestures
{
    public class TouchGesture
    {
        public enum PhaseType
        {
            Up,
            Began,
            Down,
            Ended
        }

        public PhaseType Phase { get; private set; }
        public Vector2 LocalPos { get; private set; }
        public Vector2 ScreenPos { get; private set; }

        private bool _isValid;

        private Camera _camera;
        private RectTransform _zone;

        public TouchGesture(RectTransform zone, Camera camera)
        {
            _zone = zone;
            _camera = camera;
        }

        public void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            UpdatePc();
#else
            UpdateMobile();
#endif
        }

        private void UpdatePc()
        {
            ScreenPos = Input.mousePosition;
            LocalPos = Positions.ToLocalPos(Input.mousePosition, _zone, _camera);

            if (Input.GetMouseButtonDown(0))
            {
                if (Positions.IsHitByRaycast2D(Input.mousePosition, _zone))
                {
                    Phase = PhaseType.Began;
                    _isValid = true;
                }
            }
            else if (Input.GetMouseButton(0) && _isValid)
            {
                Phase = PhaseType.Down;
            }
            else if(Input.GetMouseButtonUp(0))
            {
                if (_isValid) Phase = PhaseType.Ended;
                _isValid = false;
            }
            else
            {
                Phase = PhaseType.Up;
            }
        }

        private void UpdateMobile()
        {
            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    if (Positions.IsHitByRaycast2D(touch.position, _zone))
                    {
                        Phase = PhaseType.Began;
                        _isValid = true;

                        ScreenPos = touch.position;
                        LocalPos = Positions.ToLocalPos(touch.position, _zone, _camera);
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    if(_isValid) Phase = PhaseType.Ended;
                    _isValid = false;
                }
                else if(_isValid)
                {
                    Phase = PhaseType.Down;

                    ScreenPos = touch.position;
                    LocalPos = Positions.ToLocalPos(touch.position, _zone, _camera);
                }
            }
            else
            {
                Phase = PhaseType.Up;
            }
        }
    }
}
