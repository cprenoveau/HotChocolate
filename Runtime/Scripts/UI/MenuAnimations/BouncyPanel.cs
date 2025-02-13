using HotChocolate.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.UI.MenuAnimations
{
    public class BouncyPanel : MonoBehaviour, IMenuAnimation
    {
        public float pushDelay;
        public float popDelay;
        public float focusInDelay;
        public float focusOutDelay;
        public List<string> onlyFromMenus = new List<string>();

        public BouncyPanelConfig pushConfig;
        public BouncyPanelConfig focusConfig;

        public bool Disposed { get; private set; }
        public IEnumerable<string> OnlyFromMenus => onlyFromMenus;

        public void Dispose()
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

        private IEnumerator InAnimationCoroutine(BouncyPanelConfig config, float delay)
        {
            if (config == null)
                yield break;

            transform.localScale = Vector3.zero;

            var clip = new Motion.ClipSequence();

            var inAnimation = new Motion.Tween<Vector3>(config.inAnimationDuration / 2f, config.inScaleStart, config.inScaleMax, Vector3.Lerp, Motion.EasingUtil.EasingFunction(config.inAnimationEasing));
            inAnimation.OnUpdate += UpdateScale;

            var settleAnimation = new Motion.Tween<Vector3>(config.inAnimationDuration / 2f, config.inScaleMax, Vector3.one, Vector3.Lerp, Motion.EasingUtil.EasingFunction(config.inAnimationSettleEasing));
            settleAnimation.OnUpdate += UpdateScale;

            if (config.inDelay + delay > 0)
            {
                clip.Append(new Motion.Silence(config.inDelay + delay));
            }

            clip.Append(inAnimation);
            clip.Append(settleAnimation);

            while (clip.Play(Time.unscaledDeltaTime))
            {
                yield return null;
            }
        }

        private IEnumerator OutAnimationCoroutine(BouncyPanelConfig config, float delay)
        {
            if (config == null)
                yield break;

            var clip = new Motion.ClipSequence();

            var outAnimation = new Motion.Tween<Vector3>(config.outAnimationDuration / 2f, transform.localScale, config.outScaleMax, Vector3.Lerp, Motion.EasingUtil.EasingFunction(config.outAnimationEasing));
            outAnimation.OnUpdate += UpdateScale;

            var settleAnimation = new Motion.Tween<Vector3>(config.outAnimationDuration / 2f, config.outScaleMax, config.outScaleEnd, Vector3.Lerp, Motion.EasingUtil.EasingFunction(config.outAnimationSettleEasing));
            settleAnimation.OnUpdate += UpdateScale;

            if (config.outDelay + delay > 0)
            {
                clip.Append(new Motion.Silence(config.outDelay + delay));
            }

            clip.Append(outAnimation);
            clip.Append(settleAnimation);

            while (clip.Play(Time.unscaledDeltaTime))
            {
                yield return null;
            }
        }

        private void UpdateScale(Vector3 value, float progress)
        {
            transform.localScale = value;
        }
    }
}
