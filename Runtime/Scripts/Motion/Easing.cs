using System.Collections;
using System.Collections.Generic;
using System;

namespace HotChocolate.Motion
{
    public static class Easing
    {
        public static float Linear(float progress)
        {
            return progress;
        }

        public static float ElasticEaseIn(float progress)
        {
            if (progress == 0.0f) return 0.0f;
            if (progress == 1.0f) return 1.0f;
            float p = .3f;
            float s = p / 4.0f;
            float postFix = (float)System.Math.Pow(2.0f, 10.0f * (progress -= 1)); // this is a fix, again, with post-increment operators
            return -(float)(postFix * System.Math.Sin((progress - s) * (2.0f * System.Math.PI) / p));
        }

        public static float ElasticEaseOut(float progress)
        {
            if (progress == 0.0f)
            {
                return 0.0f;
            }
            if (progress == 1.0f)
            {
                return 1.0f;
            }
            float p = .3f;
            float s = p / 4.0f;
            return (float)System.Math.Pow(2.0f, -10.0f * progress) * (float)System.Math.Sin((progress - s) * (2.0f * System.Math.PI) / p) + 1.0f;
        }

        public static float ElasticEaseInOut(float progress)
        {
            if (progress == 0.0f) return 0.0f;
            if (progress == 1.0f) return 1.0f;
            progress *= 2.0f;
            float p = .3f * 1.5f;
            float s = p / 4.0f;

            if (progress < 1.0f)
            {
                float postFix = (float)System.Math.Pow(2.0f, 10.0f * (progress -= 1.0f));
                return -.5f * (postFix * (float)System.Math.Sin((progress - s) * (2.0f * System.Math.PI) / p));
            }
            else
            {
                float postFix = (float)System.Math.Pow(2.0f, -10.0f * (progress -= 1.0f));
                return postFix * (float)System.Math.Sin((progress - s) * (2.0f * System.Math.PI) / p) * .5f + 1.0f;
            }
        }

        public static float BounceEaseIn(float progress)
        {
            return 1.0f - BounceEaseOut(1.0f - progress);
        }

        public static float BounceEaseOut(float progress)
        {
            if (progress < (1.0f / 2.75f))
            {
                return (7.5625f * progress * progress);
            }
            else if (progress < (2.0f / 2.75f))
            {
                float postFix = progress -= (1.5f / 2.75f);
                return (7.5625f * postFix * progress + .75f);
            }
            else if (progress < (2.5f / 2.75f))
            {
                float postFix = progress -= (2.25f / 2.75f);
                return (7.5625f * postFix * progress + .9375f);
            }
            else
            {
                float postFix = progress -= (2.625f / 2.75f);
                return (7.5625f * postFix * progress + .984375f);
            }
        }

        public static float BounceEaseInOut(float progress)
        {
            if (progress < 0.5f)
            {
                return BounceEaseIn(progress * 2.0f) * .5f;
            }
            else
            {
                return BounceEaseOut(progress * 2.0f - 1.0f) * .5f + .5f;
            }
        }

        public static float QuadEaseIn(float progress)
        {
            return progress * progress;
        }

        public static float QuadEaseOut(float progress)
        {
            return -progress * (progress - 2.0f);
        }

        public static float QuadEaseInOut(float progress)
        {
            progress *= 2.0f;
            if (progress < 1) return 0.5f * progress * progress;
            --progress;
            return -0.5f * (((progress - 2) * (progress)) - 1);
        }

        public static float BackEaseIn(float progress)
        {
            float s = 1.70158f;
            return progress * progress * ((s + 1) * progress - s);
        }

        public static float BackEaseOut(float progress)
        {
            float s = 1.70158f;
            progress -= 1.0f;
            return ((progress) * progress * ((s + 1) * progress + s) + 1);
        }

        public static float BackEaseInOut(float progress)
        {
            float s = 1.70158f;
            progress *= 2.0f;
            if (progress < 1.0f)
            {
                s *= 1.525f;
                return .5f * (progress * progress * (((s) + 1) * progress - s));
            }
            float postFix = progress -= 2.0f;
            s *= 1.525f;
            return .5f * ((postFix) * progress * (((s) + 1.0f) * progress + s) + 2.0f);
        }

        public static float ExpoEaseIn(float progress)
        {
            return (progress == 0.0f) ? 0 : (float)System.Math.Pow(2.0f, 10 * (progress - 1));
        }

        public static float ExpoEaseOut(float progress)
        {
            return (progress == 1.0f) ? 1 : (float)(-System.Math.Pow(2.0f, -10 * progress) + 1);
        }

        public static float ExpoEaseInOut(float progress)
        {
            if (progress == 0.0f) return 0.0f;
            if (progress == 1.0f) return 1.0f;
            progress *= 2.0f;
            if (progress < 1.0f) return 0.5f * (float)System.Math.Pow(2.0f, 10 * (progress - 1));
            return 0.5f * (float)(-System.Math.Pow(2.0f, -10 * --progress) + 2);
        }

        public static float CircEaseIn(float progress)
        {
            return -(float)(System.Math.Sqrt(1 - progress * progress) - 1);
        }

        public static float CircEaseOut(float progress)
        {
            progress -= 1.0f;
            return (float)System.Math.Sqrt(1 - progress * progress);
        }

        public static float CircEaseInOut(float progress)
        {
            progress *= 2.0f;
            if (progress < 1)
            {
                return -0.5f * (float)(System.Math.Sqrt(1 - progress * progress) - 1);
            }
            progress -= 2;
            return .5f * (float)(System.Math.Sqrt(1 - progress * progress) + 1);
        }

        public static float QuartEaseIn(float progress)
        {
            return progress * progress * progress * progress;
        }

        public static float QuartEaseOut(float progress)
        {
            progress -= 1.0f;
            return -(progress * progress * progress * progress - 1);
        }

        public static float QuartEaseInOut(float progress)
        {
            progress -= 1.0f;
            return -(progress * progress * progress * progress - 1);
        }

        public static float QuintEaseIn(float progress)
        {
            return progress * progress * progress * progress * progress;
        }

        public static float QuintEaseOut(float progress)
        {
            progress -= 1.0f;
            return (progress * progress * progress * progress * progress + 1);
        }

        public static float QuintEaseInOut(float progress)
        {
            progress *= 2.0f;
            if (progress < 1)
            {
                return .5f * progress * progress * progress * progress * progress;
            }
            progress -= 2;
            return .5f * (progress * progress * progress * progress * progress + 2);
        }

        public static float SineEaseIn(float progress)
        {
            return -(float)System.Math.Cos(progress * (System.Math.PI / 2)) + 1.0f;
        }

        public static float SineEaseOut(float progress)
        {
            return (float)System.Math.Sin(progress * (System.Math.PI / 2));
        }

        public static float SineEaseInOut(float progress)
        {
            return -.5f * (float)(System.Math.Cos(System.Math.PI * progress) - 1.0f);
        }
    }
}