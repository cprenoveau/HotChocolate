using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotChocolate.Motion
{
    public class Silence : IClip
    {
        public delegate void FinishEventHandler();
        public event FinishEventHandler OnFinish = delegate {};

        public float Duration { get; private set; }

        private float _currentTime;
        private bool _finished;

        public Silence(float duration)
        {
            Duration = duration;
        }

        public bool Play(float elapsed, bool reverse = false)
        {
            if (_finished)
            {
                return false;
            }

            _currentTime += elapsed;

            if (_currentTime >= Duration)
            {
                _finished = true;
                OnFinish();

                return false;
            }
            else
            {
                return true;
            }
        }

        public void Seek(float progress)
        {
            _finished = false;
            progress = Mathf.Clamp01(progress);

            _currentTime = Duration * progress;
        }
    }
}
