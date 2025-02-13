using HotChocolate.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.FTUE.Widgets
{
    public enum DialogueBubbleAnchor
    {
        Top,
        Bottom
    }

    public enum DialogueCharacterAnchor
    {
        Left,
        Right
    }

    public class TutorialDialogueBox : MonoBehaviour
    {
        [System.Serializable]
        public class Bubble
        {
            public DialogueBubbleAnchor bubbleAnchor;
            public DialogueCharacterAnchor characterAnchor;
            public GameObject bubble;
            public TextMeshProUGUI message;
            public TMP_Text characterName;
        }

        [System.Serializable]
        public class Character
        {
            public DialogueCharacterAnchor characterAnchor;
            public GameObject character;
            public Image characterImage;
        }

        public List<Bubble> bubbles = new List<Bubble>();
        public List<Character> characters = new List<Character>();

        public bool AnimationPlayed { get; set; }

        public void Init(DialogueBubbleAnchor bubbleAnchor, DialogueCharacterAnchor characterAnchor, string message, string characterName, Sprite characterSprite)
        {
            SetBubble(bubbleAnchor, characterAnchor, message, characterName);
            SetCharacter(characterAnchor, characterSprite);

            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }

        public async Task Push()
        {
            if (!AnimationPlayed)
            {
                await Menu.PlayPushAnimationDefault(null, this, default);
                AnimationPlayed = true;
            }
            else
            {
                await Menu.PlayFocusInAnimationDefault(null, this, default);
            }
        }

        public async Task Pop()
        {
            await Menu.PlayPopAnimationDefault(null, this, default);
            Destroy(gameObject);
        }

        private void SetBubble(DialogueBubbleAnchor bubbleAnchor, DialogueCharacterAnchor characterAnchor, string message, string characterName)
        {
            foreach(var b in bubbles)
            {
                b.bubble.SetActive(false);
            }

            var bubble = bubbles.Find(b => b.bubbleAnchor == bubbleAnchor && b.characterAnchor == characterAnchor);
            if(bubble != null)
            {
                bubble.bubble.SetActive(true);
                bubble.message.text = message;

                if (bubble.characterName != null && !string.IsNullOrEmpty(characterName))
                    bubble.characterName.text = characterName;

                var layout = bubble.message.GetComponentInParent<LayoutGroup>();
                if (layout != null)
                {
                    Canvas.ForceUpdateCanvases();
                    layout.enabled = false;
                    layout.enabled = true;
                }
            }
        }

        private void SetCharacter(DialogueCharacterAnchor characterAnchor, Sprite sprite)
        {
            foreach(var c in characters)
            {
                c.character.SetActive(false);
            }

            var character = characters.Find(c => c.characterAnchor == characterAnchor);
            if(character != null)
            {
                character.character.SetActive(true);

                if (sprite != null)
                    character.characterImage.sprite = sprite;
            }
        }
    }
}
