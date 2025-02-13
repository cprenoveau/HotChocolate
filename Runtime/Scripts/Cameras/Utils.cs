using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.Cameras
{
    public static class Utils
    {
        public static Plane FloorPlane { get { return new Plane(Vector3.up, Vector3.zero); } }

        public static Vector3 PosOnPlaneFromViewport(Vector2 viewPortPos, Plane plane, Camera camera)
        {
            var ray = camera.ViewportPointToRay(viewPortPos);
            plane.Raycast(ray, out float d);

            return ray.GetPoint(d);
        }

        public static Vector3 PosOnPlaneFromScreen(Vector2 screenPos, Plane plane, Camera camera)
        {
            var ray = camera.ScreenPointToRay(screenPos);
            plane.Raycast(ray, out float d);

            return ray.GetPoint(d);
        }

        public static float ZoomSpeedModifier(float zoom, float minZoom, float maxZoom, float zoomModifierAtMin)
        {
            return Mathf.Log(((zoom - minZoom) / (maxZoom - minZoom)) + zoomModifierAtMin * (1f - zoomModifierAtMin) + 1f);
        }

        public static Vector3 FlatForward(Camera camera)
        {
            return FlatForward(camera.transform);
        }

        public static Vector3 FlatRight(Camera camera)
        {
            return FlatRight(camera.transform);
        }

        public static Vector3 FlatForward(Transform transform)
        {
            return new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        }

        public static Vector3 FlatRight(Transform transform)
        {
            return new Vector3(transform.right.x, 0, transform.right.z).normalized;
        }

        public static Vector3 PositionInBoundaries(Transform cameraTransform, Vector3 position, Boundaries boundaries)
        {
            var floorPlane = FloorPlane;
            var ray = new Ray(position, cameraTransform.forward);

            if (floorPlane.Raycast(ray, out float d))
            {
                var floorPoint = ray.GetPoint(d);
                if (!boundaries.IsIn(floorPoint))
                {
                    floorPoint = boundaries.ClosestPoint(floorPoint);
                    position = floorPoint - cameraTransform.forward * d;
                }
            }

            return position;
        }

        public static async Task CameraTransition(Transform cameraTransform, Vector3 position, Quaternion rotation, CancellationToken ct, float duration = 1.0f, Boundaries boundaries = null)
        {
            if (boundaries != null)
            {
                position = PositionInBoundaries(cameraTransform, position, boundaries);
            }

            Vector3 startPosition = cameraTransform.position;

            if (startPosition == position)
                return;

            var positionTween = new Motion.Tween<Vector3>(duration, cameraTransform.position, position, Vector3.Lerp, Motion.Easing.QuadEaseInOut);
            positionTween.OnUpdate += (Vector3 value, float progress) => cameraTransform.position = value;

            var rotationTween = new Motion.Tween<Quaternion>(duration, cameraTransform.rotation, rotation, Quaternion.Slerp, Motion.Easing.QuadEaseInOut);
            rotationTween.OnUpdate += (Quaternion value, float progress) => cameraTransform.rotation = value;

            while(!ct.IsCancellationRequested && positionTween != null)
            {
                await Task.Yield();

                if(!positionTween.Play(Time.unscaledDeltaTime))
                {
                    positionTween = null;
                }

                if(!rotationTween.Play(Time.unscaledDeltaTime))
                {
                    rotationTween = null;
                }
            }
        }
    }
}
