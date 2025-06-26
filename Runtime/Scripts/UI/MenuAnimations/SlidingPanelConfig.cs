using UnityEngine;

namespace HotChocolate.UI.MenuAnimations
{
    [CreateAssetMenu(fileName = "SlidingPanelConfig", menuName = "HotChocolate/UI/Menu Animations/Sliding Panel", order = 1)]
    public class SlidingPanelConfig : ScriptableObject
    {
        [Tooltip("The duration in seconds of the push or focus in animation.")]
        public float inAnimationDuration = 0.5f;
        [Tooltip("The position offset at the start of the push or focus in animation.")]
        public Vector2 inStartPositionOffset = new Vector2(-1000, 0);
        [Tooltip("The amount of overshoot before settling into position.")]
        public Vector2 inOvershoot = new Vector2(0, 0);
        [Tooltip("The duration of the overshoot.")]
        public float inOvershootDuration = 0;
        [Tooltip("The time in seconds to wait before playing the push or focus in animation.")]
        public float inDelay = 0;

        [Tooltip("The tween function to use during the push or focus in animation.")]
        public Motion.EasingType inAnimationEasing = Motion.EasingType.SineEaseInOut;
        [Tooltip("The tween function to use during the overshoot.")]
        public Motion.EasingType inOvershootEasing = Motion.EasingType.SineEaseInOut;

        [Tooltip("The duration in seconds of the pop or focus out animation.")]
        public float outAnimationDuration = 0.5f;
        [Tooltip("The position offset at the end of the pop or focus out animation.")]
        public Vector2 outEndPositionOffset = new Vector2(-1000, 0);
        [Tooltip("The amount of overshoot before playing the out animation.")]
        public Vector2 outOvershoot = new Vector2(0, 0);
        [Tooltip("The duration of the overshoot.")]
        public float outOvershootDuration = 0;
        [Tooltip("The time in seconds to wait before playing the pop or focus out animation.")]
        public float outDelay = 0;

        [Tooltip("The tween function to use during the pop or focus out animation.")]
        public Motion.EasingType outAnimationEasing = Motion.EasingType.SineEaseInOut;
        [Tooltip("The tween function to use during the overshoot.")]
        public Motion.EasingType outOvershootEasing = Motion.EasingType.SineEaseInOut;
    }
}