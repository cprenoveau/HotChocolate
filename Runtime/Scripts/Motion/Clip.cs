using System.Collections;
using System.Collections.Generic;

namespace HotChocolate.Motion
{
    // Properties of a clip:
    // -Must be playable from beginning to end
    // -In reverse, must play from end to beginning
    // -Play returns true while in progress and false when clip finishes playing
    // -Seek jumps to a location on the clip (between 0 and 1, 0 being the start and 1 the end)
    // -Seek allows the clip to be played again

    public interface IClip
    {
        float Duration { get; }

        bool Play(float elapsed, bool reverse = false);
        void Seek(float progress);
    }
}
