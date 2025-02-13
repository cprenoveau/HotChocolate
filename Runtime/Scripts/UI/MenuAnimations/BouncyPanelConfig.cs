using UnityEngine;

namespace HotChocolate.UI.MenuAnimations
{
    [CreateAssetMenu(fileName = "BouncyPanelConfig", menuName = "HotChocolate/UI/Menu Animations/Bouncy Panel", order = 1)]
    public class BouncyPanelConfig : ScriptableObject
    {
        [Tooltip("The duration in seconds of the push or focus in animation.")]
        public float inAnimationDuration = 0.5f;
        [Tooltip("The starting scale of the bounce in the push or focus in animation.")]
        public Vector3 inScaleStart = Vector3.zero;
        [Tooltip("The scale at the peak of the bounce in the push or focus in animation.")]
        public Vector3 inScaleMax = new Vector3(1.2f, 1.2f, 1f);
        [Tooltip("The time in seconds to wait before playing the push or focus in animation.")]
        public float inDelay = 0;

        [Tooltip("The tween function to use during the growing phase of the bounce in the push or focus in animation.")]
        public Motion.EasingType inAnimationEasing = Motion.EasingType.SineEaseInOut;
        [Tooltip("The tween function to use during the settle phase of the bounce in the push or focus in animation.")]
        public Motion.EasingType inAnimationSettleEasing = Motion.EasingType.SineEaseInOut;

        [Tooltip("The duration in seconds of the pop or focus out animation.")]
        public float outAnimationDuration = 0.5f;
        [Tooltip("The scale at the peak of the bounce in the pop or focus out animation.")]
        public Vector3 outScaleMax = new Vector3(1.2f, 1.2f, 1f);
        [Tooltip("The scale at the end of the bounce in the pop or focus out animation.")]
        public Vector3 outScaleEnd = Vector3.zero;
        [Tooltip("The time in seconds to wait before playing the pop or focus out animation.")]
        public float outDelay = 0;

        [Tooltip("The tween function to use during the growing phase of the bounce in the pop or focus out animation.")]
        public Motion.EasingType outAnimationEasing = Motion.EasingType.SineEaseInOut;
        [Tooltip("The tween function to use during the settle phase of the bounce in the pop or focus out animation.")]
        public Motion.EasingType outAnimationSettleEasing = Motion.EasingType.SineEaseIn;
    }
}
