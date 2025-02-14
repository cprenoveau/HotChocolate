using HotChocolate.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if SOFTMASK
using SoftMasking;
#endif

namespace HotChocolate.FTUE.Widgets
{
    public class TutorialView<TData, TResult> : MonoBehaviour, IFtueElement where TData : class where TResult : IFtueResult
    {
        public GameObject blocker;
        public GameObject tutorialPanel;

        public RectTransform pointerWorldRect;
        public RectTransform hintBoxAnchor;
        public RectTransform dialogueBoxAnchor;
        public RectTransform pointerAnchor;
        public Button continueButton;
        public GameObject mask;

        public delegate Camera GetCameraDelegate(TData data);
        public GetCameraDelegate getCamera;

        public delegate Task RecenterCameraDelegate(IWorldHint worldHint, TData data, CancellationToken ct);
        public RecenterCameraDelegate recenterCamera;

        public delegate Transform FindMenuRootDelegate(IMenuHint menuHint, TData data);
        public FindMenuRootDelegate findMenuRoot;

        public delegate void BlockInputDelegate(bool block);
        public BlockInputDelegate blockInput;

        private bool done;

        private TData _data;
        private IFtueListener _listener;

        private void Awake()
        {
            continueButton.onClick.AddListener(ContinueButtonClicked);

            blocker.SetActive(false);
            tutorialPanel.SetActive(false);

            DontDestroyOnLoad(gameObject);
        }

        public void Init(
            TData data,
            IFtueListener listener,
            GetCameraDelegate getCamera,
            RecenterCameraDelegate recenterCamera,
            FindMenuRootDelegate findMenuRoot,
            BlockInputDelegate blockInput)
        {
            _data = data;
            _listener = listener;

            this.getCamera = getCamera;
            this.recenterCamera = recenterCamera;
            this.findMenuRoot = findMenuRoot;
            this.blockInput = blockInput;

            _listener.RegisterElement(this);
        }

        private void OnDestroy()
        {
            if(_listener != null)
                _listener.UnregisterElement(this);
        }

        public bool ShouldActivateFtue(IFtueStep step)
        {
            return step is IWorldHint || step is IMenuHint || step is IDialogue;
        }

        public async Task ActivateFtue(IFtueStep step, CancellationToken ct)
        {
            done = false;

            if (step is IWorldHint worldHint)
            {
                await ActivateWorldFtue(worldHint, ct).ConfigureAwait(true);
            }
            else if (step is IMenuHint menuHint)
            {
                await ActivateMenuFtue(menuHint, ct).ConfigureAwait(true);
            }
            else if (step is IDialogue dialogue)
            {
                await ActivateDialogueFtue(dialogue, ct).ConfigureAwait(true);
            }
        }

        private async Task ActivateWorldFtue(IWorldHint worldHint, CancellationToken ct)
        {
            blocker.SetActive(true);
            blockInput?.Invoke(true);

            continueButton.gameObject.SetActive(false);

            if (worldHint.RecenterCamera)
            {
                await recenterCamera(worldHint, _data, ct).ConfigureAwait(true);
            }

            var camera = getCamera?.Invoke(_data);
            if (camera != null)
            {
                var vertices = GetVertices(worldHint, camera);

                var minMax = Positions.FindViewMinMax(vertices, camera);

                pointerWorldRect.GetComponent<RectTransform>().anchorMin = minMax.Item1;
                pointerWorldRect.GetComponent<RectTransform>().anchorMax = minMax.Item2;

                var pointerInstance = pointerAnchor.GetComponentInChildren<TutorialPointer>();

                if (pointerInstance == null)
                {
                    pointerInstance = Instantiate(worldHint.Config.pointerPrefab, pointerAnchor);
                }
                else if (pointerInstance.name != worldHint.Config.pointerPrefab.name + "(Clone)")
                {
                    Destroy(pointerInstance.gameObject);
                    pointerInstance = Instantiate(worldHint.Config.pointerPrefab, pointerAnchor);
                }

                pointerInstance.Init(pointerWorldRect, pointerWorldRect.GetComponentInParent<Canvas>().worldCamera, worldHint.PointerAnchor, worldHint.Completion == WorldHintCompletion.TapAnywhere);

                var hintBoxInstance = hintBoxAnchor.GetComponentInChildren<TutorialHintBox>();
                if (worldHint.Config.hintBoxPrefab != null && !string.IsNullOrEmpty(worldHint.Hint))
                {
                    if(hintBoxInstance == null)
                    {
                        hintBoxInstance = Instantiate(worldHint.Config.hintBoxPrefab, hintBoxAnchor);
                    }
                    else if (hintBoxInstance.name != worldHint.Config.hintBoxPrefab.name + "(Clone)")
                    {
                        Destroy(hintBoxInstance.gameObject);
                        hintBoxInstance = Instantiate(worldHint.Config.hintBoxPrefab, hintBoxAnchor);
                    }

                    hintBoxInstance.Init(pointerInstance.handAnchor, null, worldHint.HintBoxAnchor, worldHint.Hint);
                }

                var dialogBoxInstance = dialogueBoxAnchor.GetComponentInChildren<TutorialDialogueBox>();
                if (worldHint.Config.dialogueBoxPrefab != null && !string.IsNullOrEmpty(worldHint.Dialogue))
                {
                    if (dialogBoxInstance == null)
                    {
                        dialogBoxInstance = Instantiate(worldHint.Config.dialogueBoxPrefab, dialogueBoxAnchor);
                    }
                    else if (dialogBoxInstance.name != worldHint.Config.dialogueBoxPrefab.name + "(Clone)")
                    {
                        Destroy(dialogBoxInstance.gameObject);
                        dialogBoxInstance = Instantiate(worldHint.Config.dialogueBoxPrefab, dialogueBoxAnchor);
                        dialogBoxInstance.AnimationPlayed = true;
                    }

                    dialogBoxInstance.Init(worldHint.DialogueBubbleAnchor, worldHint.DialogueCharacterAnchor, worldHint.Dialogue, worldHint.CharacterName, worldHint.CharacterSprite);
                }

#if SOFTMASK
                if (mask != null)
                    mask.GetComponent<SoftMask>().separateMask = pointerInstance.maskRect;
#endif

                if (worldHint.Highlightable != null)
                    worldHint.Highlightable.StartHighlight();

                tutorialPanel.SetActive(true);
                await PlayPushAnimation(dialogBoxInstance, hintBoxInstance, pointerInstance);

                continueButton.gameObject.SetActive(worldHint.Completion != WorldHintCompletion.WaitForCancel);

                GameObject lastSelectedObject = EventSystem.current.currentSelectedGameObject;

                while (!done && !ct.IsCancellationRequested)
                {
                    await Task.Yield();

                    if (continueButton.gameObject.activeSelf)
                    {
                        blockInput?.Invoke(false);
                        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
                    }

                    minMax = Positions.FindViewMinMax(vertices, camera);

                    pointerWorldRect.GetComponent<RectTransform>().anchorMin = minMax.Item1;
                    pointerWorldRect.GetComponent<RectTransform>().anchorMax = minMax.Item2;

                    pointerInstance.UpdatePosition();
                    if (hintBoxInstance != null) hintBoxInstance.UpdatePosition();
                }

                blockInput?.Invoke(true);

                await PlayPopAnimation(worldHint.DontPopDialogueBox ? null : dialogBoxInstance, worldHint.DontPopHintBox ? null : hintBoxInstance, worldHint.DontPopHintBox ? null : pointerInstance);

                if (worldHint.Highlightable != null)
                    worldHint.Highlightable.StopHighlight();

                if (continueButton.gameObject.activeSelf)
                    EventSystem.current.SetSelectedGameObject(lastSelectedObject);
            }

            if (!worldHint.DontPopDialogueBox && !worldHint.DontPopHintBox)
            {
                tutorialPanel.SetActive(false);
            }

            blocker.SetActive(false);
            blockInput?.Invoke(false);
        }

        private async Task ActivateMenuFtue(IMenuHint menuHint, CancellationToken ct)
        {
            blocker.SetActive(true);
            blockInput?.Invoke(true);

            Button button = null;
            ScrollRect scrollRect = null;
            bool scrollRectWasEnabled = true;

            continueButton.gameObject.SetActive(false);
            blocker.SetActive(menuHint.Completion == MenuHintCompletion.TapAnywhere);

            var currentMenuElement = await FindElementAsync(menuHint, ct).ConfigureAwait(true);

            if (currentMenuElement == null || (menuHint.SkipIfInactive && !currentMenuElement.gameObject.activeInHierarchy))
            {
                Debug.LogWarning("Menu Element with Tag ID " + menuHint.TagId + " not found! Exiting step.");
                done = true;
            }
            else
            {
                void UpdateButton()
                {
                    if (menuHint.Completion == MenuHintCompletion.PressButton)
                    {
                        button = currentMenuElement.GetComponentInChildren<Button>(true);
                        if (button != null)
                        {
                            button.onClick.AddListener(ContinueButtonClicked);
                            EventSystem.current.SetSelectedGameObject(button.gameObject);

                            scrollRect = button.GetComponentInParent<ScrollRect>();
                            if (scrollRect != null)
                            {
                                scrollRectWasEnabled = scrollRect.enabled;
                                scrollRect.enabled = false;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Menu Element with Tag ID " + menuHint.TagId + " did not contain button! Exiting step.");
                            done = true;
                        }
                    }
                }

                UpdateButton();

                if (!done)
                {
                    var menuRoot = findMenuRoot(menuHint, _data);
                    var canvas = menuRoot != null ? menuRoot.GetComponent<Canvas>() : null;

                    var pointerInstance = pointerAnchor.GetComponentInChildren<TutorialPointer>();

                    if (pointerInstance == null)
                    {
                        pointerInstance = Instantiate(menuHint.Config.pointerPrefab, pointerAnchor);
                    }
                    else if (pointerInstance.name != menuHint.Config.pointerPrefab.name + "(Clone)")
                    {
                        Destroy(pointerInstance.gameObject);
                        pointerInstance = Instantiate(menuHint.Config.pointerPrefab, pointerAnchor);
                    }

                    void InitPointer()
                    {
                        var rectTransform = currentMenuElement.GetComponent<RectTransform>();
                        pointerInstance.Init(rectTransform, canvas != null ? canvas.worldCamera : null, menuHint.PointerAnchor, menuHint.Completion == MenuHintCompletion.TapAnywhere);
                    }
                    InitPointer();

                    var hintBoxInstance = hintBoxAnchor.GetComponentInChildren<TutorialHintBox>();
                    if (menuHint.Config.hintBoxPrefab != null && !string.IsNullOrEmpty(menuHint.Hint))
                    {
                        if (hintBoxInstance == null)
                        {
                            hintBoxInstance = Instantiate(menuHint.Config.hintBoxPrefab, hintBoxAnchor);
                        }
                        else if (hintBoxInstance.name != menuHint.Config.hintBoxPrefab.name + "(Clone)")
                        {
                            Destroy(hintBoxInstance.gameObject);
                            hintBoxInstance = Instantiate(menuHint.Config.hintBoxPrefab, hintBoxAnchor);
                        }

                        hintBoxInstance.Init(pointerInstance.handAnchor, null, menuHint.HintBoxAnchor, menuHint.Hint);
                    }

                    var dialogBoxInstance = dialogueBoxAnchor.GetComponentInChildren<TutorialDialogueBox>();
                    if (menuHint.Config.dialogueBoxPrefab != null && !string.IsNullOrEmpty(menuHint.Dialogue))
                    {
                        if (dialogBoxInstance == null)
                        {
                            dialogBoxInstance = Instantiate(menuHint.Config.dialogueBoxPrefab, dialogueBoxAnchor);
                        }
                        else if (dialogBoxInstance.name != menuHint.Config.dialogueBoxPrefab.name + "(Clone)")
                        {
                            Destroy(dialogBoxInstance.gameObject);
                            dialogBoxInstance = Instantiate(menuHint.Config.dialogueBoxPrefab, dialogueBoxAnchor);
                            dialogBoxInstance.AnimationPlayed = true;
                        }

                        dialogBoxInstance.Init(menuHint.DialogueBubbleAnchor, menuHint.DialogueCharacterAnchor, menuHint.Dialogue, menuHint.CharacterName, menuHint.CharacterSprite);
                    }

#if SOFTMASK
                    if (mask != null)
                        mask.GetComponent<SoftMask>().separateMask = pointerInstance.maskRect;
#endif

                    IHighlightable highlightable = null;
                    if (menuHint.HighlightElement)
                    {
                        highlightable = currentMenuElement.GetComponentInChildren<IHighlightable>();

                        if (highlightable != null)
                            highlightable.StartHighlight();
                    }

                    tutorialPanel.SetActive(true);
                    await PlayPushAnimation(dialogBoxInstance, hintBoxInstance, pointerInstance);

                    continueButton.gameObject.SetActive(menuHint.Completion == MenuHintCompletion.TapAnywhere);

                    GameObject lastSelectedObject = EventSystem.current.currentSelectedGameObject;

                    while (!done && !ct.IsCancellationRequested)
                    {
                        pointerInstance.UpdatePosition();
                        if (hintBoxInstance != null) hintBoxInstance.UpdatePosition();

                        await Task.Yield();
                        blockInput?.Invoke(false);

                        if (continueButton.gameObject.activeSelf)
                            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);

                        if (currentMenuElement == null)
                        {
                            blocker.SetActive(true);
                            blockInput?.Invoke(true);

                            currentMenuElement = await FindElementAsync(menuHint, ct).ConfigureAwait(true);

                            if (currentMenuElement != null)
                            {
                                UpdateButton();
                                InitPointer();

                                blocker.SetActive(menuHint.Completion == MenuHintCompletion.TapAnywhere);
                            }
                            else
                            {
                                Debug.LogWarning("Menu Element with Tag ID " + menuHint.TagId + " not found! Exiting step.");
                                done = true;
                            }
                        }
                    }

                    blockInput?.Invoke(true);

                    await PlayPopAnimation(menuHint.DontPopDialogueBox ? null : dialogBoxInstance, menuHint.DontPopHintBox ? null : hintBoxInstance, menuHint.DontPopHintBox ? null : pointerInstance);

                    if (highlightable != null)
                        highlightable.StopHighlight();

                    if (continueButton.gameObject.activeSelf)
                        EventSystem.current.SetSelectedGameObject(lastSelectedObject);
                }
            }

            if (button != null)
            {
                button.onClick.RemoveListener(ContinueButtonClicked);

                if (scrollRect != null)
                    scrollRect.enabled = scrollRectWasEnabled;
            }

            if (!menuHint.DontPopDialogueBox && !menuHint.DontPopHintBox)
            {
                tutorialPanel.SetActive(false);
            }

            blocker.SetActive(false);
            blockInput?.Invoke(false);
        }

        private async Task ActivateDialogueFtue(IDialogue dialogue, CancellationToken ct)
        {
            blocker.SetActive(true);
            blockInput?.Invoke(true);

            continueButton.gameObject.SetActive(false);

            if (mask != null)
                mask.SetActive(false);

            var dialogBoxInstance = dialogueBoxAnchor.GetComponentInChildren<TutorialDialogueBox>();

            if (dialogBoxInstance == null)
            {
                dialogBoxInstance = Instantiate(dialogue.Config.dialogueBoxPrefab, dialogueBoxAnchor);
            }
            else if (dialogBoxInstance.name != dialogue.Config.dialogueBoxPrefab.name + "(Clone)")
            {
                Destroy(dialogBoxInstance.gameObject);
                dialogBoxInstance = Instantiate(dialogue.Config.dialogueBoxPrefab, dialogueBoxAnchor);
                dialogBoxInstance.AnimationPlayed = true;
            }

            dialogBoxInstance.Init(dialogue.DialogueBubbleAnchor, dialogue.DialogueCharacterAnchor, dialogue.Dialogue, dialogue.CharacterName, dialogue.CharacterSprite);

            tutorialPanel.SetActive(true);
            await PlayPushAnimation(dialogBoxInstance, null, null);

            continueButton.gameObject.SetActive(true);
            GameObject lastSelectedObject = EventSystem.current.currentSelectedGameObject;

            while (!done && !ct.IsCancellationRequested)
            {
                await Task.Yield();

                blockInput?.Invoke(false);
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
            }

            blockInput?.Invoke(true);

            await PlayPopAnimation(dialogue.DontPopDialogueBox ? null : dialogBoxInstance, null, null);

            EventSystem.current.SetSelectedGameObject(lastSelectedObject);

            if (!dialogue.DontPopDialogueBox)
            {
                tutorialPanel.SetActive(false);
            }

            blocker.SetActive(false);
            blockInput?.Invoke(false);

            if (mask != null)
                mask.SetActive(true);
        }

        private async Task UpdatePosition(TutorialPointer pointerInstance, TutorialHintBox hintBoxInstance, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (pointerInstance != null) pointerInstance.UpdatePosition();
                if (hintBoxInstance != null) hintBoxInstance.UpdatePosition();

                await Task.Yield();
            }
        }

        private async Task PlayPushAnimation(TutorialDialogueBox dialogBoxInstance, TutorialHintBox hintBoxInstance, TutorialPointer pointerInstance)
        {
            List<Task> tasks = new List<Task>();

            if (dialogBoxInstance != null)
                tasks.Add(dialogBoxInstance.Push());

            if (hintBoxInstance != null)
                tasks.Add(hintBoxInstance.Push());

            if (pointerInstance != null)
                tasks.Add(pointerInstance.Push());

            var ctSource = new CancellationTokenSource();
            UpdatePosition(pointerInstance, hintBoxInstance, ctSource.Token).FireAndForgetTask();

            await Task.WhenAll(tasks).ConfigureAwait(true);

            ctSource.Cancel();
            ctSource.Dispose();
        }

        private async Task PlayPopAnimation(TutorialDialogueBox dialogBoxInstance, TutorialHintBox hintBoxInstance, TutorialPointer pointerInstance)
        {
            List<Task> tasks = new List<Task>();

            if (dialogBoxInstance != null)
                tasks.Add(dialogBoxInstance.Pop());

            if (hintBoxInstance != null)
                tasks.Add(hintBoxInstance.Pop());

            if (pointerInstance != null)
                tasks.Add(pointerInstance.Pop());

            await Task.WhenAll(tasks).ConfigureAwait(true);
            await Task.Yield();
            await Task.Yield();
        }

        private async Task<Transform> FindElementAsync(IMenuHint menuHint, CancellationToken ct)
        {
            Transform currentMenuElement = null;

            if (string.IsNullOrEmpty(menuHint.TagId))
            {
                return tutorialPanel.transform;
            }

            var menuRoot = findMenuRoot(menuHint, _data);
            if(!string.IsNullOrEmpty(menuHint.SearchParent))
            {
                menuRoot = FindByName(menuRoot, menuHint.SearchParent);
            }

            float totalTime = 0;
            while (currentMenuElement == null && totalTime < menuHint.SearchTimeout && !ct.IsCancellationRequested)
            {
                currentMenuElement = FindByTag(menuRoot, menuHint.TagId, menuHint.IncludeInactive);

                if (currentMenuElement == null)
                {
                    await Task.Delay(500).ConfigureAwait(true);
                    totalTime += 0.5f;
                }
            }

            return currentMenuElement;
        }

        private Transform FindByName(Transform parent, string name)
        {
            if (parent == null)
                return null;

            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                var element = queue.Dequeue();
                if (element.name == name)
                {
                    return element;
                }

                foreach (Transform child in element)
                {
                    queue.Enqueue(child);
                }
            }

            return null;
        }

        private Transform FindByTag(Transform parent, string id, bool includeInactive)
        {
            if (parent == null)
                return null;

            var tags = parent.GetComponentsInChildren<FtueTag>(includeInactive);
            var tag = tags.FirstOrDefault(t => t.id == id);

            return tag != null ? tag.transform : null;
        }

        private Vector3[] GetVertices(IWorldHint worldHint, Camera camera)
        {
            if (worldHint.Collider != null)
            {
                return Positions.GetVertices(worldHint.Collider.bounds);
            }
            else
            {
                Vector3[] vertices = new Vector3[2];
                vertices[0] = worldHint.Position - camera.transform.right * worldHint.Radius - camera.transform.up * worldHint.Radius;
                vertices[1] = worldHint.Position + camera.transform.right * worldHint.Radius + camera.transform.up * worldHint.Radius;

                return vertices;
            }
        }

        private void ContinueButtonClicked()
        {
            done = true;
        }
    }
}
