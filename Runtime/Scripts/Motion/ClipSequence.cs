using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HotChocolate.Motion
{
    public class ClipSequence : IClip
    {
        public delegate void FinishEventHandler();
        public event FinishEventHandler OnFinish = delegate {};

        public float Duration {
            get
            {
                float duration = 0;
                foreach(IClip clip in _clips)
                {
                    duration += clip.Duration;
                }

                return duration;
            }
        }

        private List<IClip> _clips = new List<IClip>();
        private int _currentClip;
        private bool _finished;

        public void Append(IClip clip)
        {
            _clips.Add(clip);
        }

        public bool Play(float elapsed, bool reverse = false)
        {
            if (_finished || _clips.Count == 0)
            {
                return false;
            }

            int i = reverse ? _clips.Count - _currentClip - 1 : _currentClip;

            if (!_clips[i].Play(elapsed, reverse))
            {
                _currentClip++;

                if (_currentClip >= _clips.Count)
                {
                    _finished = true;
                    OnFinish();

                    return false;
                }
            }

            return true;
        }

        public void Seek(float progress)
        {
            _finished = false;

            progress = Mathf.Clamp01(progress);

            foreach (IClip clip in _clips)
            {
                clip.Seek(0);
            }

            float time = Duration * progress;

            for(int i = 0; i < _clips.Count; ++i)
            {
                float clipDuration = _clips[i].Duration;
                if (time < clipDuration || i == _clips.Count - 1)
                {
                    _currentClip = i;
                    _clips[i].Seek(time / clipDuration);
                    break;
                }
                else
                {
                    time -= Duration;
                }
            }
        }
    }
}
