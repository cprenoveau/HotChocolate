using HotChocolate.UI;
using HotChocolate.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace HotChocolate.FTUE.Widgets
{
    public class TutorialHintBox : MonoBehaviour
    {
        public TutorialWidgetAnchor AnchorType
        {
            get { return anchorType; }
            set
            {
                anchorType = value;
                Utils.Anchor(panelAnchor, anchorType);
            }
        }

        private TutorialWidgetAnchor anchorType;

        [System.Serializable]
        public class TutorialWidgetBubble
        {
            public TutorialWidgetAnchor anchorType;
            public GameObject bg;
        }

        public List<TutorialWidgetBubble> bubbleBg = new List<TutorialWidgetBubble>();
        public TMP_Text message;
        public RectTransform panelAnchor;

        [HideInInspector]
        public RectTransform self;
        [HideInInspector]
        public RectTransform parent;
        [HideInInspector]
        public RectTransform target;

        [HideInInspector]
        public Vector3[] worldPoints = new Vector3[4];
        [HideInInspector]
        public Vector2[] localPoints = new Vector2[2]; //bottom-left, top-right

        private Camera _camera;
        private Canvas _canvas;

        public void Init(RectTransform target, Camera camera, TutorialWidgetAnchor anchorType, string message)
        {
            this.target = target;
            this.anchorType = anchorType;

            _camera = camera;
            _canvas = GetComponentInParent<Canvas>();

            SetBubbleBg(anchorType);

            self = GetComponent<RectTransform>();
            parent = transform.parent.GetComponent<RectTransform>();

            this.message.text = message;

            Utils.Anchor(panelAnchor, anchorType);
            UpdatePosition();
        }

        public async Task Push()
        {
            await Menu.PlayPushAnimationDefault(null, this, default);
        }

        public async Task Pop()
        {
            await Menu.PlayPopAnimationDefault(null, this, default);
            Destroy(gameObject);
        }

        private void SetBubbleBg(TutorialWidgetAnchor anchorType)
        {
            foreach (var bubble in bubbleBg)
                bubble.bg.SetActive(false);

            var toShow = bubbleBg.Find(b => b.anchorType == anchorType);
            if (toShow != null)
                toShow.bg.SetActive(true);
        }

        public void UpdatePosition()
        {
            if (target != null)
            {
                target.GetWorldCorners(worldPoints);

                (Vector2 min, Vector2 max) = Positions.FindMinMax(worldPoints);

                self.anchorMin = Vector2.zero;
                self.anchorMax = Vector2.one;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(self, min, _camera, out localPoints[0]);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(self, max, _camera, out localPoints[1]);

                self.anchorMin = new Vector2(localPoints[0].x / parent.rect.width, localPoints[0].y / parent.rect.height);
                self.anchorMax = new Vector2(localPoints[1].x / parent.rect.width, localPoints[1].y / parent.rect.height);

                FitPanelOnScreen();
            }
        }

        private void FitPanelOnScreen()
        {
            panelAnchor.anchoredPosition = Vector2.zero;

            var screenRect = Positions.ScreenRect(panelAnchor);
            var screenOffset = Vector2.zero;

            if (screenRect.min.x < 0)
            {
                screenOffset.x = -screenRect.min.x;
            }
            else if(screenRect.max.x > Screen.width)
            {
                screenOffset.x = -(screenRect.max.x - Screen.width);
            }

            if(screenRect.min.y < 0)
            {
                screenOffset.y = -screenRect.min.y;
            }
            else if(screenRect.max.y > Screen.height)
            {
                screenOffset.y = -(screenRect.max.y - Screen.height);
            }

            panelAnchor.transform.localPosition += (Vector3)screenOffset * 1 / _canvas.scaleFactor;
        }
    }
}
