using UnityEngine;

namespace HotChocolate.FTUE.Widgets
{
    public interface IDialogue
    {
        string Dialogue { get; }
        string CharacterName { get; }
        Sprite CharacterSprite { get; }
        DialogueBubbleAnchor DialogueBubbleAnchor { get; }
        DialogueCharacterAnchor DialogueCharacterAnchor { get; }
        bool DontPopDialogueBox { get; }
        TutorialHintConfig Config { get; }
    }
}
