using System;
using UnityEngine;

namespace HotChocolate.Utils
{
    public enum Quadrant
    {
        UpperRight,
        UpperLeft,
        LowerLeft,
        LowerRight
    }

    public struct Angle : IEquatable<Angle>
    {
        public static Angle FromDegrees(float degrees)
        {
            return new Angle { Degrees = degrees };
        }

        public static Angle FromRadians(float radians)
        {
            return new Angle { Radians = radians };
        }

        public static Angle Zero { get { return new Angle { Radians = 0 }; } }

        public float Degrees
        {
            get { return Radians * Mathf.Rad2Deg; }
            set { Radians = value * Mathf.Deg2Rad; }
        }

        public float Radians { get; set; }

        public static Angle operator +(Angle a, Angle b)
        {
            return FromRadians(a.Radians + b.Radians);
        }

        public static Angle Add(Angle a, Angle b)
        {
            return FromRadians(a.Radians + b.Radians);
        }

        public static Angle operator -(Angle a, Angle b)
        {
            return FromRadians(a.Radians - b.Radians);
        }

        public static Angle Subtract(Angle a, Angle b)
        {
            return FromRadians(a.Radians - b.Radians);
        }

        public static Angle operator *(Angle a, Angle b)
        {
            return FromRadians(a.Radians * b.Radians);
        }

        public static Angle Multiply(Angle a, Angle b)
        {
            return FromRadians(a.Radians * b.Radians);
        }

        public static Angle operator *(Angle a, float amount)
        {
            return FromRadians(a.Radians * amount);
        }

        public static Angle operator *(float amount, Angle a)
        {
            return FromRadians(a.Radians * amount);
        }

        public static Angle operator /(Angle a, Angle b)
        {
            return FromRadians(a.Radians / b.Radians);
        }

        public static Angle Divide(Angle a, Angle b)
        {
            return FromRadians(a.Radians / b.Radians);
        }

        public static Angle operator /(Angle a, float amount)
        {
            return FromRadians(a.Radians / amount);
        }

        public override bool Equals(object obj)
        {
            return (obj is Angle angle && angle.Radians == this.Radians);
        }

        public override int GetHashCode()
        {
            return Radians.GetHashCode();
        }

        public static bool operator ==(Angle left, Angle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Angle left, Angle right)
        {
            return !(left == right);
        }

        public bool Equals(Angle other)
        {
            return other.Radians == Radians;
        }
    }

    public static class Trigo
    {
        public static Vector2 PosOnCircle(Vector2 center, float xRadius, float yRadius, Angle angle)
        {
            float x = center.x + xRadius * Mathf.Cos(angle.Radians);
            float y = center.y + yRadius * Mathf.Sin(angle.Radians);

            return new Vector2(x, y);
        }

        public static Angle HorizontalMirror(Angle angle)
        {
            var quadrant = AngleQuadrant(angle);

            if (quadrant == Quadrant.UpperLeft || quadrant == Quadrant.UpperRight)
            {
                return ClampAngle(angle * -1);
            }
            else
            {
                return ClampAngle(Angle.FromRadians(Mathf.PI * 2f - angle.Radians));
            }
        }

        public static Angle VerticalMirror(Angle angle)
        {
            var quadrant = AngleQuadrant(angle);

            if(quadrant == Quadrant.UpperLeft || quadrant == Quadrant.UpperRight)
            {
                return ClampAngle(Angle.FromRadians(Mathf.PI - angle.Radians));
            }
            else
            {
                return ClampAngle(Angle.FromRadians(Mathf.PI * 3f - angle.Radians));
            }
        }

        public static Quadrant AngleQuadrant(Angle angle)
        {
            Angle clampedAngle = ClampAngle(angle);

            if (clampedAngle.Radians >= Mathf.PI * 1.5f)
            {
                return Quadrant.LowerRight;
            }

            if (clampedAngle.Radians >= Math.PI)
            {
                return Quadrant.LowerLeft;
            }

            if (clampedAngle.Radians >= Math.PI * 0.5f)
            {
                return Quadrant.UpperLeft;
            }

            return Quadrant.UpperRight;
        }

        public static Angle PositionToAngle(Vector2 pos)
        {
            float rad = Mathf.Atan2(pos.y, pos.x);
            if (pos.y < 0)
            {
                rad = 2 * Mathf.PI + rad;
            }

            return Angle.FromRadians(rad);
        }

        public static Angle ClampAngle(Angle angle)
        {
            return Angle.FromRadians(ClampAngleRadians(angle.Radians));
        }

        public static float ClampAngleRadians(float radians)
        {
            if (radians < 0)
            {
                return Mathf.PI * 2f + radians;
            }
            else if (radians > Mathf.PI * 2f)
            {
                return radians - Mathf.PI * 2f;
            }

            return radians;
        }

        public static Angle AngleBetween(Angle origin, Angle target)
        {
            var a1 = ClampAngle(origin);
            var a2 = ClampAngle(target);

            float diff = a2.Degrees - a1.Degrees;
            if (diff >= 0f)
            {
                if (diff < 180f)
                    return Angle.FromDegrees(diff);
                else
                    return Angle.FromDegrees(diff - 360f);
            }
            else
            {
                if (diff > -180f)
                    return Angle.FromDegrees(diff);
                else
                    return Angle.FromDegrees(diff + 360f);
            }
        }
    }
}
