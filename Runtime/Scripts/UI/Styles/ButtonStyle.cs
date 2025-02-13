using HotChocolate.Motion;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HotChocolate.UI.Styles
{
    [RequireComponent(typeof(Button))]
    public class ButtonStyle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public ButtonStyleConfig config;

        private void Start()
        {
            var button = GetComponent<Button>();
            if (config.type == ButtonStyleConfig.ButtonType.UnityStandard)
            {
                button.transition = config.transition;
                if (button.transition == Selectable.Transition.Animation)
                {
                    if (GetComponent<Animator>() == null) gameObject.AddComponent<Animator>();
                    GetComponent<Animator>().runtimeAnimatorController = config.animator;
                }

                button.colors = config.colors;
                button.animationTriggers = config.animationTriggers;
                button.spriteState = config.spriteState;
            }
            else
            {
                button.transition = Selectable.Transition.None;
                transform.localScale = Vector3.one;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (config.type == ButtonStyleConfig.ButtonType.BouncyTween && GetComponent<Button>().interactable)
            {
                DoBounce(config.tweenScale);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (config.type == ButtonStyleConfig.ButtonType.BouncyTween)
            {
                DoBounce(1f);
            }
        }

        private Tween<float> _bounceTween;

        private void DoBounce(float toScale)
        {
            _bounceTween = new Tween<float>(config.tweenDuration, transform.localScale.x, toScale, Mathf.Lerp, EasingUtil.EasingFunction(config.tweenEasing));
            _bounceTween.OnUpdate += UpdateBounce;
        }

        private void UpdateBounce(float scale, float progress)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }

        private void Update()
        {
            if (_bounceTween != null && !_bounceTween.Play(Time.unscaledDeltaTime))
            {
                _bounceTween = null;
            }
        }
    }
}
