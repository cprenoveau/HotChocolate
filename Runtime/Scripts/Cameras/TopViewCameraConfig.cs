using UnityEngine;

namespace HotChocolate.Cameras
{
    [CreateAssetMenu(fileName = "TopViewCameraConfig", menuName = "HotChocolate/Cameras/Top View Camera Config", order = 1)]
    public class TopViewCameraConfig : ScriptableObject
    {
        [Tooltip("The shortest height the camera can get from the ground")]
        public float minZoom = 5f;
        [Tooltip("The farthest height the camera can get from the ground")]
        public float maxZoom = 50f;

        public float zoomSpeed = 1f;
        [Tooltip("The speed modifier at min zoom. Zooming speed is logarithmic"), Range(0, 1)]
        public float zoomSpeedModifierAtMin = 0.5f;

        [Tooltip("The name of the layer to raycast the ground. Can be null if ground is 0")]
        public string groundLayerMask = "Ground";

        [Tooltip("The duration in seconds of the bounce when the camera zooms out of range")]
        public float bounceDuration = 0.2f;
        public float floorBounceAmount = 1f;
        public float ceilingBounceAmount = 2f;

        [Tooltip("How smoothly should the camera move from one ground level to another"), Range(0,0.999f)]
        public float obstacleClimbSmoothness = 0.5f;

        public bool allowRotation = true;
        public bool allowBounce = true;
    }
}
