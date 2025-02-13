using HotChocolate.Gestures;
using HotChocolate.Motion;
using HotChocolate.Utils;
using UnityEngine;

namespace HotChocolate.Cameras
{
    public class TopViewCamera
    {
        public Boundaries Boundaries { get; set; }
        public Vector3 Ground { get; set; } = Vector3.zero;

        private Camera _camera;
        private TopViewCameraConfig _config;
        private Tween<Vector3> _bounce;

        private Vector3? _lastZoomDir;
        private Vector2? _lastScreenPos;
        private int _lastFingerCount;
        private float _speedMultX;
        private float _speedMultY;

        private Plane _floorZeroPlane;

        public void Init(Camera camera, TopViewCameraConfig config)
        {
            _camera = camera;
            _config = config;

            _floorZeroPlane = Utils.FloorPlane;
            RaycastGround();
        }

        public void Reset()
        {
            _lastScreenPos = null;
        }

        public void Move(float deltaTime, MoveGesture move)
        {
            if (_camera == null)
                return;

            if (_bounce != null)
            {
                if (!_bounce.Play(deltaTime)) _bounce = null;
            }
            else
            {
                Pan(move.IsActive, move.DeltaPos, move.ScreenPos);
                Zoom(move.DeltaScale, move.LocalScale, move.ScreenPos);
                Rotate(move.DeltaAngle);
            }
        }

        public void Move(float deltaTime, bool touchIsActive, Vector2 deltaPos, float deltaScale, float localScale, float deltaAngle, Vector2 screenPos)
        {
            if (_camera == null)
                return;

            if (_bounce != null)
            {
                if (!_bounce.Play(deltaTime)) _bounce = null;
            }
            else
            {
                Pan(touchIsActive, deltaPos, screenPos);
                Zoom(deltaScale, localScale, screenPos);
                Rotate(deltaAngle);
            }
        }

        public void Pan(bool touchIsActive, Vector2 deltaPos, Vector2 screenPos)
        {
            if (_camera == null)
                return;

            if (touchIsActive)
            {
                int touchCount = Input.touchCount;
                if (_lastFingerCount != touchCount)
                {
                    _lastFingerCount = touchCount;
                    _lastScreenPos = null;
                    return;
                }

                if (_lastScreenPos.HasValue)
                {
                    var groundPlane = new Plane(Vector3.up, -Ground.y);

                    var posOnGround = Utils.PosOnPlaneFromScreen(screenPos, groundPlane, _camera);
                    var lastPosOnGround = Utils.PosOnPlaneFromScreen(_lastScreenPos.Value, groundPlane, _camera);

                    if (deltaPos.x != 0 || deltaPos.y != 0)
                    {
                        _camera.transform.position += Vector3.forward * (lastPosOnGround.z - posOnGround.z);

                        if (!IsInBoundaries())
                        {
                            MoveBackIntoBoundaries();
                        }

                        _camera.transform.position += Vector3.right * (lastPosOnGround.x - posOnGround.x);

                        if (!IsInBoundaries())
                        {
                            MoveBackIntoBoundaries();
                        }
                    }

                    _speedMultX = deltaPos.x != 0 ? Vector3.Dot(Utils.FlatRight(_camera), (lastPosOnGround - posOnGround)) / deltaPos.x : 0;
                    _speedMultY = deltaPos.y != 0 ? Vector3.Dot(Utils.FlatForward(_camera), (lastPosOnGround - posOnGround)) / deltaPos.y : 0;
                }

                _lastScreenPos = screenPos;

                RaycastGround();
            }
            else
            {
                _lastScreenPos = null;

                _camera.transform.position += Utils.FlatForward(_camera) * -deltaPos.y * Mathf.Abs(_speedMultY);

                if (!IsInBoundaries())
                {
                    MoveBackIntoBoundaries();
                }

                _camera.transform.position += Utils.FlatRight(_camera) * -deltaPos.x * Mathf.Abs(_speedMultX);

                if (!IsInBoundaries())
                {
                    MoveBackIntoBoundaries();
                }

                if (deltaPos.x != 0 || deltaPos.y != 0)
                {
                    RaycastGround();
                }
            }
        }

        public bool IsInBoundaries()
        {
            if (Boundaries == null)
                return true;

            return Boundaries.IsIn(Utils.PosOnPlaneFromViewport(new Vector2(0.5f, 0.5f), _floorZeroPlane, _camera));
        }

        private void MoveBackIntoBoundaries()
        {
            if (Boundaries == null)
                return;

            _camera.transform.position = Utils.PositionInBoundaries(_camera.transform, _camera.transform.position, Boundaries);
        }

        private void RaycastGround()
        {
            if (string.IsNullOrEmpty(_config.groundLayerMask))
                return;

            float ceilingDistance = _config.maxZoom + _config.ceilingBounceAmount;
            float floorDistance = _config.minZoom - _config.floorBounceAmount;

            var groundHit = Positions.Raycast3D(
                new Ray(_camera.transform.position - _camera.transform.forward * 20f, _camera.transform.forward),
                float.MaxValue,
                LayerMask.GetMask(_config.groundLayerMask)
                );

            if (groundHit.HasValue)
            {
                Ground = groundHit.Value.point;
            }

            if (_camera.transform.position.y - Ground.y < floorDistance)
            {
                float diff = Ground.y + floorDistance - _camera.transform.position.y;
                _camera.transform.position += Vector3.up * Mathf.Lerp(0, diff, (1f - _config.obstacleClimbSmoothness));
            }
            else if (_camera.transform.position.y - Ground.y > ceilingDistance)
            {
                float diff = Ground.y + ceilingDistance - _camera.transform.position.y;
                _camera.transform.position += Vector3.up * Mathf.Lerp(0, diff, (1f - _config.obstacleClimbSmoothness));
            }
        }

        public void Zoom(float deltaScale, float localScale, Vector2 screenPos)
        {
            if (_camera == null)
                return;

            float ceilingDistance = _config.maxZoom + _config.ceilingBounceAmount;
            float floorDistance = _config.minZoom - _config.floorBounceAmount;

            if (Mathf.Abs(deltaScale) > 0)
            {
                float deltaZoom = deltaScale * _config.zoomSpeed * Utils.ZoomSpeedModifier(_camera.transform.position.y - Ground.y, _config.minZoom, _config.maxZoom, _config.zoomSpeedModifierAtMin);

                var touchPos = Utils.PosOnPlaneFromScreen(screenPos, _floorZeroPlane, _camera);

                Vector3 v = (touchPos - _camera.transform.position);
                Vector3 dir = v.normalized;

                Vector3 newPosition = _camera.transform.position + deltaZoom * dir;
                float vDistance = _camera.transform.position.y - Ground.y;

                if (newPosition.y <= _camera.transform.position.y && vDistance <= floorDistance)
                {
                    return;
                }

                if (newPosition.y >= _camera.transform.position.y && vDistance >= ceilingDistance)
                {
                    return;
                }

                _lastZoomDir = dir;

                _camera.transform.position = newPosition;
                vDistance = _camera.transform.position.y - Ground.y;

                if (vDistance < floorDistance)
                {
                    _camera.transform.position = FloorPosition(floorDistance, _lastZoomDir.Value);
                }
                else if (vDistance > ceilingDistance)
                {
                    _camera.transform.position = CeilingPosition(ceilingDistance, _lastZoomDir.Value);
                }

                if (!IsInBoundaries())
                {
                    MoveBackIntoBoundaries();
                }

                RaycastGround();
            }
            else if (_lastZoomDir.HasValue && localScale == 0)
            {
                float vDistance = _camera.transform.position.y - Ground.y;

                if (vDistance > _config.maxZoom)
                {
                    Bounce(CeilingPosition(_config.maxZoom, _lastZoomDir.Value), _config.bounceDuration);
                }
                else if (vDistance < _config.minZoom)
                {
                    Bounce(FloorPosition(_config.minZoom, _lastZoomDir.Value), _config.bounceDuration);
                }

                _lastZoomDir = null;
            }
        }

        public Vector3 CeilingPosition(float maxHeight, Vector3 zoomDir)
        {
            float vDistance = _camera.transform.position.y - Ground.y;
            var angle = Quaternion.LookRotation(zoomDir).eulerAngles;

            float y = vDistance - maxHeight;
            float x = y * Mathf.Sin(angle.x * Mathf.Deg2Rad);
            float h = Mathf.Sqrt(x * x + y * y);

            return _camera.transform.position + zoomDir * h;
        }

        public Vector3 FloorPosition(float minHeight, Vector3 zoomDir)
        {
            float vDistance = _camera.transform.position.y - Ground.y;
            var angle = Quaternion.LookRotation(zoomDir).eulerAngles;

            float y = minHeight - vDistance;
            float x = y * Mathf.Sin(angle.x * Mathf.Deg2Rad);
            float h = Mathf.Sqrt(x * x + y * y);

            return _camera.transform.position - zoomDir * h;
        }

        public void Rotate(float deltaAngle)
        {
            if (_camera == null)
                return;

            if (_config.allowRotation)
            {
                var groundPlane = new Plane(Vector3.up, -Ground.y);

                Vector3 center = Utils.PosOnPlaneFromViewport(new Vector2(0.5f, 0.5f), groundPlane, _camera);
                center.y = _camera.transform.position.y;

                _camera.transform.RotateAround(center, Vector3.up, deltaAngle);
            }
        }

        private void Bounce(Vector3 toPosition, float duration)
        {
            if (_config.allowBounce)
            {
                _bounce = new Tween<Vector3>(duration, _camera.transform.position, toPosition, Vector3.Lerp, Easing.QuadEaseInOut);
                _bounce.OnUpdate += BounceUpdate;
            }
            else
            {
                BounceUpdate(toPosition, 1f);
            }
        }

        private void BounceUpdate(Vector3 position, float progress)
        {
            _camera.transform.position = position;

            if (!IsInBoundaries())
            {
                MoveBackIntoBoundaries();
                _bounce = null;
            }
        }
    }
}
