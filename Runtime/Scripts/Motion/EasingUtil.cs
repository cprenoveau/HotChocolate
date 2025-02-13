using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotChocolate.Motion
{
    public enum EasingType
    {
        Linear,
        ElasticEaseIn,
        ElasticEaseOut,
        ElasticEaseInOut,
        BounceEaseIn,
        BounceEaseOut,
        BounceEaseInOut,
        QuadEaseIn,
        QuadEaseOut,
        QuadEaseInOut,
        BackEaseIn,
        BackEaseOut,
        BackEaseInOut,
        ExpoEaseIn,
        ExpoEaseOut,
        ExpoEaseInOut,
        CircEaseIn,
        CircEaseOut,
        CircEaseInOut,
        QuartEaseIn,
        QuartEaseOut,
        QuartEaseInOut,
        QuintEaseIn,
        QuintEaseOut,
        QuintEaseInOut,
        SineEaseIn,
        SineEaseOut,
        SineEaseInOut
    }

    public static class EasingUtil
    {
        public static EasingFunction EasingFunction(EasingType type)
        {
            switch (type)
            {
                case EasingType.Linear:
                    return Easing.Linear;

                case EasingType.ElasticEaseIn:
                    return Easing.ElasticEaseIn;

                case EasingType.ElasticEaseOut:
                    return Easing.ElasticEaseOut;

                case EasingType.ElasticEaseInOut:
                    return Easing.ElasticEaseInOut;

                case EasingType.BounceEaseIn:
                    return Easing.BounceEaseIn;

                case EasingType.BounceEaseOut:
                    return Easing.BounceEaseOut;

                case EasingType.BounceEaseInOut:
                    return Easing.BounceEaseInOut;

                case EasingType.QuadEaseIn:
                    return Easing.QuadEaseIn;

                case EasingType.QuadEaseOut:
                    return Easing.QuadEaseOut;

                case EasingType.QuadEaseInOut:
                    return Easing.QuadEaseInOut;

                case EasingType.BackEaseIn:
                    return Easing.BackEaseIn;

                case EasingType.BackEaseOut:
                    return Easing.BackEaseOut;

                case EasingType.BackEaseInOut:
                    return Easing.BackEaseInOut;

                case EasingType.ExpoEaseIn:
                    return Easing.ExpoEaseIn;

                case EasingType.ExpoEaseOut:
                    return Easing.ExpoEaseOut;

                case EasingType.ExpoEaseInOut:
                    return Easing.ExpoEaseInOut;

                case EasingType.CircEaseIn:
                    return Easing.CircEaseIn;

                case EasingType.CircEaseOut:
                    return Easing.CircEaseOut;

                case EasingType.CircEaseInOut:
                    return Easing.CircEaseInOut;

                case EasingType.QuartEaseIn:
                    return Easing.QuartEaseIn;

                case EasingType.QuartEaseOut:
                    return Easing.QuartEaseOut;

                case EasingType.QuartEaseInOut:
                    return Easing.QuartEaseInOut;

                case EasingType.QuintEaseIn:
                    return Easing.QuintEaseIn;

                case EasingType.QuintEaseOut:
                    return Easing.QuintEaseOut;

                case EasingType.QuintEaseInOut:
                    return Easing.QuintEaseInOut;

                case EasingType.SineEaseIn:
                    return Easing.SineEaseIn;

                case EasingType.SineEaseOut:
                    return Easing.SineEaseOut;

                case EasingType.SineEaseInOut:
                    return Easing.SineEaseInOut;

                default:
                    return Easing.Linear;
            }
        }
    }
}
