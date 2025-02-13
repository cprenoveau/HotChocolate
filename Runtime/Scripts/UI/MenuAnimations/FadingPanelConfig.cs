using UnityEngine;

namespace HotChocolate.UI.MenuAnimations
{
    [CreateAssetMenu(fileName = "FadingPanelConfig", menuName = "HotChocolate/UI/Menu Animations/Fading Panel", order = 1)]
    public class FadingPanelConfig : ScriptableObject
    {
        [Tooltip("The duration in seconds of the push or focus in animation.")]
        public float fadeInDuration = 0.5f;
        [Tooltip("The starting alpha of the fade in the push or focus in animation.")]
        public float inAlphaStart = 0;
        [Tooltip("The alpha at the end of the fade in the push or focus in animation.")]
        public float inAlphaEnd = 1f;
        [Tooltip("The time in seconds to wait before playing the push or focus in animation.")]
        public float inDelay = 0;

        [Tooltip("The tween function to use during the push or focus in animation.")]
        public Motion.EasingType inAnimationEasing = Motion.EasingType.SineEaseInOut;

        [Tooltip("The duration in seconds of the pop or focus out animation.")]
        public float fadeOutDuration = 0.5f;
        [Tooltip("The alpha at the end of the fade in the pop or focus out animation.")]
        public float outAlphaEnd = 0;
        [Tooltip("The time in seconds to wait before playing the pop or focus out animation.")]
        public float outDelay = 0;

        [Tooltip("The tween function to use during the pop or focus out animation.")]
        public Motion.EasingType outAnimationEasing = Motion.EasingType.SineEaseInOut;
    }
}
