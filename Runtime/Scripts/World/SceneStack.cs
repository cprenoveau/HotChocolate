using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace HotChocolate.World
{
    public sealed class SceneStack : MonoBehaviour
    {
        public int Count => _stack.Count;
        public ISceneData Find(string id) => _stack.Find(m => m.SceneAddress == id);
        public int FindIndex(string id) => _stack.FindIndex(m => m.SceneAddress == id);
        public ISceneData Top => _stack.Count > 0 ? _stack.Last() : null;
        public List<ISceneData> Scenes => _stack;

        public delegate void ScenePushed(ISceneData instance);
        public event ScenePushed OnScenePushed;

        public delegate void ScenePopped(ISceneData instance);
        public event ScenePopped OnScenePopped;

        public Scene.Load loadSceneDefault = Scene.LoadSceneDefault;
        public Scene.Unload unloadSceneDefault = Scene.UnloadSceneDefault;

        private List<ISceneData> _stack = new List<ISceneData>();
        private CancellationTokenSource _ctSource = new CancellationTokenSource();

        private void OnDestroy()
        {
            _ctSource.Cancel();
            _ctSource.Dispose();
        }

        public async Task Push(string sceneAddress)
        {
            await Push(sceneAddress, loadSceneDefault).ConfigureAwait(true);
        }

        public async Task Push(string sceneAddress, Scene.Load loadSceneTask)
        {
            if (Find(sceneAddress) != null)
            {
                Debug.LogWarning("Push: scene with name " + sceneAddress + " already exists in the stack. Call PopAllAbove instead.");
                return;
            }

            var instance = await loadSceneTask(sceneAddress, _ctSource.Token).ConfigureAwait(true);

            if (_ctSource.IsCancellationRequested)
                return;

            _stack.Add(instance);

            OnScenePushed?.Invoke(instance);
        }

        public async Task Jump(string sceneAddress, string fromSceneName)
        {
            await Jump(sceneAddress, fromSceneName, unloadSceneDefault, loadSceneDefault).ConfigureAwait(true);
        }

        public async Task Jump(string sceneAddress, string fromSceneName, Scene.Unload unloadSceneTask, Scene.Load loadSceneTask)
        {
            if (Find(sceneAddress) != null)
            {
                await PopAllAbove(sceneAddress, unloadSceneTask, loadSceneTask).ConfigureAwait(true);
                return;
            }

            if (string.IsNullOrEmpty(fromSceneName))
            {
                await Jump(sceneAddress, unloadSceneTask, loadSceneTask).ConfigureAwait(true);
                return;
            }

            if (Top != null && Top.SceneAddress == fromSceneName)
            {
                await Push(sceneAddress, loadSceneTask).ConfigureAwait(true);
                return;
            }

            var targetMenuIndex = _stack.FindIndex(m => m.SceneAddress == fromSceneName);
            if (targetMenuIndex == -1)
            {
                Debug.LogWarning("Jump: scene with name " + fromSceneName + " doesn't exist in the stack");
                return;
            }

            var instance = await loadSceneTask(sceneAddress, _ctSource.Token).ConfigureAwait(true);

            if (_ctSource.IsCancellationRequested)
                return;

            int startIndex = _stack.Count - 1;
            for (int i = startIndex; i > targetMenuIndex; --i)
            {
                await Pop(unloadSceneTask, loadSceneTask, focusOut: i == startIndex, focusIn: false).ConfigureAwait(true);
            }

            _stack.Add(instance);

            OnScenePushed?.Invoke(instance);
        }

        private async Task Jump(string sceneAddress, Scene.Unload unloadSceneTask, Scene.Load loadSceneTask)
        {
            var instance = await loadSceneTask(sceneAddress, _ctSource.Token).ConfigureAwait(true);

            if (_ctSource.IsCancellationRequested)
                return;

            await PopAll(unloadSceneTask, loadSceneTask, focusIn: false).ConfigureAwait(true);

            var toPop = Top;
            OnScenePopped?.Invoke(toPop);
            _stack.Clear();

            _stack.Add(instance);

            OnScenePushed?.Invoke(instance);
        }

        public async Task Pop()
        {
            await Pop(unloadSceneDefault, loadSceneDefault, true, true).ConfigureAwait(true);
        }

        public async Task Pop(Scene.Unload unloadSceneTask, Scene.Load loadSceneTask)
        {
            await Pop(unloadSceneTask, loadSceneTask, true, true).ConfigureAwait(true);
        }

        private async Task Pop(Scene.Unload unloadSceneTask, Scene.Load loadSceneTask, bool focusOut, bool focusIn)
        {
            if (_stack.Count <= 1)
            {
                return;
            }

            var toPop = Top;
            _stack.RemoveAt(_stack.Count - 1);

            OnScenePopped?.Invoke(toPop);
            await unloadSceneTask(toPop, _ctSource.Token).ConfigureAwait(true);

            if (_ctSource.IsCancellationRequested)
                return;

            if (_stack.Count > 0 && focusIn)
            {
                if (!Top.Scene.isLoaded)
                {
                    var instance = await loadSceneTask(Top.SceneAddress, _ctSource.Token).ConfigureAwait(true);

                    if (_ctSource.IsCancellationRequested)
                        return;

                    _stack[_stack.Count - 1] = instance;
                }
            }
        }

        public async Task PopAllAbove(string sceneName)
        {
            await PopAllAbove(sceneName, unloadSceneDefault, loadSceneDefault).ConfigureAwait(true);
        }

        public async Task PopAllAbove(string sceneAddress, Scene.Unload unloadSceneTask, Scene.Load loadSceneTask)
        {
            var targetMenuIndex = _stack.FindIndex(m => m.SceneAddress == sceneAddress);
            if (targetMenuIndex == -1)
            {
                Debug.LogWarning("PopAllAbove: scene with name " + sceneAddress + " doesnt exist in the stack");
                return;
            }

            int startIndex = _stack.Count - 1;
            for (int i = startIndex; i > targetMenuIndex; --i)
            {
                await Pop(unloadSceneTask, loadSceneTask, focusOut: i == startIndex, focusIn: i == targetMenuIndex + 1).ConfigureAwait(true);
            }
        }

        public async Task PopAll()
        {
            await PopAll(unloadSceneDefault, loadSceneDefault).ConfigureAwait(true);
        }

        public async Task PopAll(Scene.Unload unloadSceneTask, Scene.Load loadSceneTask)
        {
            await PopAll(unloadSceneTask, loadSceneTask, true).ConfigureAwait(true);
        }

        private async Task PopAll(Scene.Unload unloadSceneTask, Scene.Load loadSceneTask, bool focusIn)
        {
            await Pop(unloadSceneTask, loadSceneTask, focusOut: true, focusIn: focusIn && _stack.Count == 2).ConfigureAwait(true);

            while (_stack.Count > 1)
            {
                await Pop(unloadSceneTask, loadSceneTask, focusOut: false, focusIn: focusIn && _stack.Count == 2).ConfigureAwait(true);
            }
        }
    }
}