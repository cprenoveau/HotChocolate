using UnityEngine;

namespace HotChocolate.UI
{
    [RequireComponent(typeof(Canvas))]
    public class SafeArea : MonoBehaviour
    {
        public bool applyHorizontal = true;
        public bool applyVertical = false;

        public RectTransform anchor;
        public Canvas Canvas { get; private set; }

#if UNITY_ANDROID || UNITY_IOS
        private Rect _lastSafeArea;
        private Vector2 _lastResolution;

        private void Awake()
        {
            Canvas = GetComponent<Canvas>();
            _lastSafeArea = Screen.safeArea;
            _lastResolution = new Vector2(Screen.width, Screen.height);
        }

        private void Start()
        {
            ApplySafeArea(_lastSafeArea);
        }

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea || _lastResolution.x != Screen.width || _lastResolution.y != Screen.height)
            {
                _lastSafeArea = Screen.safeArea;
                _lastResolution = new Vector2(Screen.width, Screen.height);

                ApplySafeArea(_lastSafeArea);
            }
        }

        private void ApplySafeArea(Rect safeArea)
        {
            if (!applyHorizontal)
            {
                safeArea.x = 0;
                safeArea.width = Screen.width;
            }

            if(!applyVertical)
            {
                safeArea.y = 0;
                safeArea.height = Screen.height;
            }

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Canvas.pixelRect.width;
            anchorMin.y /= Canvas.pixelRect.height;
            anchorMax.x /= Canvas.pixelRect.width;
            anchorMax.y /= Canvas.pixelRect.height;

            anchor.anchorMin = anchorMin;
            anchor.anchorMax = anchorMax;
        }
#endif
    }
}