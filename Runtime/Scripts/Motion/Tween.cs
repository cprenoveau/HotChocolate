using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace HotChocolate.Motion
{
    public delegate float EasingFunction(float progress);

    public class Tween<T> : IClip
    {
        public delegate T InterpolationFunction(T first, T second, float scale);
        public delegate void UpdateEventHandler(T value, float progress);
        public delegate void FinishEventHandler();

        public event UpdateEventHandler OnUpdate = delegate {};
        public event FinishEventHandler OnFinish = delegate {};

        public float Duration { get; private set; }
        public T Start { get; private set; }
        public T End { get; private set; }

        private InterpolationFunction _interpolation;
        private EasingFunction _easing;
        private float _progress;
        private bool _finished;

        public Tween(
            float duration,
            T start,
            T end,
            InterpolationFunction interpolation,
            EasingFunction easing)
        {
            Duration = duration;
            Start = start;
            End = end;
            _interpolation = interpolation;
            _easing = easing;
        }
            
        public bool Play(float elapsed, bool reverse = false)
        {
            if (_finished)
            {
                return false;
            }

            float p = reverse ? 1f - _progress : _progress;
            var currentValue = _interpolation(Start,End,_easing(p));

            OnUpdate(currentValue, _progress);

            if (_progress == 1f)
            {
                _finished = true;
                OnFinish();

                return false;
            }
            else
            {
                _progress += elapsed / Duration;

                if (_progress > 1f)
                {
                    _progress = 1f;
                }

                return true;
            }
        }
            
        public void Seek(float progress)
        {
            _finished = false;
            _progress = Mathf.Clamp01(progress);
        }
    }
}
