using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.UI.Cheats
{
    public class CheatButton : MonoBehaviour
    {
        public Text label;
        public Text shortcutText;
        public Button button;

        public delegate void ButtonClicked();
        public event ButtonClicked OnButtonClicked;

        public void Init(string name, KeyCode[] shortcutKeys)
        {
            label.text = Utils.TextFormat.ToDisplayName(name);

            string shortcutStr = "";
            if (shortcutKeys != null)
            {
                foreach (var key in shortcutKeys)
                    shortcutStr += key.ToString() + " ";
            }

            shortcutText.text = shortcutStr;

            button.onClick.AddListener(() => { OnButtonClicked(); });
        }
    }
}