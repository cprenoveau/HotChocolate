using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HotChocolate.UI
{
    public interface IMenu
    {
        string Id { get; set; }
        bool HasFocus { get; set; }

        bool SoloDisplay { get; }
        Menu.BeforeFocusIn BeforeFocusIn { get; }
        Menu.AfterFocusOut AfterFocusOut { get; }

        void OnPush(MenuStack stack, object data);
        void OnPop();
        void OnFocusIn(bool fromPush, string previousMenu);
        void OnFocusOut(bool fromPop, string nextMenu);
    }

    public interface IMenuAnimation
    {
        IEnumerable<string> OnlyFromMenus { get; }

        bool Disposed { get; }
        void Dispose();

        Task PushAnimation(CancellationToken ct);
        Task PopAnimation(CancellationToken ct);
        Task FocusInAnimation(CancellationToken ct);
        Task FocusOutAnimation(CancellationToken ct);
    }

    public static partial class Menu
    {
        public delegate Task<IMenu> CreateInstance(object asset, string menuId, Transform parent, int siblingIndex, MonoBehaviour opHolder, CancellationToken ct);
        public delegate Task DestroyInstance(IMenu instance, MonoBehaviour opHolder, CancellationToken ct);

        public delegate Task PlayInAnimation(string fromMenu, IMenu instance, CancellationToken ct);
        public delegate Task PlayOutAnimation(string fromMenu, IMenu instance, CancellationToken ct);

        public delegate void BeforeFocusIn(IMenu instance);
        public delegate void AfterFocusOut(IMenu instance);

        public static Task<IMenu> CreateInstanceFromPrefab(object prefab, string menuId, Transform parent, int siblingIndex, MonoBehaviour opHolder, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            var instance = GameObject.Instantiate(prefab as GameObject, parent);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;

            if (siblingIndex != -1)
                instance.transform.SetSiblingIndex(siblingIndex);

            instance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            instance.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            instance.GetComponent<IMenu>().Id = menuId;
            return Task.FromResult(instance.GetComponent<IMenu>());
        }

        public static Task DestroyPrefabInstance(IMenu instance, MonoBehaviour opHolder, CancellationToken ct)
        {
            if (!ct.IsCancellationRequested && instance as MonoBehaviour != null)
            {
                GameObject.Destroy((instance as MonoBehaviour).gameObject);
            }

            return Task.FromResult<bool>(true);
        }

        public static async Task PlayPushAnimationDefault(string fromMenu, IMenu instance, CancellationToken ct)
        {
            await PlayPushAnimationDefault(fromMenu, instance as MonoBehaviour, ct);
        }

        public static async Task PlayPushAnimationDefault(string fromMenu, MonoBehaviour instance, CancellationToken ct)
        {
            if (instance == null)
                return;

            await PlayPushAnimationDefault(fromMenu, instance.gameObject, ct);
        }

        public static async Task PlayPushAnimationDefault(string fromMenu, GameObject instance, CancellationToken ct)
        {
            var animations = instance.GetComponentsInChildren<IMenuAnimation>(false);
            if (animations.Length > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (var animation in animations)
                {
                    if(!animation.Disposed && (string.IsNullOrEmpty(fromMenu) || animation.OnlyFromMenus.Count() == 0 || animation.OnlyFromMenus.Contains(fromMenu)))
                        tasks.Add(animation.PushAnimation(ct));
                }

                await Task.WhenAll(tasks).ConfigureAwait(true);
            }
        }

        public static async Task PlayPopAnimationDefault(string fromMenu, IMenu instance, CancellationToken ct)
        {
            await PlayPopAnimationDefault(fromMenu, instance as MonoBehaviour, ct);
        }

        public static async Task PlayPopAnimationDefault(string fromMenu, MonoBehaviour instance, CancellationToken ct)
        {
            if (instance == null)
                return;

            await PlayPopAnimationDefault(fromMenu, instance.gameObject, ct);
        }

        public static async Task PlayPopAnimationDefault(string fromMenu, GameObject instance, CancellationToken ct)
        {
            var animations = instance.GetComponentsInChildren<IMenuAnimation>(false);
            if (animations.Length > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (var animation in animations)
                {
                    if (!animation.Disposed && (string.IsNullOrEmpty(fromMenu) || animation.OnlyFromMenus.Count() == 0 || animation.OnlyFromMenus.Contains(fromMenu)))
                        tasks.Add(animation.PopAnimation(ct));
                }

                await Task.WhenAll(tasks).ConfigureAwait(true);
            }
        }

        public static async Task PlayFocusInAnimationDefault(string fromMenu, IMenu instance, CancellationToken ct)
        {
            await PlayFocusInAnimationDefault(fromMenu, instance as MonoBehaviour, ct);
        }

        public static async Task PlayFocusInAnimationDefault(string fromMenu, MonoBehaviour instance, CancellationToken ct)
        {
            if (instance == null)
                return;

            await PlayFocusInAnimationDefault(fromMenu, instance.gameObject, ct);
        }

        public static async Task PlayFocusInAnimationDefault(string fromMenu, GameObject instance, CancellationToken ct)
        {
            var animations = instance.GetComponentsInChildren<IMenuAnimation>(false);
            if (animations.Length > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (var animation in animations)
                {
                    if (!animation.Disposed && (string.IsNullOrEmpty(fromMenu) || animation.OnlyFromMenus.Count() == 0 || animation.OnlyFromMenus.Contains(fromMenu)))
                        tasks.Add(animation.FocusInAnimation(ct));
                }

                await Task.WhenAll(tasks).ConfigureAwait(true);
            }
        }

        public static async Task PlayFocusOutAnimationDefault(string fromMenu, IMenu instance, CancellationToken ct)
        {
            await PlayFocusOutAnimationDefault(fromMenu, instance as MonoBehaviour, ct);
        }

        public static async Task PlayFocusOutAnimationDefault(string fromMenu, MonoBehaviour instance, CancellationToken ct)
        {
            if (instance == null)
                return;

            await PlayFocusOutAnimationDefault(fromMenu, instance.gameObject, ct);
        }

        public static async Task PlayFocusOutAnimationDefault(string fromMenu, GameObject instance, CancellationToken ct)
        {
            var animations = instance.GetComponentsInChildren<IMenuAnimation>(false);
            if (animations.Length > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (var animation in animations)
                {
                    if (!animation.Disposed && (string.IsNullOrEmpty(fromMenu) || animation.OnlyFromMenus.Count() == 0 || animation.OnlyFromMenus.Contains(fromMenu)))
                        tasks.Add(animation.FocusOutAnimation(ct));
                }

                await Task.WhenAll(tasks).ConfigureAwait(true);
            }
        }

        public static void DisposeAllAnimations(GameObject go)
        {
            var animations = go.GetComponentsInChildren<IMenuAnimation>(true);
            foreach (var animation in animations)
            {
                animation.Dispose();
            }
        }

        public static Task NoAnimation(string fromMenu, IMenu instance, CancellationToken ct)
        {
            return Task.FromResult<object>(null);
        }

        public static void ActivateInstance(IMenu instance)
        {
            if (instance as MonoBehaviour == null)
                return;

            (instance as MonoBehaviour).gameObject.SetActive(true);
        }

        public static void DeactivateInstance(IMenu instance)
        {
            if (instance as MonoBehaviour == null)
                return;

            (instance as MonoBehaviour).gameObject.SetActive(false);
        }
    }
}
