using UnityEngine;

namespace HotChocolate.Gestures
{
    public class TouchWrapper
    {
        public int Index { get; private set; }
        public Touch? Touch { get; private set; }

        public TouchWrapper(int index)
        {
            Index = index;
        }

        public void Refresh()
        {
            if (!Touch.HasValue)
            {
                Touch = Utils.GetTouch(Index);
            }

            Touch = Touch.HasValue ? Utils.FindTouch(Touch.Value.fingerId) : null;
        }

        public void Reset()
        {
            Touch = null;
        }
    }
}
