using UnityEngine;

namespace HotChocolate.Gestures
{
    public static class Utils
    {
        public static int ActiveTouchCount()
        {
            int count = 0;
            for (int i = 0; i < Input.touchCount; ++i)
            {
                var touch = Input.GetTouch(i);
                if (TouchIsActive(touch))
                {
                    count++;
                }
            }

            return count;
        }

        public static bool TouchIsActive(Touch touch)
        {
            return (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled);
        }

        public static Touch? FindTouch(int fingerId)
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                if (Input.GetTouch(i).fingerId == fingerId)
                    return Input.GetTouch(i);
            }

            return null;
        }

        public static Touch? GetTouch(int index)
        {
            if (Input.touchCount > index)
                return Input.GetTouch(index);

            return null;
        }
    }
}
