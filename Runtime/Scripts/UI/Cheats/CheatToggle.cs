using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.UI.Cheats
{
    public class CheatToggle : MonoBehaviour
    {
        public Text label;
        public Text shortcutText;
        public Toggle toggle = null;

        public delegate void ValueChanged(bool isOn);
        public event ValueChanged OnValueChanged;

        public void Init(string name, KeyCode[] shortcutKeys, bool value)
        {
            label.text = Utils.TextFormat.ToDisplayName(name);

            string shortcutStr = "";
            if(shortcutKeys != null)
            {
                foreach (var key in shortcutKeys)
                    shortcutStr += key.ToString() + " ";
            }

            shortcutText.text = shortcutStr;

            toggle.isOn = value;
            toggle.onValueChanged.AddListener((bool isOn) => { OnValueChanged(isOn); });
        }
    }
}