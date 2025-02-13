using HotChocolate.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace HotChocolate.UI.Cheats
{
    public class CheatTab : MonoBehaviour
    {
        public Text label;
        public Tab tab;

        public int Index { get; private set; }

        public void Init(int index, string name, bool isOn)
        {
            Index = index;

            label.text = Utils.TextFormat.ToDisplayName(name);
            tab.IsOn = isOn;
        }
    }
}