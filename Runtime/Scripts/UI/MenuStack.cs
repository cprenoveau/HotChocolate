using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;
using HotChocolate.Utils;
using System.Threading;

namespace HotChocolate.UI
{
    public sealed class MenuStack : MonoBehaviour
    {
        public RectTransform menuAnchor;
        public GameObject inputBlocker;

        public int Count => _stack.Count;
        public IMenu Find(string id) => _stack.Find(m => m.Id == id);
        public int FindIndex(string id) => _stack.FindIndex(m => m.Id == id);
        public IMenu Top => _stack.Count > 0 ? _stack.Last() : null;
        public int PendingCount => _actionQueue.Count;
        public List<IMenu> Menus => _stack;

        public event Action<bool> OnBlockInput;

        public delegate void MenuPushed(IMenu instance);
        public event MenuPushed OnMenuPushed;

        public delegate void MenuPopped(IMenu instance);
        public event MenuPopped OnMenuPopped;

        public delegate void MenuFocusChanged(IMenu instance, bool hasFocus);
        public event MenuFocusChanged OnMenuFocusChanged;

        public Menu.CreateInstance createInstanceDefault = Menu.CreateInstanceFromPrefab;
        public Menu.DestroyInstance destroyInstanceDefault = Menu.DestroyPrefabInstance;

        public Menu.PlayInAnimation playPushAnimationDefault = Menu.PlayPushAnimationDefault;
        public Menu.PlayOutAnimation playPopAnimationDefault = Menu.PlayPopAnimationDefault;
        public Menu.PlayInAnimation playFocusInAnimationDefault = Menu.PlayFocusInAnimationDefault;
        public Menu.PlayOutAnimation playFocusOutAnimationDefault = Menu.NoAnimation;

        private List<IMenu> _stack = new List<IMenu>();
        private Queue<Action> _actionQueue = new Queue<Action>();

        private CancellationTokenSource _ctSource = new CancellationTokenSource();

        private void Awake()
        {
            if (inputBlocker != null)
                inputBlocker.SetActive(false);
        }

        private void OnDestroy()
        {
            _ctSource.Cancel();
        }

        /// <summary>
        /// Instantiates a new menu using the default createInstance function and adds it on top of the stack.
        /// Will fail if the menuId already exists in the stack.
        /// </summary>
        /// <param name="menuAsset">The prefab or addressable key to instantiate</param>
        /// <param name="menuId">The id to be associated to the new instance</param>
        /// <param name="data">Anything you want to pass to the new menu instance</param>
        public void Push(object menuAsset, string menuId, object data = null)
        {
            Push(menuAsset, menuId, createInstanceDefault, data);
        }

        /// <summary>
        /// Instantiates a new menu using the createInstanceTask and adds it on top of the stack.
        /// Will fail if the menuId already exists in the stack.
        /// </summary>
        /// <param name="menuAsset">The prefab or addressable key to instantiate</param>
        /// <param name="menuId">The id to be associated to the new instance</param>
        /// <param name="createInstanceTask">The function to use to instantiate this menu</param>
        /// <param name="data">Anything you want to pass to the new menu instance</param>
        public void Push(object menuAsset, string menuId, Menu.CreateInstance createInstanceTask, object data = null)
        {
            Push(menuAsset, menuId, playFocusOutAnimationDefault, createInstanceTask, playPushAnimationDefault, data);
        }

        /// <summary>
        /// Instantiates a new menu using the createInstanceTask and adds it on top of the stack.
        /// Will fail if the menuId already exists in the stack.
        /// </summary>
        /// <param name="menuAsset">The prefab or addressable key to instantiate</param>
        /// <param name="menuId">The id to be associated to the new instance</param>
        /// <param name="playFocusOutAnimationTask">The animation function to play on the menu that would focus out</param>
        /// <param name="createInstanceTask">The function used to instantiate this menu</param>
        /// <param name="playPushAnimationTask">The animation function to play on the menu that will be pushed</param>
        /// <param name="data">Anything you want to pass to the new menu instance</param>
        public void Push(object menuAsset, string menuId, Menu.PlayOutAnimation playFocusOutAnimationTask, Menu.CreateInstance createInstanceTask, Menu.PlayInAnimation playPushAnimationTask, object data = null)
        {
            QueueAction(() => { DoPush(menuAsset, menuId, playFocusOutAnimationTask, createInstanceTask, playPushAnimationTask, data, ExecuteNextAction).FireAndForgetTask(); });
        }

        /// <summary>
        /// Instantiates a new menu using the default createInstance function and adds it at the specified stack index.
        /// Will fail if the menuId already exists in the stack or if the stack is empty.
        /// </summary>
        /// <param name="menuAsset">The prefab or addressable key to instantiate</param>
        /// <param name="menuId">The id to be associated to the new instance</param>
        /// <param name="stackIndex">The index to insert the new menu instance</param>
        /// <param name="data">Anything you want to pass to the new menu instance</param>
        public void Insert(object menuAsset, string menuId, int stackIndex, object data = null)
        {
            Insert(menuAsset, menuId, stackIndex, createInstanceDefault, data);
        }

        /// <summary>
        /// Instantiates a new menu using the default createInstance function and adds it at the specified stack index.
        /// Will fail if the menuId already exists in the stack or if the stack is empty.
        /// </summary>
        /// <param name="menuAsset">The prefab or addressable key to instantiate</param>
        /// <param name="menuId">The id to be associated to the new instance</param>
        /// <param name="stackIndex">The index to insert the new menu instance</param>
        /// <param name="createInstanceTask">The function to use to instantiate this menu</param>
        /// <param name="data">Anything you want to pass to the new menu instance</param>
        public void Insert(object menuAsset, string menuId, int stackIndex, Menu.CreateInstance createInstanceTask, object data = null)
        {
            QueueAction(() => { DoInsert(menuAsset, menuId, createInstanceTask, data, stackIndex, ExecuteNextAction).FireAndForgetTask(); });
        }

        /// <summary>
        /// If fromMenu is not null or empty, pops all above fromMenu and pushes new menu.
        /// If fromMenu is null or empty, pops all and pushes new menu.
        /// If the menuId already exists in the stack, pops all above it.
        /// Fails if fromMenu is not null or empty and doesn't exist in the stack.
        /// </summary>
        /// <param name="menuAsset">The prefab or addressable key to instantiate</param>
        /// <param name="menuId">The id to be associated to this instance</param>
        /// <param name="fromMenu">The id of the menu to jump from</param>
        /// <param name="data">Anything you want to pass to the new menu instance</param>
        public void Jump(object menuAsset, string menuId, string fromMenu, object data = null)
        {
            Jump(menuAsset, menuId, fromMenu, createInstanceDefault, data);
        }

        /// <summary>
        /// If fromMenu is not null or empty, pops all above fromMenu and pushes new menu.
        /// If fromMenu is null or empty, pops all and pushes new menu.
        /// If the menuId already exists in the stack, pops all above it.
        /// Fails if fromMenu is not null or empty and doesn't exist in the stack.
        /// </summary>
        /// <param name="menuAsset">The prefab or addressable key to instantiate</param>
        /// <param name="menuId">The id to be associated to this instance</param>
        /// <param name="fromMenu">The id of the menu to jump from</param>
        /// <param name="createInstanceTask">The function to use to instantiate this menu</param>
        /// <param name="data">Anything you want to pass to the new menu instance</param>
        public void Jump(object menuAsset, string menuId, string fromMenu, Menu.CreateInstance createInstanceTask, object data = null)
        {
            Jump(menuAsset, menuId, fromMenu, playFocusOutAnimationDefault, destroyInstanceDefault, createInstanceTask, playPushAnimationDefault, data);
        }

        /// <summary>
        /// If fromMenu is not null or empty, pops all above fromMenu and pushes new menu.
        /// If fromMenu is null or empty, pops all and pushes new menu.
        /// If the menuId already exists in the stack, pops all above it.
        /// Fails if fromMenu is not null or empty and doesn't exist in the stack.
        /// </summary>
        /// <param name="menuAsset">The prefab or addressable key to instantiate</param>
        /// <param name="menuId">The id to be associated to this instance</param>
        /// <param name="fromMenu">The id of the menu to jump from</param>
        /// <param name="playFocusOutAnimationTask">The animation to play on the menu that would focus out</param>
        /// <param name="destroyInstanceTask">The function to use to destroy menu instances</param>
        /// <param name="createInstanceTask">The function to use to instantiate this menu</param>
        /// <param name="playPushAnimationTask">The animation to play on the menu that would be pushed</param>
        /// <param name="data">Anything you want to pass to the new menu instance</param>
        public void Jump(object menuAsset, string menuId, string fromMenu, Menu.PlayOutAnimation playFocusOutAnimationTask, Menu.DestroyInstance destroyInstanceTask, Menu.CreateInstance createInstanceTask, Menu.PlayInAnimation playPushAnimationTask, object data = null)
        {
            QueueAction(() => { DoJump(menuAsset, menuId, fromMenu, playFocusOutAnimationTask, destroyInstanceTask, createInstanceTask, playPushAnimationTask, data, ExecuteNextAction).FireAndForgetTask(); });
        }

        /// <summary>
        /// Removes and destroys the topmost menu instance.
        /// </summary>
        public void Pop()
        {
            Pop(playPopAnimationDefault, playFocusInAnimationDefault, destroyInstanceDefault);
        }

        /// <summary>
        /// Removes and destroys the topmost menu instance.
        /// </summary>
        /// <param name="playPopAnimationTask">The animation to play on the menu that would be popped</param>
        /// <param name="playFocusInAnimationTask">The animation to play on the menu that would focus in</param>
        public void Pop(Menu.PlayOutAnimation playPopAnimationTask, Menu.PlayInAnimation playFocusInAnimationTask)
        {
            Pop(playPopAnimationTask, playFocusInAnimationTask, destroyInstanceDefault);
        }

        /// <summary>
        /// Removes and destroys the topmost menu instance.
        /// </summary>
        /// <param name="playPopAnimationTask">The animation to play on the menu that would be popped</param>
        /// <param name="playFocusInAnimationTask">The animation to play on the menu that would focus in</param>
        /// <param name="destroyInstanceTask">The function to use to destroy menu instances</param>
        public void Pop(Menu.PlayOutAnimation playPopAnimationTask, Menu.PlayInAnimation playFocusInAnimationTask, Menu.DestroyInstance destroyInstanceTask)
        {
            QueueAction(() => { DoPop(playPopAnimationTask, playFocusInAnimationTask, destroyInstanceTask, focusOut: true, focusIn: true, ExecuteNextAction).FireAndForgetTask(); });
        }

        /// <summary>
        /// Pops all above menuId.
        /// Fails if menuId doesn't exist in the stack.
        /// </summary>
        /// <param name="menuId">The id of the menu instance to pop to</param>
        public void PopAllAbove(string menuId)
        {
            PopAllAbove(menuId, playPopAnimationDefault, playFocusInAnimationDefault, destroyInstanceDefault);
        }

        /// <summary>
        /// Pops all above menuId.
        /// Fails if menuId doesn't exist in the stack.
        /// </summary>
        /// <param name="menuId">The id of the menu instance to pop to</param>
        /// <param name="playPopAnimationTask">The animation to play on the menu that would be popped</param>
        /// <param name="playFocusInAnimationTask">The animation to play on the menu that would focus in</param>
        public void PopAllAbove(string menuId, Menu.PlayOutAnimation playPopAnimationTask, Menu.PlayInAnimation playFocusInAnimationTask)
        {
            PopAllAbove(menuId, playPopAnimationTask, playFocusInAnimationTask, destroyInstanceDefault);
        }

        /// <summary>
        /// Pops all above menuId.
        /// Fails if menuId doesn't exist in the stack.
        /// </summary>
        /// <param name="menuId">The id of the menu instance to pop to</param>
        /// <param name="playPopAnimationTask">The animation to play on the menu that would be popped</param>
        /// <param name="playFocusInAnimationTask">The animation to play on the menu that would focus in</param>
        /// <param name="destroyInstanceTask">The function to use to destroy menu instances</param>
        public void PopAllAbove(string menuId, Menu.PlayOutAnimation playPopAnimationTask, Menu.PlayInAnimation playFocusInAnimationTask, Menu.DestroyInstance destroyInstanceTask)
        {
            QueueAction(() => { DoPopAllAbove(menuId, playPopAnimationTask, playFocusInAnimationTask, destroyInstanceTask, ExecuteNextAction).FireAndForgetTask(); });
        }

        /// <summary>
        /// Removes and destroys all menu instances.
        /// </summary>
        public void PopAll()
        {
            PopAll(playPopAnimationDefault, destroyInstanceDefault);
        }

        /// <summary>
        /// Removes and destroys all menu instances.
        /// </summary>
        /// <param name="playPopAnimationTask">The animation to play on the menu that would be popped</param>
        public void PopAll(Menu.PlayOutAnimation playPopAnimationTask)
        {
            PopAll(playPopAnimationTask, destroyInstanceDefault);
        }

        /// <summary>
        /// Removes and destroys all menu instances.
        /// </summary>
        /// <param name="playPopAnimationTask">The animation to play on the menu that would be popped</param>
        /// <param name="destroyInstanceTask">The function to use to destroy menu instances</param>
        public void PopAll(Menu.PlayOutAnimation playPopAnimationTask, Menu.DestroyInstance destroyInstanceTask)
        {
            QueueAction(() => { DoPopAll(playPopAnimationTask, destroyInstanceTask, ExecuteNextAction).FireAndForgetTask(); });
        }

        private async Task DoPush(
            object menuAsset,
            string menuId,
            Menu.PlayOutAnimation playOutAnimationTask,
            Menu.CreateInstance createInstanceTask,
            Menu.PlayInAnimation playInAnimationTask,
            object data,
            Action onComplete)
        {
            if (Find(menuId) != null)
            {
                Debug.LogWarning("DoPush: menu with id " + menuId + " already exists in the stack. Call PopAllAbove instead.");

                onComplete?.Invoke();
                return;
            }

            if (inputBlocker != null)
                inputBlocker.SetActive(true);

            OnBlockInput?.Invoke(true);

            var outMenu = Top;
            if (_stack.Count > 0)
            {
                Top.OnFocusOut(false, menuId);
                Top.HasFocus = false;
                OnMenuFocusChanged?.Invoke(Top, false);
            }

            var instance = await createInstanceTask(menuAsset, menuId, menuAnchor, -1, this, _ctSource.Token).ConfigureAwait(true);

            if (_ctSource.IsCancellationRequested)
                return;

            _stack.Add(instance);

            instance.OnPush(this, data);
            OnMenuPushed?.Invoke(instance);

            instance.BeforeFocusIn.Invoke(instance);
            instance.OnFocusIn(true, outMenu != null ? outMenu.Id : null);
            instance.HasFocus = true;
            OnMenuFocusChanged?.Invoke(instance, true);

            if (instance.SoloDisplay)
            {
                for (int i = 0; i < _stack.Count - 1; ++i)
                {
                    Menu.DeactivateInstance(_stack[i]);
                }
            }

            await PlayTransition(outMenu, playOutAnimationTask, Top, playInAnimationTask).ConfigureAwait(true);

            if (!instance.SoloDisplay && outMenu != null)
            {
                outMenu.AfterFocusOut.Invoke(outMenu);
            }

            if (inputBlocker != null)
                inputBlocker.SetActive(false);

            OnBlockInput?.Invoke(false);

            onComplete?.Invoke();
        }

        private async Task DoInsert(
            object menuAsset,
            string menuId,
            Menu.CreateInstance createInstanceTask,
            object data,
            int stackIndex,
            Action onComplete)
        {
            if (_stack.Count == 0)
            {
                Debug.LogWarning("DoInsert: cannot insert menu into empty stack.");

                onComplete?.Invoke();
                return;
            }

            if (Find(menuId) != null)
            {
                Debug.LogWarning("DoInsert: menu with id " + menuId + " already exists in the stack.");

                onComplete?.Invoke();
                return;
            }

            if (inputBlocker != null)
                inputBlocker.SetActive(true);

            OnBlockInput?.Invoke(true);

            var instance = await createInstanceTask(menuAsset, menuId, menuAnchor, 0, this, _ctSource.Token).ConfigureAwait(true);

            if (_ctSource.IsCancellationRequested)
                return;

            _stack.Insert(stackIndex, instance);

            instance.OnPush(this, data);
            instance.HasFocus = false;

            if (inputBlocker != null)
                inputBlocker.SetActive(false);

            OnBlockInput?.Invoke(false);

            onComplete?.Invoke();
        }

        private async Task DoJump(
            object menuAsset,
            string menuId,
            string fromMenuId,
            Menu.PlayOutAnimation playOutAnimationTask,
            Menu.DestroyInstance destroyInstanceTask,
            Menu.CreateInstance createInstanceTask,
            Menu.PlayInAnimation playInAnimationTask,
            object data,
            Action onComplete)
        {
            string currentMenu = Top != null ? Top.Id : null;

            if (Find(menuId) != null)
            {
                await DoPopAllAbove(menuId, playOutAnimationTask, playInAnimationTask, destroyInstanceTask, null).ConfigureAwait(true);

                onComplete?.Invoke();
                return;
            }

            if (Top != null && Top.Id == fromMenuId)
            {
                await DoPush(menuAsset, menuId, playOutAnimationTask, createInstanceTask, playInAnimationTask, data, null).ConfigureAwait(true);

                onComplete?.Invoke();
                return;
            }

            int targetMenuIndex = string.IsNullOrEmpty(fromMenuId) ? -1 : _stack.FindIndex(m => m.Id == fromMenuId);
            if (targetMenuIndex == -1 && !string.IsNullOrEmpty(fromMenuId))
            {
                Debug.LogWarning("DoJump: menu with id " + fromMenuId + " doesnt exist in the stack");

                onComplete?.Invoke();
                return;
            }

            if (inputBlocker != null)
                inputBlocker.SetActive(true);

            OnBlockInput?.Invoke(true);

            int startIndex = _stack.Count - 2;
            for (int i = startIndex; i > targetMenuIndex; --i)
            {
                var toPop = _stack[i];
                _stack.RemoveAt(i);

                toPop.OnPop();
                OnMenuPopped?.Invoke(toPop);

                await destroyInstanceTask(toPop, this, _ctSource.Token).ConfigureAwait(true);
            }

            if (_ctSource.IsCancellationRequested)
                return;

            var outMenu = Top;

            if (_stack.Count > 0)
            {
                Top.OnFocusOut(false, menuId);
                Top.HasFocus = false;
                OnMenuFocusChanged?.Invoke(Top, false);

                outMenu.OnPop();
                OnMenuPopped?.Invoke(outMenu);
            }

            var instance = await createInstanceTask(menuAsset, menuId, menuAnchor, -1, this, _ctSource.Token).ConfigureAwait(true);

            if (_ctSource.IsCancellationRequested)
                return;

            _stack.Add(instance);

            instance.OnPush(this, data);
            OnMenuPushed?.Invoke(instance);

            instance.BeforeFocusIn.Invoke(instance);
            instance.OnFocusIn(true, outMenu != null ? outMenu.Id : null);
            instance.HasFocus = true;
            OnMenuFocusChanged?.Invoke(instance, true);

            if (instance.SoloDisplay)
            {
                for (int i = 0; i < _stack.Count - 1; ++i)
                {
                    Menu.DeactivateInstance(_stack[i]);
                }
            }

            await PlayTransition(outMenu, playOutAnimationTask, Top, playInAnimationTask).ConfigureAwait(true);

            if (!instance.SoloDisplay && outMenu != null)
            {
                outMenu.AfterFocusOut.Invoke(outMenu);
            }

            if (outMenu != null)
            {
                _stack.RemoveAt(_stack.Count - 2);
                await destroyInstanceTask(outMenu, this, _ctSource.Token).ConfigureAwait(true);
            }

            if (inputBlocker != null)
                inputBlocker.SetActive(false);

            OnBlockInput?.Invoke(false);

            onComplete?.Invoke();
        }

        private async Task DoPop(
            Menu.PlayOutAnimation playOutAnimationTask,
            Menu.PlayInAnimation playInAnimationTask,
            Menu.DestroyInstance destroyInstanceTask,
            bool focusOut,
            bool focusIn,
            Action onComplete)
        {
            if (_stack.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            if (inputBlocker != null)
                inputBlocker.SetActive(true);

            OnBlockInput?.Invoke(true);

            string currentMenu = Top != null ? Top.Id : null;
            string nextMenu = _stack.Count > 1 ? _stack[_stack.Count - 2].Id : null;

            if (focusOut)
            {
                Top.OnFocusOut(true, nextMenu);
                Top.HasFocus = false;
                OnMenuFocusChanged?.Invoke(Top, false);
            }

            var toPop = Top;
            var newTop = _stack.Count > 1 ? _stack[_stack.Count - 2] : null;

            toPop.OnPop();
            OnMenuPopped?.Invoke(toPop);

            if (newTop != null && focusIn)
            {
                newTop.BeforeFocusIn.Invoke(newTop);
                newTop.OnFocusIn(false, currentMenu);
                newTop.HasFocus = true;
                OnMenuFocusChanged?.Invoke(newTop, true);
            }

            if (toPop.SoloDisplay)
            {
                Menu.DeactivateInstance(toPop);
            }

            await PlayTransition(focusOut && !toPop.SoloDisplay ? toPop : null, playOutAnimationTask, focusIn ? newTop : null, playInAnimationTask).ConfigureAwait(true);

            if (toPop.SoloDisplay)
            {
                if (newTop != null && !newTop.SoloDisplay)
                {
                    foreach (var menu in _stack)
                    {
                        menu.AfterFocusOut.Invoke(menu);
                    }
                }
            }
            else
            {
                toPop.AfterFocusOut.Invoke(toPop);
            }

            await destroyInstanceTask(toPop, this, _ctSource.Token).ConfigureAwait(true);

            _stack.RemoveAt(_stack.Count - 1);

            if (inputBlocker != null)
                inputBlocker.SetActive(false);

            OnBlockInput?.Invoke(false);

            onComplete?.Invoke();
        }

        private async Task DoPopAllAbove(
            string menuId,
            Menu.PlayOutAnimation playOutAnimationTask,
            Menu.PlayInAnimation playInAnimationTask,
            Menu.DestroyInstance destroyInstanceTask,
            Action onComplete)
        {
            var targetMenuIndex = _stack.FindIndex(m => m.Id == menuId);
            if (targetMenuIndex == -1)
            {
                Debug.LogWarning("DoPopAllAbove: menu with id " + menuId + " doesnt exist in the stack");

                onComplete?.Invoke();
                return;
            }

            int startIndex = _stack.Count - 1;
            for (int i = startIndex; i > targetMenuIndex; --i)
            {
                await DoPop(playOutAnimationTask, playInAnimationTask, destroyInstanceTask, focusOut: i == startIndex, focusIn: i == targetMenuIndex + 1, null).ConfigureAwait(true);
            }

            if (Top.SoloDisplay)
            {
                for (int i = 0; i < _stack.Count - 1; ++i)
                {
                    Menu.DeactivateInstance(_stack[i]);
                }
            }

            onComplete?.Invoke();
        }

        private async Task DoPopAll(
            Menu.PlayOutAnimation playOutAnimationTask,
            Menu.DestroyInstance destroyInstanceTask,
            Action onComplete)
        {
            await DoPop(playOutAnimationTask, Menu.NoAnimation, destroyInstanceTask, focusOut: true, focusIn: false, null).ConfigureAwait(true);

            while (_stack.Count > 0)
            {
                await DoPop(playOutAnimationTask, Menu.NoAnimation, destroyInstanceTask, focusOut: false, focusIn: false, null).ConfigureAwait(true);
            }

            onComplete?.Invoke();
        }

        private async Task PlayTransition(IMenu outMenu, Menu.PlayOutAnimation outAnimation, IMenu inMenu, Menu.PlayInAnimation inAnimation)
        {
            var tasks = new List<Task>();

            if (outMenu != null)
                tasks.Add(outAnimation(null, outMenu, _ctSource.Token));

            if (inMenu != null)
                tasks.Add(inAnimation(outMenu != null ? outMenu.Id : null, inMenu, _ctSource.Token));

            await Task.WhenAll(tasks);
        }

        private void QueueAction(Action action)
        {
            _actionQueue.Enqueue(action);

            if (_actionQueue.Count == 1)
            {
                _actionQueue.Peek().Invoke();
            }
        }

        private void ExecuteNextAction()
        {
            _actionQueue.Dequeue();

            if (_actionQueue.Count > 0)
            {
                _actionQueue.Peek().Invoke();
            }
        }
    }
}