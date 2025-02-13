using HotChocolate.Motion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.UI.Styles
{
    [CreateAssetMenu(fileName = "ButtonStyle", menuName = "HotChocolate/UI/Styles/Button Style", order = 1)]
    public class ButtonStyleConfig : ScriptableObject
    {
        public enum ButtonType
        {
            UnityStandard,
            BouncyTween
        }

        public ButtonType type;

        public float tweenDuration = 0.15f;
        public float tweenScale = 1.2f;
        public EasingType tweenEasing = EasingType.SineEaseInOut;

        public Selectable.Transition transition = Selectable.Transition.ColorTint;
        public ColorBlock colors = ColorBlock.defaultColorBlock;
        public AnimationTriggers animationTriggers;
        public RuntimeAnimatorController animator;
        public SpriteState spriteState;
    }
}
