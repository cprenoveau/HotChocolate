using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HotChocolate.Utils
{
    public static class Positions
    {
        public static float Average(float min, float max)
        {
            return min + (max - min) / 2f;
        }

        public static Vector2 Average(Vector2 pos1, Vector2 pos2)
        {
            return pos1 + (pos2 - pos1) / 2f;
        }

        public static Vector3 Average(Vector3 pos1, Vector3 pos2)
        {
            return pos1 + (pos2 - pos1) / 2f;
        }

        public static Vector3 Average(List<Vector3> positions)
        {
            return Average(positions.ToArray());
        }

        public static Vector3 Average(Vector3[] positions)
        {
            if (positions.Length == 0)
                return Vector3.zero;

            float x = 0f;
            float y = 0f;
            float z = 0f;

            foreach (Vector3 pos in positions)
            {
                x += pos.x;
                y += pos.y;
                z += pos.z;
            }

            return new Vector3(x / positions.Length, y / positions.Length, z / positions.Length);
        }

        public static Vector2 ToLocalPos(Vector2 screenPos, RectTransform rect)
        {
            var canvas = rect.GetComponentInParent<Canvas>();
            return ToLocalPos(screenPos, rect, canvas != null ? canvas.worldCamera : null);
        }

        public static Vector2 ToLocalPos(Vector2 screenPos, RectTransform rect, Camera camera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, camera, out Vector2 localPos);
            return localPos;
        }

        public static Rect ScreenRect(RectTransform rectTransform)
        {
            var worldPoints = new Vector3[4];
            rectTransform.GetWorldCorners(worldPoints);

            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if(canvas.worldCamera != null)
            {
                worldPoints[0] = canvas.worldCamera.WorldToScreenPoint(worldPoints[0]);
                worldPoints[2] = canvas.worldCamera.WorldToScreenPoint(worldPoints[2]);
            }

            return new Rect()
            {
                x = worldPoints[0].x,
                y = worldPoints[0].y,
                width = worldPoints[2].x - worldPoints[0].x,
                height = worldPoints[2].y - worldPoints[0].y
            };
        }

        public static bool IsHitByRaycast2D(Vector2 screenPos, RectTransform rect)
        {
            var pointer = new PointerEventData(EventSystem.current);
            pointer.position = screenPos;

            var raycastResults = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointer, raycastResults);
            if (raycastResults.Count > 0)
            {
                return raycastResults[0].gameObject == rect.gameObject;
            }

            return false;
        }

        public static RaycastResult? Raycast2D(Vector2 screenPos)
        {
            var pointer = new PointerEventData(EventSystem.current);
            pointer.position = screenPos;

            var raycastResults = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointer, raycastResults);
            if (raycastResults.Count > 0)
            {
                return raycastResults[0];
            }

            return null;
        }

        public static RaycastHit? Raycast3D(Vector2 screenPos, Camera camera, Collider target)
        {
            Ray ray = camera.ScreenPointToRay(screenPos);
            return Raycast3D(ray, target, camera.farClipPlane);
        }

        public static RaycastHit? Raycast3D(Vector3 origin, Vector3 direction, Collider target, float maxDistance = float.MaxValue)
        {
            Ray ray = new Ray(origin, direction);
            return Raycast3D(ray, target, maxDistance);
        }

        public static RaycastHit? Raycast3D(Ray ray, Collider target, float maxDistance)
        {
            if (target.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                return hit;
            }

            return null;
        }

        public static RaycastHit? Raycast3D(Vector2 screenPos, Camera camera)
        {
            Ray ray = camera.ScreenPointToRay(screenPos);
            return Raycast3D(ray);
        }

        public static RaycastHit? Raycast3D(Vector3 origin, Vector3 direction)
        {
            Ray ray = new Ray(origin, direction);
            return Raycast3D(ray);
        }

        public static RaycastHit? Raycast3D(Ray ray)
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit;
            }

            return null;
        }

        public static RaycastHit? Raycast3D(Vector2 screenPos, Camera camera, int layerMask)
        {
            Ray ray = camera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, camera.farClipPlane, layerMask))
            {
                return hit;
            }

            return null;
        }

        public static RaycastHit? Raycast3D(Ray ray, float maxDistance, int layerMask)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
            {
                return hit;
            }

            return null;
        }

        public static (Vector2 min, Vector2 max) FindMinMax(Vector3[] vertices)
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            foreach (var vertex in vertices)
            {
                min.x = Mathf.Min(min.x, vertex.x);
                min.y = Mathf.Min(min.y, vertex.y);
                max.x = Mathf.Max(max.x, vertex.x);
                max.y = Mathf.Max(max.y, vertex.y);
            }

            return (min, max);
        }

        public static (Vector2 min, Vector2 max) FindViewMinMax(Vector3[] vertices, Camera camera, bool convertToShaderSpace = false)
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            foreach (var vertex in vertices)
            {
                var point = WorldToView(vertex, camera, convertToShaderSpace);
                min.x = Mathf.Min(min.x, point.x);
                min.y = Mathf.Min(min.y, point.y);
                max.x = Mathf.Max(max.x, point.x);
                max.y = Mathf.Max(max.y, point.y);
            }

            return (min, max);
        }

        public static (Vector2 min, Vector2 max) FindViewMinMax(Bounds bounds, Camera camera, bool convertToShaderSpace = false)
        {
            Vector3[] vertices = GetVertices(bounds);
            return FindViewMinMax(vertices, camera, convertToShaderSpace);
        }

        public static Vector2 WorldToView(Vector3 point, Camera camera, bool convertToShaderSpace)
        {
            if (convertToShaderSpace)
                return camera.WorldToViewportPoint(point) * 2f - Vector3.one;
            else
                return camera.WorldToViewportPoint(point);
        }

        private static Vector3[] vertices = new Vector3[8];
        public static Vector3[] GetVertices(Bounds bounds)
        {
            vertices[0] = bounds.center - bounds.extents;
            vertices[1] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);
            vertices[2] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);
            vertices[3] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);
            vertices[4] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z);
            vertices[5] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);
            vertices[6] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);
            vertices[7] = bounds.center + bounds.extents;

            return vertices;
        }
    }
}
