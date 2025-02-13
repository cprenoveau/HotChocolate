using HotChocolate.Utils;
using UnityEngine;

namespace HotChocolate.World
{
    public interface IClickableObject
    {
        ClickableObject.Clicked OnClicked { get; }
    }

    public static class ClickableObject
    {
        public delegate void Clicked(IClickableObject obj, object data);

        public static bool RaycastAll(Vector2 screenPos, Camera camera, object data)
        {
            return RaycastAll(screenPos, camera, ~0, data);
        }

        public static bool RaycastAll(Vector2 screenPos, Camera camera, int layerMask, object data)
        {
            var hit = Positions.Raycast3D(screenPos, camera, layerMask);
            if(hit.HasValue && hit.Value.collider.GetComponent<IClickableObject>() != null)
            {
                var obj = hit.Value.collider.GetComponent<IClickableObject>();
                obj.OnClicked(obj, data);

                return true;
            }

            return false;
        }
    }
}
