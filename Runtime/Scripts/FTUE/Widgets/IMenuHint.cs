
using UnityEngine;

namespace HotChocolate.FTUE.Widgets
{
    public enum MenuHintScope
    {
        Menu,
        Popup,
        World,
        All,
        Custom
    }

    public enum MenuHintCompletion
    {
        TapAnywhere,
        PressButton
    }

    public interface IMenuHint
    {
        string Dialogue { get; }
        string CharacterName { get; }
        Sprite CharacterSprite { get; }
        string Hint { get; }
        string TagId { get; }
        MenuHintScope SearchScope { get; }
        int CustomSearchScope { get; }
        string SearchParent { get; }
        float SearchTimeout { get; }
        MenuHintCompletion Completion { get; }
        TutorialWidgetAnchor PointerAnchor { get; }
        TutorialWidgetAnchor HintBoxAnchor { get; }
        DialogueBubbleAnchor DialogueBubbleAnchor { get; }
        DialogueCharacterAnchor DialogueCharacterAnchor { get; }
        bool IncludeInactive { get; }
        bool SkipIfInactive { get; }
        bool HighlightElement { get; }
        bool DontPopDialogueBox { get; }
        bool DontPopHintBox { get; }
        TutorialHintConfig Config { get; }
    }
}
