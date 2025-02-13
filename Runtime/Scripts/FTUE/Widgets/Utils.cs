using UnityEngine;

namespace HotChocolate.FTUE.Widgets
{
    public enum TutorialWidgetAnchor
    {
        Top,
        Bottom,
        Left,
        Right,
        Center
    }

    public static class Utils
    {
        public static void Anchor(RectTransform anchor, TutorialWidgetAnchor anchorType)
        {
            switch (anchorType)
            {
                case TutorialWidgetAnchor.Top:
                    anchor.pivot = new Vector2(0.5f, 0f);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0.5f, 1f);
                    break;

                case TutorialWidgetAnchor.Bottom:
                    anchor.pivot = new Vector2(0.5f, 1f);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0.5f, 0f);
                    break;

                case TutorialWidgetAnchor.Left:
                    anchor.pivot = new Vector2(1f, 0.5f);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0f, 0.5f);
                    break;

                case TutorialWidgetAnchor.Right:
                    anchor.pivot = new Vector2(0f, 0.5f);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(1f, 0.5f);
                    break;

                case TutorialWidgetAnchor.Center:
                    anchor.pivot = new Vector2(0.5f, 0.5f);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
            }
        }

        public static void AnchorWithRotation(RectTransform anchor, TutorialWidgetAnchor anchorType)
        {
            switch (anchorType)
            {
                case TutorialWidgetAnchor.Top:
                    anchor.pivot = new Vector2(0.5f, 0);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0.5f, 1f);
                    anchor.eulerAngles = Vector3.zero;
                    break;

                case TutorialWidgetAnchor.Bottom:
                    anchor.pivot = new Vector2(0.5f, 0);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0.5f, 0f);
                    anchor.eulerAngles = new Vector3(0, 0, 180);
                    break;

                case TutorialWidgetAnchor.Left:
                    anchor.pivot = new Vector2(0.5f, 0);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0f, 0.5f);
                    anchor.eulerAngles = new Vector3(0, 0, 90);
                    break;

                case TutorialWidgetAnchor.Right:
                    anchor.pivot = new Vector2(0.5f, 0);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(1f, 0.5f);
                    anchor.eulerAngles = new Vector3(0, 0, -90);
                    break;

                case TutorialWidgetAnchor.Center:
                    anchor.pivot = new Vector2(0.5f, 0.5f);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0.5f, 0.5f);
                    anchor.eulerAngles = Vector3.zero;
                    break;
            }
        }
    }
}
