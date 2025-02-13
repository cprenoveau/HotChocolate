using UnityEngine;

namespace HotChocolate.FTUE.Widgets
{
    [CreateAssetMenu(fileName = "TutorialHintConfig", menuName = "HotChocolate/FTUE/Tutorial Hint Config", order = 1)]
    public class TutorialHintConfig : ScriptableObject
    {
        [Tooltip("The arrow pointing at the ui element. Mandatory")]
        public TutorialPointer pointerPrefab;

        [Tooltip("A text bubble to display next to the ui element. Optional")]
        public TutorialHintBox hintBoxPrefab;

        [Tooltip("A character and a text bubble to explain the ui element. Optional")]
        public TutorialDialogueBox dialogueBoxPrefab;

        [Tooltip("Overrides the current dialogue character sprite. Optional")]
        public Sprite characterSprite;

        [Tooltip("Overrides the current dialogue character name. Optional")]
        public string characterNameKey;
    }
}
