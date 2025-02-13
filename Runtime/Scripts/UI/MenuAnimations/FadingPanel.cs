using HotChocolate.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.UI.MenuAnimations
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadingPanel : MonoBehaviour, IMenuAnimation
    {
        public float pushDelay;
        public float popDelay;
        public float focusInDelay;
        public float focusOutDelay;
        public List<string> onlyFromMenus = new List<string>();

        public FadingPanelConfig pushConfig;
        public FadingPanelConfig focusConfig;

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
            if (pushConfig != null)
                await this.StartCoroutineAsync(InAnimationCoroutine(pushConfig, pushDelay), ct).ConfigureAwait(true);
        }

        public async Task PopAnimation(CancellationToken ct)
        {
            if (pushConfig != null)
                await this.StartCoroutineAsync(OutAnimationCoroutine(pushConfig, popDelay), ct).ConfigureAwait(true);
        }

        public async Task FocusInAnimation(CancellationToken ct)
        {
            if (focusConfig != null)
                await this.StartCoroutineAsync(InAnimationCoroutine(focusConfig, focusInDelay), ct).ConfigureAwait(true);
        }

        public async Task FocusOutAnimation(CancellationToken ct)
        {
            if (focusConfig != null)
                await this.StartCoroutineAsync(OutAnimationCoroutine(focusConfig, focusOutDelay), ct).ConfigureAwait(true);
        }

        private IEnumerator InAnimationCoroutine(FadingPanelConfig config, float delay)
        {
            GetComponent<CanvasGroup>().alpha = config.inAlphaStart;

            var clip = new Motion.ClipSequence();

            var inAnimation = new Motion.Tween<float>(config.fadeInDuration, GetComponent<CanvasGroup>().alpha, config.inAlphaEnd, Mathf.Lerp, Motion.EasingUtil.EasingFunction(config.inAnimationEasing));
            inAnimation.OnUpdate += UpdateAlpha;

            if (config.inDelay + delay > 0)
            {
                clip.Append(new Motion.Silence(config.inDelay + delay));
            }

            clip.Append(inAnimation);

            while (clip.Play(Time.unscaledDeltaTime))
            {
                yield return null;
            }
        }

        private IEnumerator OutAnimationCoroutine(FadingPanelConfig config, float delay)
        {
            var clip = new Motion.ClipSequence();

            var outAnimation = new Motion.Tween<float>(config.fadeOutDuration, GetComponent<CanvasGroup>().alpha, config.outAlphaEnd, Mathf.Lerp, Motion.EasingUtil.EasingFunction(config.outAnimationEasing));
            outAnimation.OnUpdate += UpdateAlpha;

            if (config.outDelay + delay > 0)
            {
                clip.Append(new Motion.Silence(config.outDelay + delay));
            }

            clip.Append(outAnimation);

            while (clip.Play(Time.unscaledDeltaTime))
            {
                yield return null;
            }
        }

        private void UpdateAlpha(float value, float progress)
        {
            GetComponent<CanvasGroup>().alpha = value;
        }
    }
}
