using HotChocolate.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.FTUE.Widgets
{
    public class TutorialPointer : MonoBehaviour
    {
        public TutorialWidgetAnchor AnchorType
        {
            get { return anchorType; }
            set
            {
                anchorType = value;
                Utils.AnchorWithRotation(handAnchor, anchorType);
            }
        }
        private TutorialWidgetAnchor anchorType;

        public List<Image> blockers = new List<Image>();
        public RectTransform handAnchor;
        public RectTransform maskRect;

        [HideInInspector]
        public RectTransform self;
        [HideInInspector]
        public RectTransform parent;
        [HideInInspector]
        public RectTransform target;

        [HideInInspector]
        public Vector3[] worldPoints = new Vector3[4];
        [HideInInspector]
        public Vector2[] screenPoints = new Vector2[2]; //bottom-left, top-right
        [HideInInspector]
        public Vector2[] localPoints = new Vector2[2]; //bottom-left, top-right

        private Camera _camera;
        private Canvas _canvas;

        public void Init(RectTransform target, Camera camera, TutorialWidgetAnchor anchorType, bool allowInputOutsideTarget)
        {
            this.target = target;
            this.anchorType = anchorType;
            _camera = camera;

            self = GetComponent<RectTransform>();
            parent = transform.parent.GetComponent<RectTransform>();

            _canvas = parent.GetComponentInParent<Canvas>();

            UpdatePosition();
            Utils.AnchorWithRotation(handAnchor, anchorType);

            foreach (var blocker in blockers)
            {
                blocker.raycastTarget = !allowInputOutsideTarget;
            }
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

        public void UpdatePosition()
        {
            if (target != null)
            {
                target.GetWorldCorners(worldPoints);

                self.anchorMin = Vector2.zero;
                self.anchorMax = Vector2.one;

                screenPoints[0] = RectTransformUtility.WorldToScreenPoint(_camera, worldPoints[0]);
                screenPoints[1] = RectTransformUtility.WorldToScreenPoint(_camera, worldPoints[2]);

                int bottomLeftIndex = screenPoints[0].x < screenPoints[1].x ? 0 : 1;
                int topLeftIndex = bottomLeftIndex == 0 ? 1 : 0;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(self, screenPoints[bottomLeftIndex], _canvas.worldCamera, out localPoints[0]);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(self, screenPoints[topLeftIndex], _canvas.worldCamera, out localPoints[1]);

                self.anchorMin = new Vector2(localPoints[0].x / parent.rect.width, localPoints[0].y / parent.rect.height);
                self.anchorMax = new Vector2(localPoints[1].x / parent.rect.width, localPoints[1].y / parent.rect.height);
            }
        }
    }
}
