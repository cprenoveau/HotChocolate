using UnityEngine;

namespace HotChocolate.FTUE.Widgets
{
    public enum WorldHintCompletion
    {
        TapOnObject,
        TapAnywhere,
        WaitForCancel
    }

    public interface IWorldHint
    {
        string Dialogue { get; }
        string CharacterName { get; }
        Sprite CharacterSprite { get; }
        string Hint { get; }
        Vector3 Position { get; }
        float Radius { get; }
        float CameraHeight { get; }
        Collider Collider { get; }
        IHighlightable Highlightable { get; }
        WorldHintCompletion Completion { get; }
        TutorialWidgetAnchor PointerAnchor { get; }
        TutorialWidgetAnchor HintBoxAnchor { get; }
        DialogueBubbleAnchor DialogueBubbleAnchor { get; }
        DialogueCharacterAnchor DialogueCharacterAnchor { get; }
        bool RecenterCamera { get; }
        float RecenterDuration { get; }
        bool DontPopDialogueBox { get; }
        bool DontPopHintBox { get; }
        TutorialHintConfig Config { get; }
    }
}
