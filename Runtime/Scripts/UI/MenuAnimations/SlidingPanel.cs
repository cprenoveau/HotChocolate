using HotChocolate.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.UI.MenuAnimations
{
    public class SlidingPanel : MonoBehaviour, IMenuAnimation
    {
        public float pushDelay;
        public float popDelay;
        public float focusInDelay;
        public float focusOutDelay;
        public List<string> onlyFromMenus = new List<string>();

        public SlidingPanelConfig pushConfig;
        public SlidingPanelConfig focusConfig;

        private Vector2? originalPos;

        public bool Disposed { get; private set; }
        public IEnumerable<string> OnlyFromMenus => onlyFromMenus;

        public void Dispose()
        {
            Disposed = true;
        }

        private void OnDestroy()
        {
            Disposed = true;
        }

        public async Task PushAnimation(CancellationToken ct)
        {
            if(pushConfig != null)
                await this.StartCoroutineAsync(InAnimationCoroutine(pushConfig, pushDelay), ct).ConfigureAwait(true);
        }

        public async Task PopAnimation(CancellationToken ct)
        {
            if(pushConfig != null)
                await this.StartCoroutineAsync(OutAnimationCoroutine(pushConfig, popDelay), ct).ConfigureAwait(true);
        }

        public async Task FocusInAnimation(CancellationToken ct)
        {
            if(focusConfig != null)
                await this.StartCoroutineAsync(InAnimationCoroutine(focusConfig, focusInDelay), ct).ConfigureAwait(true);
        }

        public async Task FocusOutAnimation(CancellationToken ct)
        {
            if(focusConfig != null)
                await this.StartCoroutineAsync(OutAnimationCoroutine(focusConfig, focusOutDelay), ct).ConfigureAwait(true);
        }

        private IEnumerator InAnimationCoroutine(SlidingPanelConfig config, float delay)
        {
            if (!originalPos.HasValue)
                originalPos = transform.GetComponent<RectTransform>().anchoredPosition;

            transform.localPosition = originalPos.Value + config.inStartPositionOffset;

            var clip = new Motion.ClipSequence();

            var inAnimation = new Motion.Tween<Vector2>(config.inAnimationDuration - config.inOvershootDuration, transform.localPosition, originalPos.Value + config.inOvershoot, Vector2.Lerp, Motion.EasingUtil.EasingFunction(config.inAnimationEasing));
            inAnimation.OnUpdate += UpdatePosition;

            if (config.inDelay + delay > 0)
            {
                clip.Append(new Motion.Silence(config.inDelay + delay));
            }

            clip.Append(inAnimation);

            if (config.inOvershoot != Vector2.zero)
            {
                var overshootAnimation = new Motion.Tween<Vector2>(config.inOvershootDuration, originalPos.Value + config.inOvershoot, originalPos.Value, Vector2.Lerp, Motion.EasingUtil.EasingFunction(config.inOvershootEasing));
                overshootAnimation.OnUpdate += UpdatePosition;
                clip.Append(overshootAnimation);
            }

            while (clip.Play(Time.unscaledDeltaTime))
            {
                yield return null;
            }
        }

        private IEnumerator OutAnimationCoroutine(SlidingPanelConfig config, float delay)
        {
            if (!originalPos.HasValue)
                originalPos = transform.GetComponent<RectTransform>().anchoredPosition;

            var clip = new Motion.ClipSequence();

            var outAnimation = new Motion.Tween<Vector2>(config.outAnimationDuration - config.outOvershootDuration, transform.GetComponent<RectTransform>().anchoredPosition + config.outOvershoot, originalPos.Value + config.outEndPositionOffset, Vector2.Lerp, Motion.EasingUtil.EasingFunction(config.outAnimationEasing));
            outAnimation.OnUpdate += UpdatePosition;

            if (config.outDelay + delay > 0)
            {
                clip.Append(new Motion.Silence(config.outDelay + delay));
            }

            if (config.outOvershoot != Vector2.zero)
            {
                var overshootAnimation = new Motion.Tween<Vector2>(config.outOvershootDuration, transform.GetComponent<RectTransform>().anchoredPosition, transform.GetComponent<RectTransform>().anchoredPosition + config.outOvershoot, Vector2.Lerp, Motion.EasingUtil.EasingFunction(config.outOvershootEasing));
                overshootAnimation.OnUpdate += UpdatePosition;
                clip.Append(overshootAnimation);
            }

            clip.Append(outAnimation);

            while (clip.Play(Time.unscaledDeltaTime))
            {
                yield return null;
            }
        }

        private void UpdatePosition(Vector2 value, float progress)
        {
            transform.GetComponent<RectTransform>().anchoredPosition = value;
        }
    }
}
