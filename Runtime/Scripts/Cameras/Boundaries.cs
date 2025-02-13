using UnityEngine;

namespace HotChocolate.Cameras
{
    public class Boundaries : MonoBehaviour
    {
        public Rect limits;

        private void OnDrawGizmos()
        {
            var bottomLeft = BottomLeft();
            var bottomRight = BottomRight();
            var topRight = TopRight();
            var topLeft = TopLeft();

            Debug.DrawLine(bottomLeft, bottomRight, Color.blue);
            Debug.DrawLine(bottomRight, topRight, Color.blue);
            Debug.DrawLine(topRight, topLeft, Color.blue);
            Debug.DrawLine(topLeft, bottomLeft, Color.blue);
        }

        public Vector3 RandomPoint()
        {
            var bottomLeft = BottomLeft();
            var topRight = TopRight();

            return new Vector3(Random.Range(bottomLeft.x, topRight.x), 0, Random.Range(bottomLeft.z, topRight.z));
        }

        public bool IsIn(Vector3 point)
        {
            if (point.x > transform.position.x + limits.x
                && point.x < transform.position.x + limits.x + limits.width
                && point.z > transform.position.z + limits.y
                && point.z < transform.position.z + limits.y + limits.height)
            {
                return true;
            }

            return false;
        }

        public Vector3 BottomLeft()
        {
            return new Vector3(limits.x, 0, limits.y) + transform.position;
        }

        public Vector3 BottomRight()
        {
            return BottomLeft() + new Vector3(limits.width, 0f, 0f);
        }

        public Vector3 TopLeft()
        {
            return BottomLeft() + new Vector3(0f, 0f, limits.height);
        }

        public Vector3 TopRight()
        {
            return BottomLeft() + new Vector3(limits.width, 0f, limits.height);
        }

        public Vector3 Center()
        {
            return BottomLeft() + new Vector3(limits.width / 2f, 0, limits.height / 2f);
        }

        public Vector3 ClosestPoint(Vector3 point)
        {
            Vector3 bottomLeft = BottomLeft();
            Vector3 bottomRight = BottomRight();
            Vector3 topRight = TopRight();
            Vector3 topLeft = TopLeft();

            float minDistance = float.MaxValue;
            Vector3 closestPoint = Center();

            float leftDistance = Mathf.Abs(point.x - bottomLeft.x);
            if (leftDistance < minDistance)
            {
                minDistance = leftDistance;
                closestPoint = new Vector3(bottomLeft.x, 0, Mathf.Clamp(point.z, bottomLeft.z, topLeft.z));
            }

            float rightDistance = Mathf.Abs(point.x - bottomRight.x);
            if (rightDistance < minDistance)
            {
                minDistance = rightDistance;
                closestPoint = new Vector3(bottomRight.x, 0, Mathf.Clamp(point.z, bottomLeft.z, topLeft.z));
            }

            float topDistance = Mathf.Abs(point.z - topLeft.z);
            if (topDistance < minDistance)
            {
                minDistance = topDistance;
                closestPoint = new Vector3(Mathf.Clamp(point.x, bottomLeft.x, bottomRight.x), 0, topLeft.z);
            }

            float bottomDistance = Mathf.Abs(point.z - bottomLeft.z);
            if (bottomDistance < minDistance)
            {
                minDistance = bottomDistance;
                closestPoint = new Vector3(Mathf.Clamp(point.x, bottomLeft.x, bottomRight.x), 0, bottomLeft.z);
            }

            return closestPoint;
        }
    }
}